using Newtonsoft.Json;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProxySuper.Core.Services
{
    public class ShareLink
    {
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

        public static string Build(XrayType xrayType, XraySettings settings)
        {

            switch (xrayType)
            {
                case XrayType.VLESS_TCP:
                case XrayType.VLESS_TCP_XTLS:
                case XrayType.VLESS_WS:
                case XrayType.VLESS_KCP:
                case XrayType.VLESS_gRPC:
                case XrayType.Trojan_TCP:
                    return BuildVlessShareLink(xrayType, settings);
                case XrayType.VMESS_TCP:
                case XrayType.VMESS_WS:
                case XrayType.VMESS_KCP:
                    return BuildVmessShareLink(xrayType, settings);
                case XrayType.ShadowsocksAEAD:
                    return BuildShadowSocksShareLink(settings);
                default:
                    return string.Empty;
            }
        }

        private static string BuildShadowSocksShareLink(XraySettings settings)
        {
            var _method = settings.ShadowSocksMethod;
            var _password = settings.ShadowSocksPassword;
            var _server = settings.Domain;
            var _port = settings.ShadowSocksPort;

            var base64URL = Utils.Base64Encode($"{_method}:{_password}@{_server}:{_port}");
            return "ss://" + base64URL + "#ShadowSocks";
        }

        private static string BuildVmessShareLink(XrayType xrayType, XraySettings settings)
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
                case XrayType.VMESS_TCP:
                    vmess.ps = "vmess-tcp-tls";
                    vmess.net = "tcp";
                    vmess.type = "http";
                    vmess.path = settings.VMESS_TCP_Path;
                    break;
                case XrayType.VMESS_WS:
                    vmess.ps = "vmess-ws-tls";
                    vmess.net = "ws";
                    vmess.type = "none";
                    vmess.path = settings.VMESS_WS_Path;
                    break;
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

        private static string BuildVlessShareLink(XrayType xrayType, XraySettings settings)
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
            var _headerType = "none";
            var _seed = string.Empty;

            switch (xrayType)
            {
                case XrayType.VLESS_TCP:
                    _protocol = "vless";
                    _type = "tcp";
                    _descriptiveText = "vless-tcp-tls";
                    break;
                case XrayType.VLESS_TCP_XTLS:
                    _protocol = "vless";
                    _type = "tcp";
                    _security = "xtls";
                    _descriptiveText = "vless-tcp-xtls";
                    break;
                case XrayType.VLESS_WS:
                    _protocol = "vless";
                    _type = "ws";
                    _path = settings.VLESS_WS_Path;
                    _descriptiveText = "vless-ws-tls";
                    break;
                case XrayType.VLESS_KCP:
                    _protocol = "vless";
                    _type = "kcp";
                    _headerType = settings.VLESS_KCP_Type;
                    _seed = settings.VLESS_KCP_Seed;
                    _port = settings.VLESS_KCP_Port;
                    _security = "none";
                    _descriptiveText = "vless-mKCP";
                    break;
                case XrayType.VLESS_gRPC:
                    _protocol = "vless";
                    _type = "grpc";
                    _path = settings.VLESS_gRPC_ServiceName;
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
            if (xrayType != XrayType.Trojan_TCP)
            {
                // 4.3 传输层相关段
                parametersURL = $"?type={_type}&encryption={_encryption}&security={_security}&path={HttpUtility.UrlEncode(_path)}&headerType={_headerType}";

                // kcp
                if (xrayType == XrayType.VLESS_KCP)
                {
                    parametersURL += $"&seed={_seed}";
                }

                // 4.4 TLS 相关段
                if (xrayType == XrayType.VLESS_TCP_XTLS)
                {
                    parametersURL += "&flow=xtls-rprx-direct";
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
