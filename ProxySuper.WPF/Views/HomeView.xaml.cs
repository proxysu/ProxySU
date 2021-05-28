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
