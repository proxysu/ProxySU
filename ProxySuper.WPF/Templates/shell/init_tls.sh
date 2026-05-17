#!/usr/bin/env bash
#
# ProxySU TLS 证书脚本
# 用途：申请 + 续期 Let's Encrypt 证书（standalone 模式）
# 用法：
#   bash init_tls.sh issue <domain>   # 申请证书
#   bash init_tls.sh renew <domain>   # 续期证书
#

set -e

SCRIPT_VERSION="1.0.0"
TLS_DIR="/etc/proxysu/tls"
LOG_FILE="/var/log/proxysu_tls.log"

# 颜色输出
echoColor() {
    local color_code="$1"; shift
    echo -e "\033[${color_code}m$*\033[0m"
}
red()    { echoColor "31" "$*"; }
green()  { echoColor "32" "$*"; }
yellow() { echoColor "33" "$*"; }
blue()   { echoColor "36" "$*"; }

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*" | tee -a "$LOG_FILE"
}

# 检测系统并设置包管理器
checkSystem() {
    if [[ -f /etc/redhat-release ]] || grep -qi "centos" /proc/version 2>/dev/null; then
        release="centos"
        installType="yum -y install"
        removeType="yum -y remove"
    elif grep -qi "debian" /etc/issue 2>/dev/null || grep -qi "debian" /proc/version 2>/dev/null; then
        release="debian"
        installType="apt -y install"
        removeType="apt -y autoremove"
    elif grep -qi "ubuntu" /etc/issue 2>/dev/null || grep -qi "ubuntu" /proc/version 2>/dev/null; then
        release="ubuntu"
        installType="apt -y install"
        removeType="apt -y autoremove"
    else
        red "不支持的系统"
        exit 1
    fi
    green "检测到系统: $release"
}

# 安装依赖
installDeps() {
    yellow "安装依赖: socat, nginx, curl"
    $installType update -y
    $installType install -y socat nginx curl jq >/dev/null 2>&1
    green "依赖安装完成"
}

# 安装 acme.sh
installAcmeSh() {
    if [[ -f ~/.acme.sh/acme.sh ]]; then
        green "acme.sh 已安装"
    else
        yellow "安装 acme.sh..."
        curl -s https://get.acme.sh | sh -s -- --installonline >/dev/null 2>&1
        green "acme.sh 安装完成"
    fi
}

# 申请证书 (standalone)
issueCert() {
    local domain="$1"
    if [[ -z "$domain" ]]; then
        red "用法: $0 issue <domain>"
        exit 1
    fi

    blue "开始申请证书，域名: $domain"

    mkdir -p "$TLS_DIR"

    # 临时启动 nginx 用于 standalone 验证
    local nginx_running=false
    if pgrep nginx >/dev/null 2>&1; then
        nginx_running=true
        blue "停止现有 nginx..."
        systemctl stop nginx
    fi

    # 启动 standalone HTTP 服务器
    blue "启动 standalone HTTP-01 验证服务器..."
    nohup socat TCP-LISTEN:80,bind=0.0.0.0,reuseaddr,fork SYSTEM:"echo 'HTTP/1.1 200 OK'" >/dev/null 2>&1 &
    local socat_pid=$!
    sleep 2

    # 申请证书
    blue "申请证书中（可能需要 1-2 分钟）..."
    local result=0
    ~/.acme.sh/acme.sh --issue -d "$domain" --standalone --httpport 80 --keylength ec-256 --log --home "$TLS_DIR/.acme.sh" --cert-home "$TLS_DIR" 2>&1 | tee -a "$LOG_FILE" || result=1

    # 停止 socat
    kill $socat_pid 2>/dev/null || true

    # 恢复 nginx
    if $nginx_running; then
        blue "恢复 nginx..."
        systemctl start nginx
    fi

    if [[ $result -eq 0 ]] && [[ -f "$TLS_DIR/$domain.$domain.crt" || -f "$TLS_DIR/fullchain.cer" ]]; then
        green "证书申请成功！"
        installCert "$domain"
    else
        red "证书申请失败，请检查域名是否已解析且 80 端口未被占用"
        exit 1
    fi
}

