using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public enum XrayType
    {
        // 入口
        VLESS_TCP_XTLS = 100,

        // VLESS 101开头
        VLESS_TCP = 101,
        VLESS_WS = 102,
        VLESS_H2 = 103,
        VLESS_KCP = 104,
        VLESS_gRPC = 110,

        // VMESS 201开头
        VMESS_TCP = 201,
        VMESS_WS = 202,
        VMESS_H2 = 203,
        VMESS_KCP = 204,

        // Trojan 301开头
        Trojan_TCP = 301,
        Trojan_WS = 302,

        // SS
        ShadowsocksAEAD = 401
    }
}
