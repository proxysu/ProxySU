
using Newtonsoft.Json;
using ProxySuper.Core.Models.Projects;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace ProxySuper.Core.Services
{
    public class ShareLink
    {
        public static string BuildHysteria(HysteriaSettings settings)
        {
            //hysteria2://[auth@]hostname[:port]/?[key=value]&[key=value]...
            //hysteria2://letmein@example.com:123,5000-6000/?insecure=1&obfs=salamander&obfs-password=gawrgura&pinSHA256=deadbeef&sni=real.example.com
            var auth = HttpUtility.UrlEncode(settings.Password);

            if (settings.EnableObfs == true)
            { 
                //var _obfsPassword = settings.ObfsPassword;
                return $"hysteria2://{auth}@{settings.Domain}:{settings.Port}/?obfs=salamander&obfs-password={settings.ObfsPassword}";
            }
            return $"hysteria2://{auth}@{settings.Domain}:{settings.Port}";

        }
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

        #region V2Ray ShareLink
        public static string Build(V2RayType v2rayType, V2raySettings settings)
        {

            switch (v2rayType)
            {
                case V2RayType.VLESS_TCP:
                //case RayType.VLESS_RAW_XTLS:
                case V2RayType.VLESS_WS:
                case V2RayType.VLESS_KCP:
                case V2RayType.VLESS_QUIC:
                case V2RayType.VLESS_gRPC:
                case V2RayType.Trojan_TCP:
                    return BuildVlessShareLink(v2rayType, settings);
                case V2RayType.VMESS_TCP:
                case V2RayType.VMESS_WS:
                case V2RayType.VMESS_KCP:
                case V2RayType.VMESS_QUIC:
                    return BuildVmessShareLink(v2rayType, settings);
                case V2RayType.ShadowsocksAEAD:
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

        private static string BuildVmessShareLink(V2RayType v2rayType, V2raySettings settings)
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

            switch (v2rayType)
            {
                case V2RayType.VMESS_TCP:
                    vmess.ps = "vmess-tcp-tls";
                    vmess.net = "tcp";
                    vmess.type = "http";
                    vmess.path = settings.VMESS_TCP_Path;
                    break;
                case V2RayType.VMESS_WS:
                    vmess.ps = "vmess-ws-tls";
                    vmess.net = "ws";
                    vmess.type = "none";
                    vmess.path = settings.VMESS_WS_Path;
                    break;
                case V2RayType.VMESS_KCP:
                    vmess.ps = "vmess-mKCP";
                    vmess.port = settings.VMESS_KCP_Port.ToString();
                    vmess.net = "kcp";
                    vmess.type = settings.VMESS_KCP_Type;
                    vmess.path = settings.VMESS_KCP_Seed;
                    vmess.tls = "";
                    break;
                case V2RayType.VMESS_QUIC:
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

        private static string BuildVlessShareLink(V2RayType v2rayType, V2raySettings settings)
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

            switch (v2rayType)
            {
                case V2RayType.VLESS_TCP:
                    _protocol = "vless";
                    _type = "tcp";
                    _descriptiveText = "vless-tcp-tls";
                    break;
                //case RayType.VLESS_RAW_XTLS:
                //    _protocol = "vless";
                //    _type = "tcp";
                //    _security = "tls";
                //    _descriptiveText = "vless-tcp-xtls";
                //    break;
                case V2RayType.VLESS_WS:
                    _protocol = "vless";
                    _type = "ws";
                    _path = settings.VLESS_WS_Path;
                    _descriptiveText = "vless-ws-tls";
                    break;
                case V2RayType.VLESS_KCP:
                    _protocol = "vless";
                    _type = "kcp";
                    _port = settings.VLESS_KCP_Port;
                    _security = "none";
                    _descriptiveText = "vless-mKCP";
                    break;
                case V2RayType.VLESS_QUIC:
                    _protocol = "vless";
                    _port = settings.VLESS_QUIC_Port;
                    _type = "quic";
                    _security = "tls";
                    _descriptiveText = "vless-quic";
                    break;
                case V2RayType.VLESS_gRPC:
                    _protocol = "vless";
                    _type = "grpc";
                    _port = settings.VLESS_gRPC_Port;
                    _descriptiveText = "vless-gRPC";
                    break;
                case V2RayType.Trojan_TCP:
                    _protocol = "trojan";
                    _uuid = settings.TrojanPassword;
                    _descriptiveText = "trojan-tcp";
                    break;
                default:
                    throw new Exception("暂未实现的协议");
            }


            string parametersURL = string.Empty;
            if (v2rayType != V2RayType.Trojan_TCP)
            {
                // 4.3 传输层相关段
                parametersURL = $"?type={_type}&encryption={_encryption}&security={_security}&path={HttpUtility.UrlEncode(_path)}";

                // kcp
                if (v2rayType == V2RayType.VLESS_KCP)
                {
                    parametersURL += $"&seed={settings.VLESS_KCP_Seed}&headerType={settings.VLESS_KCP_Type}";
                }

                if (v2rayType == V2RayType.VLESS_QUIC)
                {
                    parametersURL += $"&quicSecurity={settings.VLESS_QUIC_Security}";
                    if (settings.VLESS_QUIC_Security != "none")
                    {
                        parametersURL += $"&key={HttpUtility.UrlEncode(settings.VLESS_QUIC_Key)}";
                    }
                    parametersURL += $"&headerType={settings.VLESS_QUIC_Type}";
                }
                /*
                // 4.4 TLS 相关段
                if (settings is XraySettings)
                {
                    if (v2rayType == RayType.VLESS_RAW_XTLS)
                    {
                        var xraySettings = settings as XraySettings;
                        parametersURL += $"&flow={xraySettings.Flow}";
                    }
                }
                */

                if (v2rayType == V2RayType.VLESS_gRPC)
                {
                    parametersURL += $"&serviceName={settings.VLESS_gRPC_ServiceName}&mode=gun";
                }
            }


            return $"{_protocol}://{HttpUtility.UrlEncode(_uuid)}@{_domain}:{_port}{parametersURL}#{HttpUtility.UrlEncode(_descriptiveText)}";
        }

        #endregion

        #region Xray ShareLink
        public static string XrayBuild(XrayType xrayType, XraySettings settings)
        {

            switch (xrayType)
            {
                case XrayType.VLESS_RAW_XTLS:
                case XrayType.VLESS_XTLS_RAW_REALITY:
                case XrayType.VLESS_XHTTP:
                case XrayType.VLESS_WS:
                case XrayType.VLESS_gRPC:
                case XrayType.Trojan_TCP:
                    return XrayBuildVlessShareLink(xrayType, settings);

                case XrayType.VMESS_KCP:
                    return XrayBuildVmessShareLink(xrayType, settings);
                case XrayType.ShadowsocksAEAD:
                    return XrayBuildShadowSocksShareLink(settings);
                default:
                    return string.Empty;
            }
        }

        private static string XrayBuildShadowSocksShareLink(XraySettings settings)
        {
            var _method = settings.ShadowSocksMethod;
            var _password = settings.ShadowSocksPassword;
            var _server = settings.Domain;
            var _port = settings.ShadowSocksPort;

            var base64URL = Utils.Base64Encode($"{_method}:{_password}@{_server}:{_port}");
            return "ss://" + base64URL + "#ShadowSocks";
        }

        private static string XrayBuildVmessShareLink(XrayType xrayType, XraySettings settings)
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
                case XrayType.VMESS_KCP:
                    vmess.ps = "vmess-mKCP";
                    vmess.port = settings.VMESS_KCP_Port.ToString();
                    vmess.net = "kcp";
                    vmess.type = settings.VMESS_KCP_Type;
                    vmess.path = settings.VMESS_KCP_Seed;
                    vmess.tls = "";
                    break;
                
                default:
                    return string.Empty;
            }

            var base64Url = Utils.Base64Encode(JsonConvert.SerializeObject(vmess));
            return $"vmess://" + base64Url;
        }

        private static string XrayBuildVlessShareLink(XrayType xrayType, XraySettings settings)
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

            var _flow = settings.Flow;
            var _sni = settings.MaskDomain;
            var _fingerprint = settings.UTLS;
            var _publicKey = settings.REALITY_publicKey;
            var _spiderX = settings.REALITY_spiderX;
            var _headerType = "none";

            var _alpn = string.Empty;
            var _mode = string.Empty;

            switch (xrayType)
            {
                case XrayType.VLESS_XTLS_RAW_REALITY:
                    _protocol = "vless";
                    _type = "tcp";
                    _security = "reality";
                    _flow = settings.Flow;
                    _sni = settings.MaskDomain;
                    _fingerprint = settings.UTLS;
                    _publicKey = settings.REALITY_publicKey;
                    _spiderX = settings.REALITY_spiderX;
                    _headerType = "none";
                    _host = settings.MaskDomain;
                    _descriptiveText = "vless-xtls-reality";
                    break;
                case XrayType.VLESS_RAW_XTLS:
                    _protocol = "vless";
                    _type = "tcp";
                    _security = "tls";
                    _descriptiveText = "vless-tcp-xtls";
                    break;
                case XrayType.VLESS_XHTTP:
                    _protocol = "vless";
                    _type = "xhttp";
                    _path = settings.VLESS_XHTTP_Path;
                    _alpn = "h3,h2,http/1.1";
                    _mode = "packet-up";
                    _descriptiveText = "vless-xhttp";
                    break;
                case XrayType.VLESS_WS:
                    _protocol = "vless";
                    _type = "ws";
                    _path = settings.VLESS_WS_Path;
                    _descriptiveText = "vless-ws-tls";
                    break;
                case XrayType.VLESS_gRPC:
                    _protocol = "vless";
                    _type = "grpc";
                    _port = settings.VLESS_gRPC_Port;
                    _descriptiveText = "vless-gRPC";
                    break;
                case XrayType.Trojan_TCP:
                    _protocol = "trojan";
                    _uuid = settings.TrojanPassword;
                    _descriptiveText = "trojan-tcp";
                    break;
                default:
                    throw new Exception("暂未实现的协议");
            }


            string parametersURL = string.Empty;
            if (xrayType != XrayType.Trojan_TCP && xrayType != XrayType.VLESS_XTLS_RAW_REALITY)
            {
                // 4.3 传输层相关段
                parametersURL = $"?type={_type}&encryption={_encryption}&security={_security}&path={HttpUtility.UrlEncode(_path)}";

                // xhttp
                if (xrayType == XrayType.VLESS_XHTTP)
                {
                    parametersURL += $"&alpn={HttpUtility.UrlEncode(_alpn)}&mode={_mode}";
                }

     
                // 4.4 TLS 相关段

                if (xrayType == XrayType.VLESS_RAW_XTLS)
                {

                    parametersURL += $"&flow={settings.Flow}";
                }

                if (xrayType == XrayType.VLESS_gRPC)
                {
                    parametersURL += $"&serviceName={settings.VLESS_gRPC_ServiceName}&mode=gun";
                }
            }

            if (xrayType == XrayType.VLESS_XTLS_RAW_REALITY)
            {
                parametersURL = $"?type={_type}&encryption={_encryption}&security={_security}&flow={_flow}&sni={_sni}&fp={_fingerprint}&pbk={_publicKey}&spx={HttpUtility.UrlEncode(_spiderX)}&headerType={_headerType}&host={_host}";

            }

            return $"{_protocol}://{HttpUtility.UrlEncode(_uuid)}@{_domain}:{_port}{parametersURL}#{HttpUtility.UrlEncode(_descriptiveText)}";
        }
        #endregion

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
