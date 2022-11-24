using MvvmCross.ViewModels;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class V2rayConfigViewModel : MvxViewModel<V2raySettings>
    {
        public V2raySettings Settings { get; set; }

        public override void Prepare(V2raySettings parameter)
        {
            Settings = parameter;
        }


        public bool Checked_VLESS_TCP
        {
            get
            {
                return Settings.Types.Contains(RayType.VLESS_TCP);
            }
        }

        public bool Checked_VLESS_WS
        {
            get
            {
                return Settings.Types.Contains(RayType.VLESS_WS);
            }
        }

        public bool Checked_VLESS_KCP
        {
            get
            {
                return Settings.Types.Contains(RayType.VLESS_KCP);
            }
        }

        public bool Checked_VLESS_QUIC
        {
            get
            {
                return Settings.Types.Contains(RayType.VLESS_QUIC);
            }
        }

        public bool Checked_VLESS_gRPC
        {
            get
            {
                return Settings.Types.Contains(RayType.VLESS_gRPC);
            }
        }

        public bool Checked_VMESS_TCP
        {
            get
            {
                return Settings.Types.Contains(RayType.VMESS_TCP);
            }
        }

        public bool Checked_VMESS_WS
        {
            get
            {
                return Settings.Types.Contains(RayType.VMESS_WS);
            }
        }

        public bool Checked_VMESS_KCP
        {
            get
            {
                return Settings.Types.Contains(RayType.VMESS_KCP);
            }
        }

        public bool Checked_VMESS_QUIC
        {
            get
            {
                return Settings.Types.Contains(RayType.VMESS_QUIC);
            }
        }

        public bool Checked_Trojan_TCP
        {
            get
            {
                return Settings.Types.Contains(RayType.Trojan_TCP);
            }
        }

        public bool CheckedShadowSocks
        {
            get
            {
                return Settings.Types.Contains(RayType.ShadowsocksAEAD);
            }
        }
    }
}
