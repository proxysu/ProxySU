using Newtonsoft.Json;
using ProxySuper.Core.Models.Projects;
using System;
using System.Text;
using System.Web;

namespace ProxySuper.Core.Services
{
    public class ShareLink
    {
        public static string BuildBrook(BrookSettings settings)
        {
            var password = HttpUtility.UrlEncode(settings.Password);

            if (settings.BrookType == BrookType.server)
            {
                var address = HttpUtility.UrlEncode($"{settings.IP}:{settings.Port}");
                return $"brook://server?password={password}&server={address}";
            }

            if (settings.BrookType == BrookType.wsserver)
            {
                var address = HttpUtility.UrlEncode($"ws://{settings.IP}:{settings.Port}");
                return $"brook://wsserver?password={password}&wsserver={address}";
            }

            if (settings.BrookType == BrookType.wssserver)
            {
                var address = HttpUtility.UrlEncode($"wss://{settings.Domain}:{settings.Port}");
                return $"brook://wssserver?password={password}&wssserver={address}";
            }

            if (settings.BrookType == BrookType.socks5)
            {
                var address = HttpUtility.UrlEncode($"socks5://{settings.IP}:{settings.Port}");
                return $"brook://socks5?password={password}&socks5={address}";
            }

            return string.Empty;
        }

        public static string BuildNaiveProxy(NaiveProxySettings settings)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("naive+https://");
            strBuilder.Append($"{settings.UserName}:{settings.Password}");
            strBuilder.Append($"@{settings.Domain}:{settings.Port}");
            strBuilder.Append("?padding=true#naive_proxy");

            return strBuilder.ToString();
        }

        public static string BuildTrojanGo(TrojanGoSettings settings)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("trojan-go://");

            strBuilder.Append($"{HttpUtility.UrlEncode(settings.Password)}@{settings.Domain}:{settings.Port}/?");
            if (settings.EnableWebSocket)
            {
                strBuilder.Append($"type=ws&path={HttpUtility.UrlEncode(settings.WebSocketPath)}&");
            }
            else
            {
                strBuilder.Append("type=original&");
            }
            strBuilder.Append($"#{HttpUtility.UrlEncode("trojan-go")}");

