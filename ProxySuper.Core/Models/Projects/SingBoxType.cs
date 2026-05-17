namespace ProxySuper.Core.Models.Projects
{
    /// <summary>
    /// SingBox 协议类型
    /// </summary>
    public enum SingBoxType
    {
        // VLESS
        SingBox_VLESS_TCP = 100,
        SingBox_VLESS_WS = 101,
        SingBox_VLESS_HTTP = 102,
        SingBox_VLESS_gRPC = 103,
        SingBox_VLESS_XHTTP_REALITY = 104,

        // VMess
        SingBox_VMESS_TCP = 200,
        SingBox_VMESS_WS = 201,
        SingBox_VMESS_HTTP = 202,

        // Trojan
        SingBox_Trojan = 300,
        SingBox_Trojan_GO = 301,

        // Shadowsocks
        SingBox_Shadowsocks = 400,
        SingBox_Shadowsocks_2022 = 401,

        // WireGuard
        SingBox_WireGuard = 500,

        // Hysteria
        SingBox_Hysteria = 600,
        SingBox_Hysteria2 = 601,

        // TUIC
        SingBox_TUIC = 700,

        // NaiveProxy
        SingBox_NaiveProxy = 800,

        // ShadowTLS
        SingBox_ShadowTLS = 900,

        // Direct/SOCKS
        SingBox_SOCKS = 1000,
        SingBox_HTTP = 1001,
    }
}