using ProxySU_Core.ViewModels.Developers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySU_Core.Models
{
    public class XraySettings : IParameters
    {

        public XraySettings()
        {
            var guid = Guid.NewGuid().ToString();
            Port = 443;
            UUID = guid;
            Types = new List<XrayType> { XrayType.VLESS_TCP_XTLS };
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
        /// vmess ws路径
        /// </summary>
        public string VMESS_WS_Path { get; set; }

        /// <summary>
        /// vmess tcp路径
        /// </summary>
        public string VMESS_TCP_Path { get; set; }

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
                case XrayType.VMESS_TCP_TLS:
                    return VMESS_TCP_Path;
                case XrayType.VMESS_WS_TLS:
                    return VMESS_WS_Path;
                case XrayType.Trojan_TCP_TLS:
                    return string.Empty;
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

        Trojan_TCP_TLS,
    }
}
