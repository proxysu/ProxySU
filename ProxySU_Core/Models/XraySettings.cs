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
            UUID = guid;
            Types = new List<XrayType>();

            VLESS_WS_Path = "/vlessws";
            VLESS_H2_Path = "/vlessh2";

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


    public enum XrayType
    {
        // 入口
        VLESS_TCP_XTLS = 100,

        // vless 101开头
        VLESS_TCP = 101,
        VLESS_WS = 102,
        VLESS_H2 = 103,
        VLESS_KCP = 104,

        // vmess 201开头
        VMESS_TCP = 201,
        VMESS_WS = 202,
        VMESS_H2 = 203,
        VMESS_KCP = 204,

        // trojan 301开头
        Trojan_TCP = 301,
        Trojan_WS = 302,

        // ss
        ShadowsocksAEAD = 401
    }
}
