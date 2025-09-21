using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models;
using ProxySuper.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
using System.Windows.Input;


namespace ProxySuper.WPF.Views
{
    [MvxContentPresentation]
    [MvxViewFor(typeof(HomeViewModel))]
    public partial class HomeView : MvxWpfView
    {

        public HomeView()
        {
            InitializeComponent();
            CheckGitHubUpdateAsync();
            UpdateLabel.MouseLeftButtonUp += UpdateLabel_MouseLeftButtonUp;
            UpdateLabel.Cursor = Cursors.Hand;
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

        private void LaunchResourcesAndTutorials(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU/wiki/%E7%9B%B8%E5%85%B3%E8%B5%84%E6%BA%90%E4%B8%8E%E6%95%99%E7%A8%8B");
        }

        private void LaunchFreeAndPaid(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU/wiki/%E5%85%8D%E8%B4%B9and%E4%BB%98%E8%B4%B9%E7%BF%BB%E5%A2%99%E8%B5%84%E6%BA%90");
        }

        ResourceDictionary resource = new ResourceDictionary();
        private void SetSimpleChinese(object sender, RoutedEventArgs e)
        {
            resource.Source = new Uri(@"Resources\Languages\zh_cn.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[0] = resource;
        }
        private void SetIrFA(object sender, RoutedEventArgs e)
        {
            resource.Source = new Uri(@"Resources\Languages\fa_IR.xaml", UriKind.Relative);
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

        private void GetRoot(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate<EnableRootViewModel>();
        }

        private async Task CheckGitHubUpdateAsync()
        {
            try
            {
                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                using (HttpClient client = new HttpClient())
                {
                    // GitHub API 需要 User-Agent
                    client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");

                    string url = "https://api.github.com/repos/proxysu/ProxySU/releases/latest";

                    string json = await client.GetStringAsync(url);
                    JObject release = JObject.Parse(json);
                    string latestVersion = release["tag_name"]?.ToString().TrimStart('v'); // 去掉 v 前缀

                    if (!string.IsNullOrEmpty(latestVersion))
                    {
                        // 尝试把版本字符串转为 Version 类型
                        if (Version.TryParse(latestVersion, out Version latestVer) &&
                            Version.TryParse(currentVersion, out Version currentVer))
                        {
                            // 只有最新版本号 > 当前版本号，才提示更新
                            if (latestVer > currentVer)
                            {
                                UpdateLabel.Content = $"检测到新版本 {latestVersion} 发布！点击下载";
                                UpdateLabel.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("检查更新失败：" + ex.Message);
            }
        }

        private void UpdateLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           
                System.Diagnostics.Process.Start(@"https://github.com/proxysu/ProxySU/releases");
            
        }

    }
}
