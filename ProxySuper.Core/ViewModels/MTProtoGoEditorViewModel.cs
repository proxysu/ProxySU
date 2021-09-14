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
    public class MTProtoGoEditorViewModel : MvxViewModel<Record, Record>
    {
        public MTProtoGoEditorViewModel(IMvxNavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public IMvxNavigationService NavigationService { get; }

        public IMvxCommand SaveCommand => new MvxCommand(Save);

        public IMvxCommand SaveAndInstallCommand => new MvxCommand(SaveAndInstall);

        public string Id { get; set; }

        public Host Host { get; set; }

        public MTProtoGoSettings Settings { get; set; }

        public override void Prepare(Record parameter)
        {
            var record = Utils.DeepClone(parameter);

            Id = record.Id;
            Host = record.Host;
            Settings = record.MTProtoGoSettings;
        }

        private void Save()
        {
            NavigationService.Close(this, new Record
            {
                Id = this.Id,
                Host = this.Host,
                MTProtoGoSettings = Settings,
            });
        }

        private void SaveAndInstall()
        {
            var record = new Record
            {
                Id = this.Id,
                Host = this.Host,
                MTProtoGoSettings = Settings,
            };
            NavigationService.Close(this, record);
            NavigationService.Navigate<MTProtoGoInstallViewModel, Record>(record);
        }
    }
}
