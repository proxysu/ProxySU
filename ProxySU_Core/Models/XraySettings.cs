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
            VLESS_TCP_Path = "/vlesstcp";
            VMESS_WS_Path = "/vmessws";
            VMESS_TCP_Path = "/vmesstcp";
            TrojanPassword = guid;
        }

        /// <summary>
        /// 访问端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// UUID
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// vless ws路径
        /// </summary>
        public string VLESS_WS_Path { get; set; }

        /// <summary>
        /// vless tcp路径
        /// </summary>
        public string VLESS_TCP_Path { get; set; }

        /// <summary>
        /// vless http2 path
        /// </summary>
        public string VLESS_H2_Path { get; set; }

        /// <summary>
        /// vless mKcp seed
        /// </summary>
        public string VLESS_mKCP_Seed { get; set; }

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
        public string VMESS_HTTP2_Path { get; set; }

        /// <summary>
        /// vmess mKcp seed
        /// </summary>
        public string VMESS_mKCP_Seed { get; set; }

        /// <summary>
        /// trojan密码
        /// </summary>
        public string TrojanPassword { get; set; }

        /// <summary>
        /// trojan ws path
        /// </summary>
        public string Trojan_WS_Path { get; set; }

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
                case XrayType.VLESS_TCP_TLS:
                    return VLESS_TCP_Path;
                case XrayType.VLESS_TCP_XTLS:
                    return VLESS_TCP_Path;
                case XrayType.VLESS_WS_TLS:
                    return VLESS_WS_Path;
                case XrayType.VLESS_H2_TLS:
                    return VLESS_H2_Path;

                case XrayType.VMESS_TCP_TLS:
                    return VMESS_TCP_Path;
                case XrayType.VMESS_WS_TLS:
                    return VMESS_WS_Path;
                case XrayType.Trojan_WS_TLS:
                    return Trojan_WS_Path;

                // no path
                case XrayType.VLESS_mKCP_Speed:
                case XrayType.Trojan_TCP_TLS:
                case XrayType.VMESS_mKCP_Speed:
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
        VLESS_TCP_TLS = 101,
        VLESS_WS_TLS = 102,
        VLESS_H2_TLS = 103,
        VLESS_mKCP_Speed = 104,

        // vmess 201开头
        VMESS_TCP_TLS = 201,
        VMESS_WS_TLS = 202,
        VMESS_H2_TLS = 203,
        VMESS_mKCP_Speed = 204,

        // trojan 301开头
        Trojan_TCP_TLS = 301,
        Trojan_WS_TLS = 302,
    }
}
