using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class MTProtoGoSettings : IProjectSettings
    {
        public MTProtoGoSettings()
        {
            Port = 443;

            Domain = string.Empty;

            Cleartext = "bing.com";

            SecretText = string.Empty;
        }

        public int Port { get; set; }

        public string Domain { get; set; }

        public List<int> FreePorts => new List<int> { Port };

        public string Email => "";

        public string Cleartext { get; set; }

        public string SecretText { get; set; }
    }
}
