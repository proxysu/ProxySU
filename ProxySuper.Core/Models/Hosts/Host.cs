using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Hosts
{
    public class Host
    {

        public Host()
        {
            Proxy = new LocalProxy();
        }


        public string Tag { get; set; }

        public string Address { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Port { get; set; } = 22;

        public string PrivateKeyPath { get; set; }

        public LocalProxy Proxy { get; set; }

        public LoginSecretType SecretType { get; set; }
    }
}
