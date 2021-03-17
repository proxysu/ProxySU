﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySU_Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProxySU_Core.Models.Developers
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
        public const int VLESS_H2_Port = 1112;
        public const int VLESS_mKCP_Port = 1113;

        public const int VMESS_TCP_Port = 2110;
        public const int VMESS_WS_Port = 2111;
        public const int VMESS_H2_Port = 2112;
        public const int VMESS_mKCP_Port = 2113;

        public const int Trojan_TCP_Port = 3110;
        public const int Trojan_WS_Port = 3111;


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
                //api = apiObj["api"],  api不能为空
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

        public static string BuildCaddyConfig(XraySettings parameters, bool useCustomWeb = false)
        {
            var caddyStr = File.ReadAllText(Path.Combine(CaddyFileDir, "base.caddyfile"));
            caddyStr = caddyStr.Replace("##domain##", parameters.Domain);
            if (!useCustomWeb && !string.IsNullOrEmpty(parameters.MaskDomain))
            {
                var prefix = "http://";
                if (parameters.MaskDomain.StartsWith("https://"))
                {
                    prefix = "https://";
                }
                var domain = parameters.MaskDomain
                    .TrimStart("http://".ToCharArray())
                    .TrimStart("https://".ToCharArray());

                caddyStr = caddyStr.Replace("##reverse_proxy##", $"reverse_proxy {prefix}{domain} {{ \n        header_up Host {domain} \n    }}");
            }
            else
            {
                caddyStr = caddyStr.Replace("##reverse_proxy##", "");
            }

            return caddyStr;
        }

        public static string BuildXrayConfig(XraySettings parameters)
        {
            var xrayConfig = LoadXrayConfig();
            var baseBound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VLESS_TCP_XTLS.json"));
            baseBound.port = parameters.Port;
            baseBound.settings.fallbacks.Add(JToken.FromObject(new
            {
                dest = 80
            }));
            xrayConfig.inbounds.Add(baseBound);
            baseBound.settings.clients[0].id = parameters.UUID;

            if (parameters.Types.Contains(XrayType.VLESS_WS_TLS))
            {
                var wsInbound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VLESS_WS_TLS.json"));
                wsInbound.port = VLESS_WS_Port;
                wsInbound.settings.clients[0].id = parameters.UUID;
                wsInbound.streamSettings.wsSettings.path = parameters.VLESS_WS_Path;
                baseBound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = VLESS_WS_Port,
                    path = parameters.VLESS_WS_Path
                }));
                xrayConfig.inbounds.Add(JToken.FromObject(wsInbound));
            }

            if (parameters.Types.Contains(XrayType.VLESS_H2_TLS))
            {
                var h2Inbound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VLESS_HTTP2_TLS.json"));
                h2Inbound.port = VLESS_H2_Port;
                h2Inbound.settings.clients[0].id = parameters.UUID;
                h2Inbound.streamSettings.httpSettings.path = parameters.VLESS_H2_Path;
                baseBound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = VLESS_H2_Port,
                    path = parameters.VLESS_H2_Path
                }));
                xrayConfig.inbounds.Add(JToken.FromObject(h2Inbound));
            }

            if (parameters.Types.Contains(XrayType.VLESS_mKCP_Speed))
            {
                var kcpInbound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VLESS_mKCP"));
            }

            if (parameters.Types.Contains(XrayType.VMESS_TCP_TLS))
            {
                var mtcpBound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VMESS_TCP_TLS.json"));
                mtcpBound.port = VMESS_TCP_Port;
                mtcpBound.settings.clients[0].id = parameters.UUID;
                mtcpBound.streamSettings.tcpSettings.header.request.path = parameters.VMESS_TCP_Path;
                baseBound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = VMESS_TCP_Port,
                    path = parameters.VMESS_TCP_Path,
                    xver = 1,
                }));
                xrayConfig.inbounds.Add(JToken.FromObject(mtcpBound));
            }

            if (parameters.Types.Contains(XrayType.VMESS_WS_TLS))
            {
                var mwsBound = LoadJsonObj(Path.Combine(ServerInboundsDir, "VMESS_WS_TLS.json"));
                mwsBound.port = VMESS_WS_Port;
                mwsBound.settings.clients[0].id = parameters.UUID;
                mwsBound.streamSettings.wsSettings.path = parameters.VMESS_WS_Path;
                baseBound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = VMESS_WS_Port,
                    path = parameters.VMESS_WS_Path,
                    xver = 1,
                }));
                xrayConfig.inbounds.Add(JToken.FromObject(mwsBound));
            }

            if (parameters.Types.Contains(XrayType.VMESS_H2_TLS)) { }

            if (parameters.Types.Contains(XrayType.VMESS_mKCP_Speed)) { }

            if (parameters.Types.Contains(XrayType.Trojan_TCP_TLS))
            {
                var trojanTcpBound = LoadJsonObj(Path.Combine(ServerInboundsDir, "Trojan_TCP_TLS.json"));
                trojanTcpBound.port = Trojan_TCP_Port;
                trojanTcpBound.settings.clients[0].password = parameters.TrojanPassword;
                baseBound.settings.fallbacks[0] = JToken.FromObject(new
                {
                    dest = Trojan_TCP_Port,
                    xver = 1,
                });
                xrayConfig.inbounds.Add(JToken.FromObject(trojanTcpBound));
            }

            if (parameters.Types.Contains(XrayType.Trojan_WS_TLS)) { }

            return JsonConvert.SerializeObject(
                xrayConfig,
                Formatting.Indented,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }

        private static dynamic LoadJsonObj(string path)
        {
            if (File.Exists(path))
            {
                var jsonStr = File.ReadAllText(path, Encoding.UTF8);
                return JToken.FromObject(JsonConvert.DeserializeObject(jsonStr));
            }
            return null;
        }

    }

}
