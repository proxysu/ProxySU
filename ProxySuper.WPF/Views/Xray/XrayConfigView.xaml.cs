using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.ViewModels;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProxySuper.WPF.Views
{
    [MvxWindowPresentation]
    public partial class XrayConfigView : MvxWindow
    {
        public XrayConfigView()
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

        public XraySettings Settings
        {
            get
            {
                return ((XrayConfigViewModel)ViewModel).Settings;
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
                case RayType.VLESS_TCP_XTLS:
                    shareLink = Settings.VLESS_TCP_XTLS_ShareLink;
                    break;
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
                case RayType.VLESS_QUIC:
                    shareLink = Settings.VLESS_QUIC_ShareLink;
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
                case RayType.VMESS_QUIC:
                    shareLink = Settings.VMESS_QUIC_ShareLink;
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


            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(shareLink, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            var qrCodeImage = qrCode.GetGraphic(20);
            var ms = new MemoryStream();
            qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(bytes);
            image.EndInit();
            QrImage.Source = image;
            QrImage.Tag = type.ToString();
        }
    }
}
