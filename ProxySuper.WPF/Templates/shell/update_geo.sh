#!/usr/bin/env bash
#
# ProxySU Geo 文件更新脚本
# 用途：下载最新的 GeoSite + GeoIP 数据库
# 数据来源：https://github.com/Loyalsoldier/v2ray-rules-dat
# 安装路径：/etc/proxysu/xray/
# 用法：
#   bash update_geo.sh          # 更新到最新版
#   bash update_geo.sh <version> # 更新到指定版本
#

set -e

SCRIPT_VERSION="1.0.0"
GEO_DIR="/etc/proxysu/xray"
LOG_FILE="/var/log/proxysu_geo.log"

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

# 检测系统
checkSystem() {
    local arch=$(uname -m)
    case "$arch" in
        amd64|x86_64)  CPU="amd64" ;;
        aarch64|arm64) CPU="arm64" ;;
        armv7l)        CPU="armv7" ;;
        *)
            red "不支持的 CPU 架构: $arch"
            exit 1
            ;;
    esac
    blue "检测到架构: $CPU"
}

# 获取最新版本号
getLatestVersion() {
    local version
    version=$(curl -sf "https://api.github.com/repos/Loyalsoldier/v2ray-rules-dat/releases/latest" | jq -r '.tag_name' 2>/dev/null)
    echo "$version"
}

# 下载 geo 文件
downloadGeo() {
    local version="$1"
    mkdir -p "$GEO_DIR"

    blue "开始下载 Geo 文件，版本: $version"

    # 下载 geosite.dat
    blue "下载 geosite.dat..."
    curl -L --fail --retry 3 -o "$GEO_DIR/geosite.dat" \
        "https://github.com/Loyalsoldier/v2ray-rules-dat/releases/download/$version/geosite.dat" \
        2>&1 | tee -a "$LOG_FILE"

    # 下载 geoip.dat
    blue "下载 geoip.dat..."
    curl -L --fail --retry 3 -o "$GEO_DIR/geoip.dat" \
        "https://github.com/Loyalsoldier/v2ray-rules-dat/releases/download/$version/geoip.dat" \
        2>&1 | tee -a "$LOG_FILE"

    # 设置权限
    chmod 644 "$GEO_DIR/geosite.dat" "$GEO_DIR/geoip.dat"

    # 验证
    if [[ -f "$GEO_DIR/geosite.dat" ]] && [[ -f "$GEO_DIR/geoip.dat" ]]; then
        local geo_size=$(stat -c%s "$GEO_DIR/geosite.dat" 2>/dev/null || stat -f%z "$GEO_DIR/geosite.dat" 2>/dev/null)
        local ip_size=$(stat -c%s "$GEO_DIR/geoip.dat" 2>/dev/null || stat -f%z "$GEO_DIR/geoip.dat" 2>/dev/null)
        green "GeoSite 大小: $geo_size bytes"
        green "GeoIP   大小: $ip_size bytes"
        green "Geo 文件更新成功！"
    else
        red "Geo 文件下载失败"
        exit 1
    fi
}

# 重启相关服务（如果有的话）
reloadService() {
    if systemctl is-active --quiet xray 2>/dev/null; then
        blue "重启 Xray 服务..."
        systemctl restart xray
    fi

    if systemctl is-active --quiet v2ray 2>/dev/null; then
        blue "重启 V2ray 服务..."
        systemctl restart v2ray
    fi

    if systemctl is-active --quiet sing-box 2>/dev/null; then
        blue "重启 Sing-Box 服务..."
        systemctl restart sing-box
    fi
}

# 查看当前版本
showVersion() {
    if [[ -f "$GEO_DIR/geosite.dat" ]]; then
        blue "当前 Geo 文件版本:"
        # geo 文件没有版本号字段，只能显示文件修改时间
        ls -lh "$GEO_DIR/"*.dat 2>/dev/null || ls -l "$GEO_DIR/"*.dat 2>/dev/null
    else
        yellow "未找到 Geo 文件，请先运行更新"
    fi
}

# 主入口
main() {
    local target_version="${1:-}"

    checkSystem

    if [[ -z "$target_version" ]]; then
        blue "获取最新版本..."
        target_version=$(getLatestVersion)
        if [[ -z "$target_version" ]]; then
            red "无法获取最新版本，请检查网络连接"
            exit 1
        fi
    fi

    blue "ProxySU Geo 更新脚本 v$SCRIPT_VERSION"
    blue "目标版本: $target_version"
    echo ""

    downloadGeo "$target_version"
    reloadService

    echo ""
    green "✅ 更新完成！"
    showVersion
}

main "$@"
