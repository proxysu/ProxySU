using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProxySU_Core.ViewModels.Developers
{
    public class ConfigBuilder
    {
        public dynamic xrayConfig { get; set; }
        public string CaddyConfig { get; set; }

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

        public const int VLESS_TCP_Port = 1110;
        public const int VLESS_WS_Port = 1111;
        public const int VMESS_TCP_Port = 2110;
        public const int VMESS_WS_Port = 2111;
        public const int Trojan_TCP_Port = 3110;


        public static dynamic LoadXrayConfig()
        {
            dynamic logObj = LoadJsonObj(Path.Combine(ServerLogDir, "00_log.json"));
            dynamic apiObj = LoadJsonObj(Path.Combine(ServerApiDir, "01_api.json"));
            dynamic dnsObj = LoadJsonObj(Path.Combine(ServerDnsDir, "02_dns.json"));
            dynamic routingObj = LoadJsonObj(Path.Combine(ServerRoutingDir, "03_routing.json"));
            dynamic policyObj = LoadJsonObj(Path.Combine(ServerPolicyDir, "04_policy.json"));
            dynamic inboundsObj = LoadJsonObj(Path.Combine(ServerInboundsDir, "05_inbounds.json"));
            dynamic outboundsObj = LoadJsonObj(Path.Combine(ServerOutboundsDir, "06_outbounds.json"));
            dynamic transportObj = LoadJsonObj(Path.Combine(ServerTransportDir, "07_transport.json"));
            dynamic statsObj = LoadJsonObj(Path.Combine(ServerStatsDir, "08_stats.json"));
            dynamic reverseObj = LoadJsonObj(Path.Combine(ServerReverseDir, "09_reverse.json"));

            return new
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


        }

        public static string BuildCaddyConfig(XrayParameters parameters)
        {
            var caddyStr = File.ReadAllText(Path.Combine(CaddyFileDir, "base.caddyfile"));
            caddyStr.Replace("##domain##", parameters.Domain);
            if (!string.IsNullOrEmpty(parameters.MaskDomain))
            {
                caddyStr.Replace("##server_proxy##", $"reverse_proxy http://{parameters.MaskDomain} {{ header_up Host {parameters.MaskDomain} }}");
            }
            else
            {
                caddyStr.Replace("##server_proxy##", "");
            }

            return caddyStr;
        }

        public static string BuildXrayConfig(XrayParameters parameters)
        {
            var xrayConfig = LoadXrayConfig();
            var baseBound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VLESS_TCP_XTLS.json"));
            baseBound["port"] = VLESS_TCP_Port;
            baseBound["settings"]["fallbacks"].Add(new
            {
                dest = 80,
                xver = 1,
            });
            xrayConfig["inbounds"].Add(baseBound);

            switch (parameters.Type)
            {
                case XrayType.VLESS_TCP_TLS:
                case XrayType.VLESS_TCP_XTLS:
                    baseBound["settings"]["clients"][0]["id"] = parameters.UUID;
                    break;
                case XrayType.VLESS_WS_TLS:
                    var wsInbound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VLESS_WS_TLS.json"));
                    wsInbound["port"] = VLESS_WS_Port;
                    wsInbound["settings"]["clients"][0]["id"] = parameters.UUID;
                    wsInbound["streamSettings"]["wsSettings"]["path"] = parameters.VlessWsPath;
                    baseBound["settings"]["fallbacks"].Add(new
                    {
                        dest = VLESS_WS_Port,
                        path = parameters.VlessWsPath,
                        xver = 1,
                    });
                    xrayConfig["inbounds"].Add(wsInbound);
                    break;
                case XrayType.VMESS_TCP_TLS:
                    var mtcpBound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VMESS_TCP_TLS.json"));
                    mtcpBound["port"] = VMESS_TCP_Port;
                    mtcpBound["settings"]["clients"][0]["id"] = parameters.UUID;
                    mtcpBound["streamSettings"]["tcpSettings"]["header"]["request"]["path"] = parameters.VmessTcpPath;
                    baseBound["settings"]["fallbacks"].Add(new
                    {
                        dest = VMESS_TCP_Port,
                        path = parameters.VmessTcpPath,
                        xver = 1,
                    });
                    xrayConfig["inbounds"].Add(mtcpBound);
                    break;
                case XrayType.VMESS_WS_TLS:
                    var mwsBound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VMESS_WS_TLS.json"));
                    mwsBound["port"] = VMESS_WS_Port;
                    mwsBound["settings"]["clients"][0]["id"] = parameters.UUID;
                    mwsBound["streamSettings"]["wsSettings"]["path"] = parameters.VmessWsPath;
                    baseBound["settings"]["fallbacks"].Add(new
                    {
                        dest = VMESS_WS_Port,
                        path = parameters.VmessWsPath,
                        xver = 1,
                    });
                    xrayConfig["inbounds"].Add(mwsBound);
                    break;
                case XrayType.Trojan_TCP_TLS:
                    var trojanTcpBound = LoadJsonObj(Path.Combine(ServerInboundsDir, "Trojan_TCP_TLS.json"));
                    trojanTcpBound["port"] = Trojan_TCP_Port;
                    trojanTcpBound["settings"]["clients"][0]["password"] = parameters.TrojanPassword;
                    baseBound["settings"]["fallbacks"][0] = new
                    {
                        dest = Trojan_TCP_Port,
                        xver = 1,
                    };
                    xrayConfig["inbounds"].Add(trojanTcpBound);
                    break;
                default:
                    break;
            }

            return JsonConvert.SerializeObject(xrayConfig, Formatting.Indented);
        }

        private static dynamic LoadJsonObj(string path)
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
