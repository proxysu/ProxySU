using MvvmCross.Commands;
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
    public class TrojanGoInstallViewModel : MvxViewModel<Record>
    {
        Host _host;

        TrojanGoSettings _settings;

        TrojanGoService _trojanGoService;

        public override void Prepare(Record parameter)
        {
            _host = parameter.Host;
            _settings = parameter.TrojanGoSettings;
        }

        public override Task Initialize()
        {
            _trojanGoService = new TrojanGoService(_host, _settings);
            _trojanGoService.Progress.StepUpdate = () => RaisePropertyChanged("Progress");
            _trojanGoService.Progress.LogsUpdate = () => RaisePropertyChanged("Logs");
            _trojanGoService.Connect();
            return base.Initialize();
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            _trojanGoService.Disconnect();
            base.ViewDestroy(viewFinishing);
        }

        public ProjectProgress Progress
        {
            get => _trojanGoService.Progress;
        }

        public string Logs
        {
            get => _trojanGoService.Progress.Logs;
        }


        #region Command

        public IMvxCommand InstallCommand => new MvxCommand(_trojanGoService.Install);

        public IMvxCommand UpdateSettingsCommand => new MvxCommand(_trojanGoService.UpdateSettings);

        public IMvxCommand UninstallCommand => new MvxCommand(_trojanGoService.Uninstall);

        public IMvxCommand UploadCertCommand => new MvxCommand(_trojanGoService.UploadCert);

        public IMvxCommand UploadWebCommand => new MvxCommand(_trojanGoService.UploadWeb);

        public IMvxCommand ApplyForCertCommand => new MvxCommand(_trojanGoService.ApplyForCert);

        #endregion
    }
}
