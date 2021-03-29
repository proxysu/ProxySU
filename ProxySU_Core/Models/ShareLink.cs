using Newtonsoft.Json;
using ProxySU_Core.Common;
using ProxySU_Core.Models.Developers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProxySU_Core.Models
{
    public class ShareLink
    {
        public static string Build(XrayType xrayType, XraySettings settings)
        {

            switch (xrayType)
            {
                case XrayType.VLESS_TCP:
                case XrayType.VLESS_TCP_XTLS:
                case XrayType.VLESS_WS:
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
            var _method = settings.ShadowsocksMethod;
            var _password = settings.ShadowsocksPassword;
            var _server = settings.Domain;
            var _port = ConfigBuilder.ShadowSocksPort;

            var base64URL = Base64.Encode($"{_method}:{_password}@{_server}:{_port}");
            return "ss://" + base64URL;
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
                host = settings.Domain,
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
                    vmess.port = ConfigBuilder.VMESS_mKCP_Port.ToString();
                    vmess.net = "kcp";
                    vmess.type = settings.VMESS_KCP_Type;
                    vmess.path = settings.VMESS_KCP_Seed;
                    vmess.tls = "";
                    break;
                default:
                    return string.Empty;
            }

            var base64Url = Base64.Encode(JsonConvert.SerializeObject(vmess));
            return $"vmess://" + base64Url;
        }

        private static string BuildVlessShareLink(XrayType xrayType, XraySettings settings)
        {
            var _protocol = string.Empty;
            var _uuid = settings.UUID;
            var _domain = settings.Domain;
            var _port = settings.Port;
            var _type = string.Empty;
            var _encryption = string.Empty;
            var _security = "tls";
            var _path = "/";
            var _host = settings.Domain;
            var _descriptiveText = string.Empty;

            switch (xrayType)
            {
                case XrayType.VLESS_TCP:
                    _protocol = "vless";
                    _type = "tcp";
                    _encryption = "none";
                    _descriptiveText = "vless-tcp-tls";
                    break;
                case XrayType.VLESS_TCP_XTLS:
                    _protocol = "vless";
                    _type = "tcp";
                    _security = "xtls";
                    _encryption = "none";
                    _descriptiveText = "vless-tcp-xtls";
                    break;
                case XrayType.VLESS_WS:
                    _protocol = "vless";
                    _type = "ws";
                    _path = settings.VLESS_WS_Path;
                    _encryption = "none";
                    _descriptiveText = "vless-ws-tls";
                    break;
                case XrayType.VMESS_TCP:
                    _protocol = "vmess";
                    _type = "tcp";
                    _path = settings.VMESS_TCP_Path;
                    _encryption = "auto";
                    _descriptiveText = "vmess-tcp-tls";
                    break;
                case XrayType.VMESS_WS:
                    _protocol = "vmess";
                    _type = "ws";
                    _path = settings.VMESS_WS_Path;
                    _encryption = "auto";
                    _descriptiveText = "vmess-ws-tls";
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
                parametersURL = $"?type={_type}&encryption={_encryption}&security={_security}&host={_host}&path={HttpUtility.UrlEncode(_path)}";


                // if mKCP
                // if QUIC

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
