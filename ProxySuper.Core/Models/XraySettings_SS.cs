using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models
{
    public partial class XraySettings
    {
        /// <summary>
        /// ss password
        /// </summary>
        public string ShadowsocksPassword { get; set; }

        /// <summary>
        /// ss method
        /// </summary>
        public string ShadowsocksMethod { get; set; }

        /// <summary>
        /// ss port
        /// </summary>
        public int ShadowsocksPort { get; set; }
    }
}
