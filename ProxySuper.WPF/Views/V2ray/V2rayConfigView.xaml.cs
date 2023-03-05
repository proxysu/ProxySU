using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.ViewModels;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProxySuper.WPF.Views.V2ray
{
    [MvxWindowPresentation]
    public partial class V2rayConfigView : MvxWindow
    {
        public V2rayConfigView()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            for (int i = 0; i < TabCtrl.Items.Count; i++)
            {
                var tabItem = TabCtrl.Items[i] as TabItem;

                if (Settings.Types.Contains((RayType)tabItem.Tag))
                {
                    TabCtrl.SelectedIndex = i;
                    break;
                }
            }
        }

        public V2raySettings Settings
        {
            get
            {
                return ((V2rayConfigViewModel)ViewModel).Settings;
            }
        }



        private void BuildQrCode(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = e.Source as TabControl;
            var item = (tabControl.SelectedItem as TabItem);
            if (item == null) return;
            var type = (RayType)item.Tag;
            BuildQrCode(type);
        }

        private void SaveImage(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName += QrImage.Tag;
            sfd.Filter = "Image Files (*.bmp, *.png, *.jpg)|*.bmp;*.png;*.jpg | All Files | *.*";
            sfd.RestoreDirectory = true;//保存对话框是否记忆上次打开的目录
            if (sfd.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)QrImage.Source));
                using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                    encoder.Save(stream);
            }
        }

        private void BuildQrCode(RayType type)
        {
            string shareLink = string.Empty;
            switch (type)
            {
                case RayType.VLESS_TCP:
                    shareLink = Settings.VLESS_TCP_ShareLink;
                    break;
                case RayType.VLESS_WS:
                    shareLink = Settings.VLESS_WS_ShareLink;
                    break;
                case RayType.VLESS_H2:
                    break;
                case RayType.VLESS_KCP:
                    shareLink = Settings.VLESS_KCP_ShareLink;
                    break;
                case RayType.VLESS_gRPC:
                    shareLink = Settings.VLESS_gRPC_ShareLink;
                    break;
                case RayType.VMESS_TCP:
                    shareLink = Settings.VMESS_TCP_ShareLink;
                    break;
                case RayType.VMESS_WS:
                    shareLink = Settings.VMESS_WS_ShareLink;
                    break;
                case RayType.VMESS_H2:
                    break;
                case RayType.VMESS_KCP:
                    shareLink = Settings.VMESS_KCP_ShareLink;
                    break;
                case RayType.Trojan_TCP:
                    shareLink = Settings.Trojan_TCP_ShareLink;
                    break;
                case RayType.Trojan_WS:
                    break;
                case RayType.ShadowsocksAEAD:
                    shareLink = Settings.ShadowSocksShareLink;
                    break;
                default:
                    break;
            }


            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(shareLink, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            MemoryStream ms = new MemoryStream();
            qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(bytes);
            image.EndInit();
            QrImage.Source = image;
            QrImage.Tag = type.ToString();
        }
    }
}
