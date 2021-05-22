using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProxySuper.Core.ViewModels
{
    public partial class XrayEditorViewModel : MvxViewModel<Record, Record>
    {
        public XrayEditorViewModel(IMvxNavigationService navigationService)
        {
            NavigationService = navigationService;
        }


        public string Id { get; set; }

        public Host Host { get; set; }

        public XraySettings Settings { get; set; }

        public IMvxCommand SaveCommand => new MvxCommand(() => Save());

        public IMvxNavigationService NavigationService { get; }

        public override void Prepare(Record parameter)
        {
            var record = Utils.DeepClone(parameter);
            Id = record.Id;
            Host = record.Host;
            Settings = record.XraySettings;
        }

        public void Save()
        {
            NavigationService.Close(this, new Record()
            {
                Id = Id,
                Host = Host,
                XraySettings = Settings,
            });
        }
    }

    public partial class XrayEditorViewModel
    {
        public IMvxCommand RandomUuid => new MvxCommand(() => GetUuid());


        public int Port
        {
            get => Settings.Port;
            set
            {
                Settings.Port = value;
                RaisePropertyChanged("Port");
            }
        }

        public int VLESS_KCP_Port
        {
            get => Settings.VLESS_KCP_Port;
            set
            {
                Settings.VLESS_KCP_Port = value;
                RaisePropertyChanged("VLESS_KCP_Port");
            }
        }

        public int VMESS_KCP_Port
        {
            get => Settings.VMESS_KCP_Port;
            set
            {
                Settings.VMESS_KCP_Port = value;
                RaisePropertyChanged("VMESS_KCP_Port");
            }
        }

        public int ShadowSocksPort
        {
            get => Settings.ShadowSocksPort;
            set
            {
                Settings.VMESS_KCP_Port = value;
                RaisePropertyChanged("ShadowSocksPort");
            }
        }


        public string UUID
        {
            get => Settings.UUID;
            set
            {
                Settings.UUID = value;
                RaisePropertyChanged("UUID");
            }
        }

        public string Domain
        {
            get => Settings.Domain;
            set
            {
                Settings.Domain = value;
                RaisePropertyChanged("Domain");
            }
        }

        public string MaskDomain
        {
            get => Settings.MaskDomain;
            set
            {
                Settings.MaskDomain = value;
                RaisePropertyChanged("MaskDomain");
            }
        }

        public string TrojanPassword
        {
            get => Settings.TrojanPassword;
            set => Settings.TrojanPassword = value;
        }
        public bool Checked_Trojan_TCP
        {
            get
            {
                return Settings.Types.Contains(XrayType.Trojan_TCP);
            }
            set
            {
                if (value == true)
                {
                    if (!Settings.Types.Contains(XrayType.Trojan_TCP))
                        Settings.Types.Add(XrayType.Trojan_TCP);
                }
                else
                {
                    Settings.Types.Remove(XrayType.Trojan_TCP);
                }
                RaisePropertyChanged("Checked_Trojan_TCP");
            }
        }
        public string Trojan_TCP_ShareLink
        {
            get => ShareLink.Build(XrayType.Trojan_TCP, Settings);
        }

        private List<string> _ssMethods = new List<string> { "aes-256-gcm", "aes-128-gcm", "chacha20-poly1305", "chacha20-ietf-poly1305" };
        public List<string> ShadowSocksMethods => _ssMethods;
        public bool CheckedShadowSocks
        {

            get => Settings.Types.Contains(XrayType.ShadowsocksAEAD);
            set
            {
                CheckBoxChanged(value, XrayType.ShadowsocksAEAD);
                RaisePropertyChanged("CheckedShadowSocks");
            }
        }
        public string ShadowSocksPassword
        {
            get => Settings.ShadowSocksPassword;
            set => Settings.ShadowSocksPassword = value;
        }
        public string ShadowSocksMethod
        {
            get => Settings.ShadowSocksMethod;
            set
            {
                var namespaceStr = typeof(ComboBoxItem).FullName + ":";
                var trimValue = value.Replace(namespaceStr, "");
                trimValue = trimValue.Trim();
                Settings.ShadowSocksMethod = trimValue;
                RaisePropertyChanged("ShadowSocksMethod");
            }
        }
        public string ShadowSocksShareLink
        {
            get => ShareLink.Build(XrayType.ShadowsocksAEAD, Settings);
        }


        private void CheckBoxChanged(bool value, XrayType type)
        {
            if (value == true)
            {
                if (!Settings.Types.Contains(type))
                {
                    Settings.Types.Add(type);
                }
            }
            else
            {
                Settings.Types.RemoveAll(x => x == type);
            }
        }



        private void GetUuid()
        {
            UUID = Guid.NewGuid().ToString();
            RaisePropertyChanged("UUID");
        }

    }

    /// <summary>
    /// VMESS
    /// </summary>
    public partial class XrayEditorViewModel
    {
        // vmess tcp
        public bool Checked_VMESS_TCP
        {
            get => Settings.Types.Contains(XrayType.VMESS_TCP);
            set
            {
                CheckBoxChanged(value, XrayType.VMESS_TCP);
                RaisePropertyChanged("Checked_VMESS_TCP");
            }
        }
        public string VMESS_TCP_Path
        {
            get => Settings.VMESS_TCP_Path;
            set => Settings.VMESS_TCP_Path = value;
        }
        public string VMESS_TCP_ShareLink
        {
            get => ShareLink.Build(XrayType.VMESS_TCP, Settings);
        }

        // vmess ws
        public bool Checked_VMESS_WS
        {
            get => Settings.Types.Contains(XrayType.VMESS_WS);
            set
            {
                CheckBoxChanged(value, XrayType.VMESS_WS);
                RaisePropertyChanged("Checked_VMESS_WS");
            }
        }
        public string VMESS_WS_Path
        {
            get => Settings.VMESS_WS_Path;
            set => Settings.VMESS_WS_Path = value;
        }
        public string VMESS_WS_ShareLink
        {
            get => ShareLink.Build(XrayType.VMESS_WS, Settings);
        }

        // vmess kcp
        public string VMESS_KCP_Seed
        {
            get => Settings.VMESS_KCP_Seed;
            set => Settings.VMESS_KCP_Seed = value;
        }
        public string VMESS_KCP_Type
        {
            get => Settings.VMESS_KCP_Type;
            set
            {
                var namespaceStr = typeof(ComboBoxItem).FullName + ":";
                var trimValue = value.Replace(namespaceStr, "");
                trimValue = trimValue.Trim();
                Settings.VMESS_KCP_Type = trimValue;
                RaisePropertyChanged("VMESS_KCP_Type");
            }
        }
        public bool Checked_VMESS_KCP
        {
            get => Settings.Types.Contains(XrayType.VMESS_KCP);
            set
            {
                CheckBoxChanged(value, XrayType.VMESS_KCP);
                RaisePropertyChanged("Checked_VMESS_KCP");
            }
        }
        public string VMESS_KCP_ShareLink
        {
            get => ShareLink.Build(XrayType.VMESS_KCP, Settings);
        }


        private List<string> _kcpTypes = new List<string> { "none", "srtp", "utp", "wechat-video", "dtls", "wireguard", };
        public List<string> KcpTypes => _kcpTypes;
    }

    /// <summary>
    /// VLESS
    /// </summary>
    public partial class XrayEditorViewModel
    {

        // vless xtls
        public bool Checked_VLESS_TCP_XTLS
        {
            get => Settings.Types.Contains(XrayType.VLESS_TCP_XTLS);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_TCP_XTLS);
                RaisePropertyChanged("Checked_VLESS_TCP_XTLS");
            }
        }
        public string VLESS_TCP_XTLS_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_TCP_XTLS, Settings);
        }

        // vless tcp
        public bool Checked_VLESS_TCP
        {
            get => Settings.Types.Contains(XrayType.VLESS_TCP);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_TCP);
                RaisePropertyChanged("Checked_VLESS_TCP");
            }
        }
        public string VLESS_TCP_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_TCP, Settings);
        }


        // vless ws
        public string VLESS_WS_Path
        {
            get => Settings.VLESS_WS_Path;
            set => Settings.VLESS_WS_Path = value;
        }
        public bool Checked_VLESS_WS
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_WS);
            }
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_WS);
                RaisePropertyChanged("Checked_VLESS_WS");
            }
        }
        public string VLESS_WS_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_WS, Settings);
        }

        // vless kcp
        public string VLESS_KCP_Seed
        {
            get => Settings.VLESS_KCP_Seed;
            set => Settings.VLESS_KCP_Seed = value;
        }
        public string VLESS_KCP_Type
        {
            get => Settings.VLESS_KCP_Type;
            set
            {
                var namespaceStr = typeof(ComboBoxItem).FullName + ":";
                var trimValue = value.Replace(namespaceStr, "");
                trimValue = trimValue.Trim();
                Settings.VLESS_KCP_Type = trimValue;
                RaisePropertyChanged("VLESS_KCP_Type");
            }
        }
        public bool Checked_VLESS_KCP
        {
            get => Settings.Types.Contains(XrayType.VLESS_KCP);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_KCP);
                RaisePropertyChanged("Checked_VLESS_KCP");
            }
        }
        public string VLESS_KCP_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_KCP, Settings);
        }

        // vless grpc
        public string VLESS_gRPC_ServiceName
        {
            get => Settings.VLESS_gRPC_ServiceName;
            set => Settings.VLESS_gRPC_ServiceName = value;
        }
        public int VLESS_gRPC_Port
        {
            get => Settings.VLESS_gRPC_Port;
            set => Settings.VLESS_gRPC_Port = value;
        }
        public bool Checked_VLESS_gRPC
        {
            get => Settings.Types.Contains(XrayType.VLESS_gRPC);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_gRPC);
                RaisePropertyChanged("Checked_VLESS_gRPC");
            }
        }
        public string VLESS_gRPC_ShareLink
        {
            get => ShareLink.Build(XrayType.VLESS_gRPC, Settings);
        }
    }

}
