using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public partial class XraySettings
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

        public string ShadowSocksShareLink
        {
            get
            {
                return ShareLink.Build(XrayType.ShadowsocksAEAD, this);
            }
        }
    }
}
