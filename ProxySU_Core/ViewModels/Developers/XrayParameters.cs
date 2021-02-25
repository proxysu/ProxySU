using System;
using System.Collections.Generic;
using System.Text;

namespace ProxySU_Core.ViewModels.Developers
{
    public class XrayParameters : IParameters
    {
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
        public string VlessWsPath { get; set; }

        /// <summary>
        /// vless tcp路径
        /// </summary>
        public string VlessTcpPath { get; set; }
        
        /// <summary>
        /// vless http路径
        /// </summary>
        public string VlessHttpPath { get; set; }

        /// <summary>
        /// vmess ws路径
        /// </summary>
        public string VmessWsPath { get; set; }

        /// <summary>
        /// vmess tcp路径
        /// </summary>
        public string VmessTcpPath { get; set; }

        /// <summary>
        /// vmess http路径
        /// </summary>
        public string VmessHttpPath { get; set; }

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
        public XrayType Type { get; set; }
    }

    public enum XrayType
    {
        Shadowsocks_AEAD,
        Shadowsocks_TCP,
        Sockets5_TLS,
        Trojan_TCP_TLS,

        // vless
        VLESS_H2C_Caddy2,
        VLESS_TCP_TLS_WS,
        VLESS_TCP_XTLS_WHATEVER,
        VLESS_mKCPSeed,

        // vmess
        VMess_HTTP2,
        VMess_TCP_TLS,
        VMess_WebSocket_TLS,
        VMess_mKCPSeed,
    }
}
