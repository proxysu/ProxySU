using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.Models;
using ProxySuper.Core.ViewModels;
using System.Windows;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// HomeView.xaml 的交互逻辑
    /// </summary>
    public partial class HomeView : MvxWpfView
    {

        public HomeView()
        {
            InitializeComponent();
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU");
        }

        private void NavToEditor(object sender, RoutedEventArgs e)
        {
            var navSrv = Mvx.IoCProvider.Resolve<IMvxNavigationService>();

            var vm = ViewModel as HomeViewModel;
            navSrv.Navigate<XrayEditorViewModel, Record, Record>(vm.Records[0]);
        }
    }
}
