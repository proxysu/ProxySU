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
    public class XrayInstallViewModel : MvxViewModel<Record>
    {
        Host _host;

        XraySettings _settings;

        XrayService _xrayService;

        MvxInteraction _refreshLogInteraction = new MvxInteraction();

        public override void ViewDestroy(bool viewFinishing = true)
        {
            _xrayService.Disconnect();
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
            _xrayService.Progress.LogsUpdate = () =>
            {
                RaisePropertyChanged("Logs");
                _refreshLogInteraction.Raise();
            };
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

        public IMvxInteraction LogsInteraction
        {
            get => _refreshLogInteraction;
        }

        public IMvxCommand InstallCommand => new MvxCommand(_xrayService.Install);
    }
}
