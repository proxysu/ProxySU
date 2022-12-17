using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class HysteriaSettings
    {
        public string Domain { get; set; } = "";

        public string Obfs { get; set; } = "";

        public string Email { get; set; } = "";

        public int Port { get; set; } = 36712;

        public int UpMbps { get; set; } = 300;

        public int DownMbps { get; set; } = 300;
    }
}
