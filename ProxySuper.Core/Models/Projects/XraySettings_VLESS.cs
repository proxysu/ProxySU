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
        /// vless xtls shareLink
        /// </summary>
        public string VLESS_TCP_XTLS_ShareLink
        {
            get
            {
                return ShareLink.Build(XrayType.VLESS_TCP_XTLS, this);
            }
        }

        /// <summary>
        /// vless tcp shareLink
        /// </summary>
        public string VLESS_TCP_ShareLink
        {
            get
            {
                return ShareLink.Build(XrayType.VLESS_TCP, this);
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
                return ShareLink.Build(XrayType.VLESS_WS, this);
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
                return ShareLink.Build(XrayType.VLESS_KCP, this);
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
                return ShareLink.Build(XrayType.VLESS_gRPC, this);
            }
        }
    }
}
