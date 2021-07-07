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
    public class BrookConfigViewModel : MvxViewModel<BrookSettings>
    {
        public BrookSettings Settings { get; set; }

        public override void Prepare(BrookSettings parameter)
        {
            Settings = parameter;
        }

        public string Link
        {
            get
            {
                return ShareLink.BuildBrook(Settings);
            }
        }
    }
}
