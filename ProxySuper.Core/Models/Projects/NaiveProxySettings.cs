using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class NaiveProxySettings : IProjectSettings
    {
        public NaiveProxySettings()
        {
            Port = 443;
        }

        public List<int> FreePorts => new List<int>();

        public ProjectType Type { get; set; } = ProjectType.NaiveProxy;

        public int Port { get; set; }

        public string Domain { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string MaskDomain { get; set; }
    }
}
