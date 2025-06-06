using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Templates;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProxySuper.Core.Services
{
    public class XrayConfigBuilder
    {
        private static string ServerLog = XrayConfigTemplates.ServerGeneralConfig_log;
        private static string ServerDns = XrayConfigTemplates.ServerGeneralConfig_dns;
        private static string ServerRouting = XrayConfigTemplates.ServerGeneralConfig_routing_BlockPrivateIP;
        private static string ServerInbounds = XrayConfigTemplates.ServerGeneralConfig_inbounds;
        private static string ServerOutbounds = XrayConfigTemplates.ServerGeneralConfig_outbounds;
        private static string CaddyFile = CaddyFiles.BaseCaddyFile;

        public static int VLESS_RAW_Port = 1110;
        public static int VLESS_WS_Port = 1111;
        public static int VLESS_H2_Port = 1112;
        public static int VLESS_XHTTP_Port = 1113;

        public static int VMESS_TCP_Port = 1210;
        public static int VMESS_WS_Port = 1211;
        public static int VMESS_H2_Port = 1212;

        public static int Trojan_TCP_Port = 1310;
        public static int Trojan_WS_Port = 1311;

        public static int FullbackPort = 8080;



        public static dynamic LoadXrayConfig()
        {
            dynamic logObj = LoadJsonObj(ServerLog);
            dynamic dnsObj = LoadJsonObj(ServerDns);
            dynamic routingObj = LoadJsonObj(ServerRouting);
            dynamic inboundsObj = LoadJsonObj(ServerInbounds);
            dynamic outboundsObj = LoadJsonObj(ServerOutbounds);

            return new
            {
                log = logObj["log"],
                dns = dnsObj["dns"],
                routing = routingObj["routing"],
                inbounds = inboundsObj["inbounds"],
                outbounds = outboundsObj["outbounds"]
            };
        }

        public static string BuildCaddyConfig(XraySettings parameters, bool useCustomWeb = false)
        {
            var caddyStr = CaddyFile;
            caddyStr = caddyStr.Replace("##domain##", parameters.IsIPAddress ? "" : parameters.Domain);
            caddyStr = caddyStr.Replace("##port##", FullbackPort.ToString());

            if (!useCustomWeb && !string.IsNullOrEmpty(parameters.MaskDomain))
            {
                var prefix = "http://";
                if (parameters.MaskDomain.StartsWith("https://"))
                {
                    prefix = "https://";
                }
                var domain = parameters.MaskDomain
                    .TrimStart("http://".ToCharArray())
                    .TrimStart("https://".ToCharArray())
                    .TrimEnd('/');

                caddyStr = caddyStr.Replace("##reverse_proxy##", $"reverse_proxy {prefix}{domain} {{ \n        header_up Host {domain} \n    }}");
            }
            else
            {
                caddyStr = caddyStr.Replace("##reverse_proxy##", "");
            }

            return caddyStr;
        }

        private static void SetClients(dynamic bound, List<string> uuidList, bool withXtls = false, string flow = "")
        {
            bound.settings.clients.Clear();
            uuidList.ForEach(id =>
            {
                object obj;
                if (!withXtls)
                {
                    obj = new { id = id };
                }
                else
                {
                    flow = "xtls-rprx-vision";//Xray不再同时兼容普通tls与xtls。服务端与客户端必须一致。
                    obj = new { id = id, flow = flow };
                }

                bound.settings.clients.Add(JToken.FromObject(obj));
            });
        }

        public static string BuildXrayConfig(XraySettings parameters)
        {
            var uuidList = new List<string>();
            uuidList.Add(parameters.UUID);
            uuidList.AddRange(parameters.MulitUUID);

            var xrayConfig = LoadXrayConfig();

            #region VLESS_XTLS_REALITY

            if (parameters.Types.Contains(XrayType.VLESS_XTLS_RAW_REALITY))
            {
                var xtlsRealityBound = LoadJsonObj(XrayConfigTemplates.VLESS_XTLS_RAW_REALITY_ServerConfig);
                xtlsRealityBound.inbounds[0].port = parameters.Port;
                xtlsRealityBound.inbounds[0].settings.port = 4431;//parameters.Port;
                xtlsRealityBound.inbounds[1].port = 4431;//parameters.Port;
                xtlsRealityBound.inbounds[1].settings.clients[0].id = parameters.UUID;
                xtlsRealityBound.inbounds[1].streamSettings.realitySettings.target = parameters.MaskDomain + ":443";
                xtlsRealityBound.inbounds[1].streamSettings.realitySettings.serverNames[0] = parameters.MaskDomain;
                xtlsRealityBound.inbounds[1].streamSettings.realitySettings.privateKey = parameters.REALITY_privateKey;
                xtlsRealityBound.routing.rules[0].domain[0] = parameters.MaskDomain;
                xtlsRealityBound.Merge(JToken.Parse(XrayConfigTemplates.ServerGeneralConfig_log));
                xtlsRealityBound.Merge(JToken.Parse(XrayConfigTemplates.ServerGeneralConfig_outbounds));
                SetClients(xtlsRealityBound.inbounds[1], uuidList, withXtls: true, flow: parameters.Flow);
                xrayConfig = xtlsRealityBound;
            }

            #endregion

            #region VLESS_XHTTP

            if (parameters.Types.Contains(XrayType.VLESS_XHTTP))
            {
                var xhttpInbound = LoadJsonObj(XrayConfigTemplates.VLESS_XHTTP_ServerConfig);
                xhttpInbound.port = VLESS_XHTTP_Port;
                xhttpInbound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = FullbackPort
                }));
                SetClients(xhttpInbound, uuidList);
                xhttpInbound.streamSettings.xhttpSettings.path = parameters.VLESS_XHTTP_Path;
                xrayConfig.inbounds.Add(JToken.FromObject(xhttpInbound));
            }

            #endregion

            #region VLESS_XTLS
            if (parameters.Types.Contains(XrayType.VLESS_RAW_XTLS) || parameters.Types.Contains(XrayType.VLESS_WS) || parameters.Types.Contains(XrayType.Trojan_TCP))
            {
                var xtlsBound = LoadJsonObj(XrayConfigTemplates.VLESS_XTLS_ServerConfig);
                xtlsBound.port = parameters.Port;
                xtlsBound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = FullbackPort
                }));
                SetClients(xtlsBound, uuidList, withXtls: true, flow: parameters.Flow);
                xrayConfig.inbounds.Add(xtlsBound);
            }
            #endregion

            #region VLESS_WS

            if (parameters.Types.Contains(XrayType.VLESS_WS))
            {
                var wsInbound = LoadJsonObj(XrayConfigTemplates.VLESS_WS_ServerConfig);
                wsInbound.port = VLESS_WS_Port;
                SetClients(wsInbound, uuidList);
                wsInbound.streamSettings.wsSettings.path = parameters.VLESS_WS_Path;
                xrayConfig.inbounds[0].settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = VLESS_WS_Port,
                    path = parameters.VLESS_WS_Path,
                    xver = 1,
                }));
                xrayConfig.inbounds.Add(JToken.FromObject(wsInbound));
            }

            #endregion

            #region Trojan_TCP

            if (parameters.Types.Contains(XrayType.Trojan_TCP))
            {
                var trojanTcpBound = LoadJsonObj(XrayConfigTemplates.Trojan_ServerConfig);
                trojanTcpBound.port = Trojan_TCP_Port;
                trojanTcpBound.settings.clients[0].password = parameters.TrojanPassword;
                trojanTcpBound.settings.fallbacks[0].dest = FullbackPort;
                xrayConfig.inbounds[0].settings.fallbacks[0] = JToken.FromObject(new
                {
                    dest = Trojan_TCP_Port,
                    xver = 1,
                });
                xrayConfig.inbounds.Add(JToken.FromObject(trojanTcpBound));
            }
            #endregion

            #region VLESS GRPC
            if (parameters.Types.Contains(XrayType.VLESS_gRPC))
            {
                var gRPCInBound = LoadJsonObj(XrayConfigTemplates.VLESS_gRPC_ServerConfig);
                gRPCInBound.port = parameters.VLESS_gRPC_Port;
                SetClients(gRPCInBound, uuidList);
                gRPCInBound.streamSettings.grpcSettings.serviceName = parameters.VLESS_gRPC_ServiceName;
                gRPCInBound.streamSettings.tlsSettings.serverName = parameters.Domain;
                xrayConfig.inbounds.Add(JToken.FromObject(gRPCInBound));
            }
            #endregion

            #region VMESS KCP
            if (parameters.Types.Contains(XrayType.VMESS_KCP))
            {
                var kcpBound = LoadJsonObj(XrayConfigTemplates.VMESS_KCP_ServerConfig);
                kcpBound.port = parameters.VMESS_KCP_Port;
                SetClients(kcpBound, uuidList);
                kcpBound.streamSettings.kcpSettings.header.type = parameters.VMESS_KCP_Type;
                kcpBound.streamSettings.kcpSettings.seed = parameters.VMESS_KCP_Seed;
                xrayConfig.inbounds.Add(JToken.FromObject(kcpBound));
            }
            #endregion

            #region Shadowsocks
            if (parameters.Types.Contains(XrayType.ShadowsocksAEAD))
            {
                var ssBound = LoadJsonObj(XrayConfigTemplates.Shadowsocks_ServerConfig);
                ssBound.port = parameters.ShadowSocksPort;
                ssBound.settings.password = parameters.ShadowSocksPassword;
                ssBound.settings.method = parameters.ShadowSocksMethod;
                xrayConfig.inbounds.Add(JToken.FromObject(ssBound));
            }
            #endregion

            return JsonConvert.SerializeObject(
                xrayConfig,
                Formatting.Indented,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }


        private static dynamic LoadJsonObj(string jsonStr)
        {
                return JToken.Parse(jsonStr);
        }

    }

}
