using ProxySuper.Core.Services;

namespace ProxySuper.Core.Models.Projects
{
    public partial class XraySettings
    {
        public string TrojanPassword { get; set; }

        public string Trojan_TCP_ShareLink
        {
            get
            {
                return ShareLink.Build(XrayType.Trojan_TCP, this);
            }
        }
    }
}
