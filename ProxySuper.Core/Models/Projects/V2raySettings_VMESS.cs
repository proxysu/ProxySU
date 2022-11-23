using ProxySuper.Core.Services;

namespace ProxySuper.Core.Models.Projects
{
    public partial class V2raySettings
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
                return ShareLink.Build(RayType.VMESS_WS, this);
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
                return ShareLink.Build(RayType.VMESS_TCP, this);
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
                return ShareLink.Build(RayType.VMESS_KCP, this);
            }
        }

        /// <summary>
        /// vmess quic security
        /// </summary>
        public string VMESS_QUIC_Security { get; set; }

        /// <summary>
        /// vmess quic type
        /// </summary>
        public string VMESS_QUIC_Type { get; set; }

        /// <summary>
        /// vmess quic port
        /// </summary>
        public int VMESS_QUIC_Port { get; set; }

        /// <summary>
        /// vmess quic key
        /// </summary>
        public string VMESS_QUIC_Key { get; set; }

        /// <summary>
        /// vmess quic ShareLink
        /// </summary>
        public string VMESS_QUIC_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VMESS_QUIC, this);
            }
        }
    }
}
