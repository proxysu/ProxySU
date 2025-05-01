using ProxySuper.Core.Services;

namespace ProxySuper.Core.Models.Projects
{
    public partial class V2raySettings
    {
        public string TrojanPassword { get; set; }

        public string Trojan_TCP_ShareLink
        {
            get
            {
                return ShareLink.Build(V2RayType.Trojan_TCP, this);
            }
        }
    }
}
