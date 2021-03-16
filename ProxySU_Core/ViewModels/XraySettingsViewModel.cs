using Newtonsoft.Json;
using ProxySU_Core.Common;
using ProxySU_Core.Models;
using ProxySU_Core.ViewModels.Developers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace ProxySU_Core.ViewModels
{
    public class XraySettingsViewModel : BaseViewModel
    {
        public XraySettings settings;

        public XraySettingsViewModel(XraySettings parameters)
        {
            this.settings = parameters;
        }

        public string UUID
        {
            get => settings.UUID;
            set => settings.UUID = value;
        }

        public string Domain
        {
            get => settings.Domain;
            set => settings.Domain = value;
        }

        public string MaskDomain
        {
            get => settings.MaskDomain;
            set => settings.MaskDomain = value;
        }

        public string VLESS_TCP_Path
        {
            get => settings.VLESS_TCP_Path;
            set => settings.VLESS_TCP_Path = value;
        }

        public string VLESS_WS_Path
        {
            get => settings.VLESS_WS_Path;
            set => settings.VLESS_WS_Path = value;
        }

        public string VMESS_TCP_Path
        {
            get => settings.VMESS_TCP_Path;
            set => settings.VMESS_TCP_Path = value;
        }

        public string VMESS_WS_Path
        {
            get => settings.VMESS_WS_Path;
            set => settings.VMESS_WS_Path = value;
        }

        public string TrojanPassword
        {
            get => settings.TrojanPassword;
            set => settings.TrojanPassword = value;
        }

        public bool Checked_VLESS_TCP
        {
            get
            {
                return settings.Types.Contains(XrayType.VLESS_TCP_TLS);
            }
            set
            {
                if (value == true)
                {
                    if (!settings.Types.Contains(XrayType.VLESS_TCP_TLS))
                        settings.Types.Add(XrayType.VLESS_TCP_TLS);

                }
                else
                {
                    settings.Types.Remove(XrayType.VLESS_TCP_TLS);
                }
                Notify("Checked_VLESS_TCP");
                Notify("VLESS_TCP_Path_Visibility");
            }
        }

        public bool Checked_VLESS_XTLS
        {
            get
            {
                return settings.Types.Contains(XrayType.VLESS_TCP_XTLS);
            }
            set
            {
                if (value == true)
                {
                    if (!settings.Types.Contains(XrayType.VLESS_TCP_XTLS))
                        settings.Types.Add(XrayType.VLESS_TCP_XTLS);
                }
                else
                {
                    settings.Types.Remove(XrayType.VLESS_TCP_XTLS);
                }
                Notify("Checked_VLESS_XTLS");
            }
        }

        public bool Checked_VLESS_WS
        {
            get
            {
                return settings.Types.Contains(XrayType.VLESS_WS_TLS);
            }
            set
            {
                if (value == true)
                {
                    if (!settings.Types.Contains(XrayType.VLESS_WS_TLS))
                        settings.Types.Add(XrayType.VLESS_WS_TLS);
                }
                else
                {
                    settings.Types.Remove(XrayType.VLESS_WS_TLS);
                }
                Notify("Checked_VLESS_WS");
                Notify("VLESS_WS_Path_Visibility");
            }
        }

        public bool Checked_VMESS_TCP
        {
            get
            {
                return settings.Types.Contains(XrayType.VMESS_TCP_TLS);
            }
            set
            {
                if (value == true)
                {
                    if (!settings.Types.Contains(XrayType.VMESS_TCP_TLS))
                        settings.Types.Add(XrayType.VMESS_TCP_TLS);
                }
                else
                {
                    settings.Types.Remove(XrayType.VMESS_TCP_TLS);
                }
                Notify("Checked_VMESS_TCP");
                Notify("VMESS_TCP_Path_Visibility");
            }
        }

        public bool Checked_VMESS_WS
        {
            get
            {
                return settings.Types.Contains(XrayType.VMESS_WS_TLS);
            }
            set
            {
                if (value == true)
                {
                    if (!settings.Types.Contains(XrayType.VMESS_WS_TLS))
                        settings.Types.Add(XrayType.VMESS_WS_TLS);
                }
                else
                {
                    settings.Types.Remove(XrayType.VMESS_WS_TLS);
                }
                Notify("Checked_VMESS_WS");
                Notify("VMESS_WS_Path_Visibility");
            }
        }

        public bool Checked_Trojan_TCP
        {
            get
            {
                return settings.Types.Contains(XrayType.Trojan_TCP_TLS);
            }
            set
            {
                if (value == true)
                {
                    if (!settings.Types.Contains(XrayType.Trojan_TCP_TLS))
                        settings.Types.Add(XrayType.Trojan_TCP_TLS);
                }
                else
                {
                    settings.Types.Remove(XrayType.Trojan_TCP_TLS);
                }
                Notify("Checked_Trojan_TCP");
                Notify("Trojan_TCP_Pwd_Visibility");
            }
        }

        public Visibility VLESS_TCP_Path_Visibility
        {
            get
            {
                return Checked_VLESS_TCP ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public Visibility VLESS_WS_Path_Visibility
        {
            get
            {
                return Checked_VLESS_WS ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public Visibility VMESS_TCP_Path_Visibility
        {
            get
            {
                return Checked_VMESS_TCP ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public Visibility VMESS_WS_Path_Visibility
        {
            get
            {
                return Checked_VMESS_WS ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public Visibility Trojan_TCP_Pwd_Visibility
        {
            get
            {
                return Checked_Trojan_TCP ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public string VLESS_TCP_XTLS_ShareLink
        {
            get => BuildVlessShareLink(XrayType.VLESS_TCP_XTLS);
        }
        public string VLESS_TCP_TLS_ShareLink
        {
            get => BuildVlessShareLink(XrayType.VLESS_TCP_TLS);
        }
        public string VLESS_WS_TLS_ShareLink
        {
            get => BuildVlessShareLink(XrayType.VLESS_WS_TLS);
        }
        public string VMESS_TCP_TLS_ShareLink
        {
            get => BuildVmessShareLink(XrayType.VMESS_TCP_TLS);
        }
        public string VMESS_WS_TLS_ShareLink
        {
            get => BuildVmessShareLink(XrayType.VMESS_WS_TLS);
        }
        public string Trojan_TCP_TLS_ShareLink
        {
            get => BuildVlessShareLink(XrayType.Trojan_TCP_TLS);
        }

        public string BuildVmessShareLink(XrayType xrayType)
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
                    vmess.path = VMESS_TCP_Path;
                    break;
                case XrayType.VMESS_WS_TLS:
                    vmess.ps = "vmess-ws-tls";
                    vmess.net = "ws";
                    vmess.type = "none";
                    vmess.path = VMESS_WS_Path;
                    break;
                default:
                    return string.Empty;
            }

            var base64Url = Base64.Encode(JsonConvert.SerializeObject(vmess));
            return $"vmess://" + base64Url;
        }

        public string BuildVlessShareLink(XrayType xrayType)
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
                    _path = VLESS_TCP_Path;
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
                    _path = VLESS_WS_Path;
                    _encryption = "none";
                    _descriptiveText = "vless-ws-tls";
                    break;
                case XrayType.VMESS_TCP_TLS:
                    _protocol = "vmess";
                    _type = "tcp";
                    _path = VMESS_TCP_Path;
                    _encryption = "auto";
                    _descriptiveText = "vmess-tcp-tls";
                    break;
                case XrayType.VMESS_WS_TLS:
                    _protocol = "vmess";
                    _type = "ws";
                    _path = VMESS_WS_Path;
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

    public class Vmess
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
