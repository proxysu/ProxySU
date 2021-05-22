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

        [JsonIgnore]
        public IMvxNavigationService _navigationService;

        [JsonIgnore]
        public IMvxNavigationService NavigationService
        {
            get
            {
                if (_navigationService == null)
                {
                    _navigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
                }
                return _navigationService;
            }
        }


        [JsonIgnore]
        public IMvxCommand NavToInstallerCommand => new MvxAsyncCommand(NavigateToInstaller);

        [JsonIgnore]
        public IMvxCommand NavToEditorCommand => new MvxAsyncCommand(NavigateToEditor);

        [JsonIgnore]
        public IMvxCommand NavToConfigCommand => new MvxAsyncCommand(NavigateToConfig);

        public async Task NavigateToEditor()
        {
            if (Type == ProjectType.Xray)
            {
                var result = await NavigationService.Navigate<XrayEditorViewModel, Record, Record>(this);
                if (result == null) return;

                this.Host = result.Host;
                this.XraySettings = result.XraySettings;

                await RaisePropertyChanged("Host");
            }

            if (Type == ProjectType.TrojanGo)
            {
                var result = await NavigationService.Navigate<TrojanGoEditorViewModel, Record, Record>(this);
                if (result == null) return;

                this.Host = result.Host;
                this.TrojanGoSettings = result.TrojanGoSettings;
            }
        }

        public async Task NavigateToInstaller()
        {
            if (Type == ProjectType.Xray)
            {
                await NavigationService.Navigate<XrayInstallerViewModel, Record>(this);
            }
            if (Type == ProjectType.TrojanGo)
            {
                await NavigationService.Navigate<TrojanGoInstallerViewModel, Record>(this);
            }
        }

        public async Task NavigateToConfig()
        {
            if (Type == ProjectType.Xray)
            {
                await NavigationService.Navigate<XrayConfigViewModel, XraySettings>(this.XraySettings);
            }
            if (Type == ProjectType.TrojanGo)
            {
                await NavigationService.Navigate<TrojanGoConfigViewModel, TrojanGoSettings>(this.TrojanGoSettings);
            }
        }
    }
}
