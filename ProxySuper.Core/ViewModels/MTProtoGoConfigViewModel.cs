using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class MTProtoGoConfigViewModel : MvxViewModel<MTProtoGoSettings>
    {
        public MTProtoGoSettings Settings { get; set; }

        public override void Prepare(MTProtoGoSettings parameter)
        {
            Settings = parameter;
        }
    }
}
