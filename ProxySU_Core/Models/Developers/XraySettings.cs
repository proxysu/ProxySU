using Newtonsoft.Json;
using ProxySU_Core.Common;
using ProxySU_Core.Models.Developers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProxySU_Core.Models
{
    public class XraySettings : IParameters
    {

        public XraySettings()
        {
            var guid = Guid.NewGuid().ToString();
            Port = 443;
            VLESS_KCP_Port = 2001;
            VLESS_gRPC_Port = 2002;
            VMESS_KCP_Port = 3001;
            ShadowSocksPort = 4001;

            UUID = guid;
            Types = new List<XrayType>();

            VLESS_WS_Path = "/vlessws";
            VLESS_H2_Path = "/vlessh2";
            VLESS_KCP_Type = "none";
            VLESS_KCP_Seed = guid;
            VLESS_gRPC_ServiceName = "xray_gRPC";

            VMESS_WS_Path = "/vmessws";
            VMESS_TCP_Path = "/vmesstcp";
            VMESS_H2_Path = "/vmessh2";
            VMESS_KCP_Seed = guid;
            VMESS_KCP_Type = "none";

            TrojanPassword = guid;
            Trojan_WS_Path = "/trojanws";

            ShadowsocksPassword = guid;
            ShadowsocksMethod = "aes-128-gcm";
        }

        /// <summary>
        /// 访问端口
        /// </summary>
        public int Port { get; set; }


        /// <summary>
        /// UUID
        /// </summary>
        public string UUID { get; set; }

        #region vless


        /// <summary>
        /// vless ws路径
        /// </summary>
        public string VLESS_WS_Path { get; set; }

        /// <summary>
        /// vless http2 path
        /// </summary>
        public string VLESS_H2_Path { get; set; }

        /// <summary>
        /// vless kcp seed
        /// </summary>
        public string VLESS_KCP_Seed { get; set; }

        /// <summary>
        /// vless kcp type
        /// </summary>
        public string VLESS_KCP_Type { get; set; }

        /// <summary>
        /// vless kcp端口
        /// </summary>
        public int VLESS_KCP_Port { get; set; }

        /// <summary>
        /// grpc service name
        /// </summary>
        public string VLESS_gRPC_ServiceName { get; set; }

        /// <summary>
        /// grpc port
        /// </summary>
        public int VLESS_gRPC_Port { get; set; }

        #endregion

        #region vmess
        /// <summary>
        /// vmess ws路径
        /// </summary>
        public string VMESS_WS_Path { get; set; }

        /// <summary>
        /// vmess tcp路径
        /// </summary>
        public string VMESS_TCP_Path { get; set; }

        /// <summary>
        /// vmess http2 path
        /// </summary>
        public string VMESS_H2_Path { get; set; }

        /// <summary>
        /// vmess kcp seed
        /// </summary>
        public string VMESS_KCP_Seed { get; set; }

        /// <summary>
        /// vmess kcp type
        /// </summary>
        public string VMESS_KCP_Type { get; set; }

        /// <summary>
        /// vmess kcp端口
        /// </summary>
        public int VMESS_KCP_Port { get; set; }
        #endregion

        #region Trojan
        /// <summary>
        /// trojan密码
        /// </summary>
        public string TrojanPassword { get; set; }

        /// <summary>
        /// trojan ws path
        /// </summary>
        public string Trojan_WS_Path { get; set; }
        #endregion

        #region ShadowsocksAEAD
        /// <summary>
        /// ss password
        /// </summary>
        public string ShadowsocksPassword { get; set; }

        /// <summary>
        /// ss method
        /// </summary>
        public string ShadowsocksMethod { get; set; }

        /// <summary>
        /// ss端口
        /// </summary>
        public int ShadowSocksPort { get; set; }
        #endregion


        /// <summary>
        /// 域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 伪装域名
        /// </summary>
        public string MaskDomain { get; set; }

        /// <summary>
        /// 安装类型
        /// </summary>
        public List<XrayType> Types { get; set; }

        public List<int> FreePorts
        {
            get
            {
                return new List<int>
                {
                    VLESS_gRPC_Port,
                    VLESS_KCP_Port,
                    VMESS_KCP_Port,
                    ShadowSocksPort,
                };
            }
        }

        public string GetPath(XrayType type)
        {
            switch (type)
            {
                case XrayType.VLESS_WS:
                    return VLESS_WS_Path;
                case XrayType.VLESS_H2:
                    return VLESS_H2_Path;

                case XrayType.VMESS_TCP:
                    return VMESS_TCP_Path;
                case XrayType.VMESS_WS:
                    return VMESS_WS_Path;
                case XrayType.Trojan_WS:
                    return Trojan_WS_Path;

                // no path
                case XrayType.VLESS_TCP_XTLS:
                case XrayType.VLESS_TCP:
                case XrayType.VLESS_KCP:
                case XrayType.VMESS_KCP:
                case XrayType.Trojan_TCP:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

    }


}
