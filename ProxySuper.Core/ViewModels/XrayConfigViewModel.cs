using MvvmCross.ViewModels;
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

        public string Flow
        {
            get { return Settings.Flow; }
        }

        public string UTLS
        {
            get { return Settings.UTLS; }
        }

        public bool Checked_VLESS_XTLS_RAW_REALITY
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_XTLS_RAW_REALITY);
            }
        }


        public bool Checked_VLESS_RAW_XTLS
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_RAW_XTLS);
            }
        }


        public bool Checked_VLESS_XHTTP
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_XHTTP);
            }
        }

        public bool Checked_VLESS_WS
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_WS);
            }
        }

        public bool Checked_VLESS_gRPC
        {
            get
            {
                return Settings.Types.Contains(XrayType.VLESS_gRPC);
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
