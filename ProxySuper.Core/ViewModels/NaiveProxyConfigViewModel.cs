using MvvmCross.ViewModels;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class NaiveProxyConfigViewModel : MvxViewModel<NaiveProxySettings>
    {
        public NaiveProxySettings Settings { get; set; }

        public override void Prepare(NaiveProxySettings parameter)
        {
            Settings = parameter;
        }

        public string Link
        {
            get
            {
                return ShareLink.BuildNaiveProxy(Settings);
            }
        }
    }
}
