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
    /// TrojanGoTemplateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TrojanGoTemplateWindow : Window
    {
        public TrojanGoTemplateWindow()
        {
            InitializeComponent();
            RadioButtonTrojanGoTLS2Web.IsChecked = true;
            CheckBoxMuxSelect.IsChecked = false;
            GridTrojanGoMuxSelected.Visibility = Visibility.Collapsed;
            TextBlockExplainCheckBoxMuxSelect.Visibility = Visibility.Collapsed;
        }
        private void ButtondDecide_Click(object sender, RoutedEventArgs e)
        {
            bool preDomainMask = ClassModel.PreDomainMask(TextBoxMaskSites.Text);
            bool domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);

            //if (string.IsNullOrEmpty(PreTrim(TextBoxDomain.Text)) == true)
            //{
            //    //****** "域名不能为空，请检查相关参数设置！" ******
            //    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_DomainNotEmpty").ToString());
            //    return;
            //}
            //传递域名
            MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
            //传递伪装网站
            MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));

            //传递服务端口
            MainWindow.ReceiveConfigurationParameters[1] = "443";
            //传递密码(uuid)
            MainWindow.ReceiveConfigurationParameters[2] = PreTrim(TextBoxNewUUID.Text);
            if (RadioButtonTrojanGoTLS2Web.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TrojanGoTLS2Web";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonTrojanGoTLS2Web.Content.ToString();

            }
            else if (RadioButtonTrojanGoWebSocketTLS2Web.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TrojanGoWebSocketTLS2Web";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonTrojanGoWebSocketTLS2Web.Content.ToString();
                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
            }
            //传递Mux的concurrency与idle_timeout
            if (CheckBoxMuxSelect.IsChecked == true)
            {
                MainWindow.ReceiveConfigurationParameters[9] = "true";
                MainWindow.ReceiveConfigurationParameters[3] = PreTrim(TextBoxConcurrency.Text);
                MainWindow.ReceiveConfigurationParameters[5] = PreTrim(TextBoxIdle_timeout.Text);
            }
            if (domainNotEmpty == true && preDomainMask == true)
            {
                this.Close();
            }

        }
        //更新密码
        private void ButtonNewUUID_Click(object sender, RoutedEventArgs e)
        {
            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
        }
        //更新路径
        private void ButtonPath_Click(object sender, RoutedEventArgs e)
        {
            string path = GenerateRandomPath();
            TextBoxPath.Text = $"/{path}";
            //MessageBox.Show(path);
        }
        //private void ButtonServerListenPort_Click(object sender, RoutedEventArgs e)
        //{
        //    TextBoxServerListenPort.Text = GetRandomPort();
        //}
        private void ButtondCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void RadioButtonTrojanTLS2Web_Checked(object sender, RoutedEventArgs e)
        {
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            ButtonPath.Visibility = Visibility.Collapsed;
            //Random random = new Random();
            //int randomServerPort = random.Next(10000, 50000);
            //TextBoxServerListenPort.Text = "443";
        }
        private void RadioButtonTrojanGoWebSocketTLS2Web_Checked(object sender, RoutedEventArgs e)
        {
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            ButtonPath.Visibility = Visibility.Visible;
            TextBoxPath.Text = "/trojan";
        }

        //生成随机UUID
        private string GenerateRandomUUID()
        {
            Guid uuid = Guid.NewGuid();
            return uuid.ToString();
        }

        //生成随机端口
        private int GetRandomPort()
        {
            Random random = new Random();
            return random.Next(10001, 60000);
        }
        //生成随机Path
        private string GenerateRandomPath()
        {
            Random random = new Random();
            int randomSerialNum = random.Next(0, 4);
            //Guid uuid = Guid.NewGuid();
            string uuid = GenerateRandomUUID();
            string[] pathArray = uuid.Split('-');
            string path = pathArray[randomSerialNum];
            return path;
        }
        //域名检测是否为空
        //private bool TestDomainIsEmpty()
        //{
        //    if (string.IsNullOrEmpty(PreTrim(TextBoxDomain.Text)) == true)
        //    {
        //        //****** "域名不能为空，请检查相关参数设置！" ******
        //        MessageBox.Show(Application.Current.FindResource("MessageBoxShow_DomainNotEmpty").ToString());
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        private void CheckBoxMuxSelect_Checked(object sender, RoutedEventArgs e)
        {
            GridTrojanGoMuxSelected.Visibility = Visibility.Visible;
            TextBlockExplainCheckBoxMuxSelect.Visibility = Visibility.Visible;
        }

        private void CheckBoxMuxSelect_Unchecked(object sender, RoutedEventArgs e)
        {
            GridTrojanGoMuxSelected.Visibility = Visibility.Collapsed;
            TextBlockExplainCheckBoxMuxSelect.Visibility = Visibility.Collapsed;
        }

        //TextBox输入内容做预处理
        private string PreTrim(string preString)
        {
            return preString.Trim();
        }

        //处理伪装网站域名中的前缀
        //private string DisguiseURLprocessing(string fakeUrl)
        //{
            //var uri = new Uri(fakeUrl);
            //return uri.Host;
            //if (fakeUrl.Length >= 7)
            //{
            //    string testDomainMask = fakeUrl.Substring(0, 7);
            //    if (String.Equals(testDomainMask, "https:/") || String.Equals(testDomainMask, "http://"))
            //    {
            //        string[] tmpUrl = fakeUrl.Split('/');
            //        fakeUrl = tmpUrl[2];
            //    }

            //}
            //return fakeUrl;
       // }
    }
}
