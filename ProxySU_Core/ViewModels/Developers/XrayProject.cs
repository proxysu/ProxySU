using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProxySU_Core.ViewModels.Developers
{
    public class XrayProject : Project<XrayParameters>
    {

        private const string ServerLogPath = "/Templates/xray/server/00_log/00_log.json";
        private const string ServerApiPath = "/Templates/xray/server/01_api/01_api.json";
        private const string ServerDnsPath = "/Templates/xray/server/02_dns/02_dns.json";
        private const string ServerRoutingPath = "/Templates/xray/server/03_routing/03_routing.json";
        private const string ServerPolicyPath = "/Templates/xray/server/04_policy/04_policy.json";
        private const string ServerInboundsPath = "/Templates/xray/server/05_inbounds/05_inbounds.json";
        private const string ServerOutboundsPath = "/Templates/xray/server/06_outbounds/06_outbounds.json";
        private const string ServerTransportPath = "/Templates/xray/server/07_transport/07_transport.json";
        private const string ServerStatsPath = "/Templates/xray/server/08_stats/08_stats.json";
        private const string ServerReversePath = "/Templates/xray/server/09_reverse/09_reverse.json";

        public XrayProject(SshClient sshClient, XrayParameters parameters, Action<string> writeOutput) : base(sshClient, parameters, writeOutput)
        {
        }

        public override void Execute()
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

                EnsureSystemEnv();

                ConfigureSoftware();

                ConfigureIPv6();

                ConfigureFirewall();

                SyncTimeDiff();

                ValidateDomain();

                InstallXray();
            }
            catch (Exception ex)
            {
                MessageBox.Show("安装终止，" + ex.Message);
            }
        }

        private void InstallXray()
        {
            RunCmd("bash -c \"$(curl -L https://github.com/XTLS/Xray-install/raw/main/install-release.sh)\" @ install");

            if (FileExists("/usr/local/bin/xray"))
            {
                WriteOutput("Xray安装成功");
            }

            RunCmd($"sed -i 's/User=nobody/User=root/g' /etc/systemd/system/xray.service");
            RunCmd($"sed -i 's/CapabilityBoundingSet=/#CapabilityBoundingSet=/g' /etc/systemd/system/xray.service");
            RunCmd($"sed -i 's/AmbientCapabilities=/#AmbientCapabilities=/g' /etc/systemd/system/xray.service");
            RunCmd($"systemctl daemon-reload");

            if (FileExists("/usr/local/etc/xray/config.json"))
            {
                RunCmd(@"mv /usr/local/etc/xray/config.json /usr/local/etc/xray/config.json.1");
            }

            UploadXrayConfig();
        }

        private int GetRandomPort()
        {
            var random = new Random();
            return random.Next(10001, 60000);
        }

        private void UploadXrayConfig()
        {
            dynamic logObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerLogPath));
            dynamic apiObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerApiPath));
            dynamic dnsObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerDnsPath));
            dynamic routingObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerRoutingPath));
            dynamic policyObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerPolicyPath));
            dynamic inboundsObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerInboundsPath));
            dynamic outboundsObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerOutboundsPath));
            dynamic transportObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerTransportPath));
            dynamic statsObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerStatsPath));
            dynamic reverseObj = JsonConvert.DeserializeObject(File.ReadAllText(ServerReversePath));

            switch (Parameters.Type)
            {
                case XrayType.Shadowsocks_AEAD:
                    break;
                case XrayType.Shadowsocks_TCP:
                    break;
                case XrayType.Sockets5_TLS:
                    break;
                case XrayType.Trojan_TCP_TLS:
                    break;
                case XrayType.VLESS_H2C_Caddy2:
                    inboundsObj = JsonConvert.DeserializeObject(File.ReadAllText("/Templates/xray/server/05_inbounds/VLESS_HTTP2_TLS.json"));
                    inboundsObj[0]["port"] = GetRandomPort();
                    inboundsObj[0]["settings"]["clients"][0]["id"] = Parameters.UUID;
                    inboundsObj[0]["streamSettings"]["httpSettings"]["path"] = Parameters.VlessHttpPath;
                    inboundsObj[0]["streamSettings"]["httpSettings"]["host"][0] = Parameters.Domain;
                    break;
                case XrayType.VLESS_TCP_TLS_WS:
                    inboundsObj = JsonConvert.DeserializeObject(File.ReadAllText("/Templates/xray/server/05_inbounds/VLESS_TCP_TLS_WS.json"));
                    inboundsObj[0]["port"] = GetRandomPort();
                    inboundsObj[0]["settings"]["clients"][0]["id"] = Parameters.UUID;
                    inboundsObj[0]["streamSettings"]["httpSettings"]["path"] = Parameters.VlessWsPath;
                    break;
                case XrayType.VLESS_TCP_XTLS_WHATEVER:
                    break;
                case XrayType.VLESS_mKCPSeed:
                    break;
                case XrayType.VMess_HTTP2:
                    break;
                case XrayType.VMess_TCP_TLS:
                    break;
                case XrayType.VMess_WebSocket_TLS:
                    break;
                case XrayType.VMess_mKCPSeed:
                    break;
                default:
                    break;
            }

            var serverConfig = new
            {
                log = logObj["log"],
                api = apiObj["api"],
                dns = dnsObj["dns"],
                routing = routingObj["routing"],
                policy = policyObj["policy"],
                inbounds = inboundsObj["inbounds"],
                outbounds = outboundsObj["outbounds"],
                transport = transportObj["transport"],
                stats = statsObj["stats"],
                reverse = reverseObj["reverse"]
            };

            var json = JsonConvert.SerializeObject(serverConfig);
            var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            UploadFile(jsonStream, "/usr/local/etc/xray/config.json");
            jsonStream.Dispose();
        }

        private void InstallCert()
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
            RunCmd($"/root/.acme.sh/acme.sh  --installcert  -d {Parameters.Domain}  --certpath /usr/local/etc/xray/ssl/xray_ssl.crt --keypath /usr/local/etc/xray/ssl/xray_ssl.key  --capath  /usr/local/etc/xray/ssl/xray_ssl.crt  --reloadcmd  \"systemctl restart xray\"");
            result = RunCmd(@"if [ ! -f ""/usr/local/etc/xray/ssl/xray_ssl.key"" ]; then echo ""0""; else echo ""1""; fi | head -n 1");
            if (result.Contains("1"))
            {
                WriteOutput("安装证书成功");
            }
            else
            {
                throw new Exception("安装证书失败，请联系开发者！");
            }


            RunCmd(@"chmod 644 /usr/local/etc/xray/ssl/xray_ssl.key");
        }

        private void InstallCaddy()
        {

        }

        private string GetRandomEmail()
        {
            Random r = new Random();
            var num = r.Next(200000000, 900000000);
            return $"{num}@qq.com";
        }
    }
}
