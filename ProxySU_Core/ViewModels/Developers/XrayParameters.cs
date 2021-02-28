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
        /// vmess ws路径
        /// </summary>
        public string VmessWsPath { get; set; }

        /// <summary>
        /// vmess tcp路径
        /// </summary>
        public string VmessTcpPath { get; set; }

        /// <summary>
        /// trojan tcp路径
        /// </summary>
        public string TrojanTcpPath { get; set; }

        /// <summary>
        /// trojan密码
        /// </summary>
        public string TrojanPassword { get; set; }

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


        public string GetPath()
        {
            switch (Type)
            {
                case XrayType.VLESS_TCP_TLS:
                    return VlessTcpPath;
                case XrayType.VLESS_TCP_XTLS:
                    return VlessTcpPath;
                case XrayType.VLESS_WS_TLS:
                    return VlessWsPath;
                case XrayType.VMESS_TCP_TLS:
                    return VmessTcpPath;
                case XrayType.VMESS_WS_TLS:
                    return VmessWsPath;
                case XrayType.Trojan_TCP_TLS:
                    return TrojanTcpPath;
                default:
                    return string.Empty;
            }
        }
    }

    public enum XrayType
    {
        VLESS_TCP_TLS,
        VLESS_TCP_XTLS,
        VLESS_WS_TLS,

        VMESS_TCP_TLS,
        VMESS_WS_TLS,

        Trojan_TCP_TLS
    }
}
