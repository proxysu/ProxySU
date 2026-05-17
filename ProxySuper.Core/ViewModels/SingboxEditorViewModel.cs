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

namespace ProxySuper.Core.ViewModels
{
    public partial class SingboxEditorViewModel : MvxViewModel<Record, Record>
    {
        public SingboxEditorViewModel(IMvxNavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public IMvxNavigationService NavigationService { get; }

        public string Id { get; set; }

        public Host Host { get; set; }

        public SingboxSettings Settings { get; set; }

        public IMvxCommand SaveCommand => new MvxCommand(() => Save());

        public IMvxCommand SaveAndInstallCommand => new MvxCommand(SaveAndInstall);

        public override void Prepare(Record parameter)
        {
            var record = Utils.DeepClone(parameter);
            Id = record.Id;
            Host = record.Host;
            Settings = record.SingboxSettings;
        }

        public void Save()
        {
            NavigationService.Close(this, new Record()
            {
                Id = Id,
                Host = Host,
                SingboxSettings = Settings,
            });
        }

        private void SaveAndInstall()
        {
            var record = new Record
            {
                Id = this.Id,
                Host = this.Host,
                SingboxSettings = Settings,
            };
            NavigationService.Close(this, record);
            NavigationService.Navigate<SingboxInstallViewModel, Record>(record);
        }
    }

    /// <summary>
    /// Checked_XXX 协议复选框属性 + 协议参数属性
    /// </summary>
    public partial class SingboxEditorViewModel
    {
        /// <summary>
        /// CheckBox 统一操作：添加或移除 SingBoxType 枚举值
        /// </summary>
        private void CheckBoxChanged(bool value, SingBoxType type)
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

        // ==================== VLESS（绿色） ====================

        public bool Checked_VLESS_TCP
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_VLESS_TCP);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_VLESS_TCP);
                RaisePropertyChanged("Checked_VLESS_TCP");
            }
        }

        public bool Checked_VLESS_WS
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_VLESS_WS);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_VLESS_WS);
                RaisePropertyChanged("Checked_VLESS_WS");
            }
        }

        public bool Checked_VLESS_gRPC
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_VLESS_gRPC);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_VLESS_gRPC);
                RaisePropertyChanged("Checked_VLESS_gRPC");
            }
        }

        // ==================== VMESS（蓝色） ====================

        public bool Checked_VMESS_TCP
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_VMESS_TCP);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_VMESS_TCP);
                RaisePropertyChanged("Checked_VMESS_TCP");
            }
        }

        public bool Checked_VMESS_WS
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_VMESS_WS);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_VMESS_WS);
                RaisePropertyChanged("Checked_VMESS_WS");
            }
        }

        public bool Checked_VMESS_HTTP
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_VMESS_HTTP);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_VMESS_HTTP);
                RaisePropertyChanged("Checked_VMESS_HTTP");
            }
        }

        // ==================== Trojan（青绿） ====================

        public bool Checked_Trojan
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_Trojan);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_Trojan);
                RaisePropertyChanged("Checked_Trojan");
            }
        }

        // ==================== Shadowsocks（紫色） ====================

        public bool Checked_Shadowsocks
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_Shadowsocks);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_Shadowsocks);
                RaisePropertyChanged("Checked_Shadowsocks");
            }
        }

        public bool Checked_Shadowsocks_2022
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_Shadowsocks_2022);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_Shadowsocks_2022);
                RaisePropertyChanged("Checked_Shadowsocks_2022");
            }
        }

        // ==================== WireGuard（橙色） ====================

        public bool Checked_WireGuard
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_WireGuard);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_WireGuard);
                RaisePropertyChanged("Checked_WireGuard");
            }
        }

        // ==================== Hysteria（红色） ====================

        public bool Checked_Hysteria
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_Hysteria);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_Hysteria);
                RaisePropertyChanged("Checked_Hysteria");
            }
        }

        public bool Checked_Hysteria2
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_Hysteria2);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_Hysteria2);
                RaisePropertyChanged("Checked_Hysteria2");
            }
        }

        // ==================== TUIC（粉色） ====================

        public bool Checked_TUIC
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_TUIC);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_TUIC);
                RaisePropertyChanged("Checked_TUIC");
            }
        }

        // ==================== NaiveProxy（深蓝） ====================

        public bool Checked_NaiveProxy
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_NaiveProxy);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_NaiveProxy);
                RaisePropertyChanged("Checked_NaiveProxy");
            }
        }

        // ==================== ShadowTLS（灰色） ====================

        public bool Checked_ShadowTLS
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_ShadowTLS);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_ShadowTLS);
                RaisePropertyChanged("Checked_ShadowTLS");
            }
        }

        // ==================== SOCKS / HTTP（褐色） ====================

        public bool Checked_SOCKS
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_SOCKS);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_SOCKS);
                RaisePropertyChanged("Checked_SOCKS");
            }
        }

        public bool Checked_HTTP
        {
            get => Settings.Types.Contains(SingBoxType.SingBox_HTTP);
            set
            {
                CheckBoxChanged(value, SingBoxType.SingBox_HTTP);
                RaisePropertyChanged("Checked_HTTP");
            }
        }
    }
}
