using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.ViewModels;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// TrojanGoConfigView.xaml 的交互逻辑
    /// </summary>
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
