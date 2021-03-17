using Newtonsoft.Json;
using ProxySU_Core.Common;
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
                case XrayType.VLESS_TCP_TLS:
                case XrayType.VLESS_TCP_XTLS:
                case XrayType.VLESS_WS_TLS:
                case XrayType.Trojan_TCP_TLS:
                    return BuildVlessShareLink(xrayType, settings);
                case XrayType.VMESS_TCP_TLS:
                case XrayType.VMESS_WS_TLS:
                    return BuildVmessShareLink(xrayType, settings);
                default:
                    return string.Empty;
            }
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
                case XrayType.VMESS_TCP_TLS:
                    vmess.ps = "vmess-tcp-tls";
                    vmess.net = "tcp";
                    vmess.type = "http";
                    vmess.path = settings.VMESS_TCP_Path;
                    break;
                case XrayType.VMESS_WS_TLS:
                    vmess.ps = "vmess-ws-tls";
                    vmess.net = "ws";
                    vmess.type = "none";
                    vmess.path = settings.VMESS_WS_Path;
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
                case XrayType.VLESS_TCP_TLS:
                    _protocol = "vless";
                    _type = "tcp";
                    _path = settings.VLESS_TCP_Path;
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
                case XrayType.VLESS_WS_TLS:
                    _protocol = "vless";
                    _type = "ws";
                    _path = settings.VLESS_WS_Path;
                    _encryption = "none";
                    _descriptiveText = "vless-ws-tls";
                    break;
                case XrayType.VMESS_TCP_TLS:
                    _protocol = "vmess";
                    _type = "tcp";
                    _path = settings.VMESS_TCP_Path;
                    _encryption = "auto";
                    _descriptiveText = "vmess-tcp-tls";
                    break;
                case XrayType.VMESS_WS_TLS:
                    _protocol = "vmess";
                    _type = "ws";
                    _path = settings.VMESS_WS_Path;
                    _encryption = "auto";
                    _descriptiveText = "vmess-ws-tls";
                    break;
                case XrayType.Trojan_TCP_TLS:
                    _protocol = "trojan";
                    _descriptiveText = "trojan-tcp";
                    break;
                default:
                    throw new Exception("暂未实现的协议");
            }


            string parametersURL = string.Empty;
            if (xrayType != XrayType.Trojan_TCP_TLS)
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
