﻿using MvvmCross.ViewModels;
using ProxySuper.Core.Models.Projects;

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

        public bool Checked_VLESS_gRPC
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_gRPC);
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
