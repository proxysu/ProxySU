using ProxySuper.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace ProxySuper.Core.Models.Projects
{
    public partial class V2raySettings
    {
        /// <summary>
        /// vless tcp shareLink
        /// </summary>
        public string VLESS_TCP_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_TCP, this);
            }
        }

        /// <summary>
        /// websocket path
        /// </summary>
        public string VLESS_WS_Path { get; set; }

        /// <summary>
        /// VLESS WS ShareLink
        /// </summary>
        public string VLESS_WS_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_WS, this);
            }
        }

        /// <summary>
        /// kcp seed
        /// </summary>
        public string VLESS_KCP_Seed { get; set; }

        /// <summary>
        /// kcp type
        /// </summary>
        public string VLESS_KCP_Type { get; set; }

        /// <summary>
        /// kcp port
        /// </summary>
        public int VLESS_KCP_Port { get; set; }

        /// <summary>
        /// VLESS KCP ShareLink
        /// </summary>
        public string VLESS_KCP_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_KCP, this);
            }
        }

        /// <summary>
        /// vless quic security
        /// </summary>
        public string VLESS_QUIC_Security { get; set; }

        /// <summary>
        /// vless quic type
        /// </summary>
        public string VLESS_QUIC_Type { get; set; }

        /// <summary>
        /// vless quic port
        /// </summary>
        public int VLESS_QUIC_Port { get; set; }

        /// <summary>
        /// vless quic key
        /// </summary>
        public string VLESS_QUIC_Key { get; set; }

        /// <summary>
        /// vless quic ShareLink
        /// </summary>
        public string VLESS_QUIC_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_QUIC, this);
            }
        }

        /// <summary>
        /// grpc port
        /// </summary>
        public int VLESS_gRPC_Port { get; set; }

        /// <summary>
        /// grpc service name
        /// </summary>
        public string VLESS_gRPC_ServiceName { get; set; }

        /// <summary>
        /// vless grpc share link
        /// </summary>
        public string VLESS_gRPC_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_gRPC, this);
            }
        }
    }
}
