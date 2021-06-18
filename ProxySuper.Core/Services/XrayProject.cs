using Newtonsoft.Json;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Projects;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Services
{
    public class XrayProject : ProjectBase<XraySettings>
    {

        private const string ServerLogDir = @"Templates\xray\server\00_log";
        private const string ServerApiDir = @"Templates\xray\server\01_api";
        private const string ServerDnsDir = @"Templates\xray\server\02_dns";
        private const string ServerRoutingDir = @"Templates\xray\server\03_routing";
        private const string ServerPolicyDir = @"Templates\xray\server\04_policy";
        private const string ServerInboundsDir = @"Templates\xray\server\05_inbounds";
        private const string ServerOutboundsDir = @"Templates\xray\server\06_outbounds";
        private const string ServerTransportDir = @"Templates\xray\server\07_transport";
        private const string ServerStatsDir = @"Templates\xray\server\08_stats";
        private const string ServerReverseDir = @"Templates\xray\server\09_reverse";
        private const string CaddyFileDir = @"Templates\xray\caddy";

        public XrayProject(SshClient sshClient, XraySettings parameters, Action<string> writeOutput) : base(sshClient, parameters, writeOutput)
        {
        }

        /// <summary>
        /// 安装Xray
        /// </summary>
        public override void Install()
        {
            try
            {
                EnsureRootAuth();

                if (FileExists("/usr/local/bin/xray"))
                {
                    var btnResult = MessageBox.Show("已经安装Xray，是否需要重装？", "提示", MessageBoxButton.YesNo);
                    if (btnResult == MessageBoxResult.No)
                    {
                        MessageBox.Show("安装终止", "提示");
                        return;
                    }
                }

                WriteOutput("检测安装系统环境...");
                EnsureSystemEnv();
                WriteOutput("检测安装系统环境完成");

                WriteOutput("配置服务器端口...");
                ConfigurePort();
                WriteOutput("端口配置完成");

                WriteOutput("安装必要的系统工具...");
                ConfigureSoftware();
                WriteOutput("系统工具安装完成");

                WriteOutput("检测IP6...");
                ConfigureIPv6();
                WriteOutput("检测IP6完成");

                WriteOutput("配置防火墙...");
                ConfigureFirewall();
                WriteOutput("防火墙配置完成");

                WriteOutput("同步系统和本地时间...");
                SyncTimeDiff();
                WriteOutput("时间同步完成");

                WriteOutput("检测域名是否绑定本机IP...");
                ValidateDomain();
                WriteOutput("域名检测完成");

                WriteOutput("安装Caddy...");
                InstallCaddy();
                WriteOutput("Caddy安装完成");

                WriteOutput("安装Xray-Core...");
                InstallXrayWithCert();
                WriteOutput("Xray-Core安装完成");

                WriteOutput("启动BBR");
                EnableBBR();

                UploadCaddyFile();
                WriteOutput("************");
                WriteOutput("安装完成，尽情享用吧......");
                WriteOutput("************");
            }
            catch (Exception ex)
            {
                var errorLog = "安装终止，" + ex.Message;
                WriteOutput(errorLog);
                MessageBox.Show("安装失败，请联系开发者或上传日志文件(Logs文件夹下)到github提问。");
            }
        }

        public void UninstallProxy()
        {
            EnsureRootAuth();
            EnsureSystemEnv();
            WriteOutput("卸载Caddy");
            UninstallCaddy();
            WriteOutput("卸载Xray");
            UninstallXray();
            WriteOutput("卸载证书");
            UninstallAcme();
            WriteOutput("关闭端口");
            ClosePort(Parameters.ShadowSocksPort, Parameters.VMESS_KCP_Port);

            WriteOutput("************ 卸载完成 ************");
        }

        /// <summary>
        /// 更新xray内核
        /// </summary>
        public void UpdateXrayCore()
        {
            EnsureRootAuth();
            EnsureSystemEnv();
            RunCmd("bash -c \"$(curl -L https://github.com/XTLS/Xray-install/raw/main/install-release.sh)\" @ install");
            RunCmd("systemctl restart xray");
            WriteOutput("************ 更新xray内核完成 ************");
        }

        /// <summary>
        /// 更新xray配置
        /// </summary>
        public void UpdateXraySettings()
        {
            EnsureRootAuth();
            EnsureSystemEnv();
            ConfigureFirewall();
            var configJson = XrayConfigBuilder.BuildXrayConfig(Parameters);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(configJson));
            RunCmd("rm -rf /usr/local/etc/xray/config.json");
            UploadFile(stream, "/usr/local/etc/xray/config.json");
            ConfigurePort();
            UploadCaddyFile(string.IsNullOrEmpty(Parameters.MaskDomain));
            RunCmd("systemctl restart xray");
            WriteOutput("************ 更新Xray配置成功，更新配置不包含域名，如果域名更换请重新安装。 ************");
        }

        /// <summary>
        /// 重装Caddy
        /// </summary>
        public void DoUninstallCaddy()
        {
            EnsureRootAuth();
            EnsureSystemEnv();
            UninstallCaddy();
            WriteOutput("************ 卸载Caddy完成 ************");
        }

        /// <summary>
        /// 安装证书
        /// </summary>
        public void InstallCertToXray()
        {
            EnsureRootAuth();
            EnsureSystemEnv();
            InstallCert(
                dirPath: "/usr/local/etc/xray/ssl",
                certName: "xray_ssl.crt",
                keyName: "xray_ssl.key");

            RunCmd("systemctl restart xray");
            WriteOutput("************ 安装证书完成 ************");
        }

        /// <summary>
        /// 上传证书
        /// </summary>
        /// <param name="keyStrem"></param>
        /// <param name="crtStream"></param>
        public void UploadCert(Stream stream)
        {
            EnsureRootAuth();
            EnsureSystemEnv();

            // 转移旧文件
            var oldFileName = $"ssl_{DateTime.Now.Ticks}";
            RunCmd($"mv /usr/local/etc/xray/ssl /usr/local/etc/xray/{oldFileName}");

            // 上传新文件
            RunCmd("mkdir /usr/local/etc/xray/ssl");
            UploadFile(stream, "/usr/local/etc/xray/ssl/ssl.zip");
            RunCmd("unzip /usr/local/etc/xray/ssl/ssl.zip -d /usr/local/etc/xray/ssl");

            // 改名
            var crtFiles = RunCmd("find /usr/local/etc/xray/ssl/*.crt").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (crtFiles.Length > 0)
            {
                RunCmd($"mv {crtFiles[0]} /usr/local/etc/xray/ssl/xray_ssl.crt");
            }
            else
            {
                WriteOutput("************ 上传证书失败，请联系开发者 ************");
                RunCmd("rm -rf /usr/local/etc/xray/ssl");
                RunCmd($"mv /usr/local/etc/xray/ssl{oldFileName} /usr/local/etc/xray/ssl");
                WriteOutput("操作已回滚");
                return;
            }

            var keyFiles = RunCmd("find /usr/local/etc/xray/ssl/*.key").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (keyFiles.Length > 0)
            {
                RunCmd($"mv {keyFiles[0]} /usr/local/etc/xray/ssl/xray_ssl.key");
            }
            else
            {
                WriteOutput("************ 上传证书失败，请联系开发者 ************");
                RunCmd("rm -rf /usr/local/etc/xray/ssl");
                RunCmd($"mv /usr/local/etc/xray/ssl{oldFileName} /usr/local/etc/xray/ssl");
                WriteOutput("操作已回滚");
                return;
            }

            RunCmd("systemctl restart xray");
            WriteOutput("************ 上传证书完成 ************");
        }

        /// <summary>
        /// 上传静态网站
        /// </summary>
        /// <param name="stream"></param>
        public void UploadWeb(Stream stream)
        {
            EnsureRootAuth();
            EnsureSystemEnv();
            if (!FileExists("/usr/share/caddy"))
            {
                RunCmd("mkdir /usr/share/caddy");
            }
            RunCmd("rm -rf /usr/share/caddy/*");
            UploadFile(stream, "/usr/share/caddy/caddy.zip");
            RunCmd("unzip /usr/share/caddy/caddy.zip -d /usr/share/caddy");
            RunCmd("chmod -R 777 /usr/share/caddy");
            UploadCaddyFile(useCustomWeb: true);
            WriteOutput("************ 上传网站模板完成 ************");
        }

        /// <summary>
        /// 上传Caddy文件
        /// </summary>
        /// <param name="useCustomWeb"></param>
        private void UploadCaddyFile(bool useCustomWeb = false)
        {
            var configJson = XrayConfigBuilder.BuildCaddyConfig(Parameters, useCustomWeb);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(configJson));
            if (FileExists("/etc/caddy/Caddyfile"))
            {
                RunCmd("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.back");
            }
            UploadFile(stream, "/etc/caddy/Caddyfile");
            RunCmd("systemctl restart caddy");
        }



        private void UninstallXray()
        {
            RunCmd("bash -c \"$(curl -L https://github.com/XTLS/Xray-install/raw/main/install-release.sh)\" @ remove");
        }

        private void UninstallAcme()
        {
            RunCmd("acme.sh --uninstall");
            RunCmd("rm -r  ~/.acme.sh");
        }

        private void InstallXrayWithCert()
        {
            RunCmd("bash -c \"$(curl -L https://github.com/XTLS/Xray-install/raw/main/install-release.sh)\" @ install");

            if (!FileExists("/usr/local/bin/xray"))
            {
                WriteOutput("Xray-Core安装失败，请联系开发者");
                throw new Exception("Xray-Core安装失败，请联系开发者");
            }

            RunCmd($"sed -i 's/User=nobody/User=root/g' /etc/systemd/system/xray.service");
            RunCmd($"sed -i 's/CapabilityBoundingSet=/#CapabilityBoundingSet=/g' /etc/systemd/system/xray.service");
            RunCmd($"sed -i 's/AmbientCapabilities=/#AmbientCapabilities=/g' /etc/systemd/system/xray.service");
            RunCmd($"systemctl daemon-reload");

            if (FileExists("/usr/local/etc/xray/config.json"))
            {
                RunCmd(@"mv /usr/local/etc/xray/config.json /usr/local/etc/xray/config.json.1");
            }

            WriteOutput("安装TLS证书");
            InstallCertToXray();
            WriteOutput("TLS证书安装完成");


            var configJson = XrayConfigBuilder.BuildXrayConfig(Parameters);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(configJson));
            UploadFile(stream, "/usr/local/etc/xray/config.json");
            RunCmd("systemctl restart xray");
        }

        private int GetRandomPort()
        {
            var random = new Random();
            return random.Next(10001, 60000);
        }

        private dynamic LoadJsonObj(string path)
        {
            if (File.Exists(path))
            {
                var jsonStr = File.ReadAllText(path, Encoding.UTF8);
                return JsonConvert.DeserializeObject(jsonStr);
            }
            return null;
        }

    }
}
