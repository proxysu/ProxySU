using MvvmCross.ViewModels;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;

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
