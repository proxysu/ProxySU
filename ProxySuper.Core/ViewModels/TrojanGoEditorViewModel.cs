using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;

namespace ProxySuper.Core.ViewModels
{
    public class TrojanGoEditorViewModel : MvxViewModel<Record, Record>
    {
        public TrojanGoEditorViewModel(IMvxNavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public IMvxNavigationService NavigationService { get; }

        public IMvxCommand SaveCommand => new MvxCommand(Save);

        public IMvxCommand SaveAndInstallCommand => new MvxCommand(SaveAndInstall);

        public string Id { get; set; }

        public Host Host { get; set; }

        public TrojanGoSettings Settings { get; set; }

        public override void Prepare(Record parameter)
        {
            var record = Utils.DeepClone(parameter);

            Id = record.Id;
            Host = record.Host;
            Settings = record.TrojanGoSettings;
        }

        private void Save()
        {
            NavigationService.Close(this, new Record
            {
                Id = this.Id,
                Host = this.Host,
                TrojanGoSettings = Settings,
            });
        }

        private void SaveAndInstall()
        {
            var record = new Record
            {
                Id = this.Id,
                Host = this.Host,
                TrojanGoSettings = Settings,
            };
            NavigationService.Close(this, record);
            NavigationService.Navigate<TrojanGoInstallViewModel, Record>(record);
        }
    }


}
