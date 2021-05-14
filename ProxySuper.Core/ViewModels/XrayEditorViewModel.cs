using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class XrayEditorViewModel : MvxViewModel<Record>
    {
        public Host Host { get; set; }

        public XraySettings Settings { get; set; }

        public override void Prepare(Record parameter)
        {
            Host = parameter.Host;
            Settings = parameter.Settings as XraySettings;
        }
    }
}
