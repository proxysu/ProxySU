using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Services
{
    public class V2rayConfigBuilder
    {
        private static string ServerLog = V2rayConfigTemplates.ServerGeneralConfig_log;
        private static string ServerDns = V2rayConfigTemplates.ServerGeneralConfig_dns;
        private static string ServerRouting = V2rayConfigTemplates.ServerGeneralConfig_routing_BlockPrivateIP;
        private static string ServerInbounds = V2rayConfigTemplates.ServerGeneralConfig_inbounds;
        private static string ServerOutbounds = V2rayConfigTemplates.ServerGeneralConfig_outbounds;
        private static string CaddyFile = CaddyFiles.BaseCaddyFile;

        public static int VLESS_TCP_Port = 1110;
        public static int VLESS_WS_Port = 1111;
        public static int VLESS_H2_Port = 1112;

        public static int VMESS_TCP_Port = 1210;
        public static int VMESS_WS_Port = 1211;
        public static int VMESS_H2_Port = 1212;

        public static int Trojan_TCP_Port = 1310;
        public static int Trojan_WS_Port = 1311;

        public static int FullbackPort = 8080;



        public static dynamic LoadV2rayConfig()
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

        public static string BuildCaddyConfig(V2raySettings parameters, bool useCustomWeb = false)
        {
            var caddyStr = CaddyFile;// File.ReadAllText(Path.Combine(CaddyFileDir, "base.caddyfile"));
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
                    .TrimStart("https://".ToCharArray());

                caddyStr = caddyStr.Replace("##reverse_proxy##", $"reverse_proxy {prefix}{domain} {{ \n        header_up Host {domain} \n    }}");
            }
            else
            {
                caddyStr = caddyStr.Replace("##reverse_proxy##", "");
            }

            return caddyStr;
        }

        private static void SetClients(dynamic bound, List<string> uuidList)
        {
            bound.settings.clients.Clear();
            uuidList.ForEach(id =>
            {
                object obj;

                obj = new { id = id };

                bound.settings.clients.Add(JToken.FromObject(obj));
            });
        }


        public static string BuildV2rayConfig(V2raySettings parameters)
        {
            var uuidList = new List<string>();
            uuidList.Add(parameters.UUID);
            uuidList.AddRange(parameters.MulitUUID);

            var xrayConfig = LoadV2rayConfig();

            var baseBound = LoadJsonObj(V2rayConfigTemplates.VLESS_TCP_TLS_ServerConfig);//GetBound("VLESS_TCP_TLS.json");
            baseBound.port = parameters.Port;
            baseBound.settings.fallbacks.Add(JToken.FromObject(new
            {
                dest = FullbackPort
            }));
            xrayConfig.inbounds.Add(baseBound);
            SetClients(baseBound, uuidList);

            #region Fullbacks

            if (parameters.Types.Contains(V2RayType.VLESS_WS))
            {
                var wsInbound = LoadJsonObj(V2rayConfigTemplates.VLESS_WS_ServerConfig); //GetBound("VLESS_WS.json");
                wsInbound.port = VLESS_WS_Port;
                SetClients(wsInbound, uuidList);
                wsInbound.streamSettings.wsSettings.path = parameters.VLESS_WS_Path;
                baseBound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = VLESS_WS_Port,
                    path = parameters.VLESS_WS_Path,
                    xver = 1,
                }));
                xrayConfig.inbounds.Add(JToken.FromObject(wsInbound));
            }

            if (parameters.Types.Contains(V2RayType.VMESS_TCP))
            {
                var mtcpBound = LoadJsonObj(V2rayConfigTemplates.VMESS_TCP_ServerConfig); //GetBound("VMESS_TCP.json");
                mtcpBound.port = VMESS_TCP_Port;
                SetClients(mtcpBound, uuidList);
                mtcpBound.streamSettings.tcpSettings.header.request.path = parameters.VMESS_TCP_Path;
                baseBound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = VMESS_TCP_Port,
                    path = parameters.VMESS_TCP_Path,
                    xver = 1,
                }));
                xrayConfig.inbounds.Add(JToken.FromObject(mtcpBound));
            }

            if (parameters.Types.Contains(V2RayType.VMESS_WS))
            {
                var mwsBound = LoadJsonObj(V2rayConfigTemplates.VMESS_WS_ServerConfig); //GetBound("VMESS_WS.json");
                mwsBound.port = VMESS_WS_Port;
                SetClients(mwsBound, uuidList);
                mwsBound.streamSettings.wsSettings.path = parameters.VMESS_WS_Path;
                baseBound.settings.fallbacks.Add(JToken.FromObject(new
                {
                    dest = VMESS_WS_Port,
                    path = parameters.VMESS_WS_Path,
                    xver = 1,
                }));
                xrayConfig.inbounds.Add(JToken.FromObject(mwsBound));
            }

            if (parameters.Types.Contains(V2RayType.Trojan_TCP))
            {
                var trojanTcpBound = LoadJsonObj(V2rayConfigTemplates.Trojan_ServerConfig); //GetBound("Trojan_TCP.json");
                trojanTcpBound.port = Trojan_TCP_Port;
                trojanTcpBound.settings.clients[0].password = parameters.TrojanPassword;
                trojanTcpBound.settings.fallbacks[0].dest = FullbackPort;
                baseBound.settings.fallbacks[0] = JToken.FromObject(new
                {
                    dest = Trojan_TCP_Port,
                    xver = 1,
                });
                xrayConfig.inbounds.Add(JToken.FromObject(trojanTcpBound));
            }
            #endregion

            #region VLESS GRPC
            if (parameters.Types.Contains(V2RayType.VLESS_gRPC))
            {
                var gRPCInBound = LoadJsonObj(V2rayConfigTemplates.VLESS_gRPC_ServerConfig); //GetBound("VLESS_gRPC.json");
                gRPCInBound.port = parameters.VLESS_gRPC_Port;
                SetClients(gRPCInBound, uuidList);
                gRPCInBound.streamSettings.grpcSettings.serviceName = parameters.VLESS_gRPC_ServiceName;
                gRPCInBound.streamSettings.tlsSettings.serverName = parameters.Domain;
                xrayConfig.inbounds.Add(JToken.FromObject(gRPCInBound));
            }
            #endregion

            #region VLESS KCP
            if (parameters.Types.Contains(V2RayType.VLESS_KCP))
            {
                var kcpBound = LoadJsonObj(V2rayConfigTemplates.VLESS_KCP_ServerConfig); //GetBound("VLESS_KCP.json");
                kcpBound.port = parameters.VLESS_KCP_Port;
                SetClients(kcpBound, uuidList);
                kcpBound.streamSettings.kcpSettings.header.type = parameters.VLESS_KCP_Type;
                kcpBound.streamSettings.kcpSettings.seed = parameters.VLESS_KCP_Seed;
                xrayConfig.inbounds.Add(JToken.FromObject(kcpBound));
            }
            #endregion

            #region VLESS QUIC
            if (parameters.Types.Contains(V2RayType.VLESS_QUIC))
            {
                var quicBound = LoadJsonObj(V2rayConfigTemplates.VLESS_QUIC_ServerConfig); //GetBound("VLESS_QUIC.json");
                quicBound.port = parameters.VLESS_QUIC_Port;
                SetClients(quicBound, uuidList);
                quicBound.streamSettings.quicSettings.security = parameters.VLESS_QUIC_Security;
                quicBound.streamSettings.quicSettings.key = parameters.VLESS_QUIC_Key;
                quicBound.streamSettings.quicSettings.header.type = parameters.VLESS_QUIC_Type;
                xrayConfig.inbounds.Add(JToken.FromObject(quicBound));
            }
            #endregion

            #region VMESS KCP
            if (parameters.Types.Contains(V2RayType.VMESS_KCP))
            {
                var kcpBound = LoadJsonObj(V2rayConfigTemplates.VMESS_KCP_ServerConfig); //GetBound("VMESS_KCP.json");
                kcpBound.port = parameters.VMESS_KCP_Port;
                SetClients(kcpBound, uuidList);
                kcpBound.streamSettings.kcpSettings.header.type = parameters.VMESS_KCP_Type;
                kcpBound.streamSettings.kcpSettings.seed = parameters.VMESS_KCP_Seed;
                xrayConfig.inbounds.Add(JToken.FromObject(kcpBound));
            }
            #endregion

            #region VMESS QUIC
            if (parameters.Types.Contains(V2RayType.VMESS_QUIC))
            {
                var quicBound = LoadJsonObj(V2rayConfigTemplates.VMESS_QUIC_ServerConfig); //GetBound("VMESS_QUIC.json");
                quicBound.port = parameters.VMESS_QUIC_Port;
                SetClients(quicBound, uuidList);
                quicBound.streamSettings.quicSettings.security = parameters.VMESS_QUIC_Security;
                quicBound.streamSettings.quicSettings.key = parameters.VMESS_QUIC_Key;
                quicBound.streamSettings.quicSettings.header.type = parameters.VMESS_QUIC_Type;
                xrayConfig.inbounds.Add(JToken.FromObject(quicBound));
            }
            #endregion

            #region Shadowsocks
            if (parameters.Types.Contains(V2RayType.ShadowsocksAEAD))
            {
                var ssBound = LoadJsonObj(V2rayConfigTemplates.Shadowsocks_ServerConfig); //GetBound("Shadowsocks-AEAD.json");
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
