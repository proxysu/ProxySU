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
        /// websocket path
        /// </summary>
        public string VLESS_WS_Path { get; set; }

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
        /// grpc port
        /// </summary>
        public int VLESS_gRPC_Port { get; set; }

        /// <summary>
        /// grpc service name
        /// </summary>
        public string VLESS_gRPC_ServiceName { get; set; }
    }
}
