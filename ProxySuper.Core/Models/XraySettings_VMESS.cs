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
        /// vmess websocket path
        /// </summary>
        public string VMESS_WS_Path { get; set; }

        /// <summary>
        /// mvess tcp path
        /// </summary>
        public string VMESS_TCP_Path { get; set; }

        /// <summary>
        /// vmess kcp seed
        /// </summary>
        public string VMESS_KCP_Seed { get; set; }

        /// <summary>
        /// vmess kcp type
        /// </summary>
        public string VMESS_KCP_Type { get; set; }

        /// <summary>
        /// vmess kcp port
        /// </summary>
        public int VMESS_KCP_Port { get; set; }
    }
}