            return strBuilder.ToString();
        }

        public static string Build(RayType xrayType, V2raySettings settings)
        {

            switch (xrayType)
            {
                case RayType.VLESS_TCP:
                case RayType.VLESS_TCP_XTLS:
                case RayType.VLESS_WS:
                case RayType.VLESS_KCP:
                case RayType.VLESS_QUIC:
                case RayType.VLESS_gRPC:
                case RayType.Trojan_TCP:
                    return BuildVlessShareLink(xrayType, settings);
                case RayType.VMESS_TCP:
                case RayType.VMESS_WS:
                case RayType.VMESS_KCP:
                case RayType.VMESS_QUIC:
                    return BuildVmessShareLink(xrayType, settings);
                case RayType.ShadowsocksAEAD:
                    return BuildShadowSocksShareLink(settings);
                default:
                    return string.Empty;
            }
        }

        private static string BuildShadowSocksShareLink(V2raySettings settings)
        {
            var _method = settings.ShadowSocksMethod;
            var _password = settings.ShadowSocksPassword;
            var _server = settings.Domain;
            var _port = settings.ShadowSocksPort;

            var base64URL = Utils.Base64Encode($"{_method}:{_password}@{_server}:{_port}");
            return "ss://" + base64URL + "#ShadowSocks";
        }

        private static string BuildVmessShareLink(RayType xrayType, V2raySettings settings)
        {
            var vmess = new Vmess
            {
                v = "2",
                add = settings.Domain,
                port = settings.Port.ToString(),
                id = settings.UUID,
                aid = "0",
                net = "",
                type = "none",
                host = "",
                path = "",
                tls = "tls",
                ps = "",
            };

            switch (xrayType)
            {
                case RayType.VMESS_TCP:
                    vmess.ps = "vmess-tcp-tls";
                    vmess.net = "tcp";
                    vmess.type = "http";
                    vmess.path = settings.VMESS_TCP_Path;
                    break;
                case RayType.VMESS_WS:
                    vmess.ps = "vmess-ws-tls";
                    vmess.net = "ws";
                    vmess.type = "none";
                    vmess.path = settings.VMESS_WS_Path;
                    break;
                case RayType.VMESS_KCP:
                    vmess.ps = "vmess-mKCP";
                    vmess.port = settings.VMESS_KCP_Port.ToString();
                    vmess.net = "kcp";
                    vmess.type = settings.VMESS_KCP_Type;
                    vmess.path = settings.VMESS_KCP_Seed;
                    vmess.tls = "";
                    break;
                case RayType.VMESS_QUIC:
                    vmess.ps = "vmess-quic";
                    vmess.port = settings.VMESS_QUIC_Port.ToString();
                    vmess.net = "quic";
                    vmess.type = settings.VMESS_QUIC_Type;
                    vmess.path = settings.VMESS_QUIC_Key;
                    vmess.host = settings.VMESS_QUIC_Security;
                    vmess.tls = "tls";
                    break;
                default:
                    return string.Empty;
            }

            var base64Url = Utils.Base64Encode(JsonConvert.SerializeObject(vmess));
            return $"vmess://" + base64Url;
        }

        private static string BuildVlessShareLink(RayType xrayType, V2raySettings settings)
        {
            var _protocol = string.Empty;
            var _uuid = settings.UUID;
            var _domain = settings.Domain;
            var _port = settings.Port;
            var _type = string.Empty;
            var _encryption = "none";
            var _security = "tls";
            var _path = "/";
            var _host = settings.Domain;
            var _descriptiveText = string.Empty;

            switch (xrayType)
            {
                case RayType.VLESS_TCP:
                    _protocol = "vless";
                    _type = "tcp";
                    _descriptiveText = "vless-tcp-tls";
                    break;
                case RayType.VLESS_TCP_XTLS:
                    _protocol = "vless";
                    _type = "tcp";
                    _security = "tls";
                    _descriptiveText = "vless-tcp-xtls";
                    break;
                case RayType.VLESS_WS:
                    _protocol = "vless";
                    _type = "ws";
                    _path = settings.VLESS_WS_Path;
                    _descriptiveText = "vless-ws-tls";
                    break;
                case RayType.VLESS_KCP:
                    _protocol = "vless";
                    _type = "kcp";
                    _port = settings.VLESS_KCP_Port;
                    _security = "none";
                    _descriptiveText = "vless-mKCP";
                    break;
                case RayType.VLESS_QUIC:
                    _protocol = "vless";
                    _port = settings.VLESS_QUIC_Port;
                    _type = "quic";
                    _security = "tls";
                    _descriptiveText = "vless-quic";
                    break;
                case RayType.VLESS_gRPC:
                    _protocol = "vless";
                    _type = "grpc";
                    _port = settings.VLESS_gRPC_Port;
                    _descriptiveText = "vless-gRPC";
                    break;
                case RayType.Trojan_TCP:
                    _protocol = "trojan";
                    _uuid = settings.TrojanPassword;
                    _descriptiveText = "trojan-tcp";
                    break;
                default:
                    throw new Exception("暂未实现的协议");
            }


            string parametersURL = string.Empty;
            if (xrayType != RayType.Trojan_TCP)
            {
                // 4.3 传输层相关段
                parametersURL = $"?type={_type}&encryption={_encryption}&security={_security}&path={HttpUtility.UrlEncode(_path)}";

                // kcp
                if (xrayType == RayType.VLESS_KCP)
                {
                    parametersURL += $"&seed={settings.VLESS_KCP_Seed}&headerType={settings.VLESS_KCP_Type}";
                }

                if (xrayType == RayType.VLESS_QUIC)
                {
                    parametersURL += $"&quicSecurity={settings.VLESS_QUIC_Security}";
                    if (settings.VLESS_QUIC_Security != "none")
                    {
                        parametersURL += $"&key={HttpUtility.UrlEncode(settings.VLESS_QUIC_Key)}";
                    }
                    parametersURL += $"&headerType={settings.VLESS_QUIC_Type}";
                }

                // 4.4 TLS 相关段
                if (settings is XraySettings)
                {
                    if (xrayType == RayType.VLESS_TCP_XTLS)
                    {
                        var xraySettings = settings as XraySettings;
                        parametersURL += $"&flow={xraySettings.Flow}";
                    }
                }


                if (xrayType == RayType.VLESS_gRPC)
                {
                    parametersURL += $"&serviceName={settings.VLESS_gRPC_ServiceName}&mode=gun";
                }
            }


            return $"{_protocol}://{HttpUtility.UrlEncode(_uuid)}@{_domain}:{_port}{parametersURL}#{HttpUtility.UrlEncode(_descriptiveText)}";
        }

    }



    class Vmess
    {
        public string v { get; set; }
        public string ps { get; set; }
        public string add { get; set; }
        public string port { get; set; }
        public string id { get; set; }
        public string aid { get; set; }
        public string net { get; set; }
        public string type { get; set; }
        public string host { get; set; }
        public string path { get; set; }
        public string tls { get; set; }
    }
}
