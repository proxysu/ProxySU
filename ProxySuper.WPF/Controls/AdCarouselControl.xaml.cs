using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ProxySuper.WPF.Controls
{
    public partial class AdCarouselControl : UserControl
    {
        private class AdItem
        {
            public string ImagePath { get; set; }
            public string Url { get; set; }
        }

        private readonly List<AdItem> adsLeft = new List<AdItem>
        {
            new AdItem { ImagePath="pack://application:,,,/Images/free/ziyoumeng.png", Url="https://codeload.github.com/freegate-release/website/zip/refs/tags/fglatest" },
            new AdItem { ImagePath="pack://application:,,,/Images/free/tor.png",    Url="https://github.com/proxysu/ProxySU/wiki/%E5%85%8D%E8%B4%B9and%E4%BB%98%E8%B4%B9%E7%BF%BB%E5%A2%99%E8%B5%84%E6%BA%90" },
             new AdItem { ImagePath="pack://application:,,,/Images/free/wujie.png", Url="https://github.com/wujieliulan/download/raw/master/u.zip" },
            new AdItem { ImagePath="pack://application:,,,/Images/free/saifeng.png",    Url="https://s3.amazonaws.com/0ozb-6kaj-r0p8/zh/index.html" }
        };


        private readonly List<AdItem> adsRight = new List<AdItem>
        {
            new AdItem { ImagePath="pack://application:,,,/Images/paid/nordvpn.png",    Url="https://github.com/proxysu/ProxySU/wiki/%E5%85%8D%E8%B4%B9and%E4%BB%98%E8%B4%B9%E7%BF%BB%E5%A2%99%E8%B5%84%E6%BA%90" },
            new AdItem { ImagePath="pack://application:,,,/Images/paid/expressvpn.png", Url="https://github.com/proxysu/ProxySU/wiki/%E5%85%8D%E8%B4%B9and%E4%BB%98%E8%B4%B9%E7%BF%BB%E5%A2%99%E8%B5%84%E6%BA%90" },
            new AdItem { ImagePath="pack://application:,,,/Images/paid/purevpn.png",    Url="https://github.com/proxysu/ProxySU/wiki/%E5%85%8D%E8%B4%B9and%E4%BB%98%E8%B4%B9%E7%BF%BB%E5%A2%99%E8%B5%84%E6%BA%90" },
            new AdItem { ImagePath="pack://application:,,,/Images/paid/surfshark.png", Url="https://github.com/proxysu/ProxySU/wiki/%E5%85%8D%E8%B4%B9and%E4%BB%98%E8%B4%B9%E7%BF%BB%E5%A2%99%E8%B5%84%E6%BA%90" }
        };

        private int currentIndex = 0;
        private readonly DispatcherTimer timer;
        private readonly Duration fadeDuration = new Duration(TimeSpan.FromSeconds(1));

        private bool isExpanded = true;
        private double adHeight = 150;
        private double collapsedHeight = 25; // 收起时只显示按钮栏

        public AdCarouselControl()
        {
            InitializeComponent();

            // 广告轮播定时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) => NextAd();
            timer.Start();

            ShowAd();

            // 程序启动后 40 秒自动收起
            var collapseTimer = new DispatcherTimer();
            collapseTimer.Interval = TimeSpan.FromSeconds(40);
            collapseTimer.Tick += (s, e) =>
            {
                collapseTimer.Stop();
                CollapseAd();
            };
            collapseTimer.Start();
        }

        private int currentIndexLeft = 0;
        private int currentIndexRight = 0;

        private void ShowAd()
        {
            var adLeft = adsLeft[currentIndexLeft];
            AdImage1.Source = new BitmapImage(new Uri(adLeft.ImagePath));
            AdImage1.Opacity = 0;
            AdImage1.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, fadeDuration));

            var adRight = adsRight[currentIndexRight];
            AdImage2.Source = new BitmapImage(new Uri(adRight.ImagePath));
            AdImage2.Opacity = 0;
            AdImage2.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, fadeDuration));
        }

        private void NextAd()
        {
            var fadeOut1 = new DoubleAnimation(1, 0, fadeDuration);
            var fadeOut2 = new DoubleAnimation(1, 0, fadeDuration);

            fadeOut1.Completed += (s, e) =>
            {
                currentIndexLeft = (currentIndexLeft + 1) % adsLeft.Count;
                currentIndexRight = (currentIndexRight + 1) % adsRight.Count;
                ShowAd();
            };

            AdImage1.BeginAnimation(UIElement.OpacityProperty, fadeOut1);
            AdImage2.BeginAnimation(UIElement.OpacityProperty, fadeOut2);
        }

        private void AdImage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == AdImage1)
            {
                var ad = adsLeft[currentIndexLeft];
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ad.Url,
                    UseShellExecute = true
                });
            }
            else if (sender == AdImage2)
            {
                var ad = adsRight[currentIndexRight];
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ad.Url,
                    UseShellExecute = true
                });
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (isExpanded)
                CollapseAd();
            else
                ExpandAd();
        }

        private void CollapseAd()
        {
            var anim = new DoubleAnimation(adHeight, collapsedHeight, TimeSpan.FromSeconds(0.5));
            this.BeginAnimation(HeightProperty, anim);
            ToggleButton.Content = "▲"; // 收起后显示向上箭头
            isExpanded = false;
        }

        private void ExpandAd()
        {
            var anim = new DoubleAnimation(collapsedHeight, adHeight, TimeSpan.FromSeconds(0.5));
            this.BeginAnimation(HeightProperty, anim);
            ToggleButton.Content = "▼"; // 展开后显示向下箭头
            isExpanded = true;
        }
    }
}
