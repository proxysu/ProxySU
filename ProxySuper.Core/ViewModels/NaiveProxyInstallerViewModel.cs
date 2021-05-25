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
    public class NaiveProxyInstallerViewModel : MvxViewModel<Record>
    {
        public Host Host { get; set; }

        public NaiveProxySettings Settings { get; set; }

        public override void Prepare(Record parameter)
        {
            var record = Utils.DeepClone(parameter);
            Host = record.Host;
            Settings = record.NaiveProxySettings;
        }

        private bool _connected;
        public bool Connected
        {
            get
            {
                return _connected;
            }
            set
            {
                _connected = value;
                RaisePropertyChanged("Connected");
            }
        }

        public string CommandText { get; set; }

    }
}
