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
    public class XrayInstallViewModel : MvxViewModel<Record>
    {
        Host _host;

        XraySettings _settings;

        XrayService _xrayService;

        public override void ViewDestroy(bool viewFinishing = true)
        {
            _xrayService.Disconnect();
            this.SaveInstallLog();
            base.ViewDestroy(viewFinishing);
        }

        public override void Prepare(Record parameter)
        {
            this._host = parameter.Host;
            this._settings = parameter.XraySettings;
        }

        public override Task Initialize()
        {
            _xrayService = new XrayService(_host, _settings);
            _xrayService.Progress.StepUpdate = () => RaisePropertyChanged("Progress");
            _xrayService.Progress.LogsUpdate = () => RaisePropertyChanged("Logs");
            _xrayService.Connect();

            return base.Initialize();
        }

        public ProjectProgress Progress
        {
            get => _xrayService.Progress;
        }

        public string Logs
        {
            get => _xrayService.Progress.Logs;
        }



        #region Command

        public IMvxCommand InstallCommand => new MvxCommand(_xrayService.Install);

        public IMvxCommand UpdateSettingsCommand => new MvxCommand(_xrayService.UpdateSettings);

        public IMvxCommand UpdateXrayCoreCommand => new MvxCommand(_xrayService.UpdateXrayCore);

        public IMvxCommand UninstallCommand => new MvxCommand(_xrayService.Uninstall);

        public IMvxCommand UploadCertCommand => new MvxCommand(_xrayService.UploadCert);

        public IMvxCommand UploadWebCommand => new MvxCommand(_xrayService.UploadWeb);

        public IMvxCommand ApplyForCertCommand => new MvxCommand(_xrayService.ApplyForCert);

        #endregion

        private void SaveInstallLog()
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            var fileName = Path.Combine("Logs", DateTime.Now.ToString("yyyy-MM-dd hh-mm") + ".xary.txt");
            File.WriteAllText(fileName, Logs);
        }
    }
}
