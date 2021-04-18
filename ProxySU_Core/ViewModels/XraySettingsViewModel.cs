using Newtonsoft.Json;
using ProxySU_Core.Common;
using ProxySU_Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProxySU_Core.ViewModels
{
    public partial class XraySettingsViewModel : BaseViewModel
    {
        public XraySettings settings;
        private readonly ICommand _randomUuid;

        public XraySettingsViewModel(XraySettings parameters)
        {
            _randomUuid = new BaseCommand((obj) => GetUuid());
            this.settings = parameters;
            Notify("VMESS_KCP_Type");
        }

        public ICommand RandomUuid
        {
            get
            {
                return _randomUuid;
            }
        }


        public int Port
        {
            get => settings.Port;
            set
            {
                settings.Port = value;
                Notify("Port");
            }
        }

        public int VLESS_KCP_Port
        {
            get => settings.VLESS_KCP_Port;
            set
            {
                settings.VLESS_KCP_Port = value;
                Notify("VLESS_KCP_Port");
            }
        }

        public int VMESS_KCP_Port
        {
            get => settings.VMESS_KCP_Port;
            set
            {
                settings.VMESS_KCP_Port = value;
                Notify("VMESS_KCP_Port");
            }
        }

        public int ShadowSocksPort
        {
            get => settings.ShadowSocksPort;
            set
            {
                settings.VMESS_KCP_Port = value;
                Notify("ShadowSocksPort");
            }
        }


        public string UUID
        {
            get => settings.UUID;
            set
            {
                settings.UUID = value;
                Notify("UUID");
            }
        }

        public string Domain
        {
            get => settings.Domain;
            set
            {
                settings.Domain = value;
                Notify("Domain");
            }
        }

        public string MaskDomain
        {
            get => settings.MaskDomain;
            set
            {
                settings.MaskDomain = value;
                Notify("MaskDomain");
            }
        }

        public string TrojanPassword
        {
            get => settings.TrojanPassword;
            set => settings.TrojanPassword = value;
        }
        public bool Checked_Trojan_TCP
        {
            get
            {
                return settings.Types.Contains(XrayType.Trojan_TCP);
            }
            set
            {
                if (value == true)
                {
                    if (!settings.Types.Contains(XrayType.Trojan_TCP))
                        settings.Types.Add(XrayType.Trojan_TCP);
                }
                else
                {
                    settings.Types.Remove(XrayType.Trojan_TCP);
                }
                Notify("Checked_Trojan_TCP");
            }
        }
        public string Trojan_TCP_ShareLink
        {
            get => ShareLink.Build(XrayType.Trojan_TCP, settings);
        }

        private List<string> _ssMethods = new List<string> { "aes-256-gcm", "aes-128-gcm", "chacha20-poly1305", "chacha20-ietf-poly1305" };
        public List<string> ShadowSocksMethods => _ssMethods;
        public bool CheckedShadowSocks
        {

            get => settings.Types.Contains(XrayType.ShadowsocksAEAD);
            set
            {
                CheckBoxChanged(value, XrayType.ShadowsocksAEAD);
                Notify("CheckedShadowSocks");
            }
        }
        public string ShadowSocksPassword
        {
            get => settings.ShadowsocksPassword;
            set => settings.ShadowsocksPassword = value;
        }
        public string ShadowSocksMethod
        {
            get => settings.ShadowsocksMethod;
            set
            {
                var namespaceStr = typeof(ComboBoxItem).FullName + ":";
                var trimValue = value.Replace(namespaceStr, "");
                trimValue = trimValue.Trim();
                settings.ShadowsocksMethod = trimValue;
                Notify("ShadowSocksMethod");
            }
        }
        public string ShadowSocksShareLink
        {
            get => ShareLink.Build(XrayType.ShadowsocksAEAD, settings);
        }


        private void CheckBoxChanged(bool value, XrayType type)
        {
            if (value == true)
            {
                if (!settings.Types.Contains(type))
                {
                    settings.Types.Add(type);
                }
            }
            else
            {
                settings.Types.RemoveAll(x => x == type);
            }
        }



        private void GetUuid()
        {
            UUID = Guid.NewGuid().ToString();
            Notify("UUID");
        }

    }

    /// <summary>
    /// VMESS
    /// </summary>
    public partial class XraySettingsViewModel
    {
        // vmess tcp
        public bool Checked_VMESS_TCP
        {
            get => settings.Types.Contains(XrayType.VMESS_TCP);
            set
            {
                CheckBoxChanged(value, XrayType.VMESS_TCP);
                Notify("Checked_VMESS_TCP");
            }
        }
        public string VMESS_TCP_Path
        {
            get => settings.VMESS_TCP_Path;
            set => settings.VMESS_TCP_Path = value;
        }
        public string VMESS_TCP_ShareLink
        {
            get => ShareLink.Build(XrayType.VMESS_TCP, settings);
        }

        // vmess ws
        public bool Checked_VMESS_WS
        {
            get => settings.Types.Contains(XrayType.VMESS_WS);
            set
            {
                CheckBoxChanged(value, XrayType.VMESS_WS);
                Notify("Checked_VMESS_WS");
            }
        }
        public string VMESS_WS_Path
        {
            get => settings.VMESS_WS_Path;
            set => settings.VMESS_WS_Path = value;
        }
        public string VMESS_WS_ShareLink
        {
            get => ShareLink.Build(XrayType.VMESS_WS, settings);
        }

        // vmess kcp
        public string VMESS_KCP_Seed
        {
            get => settings.VMESS_KCP_Seed;
            set => settings.VMESS_KCP_Seed = value;
        }
        public string VMESS_KCP_Type
        {
            get => settings.VMESS_KCP_Type;
            set
            {
                var namespaceStr = typeof(ComboBoxItem).FullName + ":";
                var trimValue = value.Replace(namespaceStr, "");
                trimValue = trimValue.Trim();
                settings.VMESS_KCP_Type = trimValue;
                Notify("VMESS_KCP_Type");
            }
        }
        public bool Checked_VMESS_KCP
        {
            get => settings.Types.Contains(XrayType.VMESS_KCP);
            set
            {
                CheckBoxChanged(value, XrayType.VMESS_KCP);
                Notify("Checked_VMESS_KCP");
            }
        }
        public string VMESS_KCP_ShareLink
        {
            get => ShareLink.Build(XrayType.VMESS_KCP, settings);
        }


        private List<string> _kcpTypes = new List<string> { "none", "srtp", "utp", "wechat-video", "dtls", "wireguard", };
        public List<string> KcpTypes => _kcpTypes;
    }

    /// <summary>
    /// VLESS
    /// </summary>
    public partial class XraySettingsViewModel
    {

        // vless xtls
        public bool Checked_VLESS_TCP_XTLS
        {
            get => settings.Types.Contains(XrayType.VLESS_TCP_XTLS);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_TCP_XTLS);
                Notify("Checked_VLESS_TCP_XTLS");
            }
        }
        public string VLESS_TCP_XTLS_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_TCP_XTLS, settings);
        }

        // vless tcp
        public bool Checked_VLESS_TCP
        {
            get => settings.Types.Contains(XrayType.VLESS_TCP);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_TCP);
                Notify("Checked_VLESS_TCP");
            }
        }
        public string VLESS_TCP_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_TCP, settings);
        }


        // vless ws
        public string VLESS_WS_Path
        {
            get => settings.VLESS_WS_Path;
            set => settings.VLESS_WS_Path = value;
        }
        public bool Checked_VLESS_WS
        {
            get
            {
                return settings.Types.Contains(XrayType.VLESS_WS);
            }
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_WS);
                Notify("Checked_VLESS_WS");
            }
        }
        public string VLESS_WS_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_WS, settings);
        }

        // vless kcp
        public string VLESS_KCP_Seed
        {
            get => settings.VLESS_KCP_Seed;
            set => settings.VLESS_KCP_Seed = value;
        }
        public string VLESS_KCP_Type
        {
            get => settings.VLESS_KCP_Type;
            set
            {
                var namespaceStr = typeof(ComboBoxItem).FullName + ":";
                var trimValue = value.Replace(namespaceStr, "");
                trimValue = trimValue.Trim();
                settings.VLESS_KCP_Type = trimValue;
                Notify("VLESS_KCP_Type");
            }
        }
        public bool Checked_VLESS_KCP
        {
            get => settings.Types.Contains(XrayType.VLESS_KCP);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_KCP);
                Notify("Checked_VLESS_KCP");
            }
        }
        public string VLESS_KCP_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_KCP, settings);
        }

        // vless grpc
        public string VLESS_gRPC_ServiceName
        {
            get => settings.VLESS_gRPC_ServiceName;
            set => settings.VLESS_gRPC_ServiceName = value;
        }
        public int VLESS_gRPC_Port
        {
            get => settings.VLESS_gRPC_Port;
            set => settings.VLESS_gRPC_Port = value;
        }
        public bool Checked_VLESS_gRPC
        {
            get => settings.Types.Contains(XrayType.VLESS_gRPC);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_gRPC);
                Notify("Checked_VLESS_gRPC");
            }
        }
        public string VLESS_gRPC_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_gRPC, settings);
        }
    }

}
