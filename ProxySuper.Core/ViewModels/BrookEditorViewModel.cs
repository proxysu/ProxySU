using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class BrookEditorViewModel : MvxViewModel<Record, Record>
    {
        public string Id { get; set; }

        public Host Host { get; set; }

        public BrookSettings Settings { get; set; }

        public List<string> BrookTypes
        {
            get
            {
                return new List<string> {
                    BrookType.server.ToString(),
                    BrookType.wsserver.ToString(),
                    BrookType.wssserver.ToString(),
                };
            }
        }

        public string CheckedBrookType
        {
            get
            {
                return Settings.BrookType.ToString();
            }
            set
            {
                Settings.BrookType = (BrookType)Enum.Parse(typeof(BrookType), value);
                RaisePropertyChanged("EnablePort");
                RaisePropertyChanged("EnableDomain");
            }
        }

        public bool EnablePort => Settings.BrookType != BrookType.wssserver;

        public bool EnableDomain => Settings.BrookType == BrookType.wssserver;

        public IMvxCommand SaveCommand => new MvxCommand(() => Save());

        public IMvxNavigationService NavigationService { get; }

        public override void Prepare(Record parameter)
        {
            var record = Utils.DeepClone(parameter);
            Id = record.Id;
            Host = record.Host;
            Settings = record.BrookSettings;
        }

        public void Save()
        {
            NavigationService.Close(this, new Record()
            {
                Id = Id,
                Host = Host,
                BrookSettings = Settings,
            });
        }
    }
}
