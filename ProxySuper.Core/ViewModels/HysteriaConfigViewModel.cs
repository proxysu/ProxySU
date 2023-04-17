using MvvmCross.ViewModels;
using Newtonsoft.Json;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class HysteriaConfigViewModel : MvxViewModel<HysteriaSettings>
    {
        public HysteriaSettings Settings { get; set; }

        public override void Prepare(HysteriaSettings parameter)
        {
            Settings = parameter;
        }

        public string ClientJson { 
        
            get
            {
                var jsonData = new  
                {
                    server = $"{Settings.Domain}:{Settings.Port}",
                    obfs = Settings.Obfs,
                    up_mbps = 10,
                    down_mbps = 50,
                    socks5 = new
                    {
                        listen = "127.0.0.1:1080"
                    },
                    http = new
                    {
                        listen = "127.0.0.1:1081"
                    }
                };

                return JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            }
        }
    }
}
