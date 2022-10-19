using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

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

        public IMvxCommand SaveCommand => new MvxCommand(Save);

        public IMvxCommand SaveAndInstallCommand => new MvxCommand(SaveAndInstall);

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

        public void SaveAndInstall()
        {
            var record = new Record()
            {
                Id = Id,
                Host = Host,
                XraySettings = Settings,
            };
            NavigationService.Close(this, record);
            NavigationService.Navigate<XrayInstallViewModel, Record>(record);
        }
    }

    public partial class XrayEditorViewModel
    {
        public IMvxCommand RandomUuid => new MvxCommand(() => GetUuid());

        public bool WithTLS
        {
            get => Settings.WithTLS;
            set
            {
                Settings.WithTLS = value;
                RaisePropertyChanged("Port");
            }
        }

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
                Settings.ShadowSocksPort = value;
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

        public string MultiUUID
        {
            get => string.Join(",", Settings.MulitUUID);
            set
            {
                var input = value.Replace('，', ',');
                var arr = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                arr.RemoveAll(x => x == this.UUID);
                Settings.MulitUUID = arr;
                RaisePropertyChanged("MultiUUID");
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
        public string UTLSOption
        {
            get => Settings.UTLSOption;
            set
            {
                var namespaceStr = typeof(ComboBoxItem).FullName + ":";
                var trimValue = value.Replace(namespaceStr, "");
                trimValue = trimValue.Trim();
                Settings.UTLSOption = trimValue;
                RaisePropertyChanged("UTLSOption");
            }
        }
        private List<string> _uTlsOptions = new List<string> { string.Empty, "chrome", "firefox", "safari", "randomized" };
        public List<string> UTLSOptions => _uTlsOptions;
        public bool CheckedUTLSOptions
        {

            get => Settings.Types.Contains(RayType.VLESS_TCP_XTLS);
            set
            {
                CheckBoxChanged(value, RayType.VLESS_TCP_XTLS);
                RaisePropertyChanged("CheckedUTLSOptions");
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
                return Settings.Types.Contains(RayType.Trojan_TCP);
            }
            set
            {
                if (value == true)
                {
                    if (!Settings.Types.Contains(RayType.Trojan_TCP))
                        Settings.Types.Add(RayType.Trojan_TCP);
                }
                else
                {
                    Settings.Types.Remove(RayType.Trojan_TCP);
                }
                RaisePropertyChanged("Checked_Trojan_TCP");
            }
        }
        public string Trojan_TCP_ShareLink
        {
            get => ShareLink.Build(RayType.Trojan_TCP, Settings);
        }

        private List<string> _ssMethods = new List<string> { "aes-256-gcm", "aes-128-gcm", "chacha20-poly1305", "chacha20-ietf-poly1305" };
        public List<string> ShadowSocksMethods => _ssMethods;
        public bool CheckedShadowSocks
        {

            get => Settings.Types.Contains(RayType.ShadowsocksAEAD);
            set
            {
                CheckBoxChanged(value, RayType.ShadowsocksAEAD);
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
            get => ShareLink.Build(RayType.ShadowsocksAEAD, Settings);
        }


        private void CheckBoxChanged(bool value, RayType type)
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
            get => Settings.Types.Contains(RayType.VMESS_TCP);
            set
            {
                CheckBoxChanged(value, RayType.VMESS_TCP);
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
            get => ShareLink.Build(RayType.VMESS_TCP, Settings);
        }

        // vmess ws
        public bool Checked_VMESS_WS
        {
            get => Settings.Types.Contains(RayType.VMESS_WS);
            set
            {
                CheckBoxChanged(value, RayType.VMESS_WS);
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
            get => ShareLink.Build(RayType.VMESS_WS, Settings);
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
            get => Settings.Types.Contains(RayType.VMESS_KCP);
            set
            {
                CheckBoxChanged(value, RayType.VMESS_KCP);
                RaisePropertyChanged("Checked_VMESS_KCP");
            }
        }
        public string VMESS_KCP_ShareLink
        {
            get => ShareLink.Build(RayType.VMESS_KCP, Settings);
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
            get => Settings.Types.Contains(RayType.VLESS_TCP_XTLS);
            set
            {
                CheckBoxChanged(value, RayType.VLESS_TCP_XTLS);
                RaisePropertyChanged("Checked_VLESS_TCP_XTLS");
            }
        }
        public string VLESS_TCP_XTLS_ShareLink
        {
            get => ShareLink.Build(RayType.VLESS_TCP_XTLS, Settings);
        }

        // vless tcp
        public bool Checked_VLESS_TCP
        {
            get => Settings.Types.Contains(RayType.VLESS_TCP);
            set
            {
                CheckBoxChanged(value, RayType.VLESS_TCP);
                RaisePropertyChanged("Checked_VLESS_TCP");
            }
        }
        public string VLESS_TCP_ShareLink
        {
            get => ShareLink.Build(RayType.VLESS_TCP, Settings);
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
                return Settings.Types.Contains(RayType.VLESS_WS);
            }
            set
            {
                CheckBoxChanged(value, RayType.VLESS_WS);
                RaisePropertyChanged("Checked_VLESS_WS");
            }
        }
        public string VLESS_WS_ShareLink
        {
            get => ShareLink.Build(RayType.VLESS_WS, Settings);
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
            get => Settings.Types.Contains(RayType.VLESS_KCP);
            set
            {
                CheckBoxChanged(value, RayType.VLESS_KCP);
                RaisePropertyChanged("Checked_VLESS_KCP");
            }
        }
        public string VLESS_KCP_ShareLink
        {
            get => ShareLink.Build(RayType.VLESS_KCP, Settings);
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
            get => Settings.Types.Contains(RayType.VLESS_gRPC);
            set
            {
                CheckBoxChanged(value, RayType.VLESS_gRPC);
                RaisePropertyChanged("Checked_VLESS_gRPC");
            }
        }
        public string VLESS_gRPC_ShareLink
        {
            get => ShareLink.Build(RayType.VLESS_gRPC, Settings);
        }
    }

}
