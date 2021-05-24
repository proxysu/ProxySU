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
    public class TrojanGoConfigViewModel : MvxViewModel<TrojanGoSettings>
    {
        public TrojanGoSettings Settings { get; set; }

        public override void Prepare(TrojanGoSettings parameter)
        {
            Settings = parameter;
        }

        public string Link
        {
            get
            {
                return ShareLink.BuildTrojanGo(Settings);
            }
        }
    }
}
