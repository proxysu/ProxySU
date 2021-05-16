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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models
{
    public class Record : MvxViewModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("host")]
        public Host Host { get; set; }

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


        public IMvxCommand NavToEditorCommand => new MvxAsyncCommand(NavigateToEditor);

        public async Task NavigateToEditor()
        {
            var nav = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            if (Type == ProjectType.Xray)
            {
                var result = await nav.Navigate<XrayEditorViewModel, Record, Record>(this);
                if (result == null) return;

                this.Host = result.Host;
                this.XraySettings = result.XraySettings;

                await RaisePropertyChanged("Host");
            }
        }
    }
}
