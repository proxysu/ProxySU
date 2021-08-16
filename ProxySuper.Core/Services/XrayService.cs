using ProxySuper.Core.Helpers;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Services
{
    public class XrayService : ServiceBase<XraySettings>
    {

        public XrayService(Host host, XraySettings settings) : base(host, settings)
        {
        }

        public void Install()
        {
            Task.Run(() =>
            {
                int index = 1;
                if (!IsRootUser())
                {
                    MessageBox.Show("ProxySU需要使用Root用户进行安装！");
                    return;
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
                if (Settings.IsIPAddress)
                {
                    Progress.Desc = ("检查域名是否解析正确");
                    ValidateDomain();
                }
                Progress.Percentage = 25;

                Progress.Step = $"{index}. 同步系统和本地时间";
                SyncTimeDiff();
                Progress.Percentage = 30;

                Progress.Step = $"{index++}. 安装Caddy服务器";
                InstallCaddy();
                Progress.Percentage = 50;

                Progress.Step = $"{index++}. 安装Xray-Core";
                InstallXray();
                Progress.Percentage = 80;

                Progress.Step = $"{index++}. 上传Web服务器配置";
                UploadCaddyFile();
                Progress.Percentage = 90;

                Progress.Step = $"{index++}. 启动BBR";
                EnableBBR();
                Progress.Percentage = 100;

                Progress.Desc = ("！！！安装Xray完成！！！");
            });
        }

        public void InstallXray()
        {
            Progress.Desc = ("开始安装Xray-Core");
            RunCmd("bash -c \"$(curl -L https://github.com/XTLS/Xray-install/raw/main/install-release.sh)\" @ install");

            if (!FileExists("/usr/local/bin/xray"))
            {
                Progress.Desc = ("Xray-Core安装失败，请联系开发者");
                throw new Exception("Xray-Core安装失败，请联系开发者");
            }

            Progress.Desc = ("设置Xray-core权限");
            RunCmd($"sed -i 's/User=nobody/User=root/g' /etc/systemd/system/xray.service");
            RunCmd($"sed -i 's/CapabilityBoundingSet=/#CapabilityBoundingSet=/g' /etc/systemd/system/xray.service");
            RunCmd($"sed -i 's/AmbientCapabilities=/#AmbientCapabilities=/g' /etc/systemd/system/xray.service");
            RunCmd($"systemctl daemon-reload");

            if (FileExists("/usr/local/etc/xray/config.json"))
            {
                RunCmd(@"mv /usr/local/etc/xray/config.json /usr/local/etc/xray/config.json.1");
            }
            Progress.Percentage = 60;

            if (!Settings.IsIPAddress)
            {
                Progress.Desc = ("安装TLS证书");
                InstallCert(
                    dirPath: "/usr/local/etc/xray/ssl",
                    certName: "xray_ssl.crt",
                    keyName: "xray_ssl.key");
                Progress.Percentage = 75;
            }

            Progress.Desc = ("生成Xray服务器配置文件");
            var configJson = XrayConfigBuilder.BuildXrayConfig(Settings);
            WriteToFile(configJson, "/usr/local/etc/xray/config.json");
            RunCmd("systemctl restart xray");
        }

        private void UploadCaddyFile(bool useCustomWeb = false)
        {
            var configJson = XrayConfigBuilder.BuildCaddyConfig(Settings, useCustomWeb);

            if (FileExists("/etc/caddy/Caddyfile"))
            {
                RunCmd("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.back");
            }
            WriteToFile(configJson, "/etc/caddy/Caddyfile");
            RunCmd("systemctl restart caddy");
        }
    }
}
