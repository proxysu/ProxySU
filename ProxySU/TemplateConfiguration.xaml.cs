using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProxySU
{
    /// <summary>
    /// WindowTemplateConfiguration.xaml 的交互逻辑
    /// </summary>
    public partial class WindowTemplateConfiguration : Window
    {
        public WindowTemplateConfiguration()
        {
            InitializeComponent();
            RadioButtonTCP.IsChecked = true;
        }
      
        private void ButtondDecide_Click(object sender, RoutedEventArgs e)
        {

            if (RadioButtonTCP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TCP";

            }
            else if (RadioButtonWebSocketTLS2Web.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketTLS2Web";
           
            }
            else if (RadioButtonTCPhttp.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TCPhttp";
                MainWindow.ReceiveConfigurationParameters[5] = "http";
            }
            else if (RadioButtonMkcpNoCamouflage.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "MkcpNone";
                MainWindow.ReceiveConfigurationParameters[5] = "none";
            }
            else if (RadioButton2mKCP2SRTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2SRTP";
                MainWindow.ReceiveConfigurationParameters[5] = "srtp";
            }
            else if (RadioButton2mKCPuTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCPuTP";
                MainWindow.ReceiveConfigurationParameters[5] = "utp";
            }
            else if (RadioButton2mKCP2WechatVideo.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2WechatVideo";
                MainWindow.ReceiveConfigurationParameters[5] = "wechat-video";
            }
            else if (RadioButton2mKCP2DTLS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2DTLS";
                MainWindow.ReceiveConfigurationParameters[5] = "dtls";
            }
            else if (RadioButton2mKCP2WireGuard.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2WireGuard";
                MainWindow.ReceiveConfigurationParameters[5] = "wireguard";
            }
            else if (RadioButtonHTTP2.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "HTTP2";
            }
            else if (RadioButtonTLS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TLS";
            }
            else
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TCP";
            }
            //传递服务端口
            MainWindow.ReceiveConfigurationParameters[1] = TextBoxServerListenPort.Text.ToString();
            //传递uuid
            MainWindow.ReceiveConfigurationParameters[2] = TextBoxNewUUID.Text.ToString();
            //传递路径
            MainWindow.ReceiveConfigurationParameters[3] = TextBoxPath.Text.ToString();
            //传递域名
            MainWindow.ReceiveConfigurationParameters[4] = TextBoxDomain.Text.ToString();
            this.Close();
        }

        private void ButtondCancel_Click(object sender, RoutedEventArgs e) => Close();
        #region 其他设置中的界面控制
        private void RadioButtonTCP_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Visibility = Visibility.Visible;
            ButtonServerListenPort.Visibility = Visibility.Visible;
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            ButtonPath.Visibility = Visibility.Collapsed;
            TextBlockDomain.Visibility = Visibility.Collapsed;
            TextBoxDomain.Visibility = Visibility.Collapsed;
            ButtonDomain.Visibility = Visibility.Collapsed;
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            Random random = new Random();
            int randomServerPort = random.Next(10000, 50000);
            TextBoxServerListenPort.Text = randomServerPort.ToString();
        }

        private void RadioButtonHTTP2_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Text = "443";
            ButtonServerListenPort.Visibility = Visibility.Visible;
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            TextBoxPath.Text = "/ray";
            ButtonPath.Visibility = Visibility.Visible;
            TextBlockDomain.Visibility = Visibility.Visible;
            TextBoxDomain.Visibility = Visibility.Visible;
            ButtonDomain.Visibility = Visibility.Visible;
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
        }
        private void RadioButtonTCPhttp_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Visibility = Visibility.Visible;
            ButtonServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Text = "80";
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            ButtonPath.Visibility = Visibility.Collapsed;
            TextBlockDomain.Visibility = Visibility.Collapsed;
            TextBoxDomain.Visibility = Visibility.Collapsed;
            ButtonDomain.Visibility = Visibility.Collapsed;
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
        }
        #endregion
        //产生随机的uuid
        private void ButtonNewUUID_Click(object sender, RoutedEventArgs e)
        {
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
        }
        //产生随机服务端口
        private void ButtonServerListenPort_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int randomServerPort = random.Next(10000, 50000);
            TextBoxServerListenPort.Text = randomServerPort.ToString();
        }
        //产生随机的Path
        private void ButtonPath_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int randomSerialNum = random.Next(0, 4);
            Guid uuid = Guid.NewGuid();
            string[] pathArray = uuid.ToString().Split('-');
            string path = pathArray[randomSerialNum];
            TextBoxPath.Text = $"/{path}";
            //MessageBox.Show(path);
        }

        private void ButtonDomain_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
