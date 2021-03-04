using ProxySU_Core.Models;
using ProxySU_Core.ViewModels.Developers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public string Trojan_TCP_Path
        {
            get => settings.Trojan_TCP_Path;
            set => settings.Trojan_TCP_Path = value;
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
                    settings.Types.Add(XrayType.Trojan_TCP_TLS);
                }
                else
                {
                    settings.Types.Remove(XrayType.Trojan_TCP_TLS);
                }
                Notify("Checked_Trojan_TCP");
                Notify("Trojan_TCP_Path_Visibility");
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
        public Visibility Trojan_TCP_Path_Visibility
        {
            get
            {
                return Checked_Trojan_TCP ? Visibility.Visible : Visibility.Hidden;
            }
        }

    }
}
