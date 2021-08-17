using MvvmCross.ViewModels;
using ProxySuper.Core.ViewModels;

namespace ProxySuper.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<HomeViewModel>();
        }
    }
}
