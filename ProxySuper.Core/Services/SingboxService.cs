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

                    if (FileExists("/usr/local/bin/sing-box"))
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

                    Progress.Step = $"{index++}. 安装 Sing-Box";
                    InstallSingBox();
                    Progress.Percentage = 70;

                    Progress.Step = $"{index++}. 上传 Sing-Box 配置文件";
                    UploadConfig();
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

            // Download sing-box from official releases
            var version = "1.11.2"; // Latest stable version as of 2026
            var downloadUrl = $"https://github.com/SagerNet/sing-box/releases/download/v{version}/sing-box-{version}-linux-{arch}.tar.gz";

            Progress.Desc = "正在下载 Sing-Box...";
            RunCmd($"cd /tmp && curl -L -o sing-box.tar.gz {downloadUrl}");
            RunCmd("cd /tmp && tar -xzf sing-box.tar.gz");
            RunCmd("cp /tmp/sing-box-*/sing-box /usr/local/bin/sing-box");
            RunCmd("chmod +x /usr/local/bin/sing-box");
            RunCmd("rm -rf /tmp/sing-box*");
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

        private void StartSingBox()
        {
            Progress.Desc = "正在启动 Sing-Box...";

            // Open firewall ports before starting the service
            foreach (var port in Settings.FreePorts)
            {
                RunCmd($"firewall-cmd --permanent --add-port={port}/tcp");
                RunCmd($"firewall-cmd --permanent --add-port={port}/udp");
            }
            RunCmd("firewall-cmd --reload");

            // Generate self-signed TLS certificate if TLS is enabled
            if (Settings.WithTLS)
            {
                Progress.Desc = "正在生成自签名 TLS 证书...";
                RunCmd("mkdir -p /etc/sing-box");
                RunCmd($"openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/sing-box/key.pem -out /etc/sing-box/cert.pem -subj \"/CN={Settings.Domain}\"");
            }

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

        public void Uninstall()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "卸载 Sing-Box";
                    Progress.Percentage = 0;

                    Progress.Desc = "停止 Sing-Box 服务";
                    RunCmd("systemctl stop sing-box");
                    RunCmd("systemctl disable sing-box");
                    Progress.Percentage = 30;

                    Progress.Desc = "删除 Sing-Box 相关文件";
                    RunCmd("rm -rf /etc/systemd/system/sing-box.service");
                    RunCmd("rm -rf /usr/local/bin/sing-box");
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
