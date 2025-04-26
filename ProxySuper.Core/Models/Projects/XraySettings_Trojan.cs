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
                return ShareLink.XrayBuild(RayType.Trojan_TCP, this);
            }
        }
    }
}
