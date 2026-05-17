using ProxySuper.Core.Helpers;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Services
{
    public class SingboxService : ServiceBase<SingboxSettings>
    {
        public SingboxService(Host host, SingboxSettings settings) : base(host, settings)
        {
        }

        public void Install()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    int index = 1;
                    EnsureRootUser();

                    var binaryCheck = Settings.UseDocker ? "docker ps 2>/dev/null | grep sing-box" : "test -f /usr/local/bin/sing-box";
                    if (FileExists("/usr/local/bin/sing-box") || RunCmd(binaryCheck).Contains("sing-box"))
                    {
                        var btnResult = MessageBox.Show("已经安装 Sing-Box，是否需要重装？", "提示", MessageBoxButton.YesNo);
                        if (btnResult == MessageBoxResult.No)
                        {
                            MessageBox.Show("安装终止", "提示");
                            return;
                        }
                    }

                    Progress.Step = $"{index++}. 检测系统环境";
                    EnsureSystemEnv();
                    Progress.Percentage = 5;

                    Progress.Step = $"{index++}. 安装必要的系统工具";
                    InstallSystemTools();
                    Progress.Percentage = 15;

                    Progress.Step = $"{index++}. 配置防火墙";
                    ConfigFirewalld();
                    Progress.Percentage = 20;

                    Progress.Step = $"{index++}. 检测网络环境";
                    EnsureNetwork();
                    if (!Settings.IsIPAddress)
                    {
                        Progress.Desc = "检查域名是否解析正确";
                        ValidateDomain();
                    }
                    Progress.Percentage = 25;

                    Progress.Step = $"{index}. 同步系统和本地时间";
                    SyncTimeDiff();
                    Progress.Percentage = 30;

                    if (Settings.UseDocker)
                    {
                        Progress.Step = $"{index++}. 安装 Docker 并部署 Sing-Box";
                        InstallSingBoxDocker();
                    }
                    else
                    {
                        Progress.Step = $"{index++}. 安装 Sing-Box";
                        InstallSingBox();
                    }
                    Progress.Percentage = 60;

                    Progress.Step = $"{index++}. 安装 TLS 证书";
                    InstallSingBoxTLS();
                    Progress.Percentage = 70;

                    Progress.Step = $"{index++}. 安装 GeoIP/GeoSite 规则文件";
                    InstallGeoFiles();
                    Progress.Percentage = 80;

                    Progress.Step = $"{index++}. 上传 Sing-Box 配置文件";
                    UploadConfig();
                    Progress.Percentage = 85;

                    if (Settings.EnableSubscribe)
                    {
                        Progress.Step = $"{index++}. 安装订阅服务";
                        InstallSubscribeServer();
                    }

                    if (!Settings.UseDocker)
                    {
                        Progress.Step = $"{index++}. 设置规则文件自动更新";
                        SetupGeoUpdateCron();
                    }
                    else
                    {
                        Progress.Step = $"{index++}. 设置 Docker 容器自动更新";
                        SetupDockerAutoUpdate();
                    }
                    Progress.Percentage = 90;

                    Progress.Step = $"{index++}. 启动 Sing-Box 服务";
                    StartSingBox();

                    Progress.Step = $"{index++}. 开启 BBR 加速";
                    EnableBBR();

                    Progress.Percentage = 100;
                    Progress.Step = "安装成功";
                    Progress.Desc = string.Empty;

                    MessageBox.Show("Sing-Box 安装成功！", "提示");
                }
                catch (Exception ex)
                {
                    Progress.Percentage = 0;
                    Progress.Step = "安装失败";
                    Progress.Desc = ex.Message;
                    MessageBox.Show($"安装失败：{ex.Message}", "错误");
                }
            });
        }

        private void InstallSingBox()
        {
            // Detect architecture
            var archResult = RunCmd("uname -m");
            var arch = "amd64";
            if (archResult.Contains("arm") || archResult.Contains("aarch64"))
            {
                arch = "arm64";
            }
            else if (archResult.Contains("386"))
            {
                arch = "386";
            }

            // Auto-detect latest version from GitHub
            Progress.Desc = "正在检测最新版本...";
            var versionResult = RunCmd("curl -s https://api.github.com/repos/SagerNet/sing-box/releases/latest | jq -r .tag_name");
            var version = versionResult?.Trim();
            if (string.IsNullOrEmpty(version) || version.Length > 20)
            {
                version = "1.11.2"; // fallback
            }

            var downloadUrl = $"https://github.com/SagerNet/sing-box/releases/download/v{version}/sing-box-{version}-linux-{arch}.tar.gz";

            Progress.Desc = $"正在下载 Sing-Box v{version}...";
            RunCmd($"cd /tmp && curl -L -o sing-box.tar.gz {downloadUrl}");
            RunCmd("cd /tmp && tar -xzf sing-box.tar.gz");
            RunCmd("cp /tmp/sing-box-*/sing-box /usr/local/bin/sing-box");
            RunCmd("chmod +x /usr/local/bin/sing-box");
            RunCmd("rm -rf /tmp/sing-box*");
        }

        private void InstallGeoFiles()
        {
            Progress.Desc = "正在下载 GeoIP/GeoSite 规则文件...";
            RunCmd("mkdir -p /etc/sing-box");
            RunCmd("curl -L -o /etc/sing-box/geoip-cn.db https://github.com/SagerNet/sing-geoip/raw/release/geoip-cn.db");
            RunCmd("curl -L -o /etc/sing-box/geosite-cn.db https://github.com/SagerNet/sing-geosite/raw/release/geosite-cn.db");
            RunCmd("curl -L -o /etc/sing-box/geosite-geolocation-!cn.db https://github.com/SagerNet/sing-geosite/raw/release/geosite-geolocation-!cn.db");
        }

        private void SetupGeoUpdateCron()
        {
            Progress.Desc = "设置规则文件自动更新...";
            var script = @"#!/bin/bash
curl -L -o /etc/sing-box/geoip-cn.db https://github.com/SagerNet/sing-geoip/raw/release/geoip-cn.db
curl -L -o /etc/sing-box/geosite-cn.db https://github.com/SagerNet/sing-geosite/raw/release/geosite-cn.db
curl -L -o /etc/sing-box/geosite-geolocation-!cn.db https://github.com/SagerNet/sing-geosite/raw/release/geosite-geolocation-!cn.db
systemctl restart sing-box";
            WriteToFile(script, "/etc/sing-box/update-geo.sh");
            RunCmd("chmod +x /etc/sing-box/update-geo.sh");
            RunCmd("(crontab -l 2>/dev/null | grep -v update-geo; echo '0 4 * * 1 /etc/sing-box/update-geo.sh >/dev/null 2>&1') | crontab -");
        }

        private void InstallSingBoxDocker()
        {
            Progress.Desc = "正在安装 Docker...";
            RunCmd("curl -fsSL https://get.docker.com | sh");
            RunCmd("systemctl enable docker");
            RunCmd("systemctl start docker");

            var dockerfile = @"FROM alpine:latest
RUN apk add --no-cache curl ca-certificates tzdata
ARG SINGBOX_VERSION=latest
RUN curl -L https://github.com/SagerNet/sing-box/releases/download/v${SINGBOX_VERSION}/sing-box-${SINGBOX_VERSION}-linux-amd64.tar.gz | tar -xz -C /tmp && \
    cp /tmp/sing-box-*/sing-box /usr/local/bin/sing-box && \
    chmod +x /usr/local/bin/sing-box && \
    rm -rf /tmp/sing-box-*
COPY config.json /etc/sing-box/config.json
COPY cert.pem /etc/sing-box/cert.pem 2>/dev/null || true
COPY key.pem /etc/sing-box/key.pem 2>/dev/null || true
EXPOSE 443 80
ENTRYPOINT [""sing-box"", ""run"", ""-c"", ""/etc/sing-box/config.json""]";

            Progress.Desc = "生成 Docker 构建文件...";
            RunCmd("mkdir -p /etc/sing-box/docker");
            WriteToFile(dockerfile, "/etc/sing-box/docker/Dockerfile");
        }

        private void InstallSubscribeServer()
        {
            Progress.Desc = "正在安装订阅服务...";
            var subscribeScript = @"#!/usr/bin/env python3
import json, os, http.server
CONFIG_PATH = '/etc/sing-box/config.json'
PORT = int(os.environ.get('SUBSCRIBE_PORT', '8080'))

def get_share_links():
    try:
        with open(CONFIG_PATH) as f:
            cfg = json.load(f)
    except: return '[]'
    inbounds = cfg.get('inbounds', [])
    links = []
    for inbound in inbounds:
        tag = inbound.get('tag', 'unknown')
        port = inbound.get('listen_port', 443)
        users = inbound.get('users', [])
        for user in users:
            uuid = user.get('uuid', '')
            links.append(f'vless://{uuid}@{inbound.get(''server_name'', ''localhost'')}:{port}?encryption=none&type=tcp#{tag}')
    return json.dumps(links, indent=2)

class Handler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == '/sub':
            self.send_response(200)
            self.send_header('Content-Type', 'text/plain; charset=utf-8')
            self.end_headers()
            self.wfile.write(get_share_links().encode())
        else:
            self.send_response(200)
            self.send_header('Content-Type', 'text/html')
            self.end_headers()
            self.wfile.write(b'<h1>ProxySU Subscribe Server</h1><p>GET /sub for subscription</p>')

http.server.HTTPServer(('0.0.0.0', PORT), Handler).serve_forever()";

            WriteToFile(subscribeScript, "/etc/sing-box/subscribe-server.py");
            RunCmd($"chmod +x /etc/sing-box/subscribe-server.py");

            var subService = $@"[Unit]
Description=ProxySU Subscribe Server
After=network.target

[Service]
Type=simple
ExecStart=/usr/bin/python3 /etc/sing-box/subscribe-server.py
Environment=SUBSCRIBE_PORT={Settings.SubscribePort}
Restart=on-failure
RestartSec=5s

[Install]
WantedBy=multi-user.target";
            WriteToFile(subService, "/etc/systemd/system/sing-box-sub.service");
            RunCmd("systemctl daemon-reload");
            RunCmd("systemctl enable sing-box-sub");
            RunCmd("systemctl restart sing-box-sub");
        }

        private void SetupDockerAutoUpdate()
        {
            var updateScript = @"#!/bin/bash
docker pull sagernet/sing-box:latest 2>/dev/null || true
docker stop sing-box 2>/dev/null || true
docker rm sing-box 2>/dev/null || true
docker run -d --name sing-box --restart always --network host \
  -v /etc/sing-box/config.json:/etc/sing-box/config.json:ro \
  -v /etc/sing-box/cert.pem:/etc/sing-box/cert.pem:ro \
  -v /etc/sing-box/key.pem:/etc/sing-box/key.pem:ro \
  sagernet/sing-box:latest run -c /etc/sing-box/config.json";
            WriteToFile(updateScript, "/etc/sing-box/update-docker.sh");
            RunCmd("chmod +x /etc/sing-box/update-docker.sh");
            RunCmd("(crontab -l 2>/dev/null | grep -v update-docker; echo '0 4 * * 1 /etc/sing-box/update-docker.sh >/dev/null 2>&1') | crontab -");
        }

        private void UploadConfig()
        {
            Progress.Desc = "正在生成配置文件...";
            var config = SingboxConfigBuilder.BuildSingboxConfig(Settings);

            // Create config directory
            RunCmd("mkdir -p /etc/sing-box");

            // Write config file
            Progress.Desc = "正在上传配置文件...";
            WriteToFile(config, "/etc/sing-box/config.json");
        }

        private void InstallSingBoxTLS()
        {
            if (!Settings.WithTLS || Settings.IsIPAddress) return;

            Progress.Desc = "正在安装 acme.sh 并申请证书...";
            InstallCert("/etc/sing-box", "cert.pem", "key.pem");
        }

        private void StartSingBox()
        {
            Progress.Desc = "正在启动 Sing-Box...";

            // Open firewall ports before starting the service
            foreach (var port in Settings.FreePorts)
            {
                RunCmd($"firewall-cmd --permanent --add-port={port}/tcp");
                RunCmd($"firewall-cmd --permanent --add-port={port}/udp");
            }
            if (Settings.EnableSubscribe)
            {
                RunCmd($"firewall-cmd --permanent --add-port={Settings.SubscribePort}/tcp");
            }
            RunCmd("firewall-cmd --reload");

            // Fallback to self-signed cert if acme.sh didn't produce a cert
            if (Settings.WithTLS)
            {
                var hasCert = RunCmd("if [ -f /etc/sing-box/cert.pem ]; then echo '1'; else echo '0'; fi").Trim();
                if (hasCert != "1")
                {
                    Progress.Desc = "正在生成自签名 TLS 证书（兜底）...";
                    RunCmd("mkdir -p /etc/sing-box");
                    RunCmd($"openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/sing-box/key.pem -out /etc/sing-box/cert.pem -subj \"/CN={Settings.Domain}\"");
                }
            }

            if (Settings.UseDocker)
            {
                RunCmd("docker build -t sing-box-custom /etc/sing-box/docker");
                RunCmd("docker stop sing-box 2>/dev/null || true");
                RunCmd("docker rm sing-box 2>/dev/null || true");
                RunCmd("docker run -d --name sing-box --restart always --network host " +
                    "-v /etc/sing-box/config.json:/etc/sing-box/config.json:ro " +
                    "-v /etc/sing-box/cert.pem:/etc/sing-box/cert.pem:ro " +
                    "-v /etc/sing-box/key.pem:/etc/sing-box/key.pem:ro " +
                    "sing-box-custom");
            }
            else
            {
                // Create systemd service
                var serviceContent = @" [Unit]
Description=sing-box service
Documentation=https://sing-box.sagernet.org
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/sing-box run -c /etc/sing-box/config.json
Restart=on-failure
RestartSec=5s
LimitNOFILE=1048576

[Install]
WantedBy=multi-user.target";

                WriteToFile(serviceContent, "/etc/systemd/system/sing-box.service");
                RunCmd("systemctl daemon-reload");
                RunCmd("systemctl enable sing-box");
                RunCmd("systemctl restart sing-box");
            }
        }

        public void Uninstall()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "卸载 Sing-Box";
                    Progress.Percentage = 0;

                    Progress.Desc = "停止 Sing-Box 服务";
                    if (Settings.UseDocker)
                    {
                        RunCmd("docker stop sing-box 2>/dev/null || true");
                        RunCmd("docker rm sing-box 2>/dev/null || true");
                    }
                    else
                    {
                        RunCmd("systemctl stop sing-box");
                        RunCmd("systemctl disable sing-box");
                    }
                    if (Settings.EnableSubscribe)
                    {
                        RunCmd("systemctl stop sing-box-sub 2>/dev/null || true");
                        RunCmd("systemctl disable sing-box-sub 2>/dev/null || true");
                    }
                    Progress.Percentage = 30;

                    Progress.Desc = "删除 Sing-Box 相关文件";
                    if (!Settings.UseDocker)
                    {
                        RunCmd("rm -rf /etc/systemd/system/sing-box.service");
                        RunCmd("rm -rf /usr/local/bin/sing-box");
                    }
                    else
                    {
                        RunCmd("docker rmi sing-box-custom 2>/dev/null || true");
                    }
                    RunCmd("rm -rf /etc/sing-box");
                    Progress.Percentage = 80;

                    Progress.Desc = "重置防火墙设置";
                    ResetFirewalld();

                    Progress.Percentage = 100;
                    Progress.Desc = "卸载完成";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
    }
}
