using System;
using System.Collections.Generic;
using System.Text;

namespace ProxySU_Core.Models
{
    public class LocalProxy
    {
        public string Address { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 1080;

        public LocalProxyType Type { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

    }
}
