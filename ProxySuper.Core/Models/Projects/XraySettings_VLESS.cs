using ProxySuper.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace ProxySuper.Core.Models.Projects
{
    public partial class XraySettings
    {

        /// <summary>
        /// vless xtls 流控参数可选列表
        /// </summary>

        //流控参数在服务端只有两种 ""/null, "xtls-rprx-vision"，客户端可以选择三种：""/null, "xtls-rprx-vision", "xtls-rprx-vision-udp443".
        //但是选择了XTLS模式就是默认服务端flow不是""/null,所以这里不再填加""/null这一项。
        //如果服务端开启了流控，客户端也必须开启流控，否则会报错。
        //同样如果服务端没有开启流控，客户端也不能开启流控，否则会报错。
        public static List<string> FlowList = new List<string> { "xtls-rprx-vision", "xtls-rprx-vision-udp443" };

        /// <summary>
        /// vless xtls 流控参数
        /// </summary>
        public string Flow { get; set; } = FlowList[0];

        /// <summary>
        /// vless xtls reality  shareLink
        /// </summary>
        public string VLESS_RAW_XTLS_REALITY_ShareLink
        {
            get
            {
                return ShareLink.XrayBuild(XrayType.VLESS_XTLS_RAW_REALITY, this);
            }
        }


        /// <summary>
        /// vless xtls  shareLink
        /// </summary>
        public string VLESS_RAW_XTLS_ShareLink
        {
            get
            {
                return ShareLink.XrayBuild(XrayType.VLESS_RAW_XTLS, this);
            }
        }


        /// <summary>
        /// vless xhttp path
        /// </summary>
        public string VLESS_XHTTP_Path { get; set; }

        /// <summary>
        /// VLESS XHTTP ShareLink
        /// </summary>
        public string VLESS_XHTTP_ShareLink
        {
            get
            {
                return ShareLink.XrayBuild(XrayType.VLESS_XHTTP, this);
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
                return ShareLink.XrayBuild(XrayType.VLESS_WS, this);
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
                return ShareLink.XrayBuild(XrayType.VLESS_gRPC, this);
            }
        }
    }
}
