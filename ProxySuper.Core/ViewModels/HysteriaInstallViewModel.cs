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
    public class HysteriaInstallViewModel : MvxViewModel<Record>
    {
        public Host _host { get; set; }

        public HysteriaSettings _settings { get; set; }

        public HysteriaService _service { get; set; }

        public override void Prepare(Record parameter)
        {
            _host = parameter.Host;
            _settings = parameter.HysteriaSettings;
        }

        public override Task Initialize()
        {
            _service = new HysteriaService(_host, _settings);
            _service.Progress.StepUpdate = () => RaisePropertyChanged("Progress");
            _service.Progress.LogsUpdate = () => RaisePropertyChanged("Logs");
            _service.Connect();
            return base.Initialize();
        }

        public override void ViewDestroy(bool viewFinishing = true)
        {
            _service.Disconnect();
            this.SaveInstallLog();
            base.ViewDestroy(viewFinishing);
        }

        public ProjectProgress Progress => _service.Progress;

        public string Logs => _service.Progress.Logs;

        public IMvxCommand InstallCommand => new MvxCommand(_service.Install);

        public IMvxCommand UninstallCommand => new MvxCommand(_service.Uninstall);


        private void SaveInstallLog()
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            var fileName = System.IO.Path.Combine("Logs", DateTime.Now.ToString("yyyy-MM-dd hh-mm") + ".hysteria.txt");
            File.WriteAllText(fileName, Logs);
        }
    }
}
