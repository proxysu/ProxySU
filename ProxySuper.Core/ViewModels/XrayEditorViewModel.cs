using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
            if (Settings.Types.Count > 0)
            {
                NavigationService.Close(this, new Record()
                {
                    Id = Id,
                    Host = Host,
                    XraySettings = Settings,
                });
            }
            else
            {
                MessageBox.Show("Error:No configuration was selected!", "Tips");
            }
           
        }

        public void SaveAndInstall()
        {
            if (Settings.Types.Count > 0)
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
            else
            {
                MessageBox.Show("Error:No configuration was selected!", "Tips");
            }

        }
    }

    public partial class XrayEditorViewModel
    {
        public List<string> UTLSList { get => XraySettings.UTLSList; }

        /*
        public List<string> QuicTypes => XraySettings.DisguiseTypes;

        public List<string> QuicSecurities => new List<string>
        {
            "none",
            "aes-128-gcm",
            "chacha20-poly1305"
        };
        */

        public IMvxCommand RandomUuid => new MvxCommand(() => GetUuid());

        public bool WithTLS
        {
            get => Settings.WithTLS;
            set
            {
                Settings.WithTLS = value;
                RaisePropertyChanged("WithTLS");
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


        public string UTLS
        {
            get => Settings.UTLS;
            set
            {
                Settings.UTLS = value;
                RaisePropertyChanged(nameof(UTLS));
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

        #region Trojan
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
            get => ShareLink.XrayBuild(XrayType.Trojan_TCP, Settings);
        }
        #endregion

        #region ShadowSocks
        public int ShadowSocksPort
        {
            get => Settings.ShadowSocksPort;
            set
            {
                Settings.ShadowSocksPort = value;
                RaisePropertyChanged("ShadowSocksPort");
            }
        }
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
        public List<string> ShadowSocksMethods => new List<string>
        {
            "2022-blake3-aes-128-gcm",
            "2022-blake3-aes-256-gcm",
            "2022-blake3-chacha20-poly1305",
            "aes-256-gcm",
            "aes-128-gcm",
            "chacha20-poly1305",
            "none"
        };
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
            get => ShareLink.XrayBuild(XrayType.ShadowsocksAEAD, Settings);
        }
        #endregion

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

        #region VMESS KCP
        public List<string> KcpTypes => XraySettings.DisguiseTypes;
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
                if(value && !Checked_VLESS_RAW_XTLS 
                        　&& !Checked_VLESS_WS
                        　&& !Checked_VLESS_gRPC
                        　&& !Checked_Trojan_TCP)
                {
                    WithTLS = false;
                }
                else
                {
                    WithTLS = true;
                }
            }
        }
        public string VMESS_KCP_ShareLink
        {
            get => ShareLink.XrayBuild(XrayType.VMESS_KCP, Settings);
        }
        #endregion

    }

    /// <summary>
    /// VLESS
    /// </summary>
    public partial class XrayEditorViewModel
    {

        public List<string> FlowList { get => XraySettings.FlowList; }

        public string Flow
        {
            get => Settings.Flow;
            set
            {
                Settings.Flow = value;
                RaisePropertyChanged(nameof(Flow));
            }
        }

        public string REALITY_spiderX
        {
            get => Settings.REALITY_spiderX;
            set => Settings.REALITY_spiderX = value;
        }


        #region VLESS XTLS(RAW) REALITY

        public bool Checked_VLESS_XTLS_RAW_REALITY
        {
            get => Settings.Types.Contains(XrayType.VLESS_XTLS_RAW_REALITY);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_XTLS_RAW_REALITY);
                RaisePropertyChanged("Checked_VLESS_XTLS_RAW_REALITY");
                if (value)
                {
                    WithTLS = false;
                }
                else
                {
                    WithTLS = true;
                }
            }
        }
        public string VLESS_XTLS_RAW_REALITY_ShareLink
        {
            get => ShareLink.XrayBuild(XrayType.VLESS_XTLS_RAW_REALITY, Settings);
        }

        #endregion

        #region VLESS XTLS(RAW)
 
        public bool Checked_VLESS_RAW_XTLS
        {
            get => Settings.Types.Contains(XrayType.VLESS_RAW_XTLS);
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_RAW_XTLS);
                RaisePropertyChanged("Checked_VLESS_RAW_XTLS");
                if (value)
                {
                    WithTLS = true;
                }
            }
        }
        public string VLESS_RAW_XTLS_ShareLink
        {
            get => ShareLink.XrayBuild(XrayType.VLESS_RAW_XTLS, Settings);
        }
        #endregion

        #region VLESS XHTTP
        public string VLESS_XHTTP_Path
        {
            get => Settings.VLESS_XHTTP_Path;
            set => Settings.VLESS_XHTTP_Path = value;
        }
        public bool Checked_VLESS_XHTTP
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_XHTTP);
            }
            set
            {
                CheckBoxChanged(value, XrayType.VLESS_XHTTP);
                RaisePropertyChanged("Checked_VLESS_XHTTP");
                if (value)
                {
                    WithTLS = true;
                }
            }
        }
        public string VLESS_XHTTP_ShareLink
        {
            get => ShareLink.XrayBuild(XrayType.VLESS_XHTTP, Settings);
        }
        #endregion

        #region VLESS WS
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
                if (value)
                {
                    WithTLS = true;
                }
            }
        }
        public string VLESS_WS_ShareLink
        {
            get => ShareLink.XrayBuild(XrayType.VLESS_WS, Settings);
        }
        #endregion

        #region VLESS gRPC
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
                if (value)
                {
                    WithTLS = true;
                }
            }
        }
        public string VLESS_gRPC_ShareLink
        {
            get => ShareLink.XrayBuild(XrayType.VLESS_gRPC, Settings);
        }
        #endregion
    }

}
