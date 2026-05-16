using ProxySuper.Core.Services;

namespace ProxySuper.Core.Models.Projects
{
    public partial class XraySettings
    {
        #region VLESS XHTTP REALITY Settings

        /// <summary>
        /// VLESS XHTTP REALITY 目标网站（如 www.microsoft.com）
        /// </summary>
        public string XHTTP_Reality_Dest { get; set; } = "www.microsoft.com";

        /// <summary>
        /// VLESS XHTTP REALITY Server Names（用逗号分隔）
        /// </summary>
        public string XHTTP_Reality_ServerNames { get; set; } = "www.microsoft.com";

        /// <summary>
        /// VLESS XHTTP REALITY 私钥（Base64）
        /// </summary>
        public string XHTTP_Reality_PrivateKey { get; set; }

        /// <summary>
        /// VLESS XHTTP REALITY 公钥（Base64，自动从私钥生成）
        /// </summary>
        public string XHTTP_Reality_PublicKey { get; set; }

        /// <summary>
        /// VLESS XHTTP REALITY Short ID（可选）
        /// </summary>
        public string XHTTP_Reality_ShortId { get; set; } = "";

        /// <summary>
        /// VLESS XHTTP 监听端口
        /// </summary>
        public int VLESS_XHTTP_Port { get; set; } = 8443;

        /// <summary>
        /// VLESS XHTTP REALITY 监听端口
        /// </summary>
        public int VLESS_XHTTP_REALITY_Port { get; set; } = 8443;

        /// <summary>
        /// VLESS XHTTP 路径
        /// </summary>
        public string VLESS_XHTTP_Path { get; set; } = "/" + Utils.RandomString(8);

        #endregion

        #region WireGuard Settings

        /// <summary>
        /// WireGuard 监听端口
        /// </summary>
        public int WireGuard_Port { get; set; } = 51820;

        /// <summary>
        /// WireGuard 私钥
        /// </summary>
        public string WireGuard_PrivateKey { get; set; }

        /// <summary>
        /// WireGuard 公钥（自动生成）
        /// </summary>
        public string WireGuard_PublicKey { get; set; }

        /// <summary>
        /// WireGuard IP 地址（如 10.0.0.1/24）
        /// </summary>
        public string WireGuard_IP { get; set; } = "10.0.0.1/32";

        /// <summary>
        /// WireGuard Peer 公钥
        /// </summary>
        public string WireGuard_Peer_PublicKey { get; set; }

        /// <summary>
        /// WireGuard Peer IP 地址
        /// </summary>
        public string WireGuard_Peer_IP { get; set; } = "10.0.0.2/32";

        /// <summary>
        /// WireGuard Peer 端点（如 1.2.3.4:51820）
        /// </summary>
        public string WireGuard_Peer_Endpoint { get; set; }

        /// <summary>
        /// WireGuard MTU
        /// </summary>
        public int WireGuard_MTU { get; set; } = 1280;

        #endregion

        #region Hysteria Settings

        /// <summary>
        /// Hysteria 监听端口
        /// </summary>
        public int Hysteria_Port { get; set; } = 9120;

        /// <summary>
        /// Hysteria 认证密码
        /// </summary>
        public string Hysteria_Auth { get; set; }

        /// <summary>
        /// Hysteria 上行速度（Mbps）
        /// </summary>
        public int Hysteria_Up { get; set; } = 100;

        /// <summary>
        /// Hysteria 下行速度（Mbps）
        /// </summary>
        public int Hysteria_Down { get; set; } = 500;

        /// <summary>
        /// Hysteria 混淆密码（可选）
        /// </summary>
        public string Hysteria_Obfs { get; set; } = "";

        #endregion

        #region ShareLinks

        /// <summary>
        /// VLESS XHTTP Vision ShareLink
        /// </summary>
        public string VLESS_XHTTP_VISION_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_XHTTP_VISION, this);
            }
        }

        /// <summary>
        /// VLESS XHTTP REALITY ShareLink
        /// </summary>
        public string VLESS_XHTTP_REALITY_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_XHTTP_REALITY, this);
            }
        }

        #endregion
    }
}