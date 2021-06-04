using Microsoft.Win32;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class NaiveProxyEditorViewModel : MvxViewModel<Record, Record>
    {
        public NaiveProxyEditorViewModel(IMvxNavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public IMvxNavigationService NavigationService { get; }

        public string Id { get; set; }

        public Host Host { get; set; }

        public NaiveProxySettings Settings { get; set; }

        public override void Prepare(Record parameter)
        {
            var record = Utils.DeepClone(parameter);

            Id = record.Id;
            Host = record.Host;
            Settings = record.NaiveProxySettings;
        }


        public IMvxCommand SaveCommand => new MvxCommand(Save);

        private void Save()
        {
            NavigationService.Close(this, new Record
            {
                Id = Id,
                Host = Host,
                NaiveProxySettings = Settings
            });
        }
    }
}
