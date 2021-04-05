using System;
using System.Collections.Generic;
using System.Text;

namespace ProxySU_Core.Models.Developers
{
    public interface IParameters
    {
        int Port { get; set; }

        int KcpPort { get; set; }

        int ShadowSocksPort { get; set; }

        string Domain { get; set; }

        List<XrayType> Types { get; set; }
    }
}
