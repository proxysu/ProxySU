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

        public string Domain
        {
            get => Settings.Domain;
            set
            {
                Settings.Domain = value;
                RaisePropertyChanged("Domain");
            }
        }
       
        public int Port
        {
            get => Settings.Port;
            set
            {
                Settings.Port = value;
                RaisePropertyChanged("Port");
            }
        }

        public string Password
        {
            get => Settings.Password;
            set
            {
                Settings.Password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string ObfsPassword
        {
            get
            {
                if(Settings.EnableObfs == true)
                {
                    return Settings.ObfsPassword;
                }
                return "";
            }//=> Settings.ObfsPassword;
            set
            {
                Settings.ObfsPassword = value;
                RaisePropertyChanged("ObfsPassword");
            }
        }
        
        public string HysteriaShareLink
        {
            get => Settings.HysteriaShareLink;
            
        }


        public string ClientYamlConfig { 
        
            get
            {
                /*
                var jsonData = new  
                {
                    server = $"{Settings.Domain}:{Settings.Port}",
                    obfs = Settings.ObfsPassword,
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
                */
                return Settings.ClientHysteria2Config;//JsonConvert.SerializeObject(jsonData, Formatting.Indented);
            }
        }
    }
}
