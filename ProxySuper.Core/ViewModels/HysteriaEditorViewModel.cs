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
using System.Windows.Navigation;

namespace ProxySuper.Core.ViewModels
{
    public class HysteriaEditorViewModel : MvxViewModel<Record, Record>
    {
        public string Id { get; set; }

        public Host Host { get; set; }

        public HysteriaSettings Settings { get; set; }

        public IMvxNavigationService NavigationService { get; }

        public IMvxCommand SaveCommand => new MvxCommand(() => Save());

        public IMvxCommand SaveAndInstallCommand => new MvxCommand(SaveAndInstall);

        public HysteriaEditorViewModel(IMvxNavigationService mvxNavigationService)
        {
            NavigationService = mvxNavigationService;
        }

        public override void Prepare(Record parameter)
        {
            var record = Utils.DeepClone(parameter);

            Id = record.Id;
            Host = record.Host;
            Settings = record.HysteriaSettings;
        }

        public void Save()
        {
            NavigationService.Close(this, new Record
            {
                Id = Id,
                Host = Host,
                HysteriaSettings = Settings,
            });
        }

        public void SaveAndInstall()
        {
            var record = new Record
            {
                Id = Id,
                Host = Host,
                HysteriaSettings = Settings,
            };

            NavigationService.Close(this, record);
            NavigationService.Navigate<HysteriaInstallViewModel, Record>(record);
        }
    }
}
