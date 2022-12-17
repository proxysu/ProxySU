using MvvmCross.ViewModels;
using Newtonsoft.Json;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Text;

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

        [JsonProperty("v2raySettings")]
        public V2raySettings V2raySettings { get; set; }

        [JsonProperty("settings")]
        public XraySettings XraySettings { get; set; }

        [JsonProperty("trojanGoSettings")]
        public TrojanGoSettings TrojanGoSettings { get; set; }

        [JsonProperty("naiveProxySettings")]
        public NaiveProxySettings NaiveProxySettings { get; set; }

        [JsonProperty("brook")]
        public BrookSettings BrookSettings { get; set; }

        [JsonProperty("mtProtoGoSettings")]
        public MTProtoGoSettings MTProtoGoSettings { get; set; }

        [JsonProperty]
        public HysteriaSettings HysteriaSettings { get; set; }


        [JsonIgnore]
        public ProjectType Type
        {
            get
            {
                if (XraySettings != null) return ProjectType.Xray;

                if (V2raySettings != null) return ProjectType.V2ray;

                if (TrojanGoSettings != null) return ProjectType.TrojanGo;

                if (NaiveProxySettings != null) return ProjectType.NaiveProxy;

                if (MTProtoGoSettings != null) return ProjectType.MTProtoGo;

                if (HysteriaSettings != null) return ProjectType.Hysteria;

                return ProjectType.Brook;
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

        [JsonIgnore]
        public Action OnSave { get; set; } = () => { };

        public string GetShareLink()
        {
            if (Type == ProjectType.V2ray)
            {
                StringBuilder strBuilder = new StringBuilder();
                V2raySettings.Types.ForEach(type =>
                {
                    var link = ShareLink.Build(type, V2raySettings);
                    strBuilder.AppendLine(link);
                });
                return strBuilder.ToString();
            }

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
