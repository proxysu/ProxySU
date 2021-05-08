using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySU_Core.Models.Developers
{
    public class TrojanGoSettings : IParameters
    {
        public int Port { get; set; }

        public List<int> FreePorts
        {
            get
            {
                return new List<int>();
            }
        }

        public string Domain { get; set; }

        public List<XrayType> Types { get; set; }

        public string Password { get; set; }

        public string MaskDomain { get; set; }
    }
}
