using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.ViewModels;

namespace ProxySuper.WPF.Views
{
    [MvxWindowPresentation]
    public partial class TrojanGoConfigView : MvxWindow
    {
        public TrojanGoConfigView()
        {
            InitializeComponent();
        }

        public new TrojanGoConfigViewModel ViewModel
        {
            get
            {
                return (TrojanGoConfigViewModel)base.ViewModel;
            }
        }
    }
}
