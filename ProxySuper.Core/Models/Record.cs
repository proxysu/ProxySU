using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models
{
    [JsonObject]
    public class Record : MvxViewModel
    {
        private Host _host;

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("host")]
        public Host Host
        {
            get { return _host; }
            set
            {
                _host = value;
                RaisePropertyChanged("Host");
            }
        }

        [JsonProperty("settings")]
        public XraySettings XraySettings { get; set; }

        [JsonProperty("trojanGoSettings")]
        public TrojanGoSettings TrojanGoSettings { get; set; }


        [JsonIgnore]
        public ProjectType Type
        {
            get
            {
                if (XraySettings != null) return ProjectType.Xray;
                return ProjectType.TrojanGo;
            }
        }


    }
}
