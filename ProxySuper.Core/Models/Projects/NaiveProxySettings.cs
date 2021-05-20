using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class NaiveProxySettings : IProjectSettings
    {
        public int Port { get; set; }

        public string Domain { get; set; }

        public List<int> FreePorts => throw new NotImplementedException();

        public ProjectType Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
