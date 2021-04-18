using System;
using System.Collections.Generic;
using System.Text;

namespace ProxySU_Core.Models.Developers
{
    public interface IParameters
    {
        int Port { get; set; }

        int VLESS_gRPC_Port { get; set; }

        int VLESS_KCP_Port { get; set; }

        int VMESS_KCP_Port { get; set; }

        int ShadowSocksPort { get; set; }

        string Domain { get; set; }

        List<XrayType> Types { get; set; }
    }
}
