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
    /// TrojanTemplateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TrojanTemplateWindow : Window
    {
        public TrojanTemplateWindow()
        {
            InitializeComponent();
            RadioButtonTrojanTLS2Web.IsChecked = true;
        }
        private void ButtondDecide_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonTrojanTLS2Web.IsChecked == true)
            {
                if (string.IsNullOrEmpty(TextBoxDomain.Text.ToString()) == true)
                {
                    MessageBox.Show("域名不能为空！");
                    return;
                }
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TrojanTLS2Web";

                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = TextBoxDomain.Text.ToString();
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = TextBoxMaskSites.Text.ToString();
                //处理伪装网站域名中的前缀
                if (TextBoxMaskSites.Text.ToString().Length >= 7)
                {
                    string testDomain = TextBoxMaskSites.Text.Substring(0, 7);
                    if (String.Equals(testDomain, "https:/") || String.Equals(testDomain, "http://"))
                    {
                        //MessageBox.Show(testDomain);
                        MainWindow.ReceiveConfigurationParameters[7] = TextBoxMaskSites.Text.Replace("/", "\\/");
                    }
                    else
                    {
                        MainWindow.ReceiveConfigurationParameters[7] = "http:\\/\\/" + TextBoxMaskSites.Text;
                    }
                }
                //传递服务端口
                MainWindow.ReceiveConfigurationParameters[1] = "443";
                //传递密码(uuid)
                MainWindow.ReceiveConfigurationParameters[2] = TextBoxNewUUID.Text.ToString();
            }
           

            this.Close();
        }
            private void ButtonNewUUID_Click(object sender, RoutedEventArgs e)
        {
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
        }
        //private void ButtonServerListenPort_Click(object sender, RoutedEventArgs e)
        //{
        //    Random random = new Random();
        //    int randomServerPort = random.Next(10000, 50000);
        //    TextBoxServerListenPort.Text = randomServerPort.ToString();
        //}
        private void ButtondCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void RadioButtonTrojanTLS2Web_Checked(object sender, RoutedEventArgs e)
        {
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            //Random random = new Random();
            //int randomServerPort = random.Next(10000, 50000);
            //TextBoxServerListenPort.Text = "443";
        }
    }
}
