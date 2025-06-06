using ProxySuper.Core.Services;
using System.Collections.Generic;

namespace ProxySuper.Core.Models.Projects
{
    public partial class XraySettings
    {
        /// <summary>
        /// vmess kcp 数据包头部伪装类型
        /// </summary>
        public static List<string> DisguiseTypes = new List<string> {
            "none",
            "srtp",
            "utp",
            "wechat-video",
            "dtls",
            "wireguard",
            "dns",
        };

        /// <summary>
        /// vmess kcp seed 混淆密码
        /// </summary>
        public string VMESS_KCP_Seed { get; set; }

        /// <summary>
        /// vmess kcp type
        /// </summary>
        public string VMESS_KCP_Type { get; set; }

        /// <summary>
        /// vmess kcp port
        /// </summary>
        public int VMESS_KCP_Port { get; set; }

        /// <summary>
        /// vmess kcp ShareLink
        /// </summary>
        public string VMESS_KCP_ShareLink
        {
            get
            {
                return ShareLink.XrayBuild(XrayType.VMESS_KCP, this);
            }
        }

    }
}
