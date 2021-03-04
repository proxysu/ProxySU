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
        private XraySettings p;

        public XraySettingsViewModel(XraySettings parameters)
        {
            this.p = parameters;
        }

        public string UUID
        {
            get => p.UUID;
            set => p.UUID = value;
        }

        public string Domain
        {
            get => p.Domain;
            set => p.Domain = value;
        }

        public string MaskDomain
        {
            get => p.MaskDomain;
            set => p.MaskDomain = value;
        }

        public string VLESS_TCP_Path
        {
            get => p.VLESS_TCP_Path;
            set => p.VLESS_TCP_Path = value;
        }

        public string VLESS_WS_Path
        {
            get => p.VLESS_WS_Path;
            set => p.VLESS_WS_Path = value;
        }

        public string VMESS_TCP_Path
        {
            get => p.VMESS_TCP_Path;
            set => p.VMESS_TCP_Path = value;
        }

        public string VMESS_WS_Path
        {
            get => p.VMESS_WS_Path;
            set => p.VMESS_WS_Path = value;
        }

        public string Trojan_TCP_Path
        {
            get => p.Trojan_TCP_Path;
            set => p.Trojan_TCP_Path = value;
        }

        public string TrojanPassword
        {
            get => p.TrojanPassword;
            set => p.TrojanPassword = value;
        }

        public bool Checked_VLESS_TCP
        {
            get
            {
                return p.Types.Contains(XrayType.VLESS_TCP_TLS);
            }
            set
            {
                if (value == true)
                {
                    p.Types.Add(XrayType.VLESS_TCP_TLS);
                }
                else
                {
                    p.Types.Remove(XrayType.VLESS_TCP_TLS);
                }
                Notify("Checked_VLESS_TCP");
                Notify("VLESS_TCP_Path_Visibility");
            }
        }

        public bool Checked_VLESS_XTLS
        {
            get
            {
                return p.Types.Contains(XrayType.VLESS_TCP_XTLS);
            }
            set
            {
                if (value == true)
                {
                    p.Types.Add(XrayType.VLESS_TCP_XTLS);
                }
                else
                {
                    p.Types.Remove(XrayType.VLESS_TCP_XTLS);
                }
                Notify("Checked_VLESS_XTLS");
            }
        }

        public bool Checked_VLESS_WS
        {
            get
            {
                return p.Types.Contains(XrayType.VLESS_WS_TLS);
            }
            set
            {
                if (value == true)
                {
                    p.Types.Add(XrayType.VLESS_WS_TLS);
                }
                else
                {
                    p.Types.Remove(XrayType.VLESS_WS_TLS);
                }
                Notify("Checked_VLESS_WS");
                Notify("VLESS_WS_Path_Visibility");
            }
        }

        public bool Checked_VMESS_TCP
        {
            get
            {
                return p.Types.Contains(XrayType.VMESS_TCP_TLS);
            }
            set
            {
                if (value == true)
                {
                    p.Types.Add(XrayType.VMESS_TCP_TLS);
                }
                else
                {
                    p.Types.Remove(XrayType.VMESS_TCP_TLS);
                }
                Notify("Checked_VMESS_TCP");
                Notify("VMESS_TCP_Path_Visibility");
            }
        }

        public bool Checked_VMESS_WS
        {
            get
            {
                return p.Types.Contains(XrayType.VMESS_WS_TLS);
            }
            set
            {
                if (value == true)
                {
                    p.Types.Add(XrayType.VMESS_WS_TLS);
                }
                else
                {
                    p.Types.Remove(XrayType.VMESS_WS_TLS);
                }
                Notify("Checked_VMESS_WS");
                Notify("VMESS_WS_Path_Visibility");
            }
        }

        public bool Checked_Trojan_TCP
        {
            get
            {
                return p.Types.Contains(XrayType.Trojan_TCP_TLS);
            }
            set
            {
                if (value == true)
                {
                    p.Types.Add(XrayType.Trojan_TCP_TLS);
                }
                else
                {
                    p.Types.Remove(XrayType.Trojan_TCP_TLS);
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
