using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models
{
    public class Record
    {
        public Host Host { get; set; }

        [JsonProperty("settings")]
        public dynamic Settings { get; set; }


        public string Type
        {
            get
            {
                return Settings.type ?? "Xray";
            }
        }
    }
}
