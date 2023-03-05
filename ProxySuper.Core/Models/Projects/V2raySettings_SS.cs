using ProxySuper.Core.Services;

namespace ProxySuper.Core.Models.Projects
{
    public partial class V2raySettings
    {
        /// <summary>
        /// ss password
        /// </summary>
        public string ShadowSocksPassword { get; set; }

        /// <summary>
        /// ss method
        /// </summary>
        public string ShadowSocksMethod { get; set; }

        /// <summary>
        /// ss port
        /// </summary>
        public int ShadowSocksPort { get; set; }

        /// <summary>
        /// share link
        /// </summary>
        public string ShadowSocksShareLink
        {
            get
            {
                return ShareLink.Build(RayType.ShadowsocksAEAD, this);
            }
        }
    }
}