# 安装证书到 TLS_DIR
installCert() {
    local domain="$1"
    local acme_home="$TLS_DIR/.acme.sh/$domain"_ecc

    if [[ ! -d "$acme_home" ]]; then
        acme_home=$(find "$TLS_DIR/.acme.sh" -type d -name "*_ecc" | head -1)
    fi

    if [[ -d "$acme_home" ]]; then
        local key_file=$(find "$acme_home" -name "*.key" | head -1)
        local cert_file=$(find "$acme_home" -name "*.cer" | head -1)
        local ca_file=$(find "$acme_home" -name "ca.cer" | head -1)

        [[ -n "$key_file" ]] && cp "$key_file" "$TLS_DIR/$domain.key"
        [[ -n "$cert_file" ]] && cp "$cert_file" "$TLS_DIR/$domain.crt"
        [[ -n "$ca_file" ]] && cp "$ca_file" "$TLS_DIR/$domain.ca.crt"

        green "证书已安装到 $TLS_DIR/"
        ls -lh "$TLS_DIR/"
    fi
}

# 续期证书
renewCert() {
    local domain="$1"
    if [[ -z "$domain" ]]; then
        red "用法: $0 renew <domain>"
        exit 1
    fi

    blue "续期证书: $domain"

    if [[ ! -f "$TLS_DIR/.acme.sh/$domain"_ecc/"$domain".key ]]; then
        red "未找到证书，请先执行 issue"
        exit 1
    fi

    # 停止 nginx
    if pgrep nginx >/dev/null 2>&1; then
        systemctl stop nginx
    fi

    # 启动 standalone 验证
    nohup socat TCP-LISTEN:80,bind=0.0.0.0,reuseaddr,fork SYSTEM:"echo 'HTTP/1.1 200 OK'" >/dev/null 2>&1 &
    local socat_pid=$!
    sleep 2

    # 续期
    ~/.acme.sh/acme.sh --renew -d "$domain" --httpport 80 --home "$TLS_DIR/.acme.sh" --cert-home "$TLS_DIR" 2>&1 | tee -a "$LOG_FILE"

    kill $socat_pid 2>/dev/null || true

    # 重启 nginx
    systemctl start nginx

    installCert "$domain"
    green "续期完成"
}

# 查看证书状态
showCert() {
    local domain="$1"
    if [[ -z "$domain" ]]; then
        red "用法: $0 show <domain>"
        exit 1
    fi

    local acme_home="$TLS_DIR/.acme.sh/$domain"_ecc
    if [[ ! -d "$acme_home" ]]; then
        red "未找到证书: $domain"
        exit 1
    fi

    blue "证书信息: $domain"
    openssl x509 -in "$TLS_DIR/$domain.crt" -noout -dates -issuer 2>/dev/null || \
        ~/.acme.sh/acme.sh --info -d "$domain" --home "$TLS_DIR/.acme.sh"
}

# 主入口
main() {
    local action="${1:-}"
    local domain="${2:-}"

    checkSystem
    installDeps
    installAcmeSh

    case "$action" in
        issue)
            issueCert "$domain"
            ;;
        renew)
            renewCert "$domain"
            ;;
        show)
            showCert "$domain"
            ;;
        *)
            echo "ProxySU TLS 证书管理脚本 v$SCRIPT_VERSION"
            echo ""
            echo "用法:"
            echo "  $0 issue <domain>   申请证书"
            echo "  $0 renew <domain>   续期证书"
            echo "  $0 show <domain>    查看证书信息"
            echo ""
            echo "示例:"
            echo "  $0 issue example.com"
            echo "  $0 renew example.com"
            ;;
    esac
}

main "$@"
