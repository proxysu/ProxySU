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
    /// Window1.xaml 的交互逻辑
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

            }
            else if (RadioButtonWebSocketTLS2Web.IsChecked == true)
            {

            }
            else if (RadioButtonTCPhttp.IsChecked == true)
            {

            }
            else if (RadioButtonMkcpNoCamouflage.IsChecked == true)
            {

            }
            else if (RadioButton2mKCP2SRTP.IsChecked == true)
            {

            }
            else if (RadioButton2mKCPuTP.IsChecked == true)
            {

            }
            else if (RadioButton2mKCP2WechatVideo.IsChecked == true)
            {

            }
            else if (RadioButton2mKCP2DTLS.IsChecked == true)
            {

            }
            else if (RadioButton2mKCP2WireGuard.IsChecked == true)
            {

            }
            else if (RadioButtonHTTP2.IsChecked == true)
            {

            }
            else if (RadioButtonTLS.IsChecked == true)
            {

            }
            else
            {

            }
        }

        private void ButtondCancel_Click(object sender, RoutedEventArgs e) => Close();

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
        }

        private void RadioButtonHTTP2_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Text = "443";
            ButtonServerListenPort.Visibility = Visibility.Visible;
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            ButtonPath.Visibility = Visibility.Visible;
            TextBlockDomain.Visibility = Visibility.Visible;
            TextBoxDomain.Visibility = Visibility.Visible;
            ButtonDomain.Visibility = Visibility.Visible;
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
        }

        private void ButtonNewUUID_Click(object sender, RoutedEventArgs e)
        {
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
        }
    }
}
