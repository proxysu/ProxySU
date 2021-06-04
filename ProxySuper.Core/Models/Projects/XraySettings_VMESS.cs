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
        /// vmess websocket path
        /// </summary>
        public string VMESS_WS_Path { get; set; }

        /// <summary>
        /// vmess ws sharelink
        /// </summary>
        public string VMESS_WS_ShareLink
        {
            get
            {
                return ShareLink.Build(XrayType.VMESS_WS, this);
            }
        }

        /// <summary>
        /// mvess tcp path
        /// </summary>
        public string VMESS_TCP_Path { get; set; }

        /// <summary>
        /// vmess tcp ShareLink
        /// </summary>
        public string VMESS_TCP_ShareLink
        {
            get
            {
                return ShareLink.Build(XrayType.VMESS_TCP, this);
            }
        }

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

        /// <summary>
        /// vmess kcp ShareLink
        /// </summary>
        public string VMESS_KCP_ShareLink
        {
            get
            {
                return ShareLink.Build(XrayType.VMESS_KCP, this);
            }
        }
    }
}
