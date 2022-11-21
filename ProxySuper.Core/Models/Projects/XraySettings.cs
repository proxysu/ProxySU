using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class XraySettings : V2raySettings
    {
        public static List<string> UTLSList = new List<string> { "", "chrome", "firefox", "safari", "randomized" };

        public string UTLS { get; set; } = UTLSList[1];

        /// <summary>
        /// vless xtls shareLink
        /// </summary>
        public string VLESS_TCP_XTLS_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_TCP_XTLS, this);
            }
        }
    }
}
