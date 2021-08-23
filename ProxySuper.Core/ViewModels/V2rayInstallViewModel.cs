using MvvmCross.Commands;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class V2rayInstallViewModel : MvxViewModel<Record>
    {
        Host _host;

        V2raySettings _settings;

        V2rayService _service;

        public override void ViewDestroy(bool viewFinishing = true)
        {
            _service.Disconnect();
            this.SaveInstallLog();
            base.ViewDestroy(viewFinishing);
        }

        public override void Prepare(Record parameter)
        {
            this._host = parameter.Host;
            this._settings = parameter.V2raySettings;
        }

        public override Task Initialize()
        {
            _service = new V2rayService(_host, _settings);
            _service.Progress.StepUpdate = () => RaisePropertyChanged("Progress");
            _service.Progress.LogsUpdate = () => RaisePropertyChanged("Logs");
            _service.Connect();

            return base.Initialize();
        }

        public ProjectProgress Progress
        {
            get => _service.Progress;
        }

        public string Logs
        {
            get => _service.Progress.Logs;
        }



        #region Command

        public IMvxCommand InstallCommand => new MvxCommand(_service.Install);

        public IMvxCommand UpdateSettingsCommand => new MvxCommand(_service.UpdateSettings);

        public IMvxCommand UpdateV2rayCoreCommand => new MvxCommand(_service.UpdateV2rayCore);

        public IMvxCommand UninstallCommand => new MvxCommand(_service.Uninstall);

        public IMvxCommand UploadCertCommand => new MvxCommand(_service.UploadCert);

        public IMvxCommand UploadWebCommand => new MvxCommand(_service.UploadWeb);

        public IMvxCommand ApplyForCertCommand => new MvxCommand(_service.ApplyForCert);

        #endregion

        private void SaveInstallLog()
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            var fileName = Path.Combine("Logs", DateTime.Now.ToString("yyyy-MM-dd hh-mm") + ".v2ray.txt");
            File.WriteAllText(fileName, Logs);
        }
    }
}
