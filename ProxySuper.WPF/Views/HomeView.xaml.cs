using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.Models;
using ProxySuper.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private void LaunchUseRootSite(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU/wiki/%E8%B0%B7%E6%AD%8C%E4%BA%91%E5%BC%80%E5%90%AFroot%E8%B4%A6%E6%88%B7%E4%B8%8E%E5%AF%86%E7%A0%81%E7%99%BB%E5%BD%95");
        }

        private void LaunchCertQuestion(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU/wiki/%E5%B8%B8%E8%A7%81%E8%AF%81%E4%B9%A6%E7%94%B3%E8%AF%B7%E5%A4%B1%E8%B4%A5%E9%97%AE%E9%A2%98");
        }

        private void LaunchPrivateKeyQuestion(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU/wiki/PrivateKey%E8%BD%AC%E6%8D%A2");
        }


        ResourceDictionary resource = new ResourceDictionary();
        private void SetSimpleChinese(object sender, RoutedEventArgs e)
        {
            resource.Source = new Uri(@"Resources\Languages\zh_cn.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[0] = resource;
        }

        private void SetEnglish(object sender, RoutedEventArgs e)
        {
            resource.Source = new Uri(@"Resources\Languages\en.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[0] = resource;
        }

        private void SetTwCN(object sender, RoutedEventArgs e)
        {
            resource.Source = new Uri(@"Resources\Languages\tw_cn.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[0] = resource;
        }



        private void ShowShareLinks(object sender, RoutedEventArgs e)
        {
            var checkedRecords = ViewModel.Records.Where(x => x.IsChecked).ToList();
            if (checkedRecords.Count == 0)
            {
                MessageBox.Show("您没有选择任何主机");
                return;
            }

            NavigationService.Navigate<ShareLinkViewModel, List<Record>>(checkedRecords);
        }


    }
}
