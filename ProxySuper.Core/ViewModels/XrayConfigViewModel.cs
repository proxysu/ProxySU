using Microsoft.Win32;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using QRCoder;

namespace ProxySuper.Core.ViewModels
{
    public class XrayConfigViewModel : MvxViewModel<XraySettings>
    {

        public XraySettings Settings { get; set; }

        public override void Prepare(XraySettings parameter)
        {
            Settings = parameter;
        }

        public bool Checked_VLESS_TCP_XTLS
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_TCP_XTLS);
            }
        }

        public bool Checked_VLESS_TCP
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_TCP);
            }
        }

        public bool Checked_VLESS_WS
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_WS);
            }
        }

        public bool Checked_VLESS_KCP
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_KCP);
            }
        }

        public bool Checked_VMESS_TCP
        {
            get
            {
                return Settings.Types.Contains(XrayType.VMESS_TCP);
            }
        }

        public bool Checked_VMESS_WS
        {
            get
            {
                return Settings.Types.Contains(XrayType.VMESS_WS);
            }
        }

        public bool Checked_VMESS_KCP
        {
            get
            {
                return Settings.Types.Contains(XrayType.VMESS_KCP);
            }
        }

        public bool Checked_Trojan_TCP
        {
            get
            {
                return Settings.Types.Contains(XrayType.Trojan_TCP);
            }
        }

        public bool CheckedShadowSocks
        {
            get
            {
                return Settings.Types.Contains(XrayType.ShadowsocksAEAD);
            }
        }
    }
}
