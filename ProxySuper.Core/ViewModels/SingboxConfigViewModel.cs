using MvvmCross.ViewModels;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;

namespace ProxySuper.Core.ViewModels
{
    public class SingboxConfigViewModel : MvxViewModel<SingboxSettings>
    {
        public SingboxSettings Settings { get; set; }

        public override void Prepare(SingboxSettings parameter)
        {
            Settings = parameter;
        }
    }
}
