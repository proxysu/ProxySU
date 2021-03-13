using System;
using System.Collections.Generic;
using System.Text;

namespace ProxySU_Core.ViewModels.Developers
{
    public interface IParameters
    {
        int Port { get; set; }

        string Domain { get; set; }
    }
}
