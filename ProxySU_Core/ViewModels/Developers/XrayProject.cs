using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySU_Core.Models;

namespace ProxySU_Core.ViewModels.Developers
{
    public class XrayProject : Project<XraySettings>
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

        public void InstallCert()
        {
            EnsureRootAuth();
            EnsureSystemEnv();
            this.InstallCertToXray();
            RunCmd("systemctl restart xray");
            WriteOutput("************");
            WriteOutput("安装证书完成");
            WriteOutput("************");
        }

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
            RunCmd("systemctl restart caddy");
            WriteOutput("************\n上传网站模板完成\n************");
        }

        public void ReinstallCaddy()
        {
            EnsureRootAuth();
            EnsureSystemEnv();
            InstallCaddy();
            UploadCaddyFile();
            WriteOutput("************");
            WriteOutput("重装Caddy完成");
            WriteOutput("************");
        }


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

                WriteOutput("同步系统和本地世间...");
                SyncTimeDiff();
                WriteOutput("时间同步完成");

                WriteOutput("检测域名是否绑定本机IP...");
                ValidateDomain();
                WriteOutput("域名检测完成");

                WriteOutput("安装Xray-Core...");
                InstallXrayWithCert();
                WriteOutput("Xray-Core安装完成");

                WriteOutput("安装Caddy...");
                InstallCaddy();
                WriteOutput("Caddy安装完成");

                UploadCaddyFile();
                WriteOutput("************");
                WriteOutput("安装完成，尽情享用吧......");
                WriteOutput("************");
            }
            catch (Exception ex)
            {
                MessageBox.Show("安装终止，" + ex.Message);
            }
        }

        private void UploadCaddyFile(bool useCustomWeb = false)
        {
            var configJson = ConfigBuilder.BuildCaddyConfig(Parameters, useCustomWeb);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(configJson));
            RunCmd("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.back");
            UploadFile(stream, "/etc/caddy/Caddyfile");
            RunCmd("systemctl reload caddy");
        }

        private void InstallXrayWithCert()
        {
            RunCmd("bash -c \"$(curl -L https://github.com/XTLS/Xray-install/raw/main/install-release.sh)\" @ install");

            if (!FileExists("/usr/local/bin/xray"))
            {
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


            var configJson = ConfigBuilder.BuildXrayConfig(Parameters);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(configJson));
            RunCmd("rm -rf /usr/local/etc/xray/config.json");
            UploadFile(stream, "/usr/local/etc/xray/config.json");
            RunCmd("systemctl restart xray");
        }

        private void InstallCertToXray()
        {
            // 安装依赖
            RunCmd(GetInstallCmd("socat"));

            // 解决搬瓦工CentOS缺少问题
            RunCmd(GetInstallCmd("automake autoconf libtool"));

            // 安装Acme
            var result = RunCmd($"curl https://raw.githubusercontent.com/acmesh-official/acme.sh/master/acme.sh | sh -s -- --install-online -m  {GetRandomEmail()}");
            if (result.Contains("Install success"))
            {
                WriteOutput("安装 acme.sh 成功");
            }
            else
            {
                throw new Exception("安装 acme.sh 失败，请联系开发者！");
            }

            RunCmd("cd ~/.acme.sh/");
            RunCmd("alias acme.sh=~/.acme.sh/acme.sh");

            // 申请证书 
            if (OnlyIpv6)
            {
                var cmd = $"/root/.acme.sh/acme.sh --force --debug --issue  --standalone  -d {Parameters.Domain} --listen-v6";
                result = RunCmd(cmd);
            }
            else
            {
                var cmd = $"/root/.acme.sh/acme.sh --force --debug --issue  --standalone  -d {Parameters.Domain}";
                result = RunCmd(cmd);
            }

            if (result.Contains("Cert success"))
            {
                WriteOutput("申请证书成功");
            }
            else
            {
                throw new Exception("申请证书失败，请联系开发者！");
            }

            // 安装证书到xray
            RunCmd("mkdir -p /usr/local/etc/xray/ssl");
            RunCmd($"/root/.acme.sh/acme.sh  --installcert  -d {Parameters.Domain}  --certpath /usr/local/etc/xray/ssl/xray_ssl.crt --keypath /usr/local/etc/xray/ssl/xray_ssl.key  --capath  /usr/local/etc/xray/ssl/xray_ssl.crt");
            result = RunCmd(@"if [ ! -f ""/usr/local/etc/xray/ssl/xray_ssl.key"" ]; then echo ""0""; else echo ""1""; fi | head -n 1");
            if (result.Contains("1"))
            {
                WriteOutput("安装证书成功");
            }
            else
            {
                throw new Exception("安装证书失败，请联系开发者！");
            }

            RunCmd(@"chmod 755 /usr/local/etc/xray/ssl");
        }

        private string GetRandomEmail()
        {
            Random r = new Random();
            var num = r.Next(200000000, 900000000);
            return $"{num}@qq.com";
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
