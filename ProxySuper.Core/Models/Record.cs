using Microsoft.Win32;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using ProxySuper.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models
{
    [JsonObject]
    public class Record : MvxViewModel
    {
        public Record()
        {
            _isChecked = false;
        }

        private Host _host;
        private bool _isChecked;

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

        [JsonProperty("naiveProxySettings")]
        public NaiveProxySettings NaiveProxySettings { get; set; }


        [JsonIgnore]
        public ProjectType Type
        {
            get
            {
                if (XraySettings != null) return ProjectType.Xray;

                if (TrojanGoSettings != null) return ProjectType.TrojanGo;

                return ProjectType.NaiveProxy;
            }
        }

        [JsonIgnore]
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        public string GetShareLink()
        {
            if (Type == ProjectType.Xray)
            {
                StringBuilder strBuilder = new StringBuilder();
                XraySettings.Types.ForEach(type =>
                {
                    var link = ShareLink.Build(type, XraySettings);
                    strBuilder.AppendLine(link);
                });
                return strBuilder.ToString();
            }

            if (Type == ProjectType.TrojanGo)
            {
                return ShareLink.BuildTrojanGo(TrojanGoSettings);
            }

            if (Type == ProjectType.NaiveProxy)
            {
                return ShareLink.BuildNaiveProxy(NaiveProxySettings);
            }

            return string.Empty;
        }

    }
}
