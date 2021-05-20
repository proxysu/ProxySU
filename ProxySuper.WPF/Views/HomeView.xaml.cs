using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.Models;
using ProxySuper.Core.ViewModels;
using System;
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

        private IMvxNavigationService _navigationService;

        public IMvxNavigationService NavigationService
        {
            get
            {
                if (_navigationService == null)
                {
                    _navigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
                }
                return _navigationService;
            }

        }

        public new HomeViewModel ViewModel
        {
            get { return (HomeViewModel)base.ViewModel; }
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU");
        }

        private void NavToEditor(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate<XrayEditorViewModel, Record, Record>(ViewModel.Records[0]);
        }

        protected override void Dispose(bool disposing)
        {
            ViewModel.SaveRecords();
            base.Dispose(disposing);
        }

    }
}
