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
    /// SSpluginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SSpluginWindow : Window
    {
        //SS加密方法设定
        public class EncryptionMethodInfo
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public SSpluginWindow()
        {
            InitializeComponent();

            #region 加密方法选择 初始设置为chacha20-ietf-poly1305
            List<EncryptionMethodInfo> methodList = new List<EncryptionMethodInfo>();

            methodList.Add(new EncryptionMethodInfo { Name = "chacha20-ietf-poly1305", Value = "chacha20-ietf-poly1305" });
            methodList.Add(new EncryptionMethodInfo { Name = "xchacha20-ietf-poly1305", Value = "xchacha20-ietf-poly1305" });
            methodList.Add(new EncryptionMethodInfo { Name = "aes-256-gcm", Value = "aes-256-gcm" });
            methodList.Add(new EncryptionMethodInfo { Name = "aes-192-gcm", Value = "aes-192-gcm" });
            methodList.Add(new EncryptionMethodInfo { Name = "aes-128-gcm", Value = "aes-128-gcm" });

            ComboBoxEncryptionMethodInfo.ItemsSource = methodList;

            ComboBoxEncryptionMethodInfo.DisplayMemberPath = "Name";//显示出来的值
            ComboBoxEncryptionMethodInfo.SelectedValuePath = "Value";//实际选中后获取的结果的值
            ComboBoxEncryptionMethodInfo.SelectedIndex = 0;

            DataContext = this;
            #endregion
        }
       
       
        
        //取消不在当前活动选项卡中的其他所有选项卡中的所有RadioBuuton的选中状态
        //代码参考网址：https://blog.csdn.net/weixin_42583999/article/details/103468857
        //调用：UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        private void UncheckLayouts(TabItem activePage)
        {
            foreach (TabItem tabPage in TabControlTemplate.Items)
            {
                if (tabPage == activePage) continue;
                Grid grid = (Grid)tabPage.Content;
                foreach (UIElement element in grid.Children)
                {
                    if (element is RadioButton)
                    {
                        RadioButton radiobutton = (element as RadioButton);
                        radiobutton.IsChecked = false;
                    }

                }
            }
        }


        //伪装网站处理
        //private string DisguiseURLprocessing(string fakeUrl)
        //{
            //var uri = new Uri(fakeUrl);
            //return uri.Host;
            //处理伪装网站域名中的前缀
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

        private void ButtondDecide_Click(object sender, RoutedEventArgs e)
        {
            bool preDomainMask = ClassModel.PreDomainMask(TextBoxMaskSites.Text);
            bool domainNotEmpty = true;
            //UncheckLayouts(TabControlTemplate);
            //SS 经典模式被选中
            if (RadioButtonNonePluginSS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "NonePluginSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonNonePluginSS.Content.ToString();

            }
            //SS+obfs+http+web伪装模式被选中
            else if (RadioButtonObfsPluginHttpWebSS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "ObfsPluginHttpWebSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonObfsPluginHttpWebSS.Content.ToString();

            }
            //SS+obfs+TLS+web模式被选中
            else if (RadioButtonObfsPluginHttpsWebSS.IsChecked == true)
            {

                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomainSS.Text);
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "ObfsPluginHttpsWebSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonObfsPluginHttpsWebSS.Content.ToString();
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomainSS.Text);
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));

            }

            //V2Ray-Plugin SS+WebSocket 无TLS模式被选中
            else if (RadioButtonWebSocketSS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonWebSocketSS.Content.ToString();
                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxWebSocketPathSS.Text);
            }
 
            //V2Ray-Plugin SS+WebSocket+TLS+Web模式被选中
            else if (RadioButtonWebSocketTLSWebFrontSS.IsChecked == true || RadioButtonWebSocketTLSWebFrontSSHot.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomainSS.Text);
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketTLSWebFrontSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonWebSocketTLSWebFrontSS.Content.ToString();

                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxWebSocketPathSS.Text);
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomainSS.Text);
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));

            }
            //V2Ray-Plugin SS+QUIC模式被选中
            else if (RadioButtonQuicSS.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomainSS.Text);
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonQuicSS.Content.ToString();

                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomainSS.Text);

            }
            //SS+kcptun-plugin模式被选中
            else if (RadioButtonKcptunPluginSS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "KcptunPluginSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonKcptunPluginSS.Content.ToString();

            }
            //SS+GoQuiet-Plugin模式被选中
            else if (RadioButtonGoQuietPluginSS.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomainSS.Text);
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "GoQuietPluginSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonGoQuietPluginSS.Content.ToString();
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomainSS.Text);

            }
            //SS+Cloak-Plugin模式被选中
            else if (RadioButtonCloakPluginSS.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomainSS.Text);
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "CloakPluginSS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonCloakPluginSS.Content.ToString();
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomainSS.Text);

            }

            //传递服务端口
            MainWindow.ReceiveConfigurationParameters[1] = PreTrim(TextBoxServerListenPortSS.Text);
            //传递uuid密码
            MainWindow.ReceiveConfigurationParameters[2] = PreTrim(TextBoxNewUUIDSS.Text);
            //传递加密方式
            MainWindow.ReceiveConfigurationParameters[3] = GetEncryptionMethodSS();

            if (domainNotEmpty == true && preDomainMask == true)
            {
                this.Close();
            }

        }

        private void ButtondCancel_Click(object sender, RoutedEventArgs e) => Close();


        #region 其他设置中的界面控制

        //无插件的界面
        private void RadioButtonNonePluginSS_Checked(object sender, RoutedEventArgs e)
        {
            //隐藏Websocket Path
            TextBlockWebSocketPathSS.Visibility = Visibility.Collapsed;
            TextBoxWebSocketPathSS.Visibility = Visibility.Collapsed;
            ButtonWebSocketPathSS.Visibility = Visibility.Collapsed;
                  
            //隐藏域名
            TextBlockDomainSS.Visibility = Visibility.Collapsed;
            TextBoxDomainSS.Visibility = Visibility.Collapsed;
            //检测域名按钮
            ButtonDomain.Visibility = Visibility.Collapsed;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;

            //初始化密码
            TextBoxNewUUIDSS.Text = GenerateRandomUUID();
            //初始化端口

            TextBoxServerListenPortSS.Text = GenerateRandomPort().ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        //使用域名，启用TLS 无Websocket的界面
        private void RadioButtonUseDomainTls_Checked(object sender, RoutedEventArgs e)
        {

            TextBoxServerListenPortSS.Text = "443";
            //隐藏Websocket Path
            TextBlockWebSocketPathSS.Visibility = Visibility.Collapsed;
            TextBoxWebSocketPathSS.Visibility = Visibility.Collapsed;
            ButtonWebSocketPathSS.Visibility = Visibility.Collapsed;

            //显示域名
            TextBlockDomainSS.Visibility = Visibility.Visible;
            TextBoxDomainSS.Visibility = Visibility.Visible;
            //检测域名按钮
            ButtonDomain.Visibility = Visibility.Collapsed;
            //显示伪装网站
            TextBlockMaskSites.Visibility = Visibility.Visible;
            TextBoxMaskSites.Visibility = Visibility.Visible;

            //初始化密码
            TextBoxNewUUIDSS.Text = GenerateRandomUUID();
            //初始化端口
            //TextBoxServerListenPortSS.Text = GenerateRandomPort().ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }

        //使用V2ray-plugin 的Websocket over https (TLS)
        private void RadioButtonUseDomainWebsocketTls_Checked(object sender, RoutedEventArgs e)
        {

            TextBoxServerListenPortSS.Text = "443";
            //显示Websocket Path
            TextBlockWebSocketPathSS.Visibility = Visibility.Visible;
            TextBoxWebSocketPathSS.Visibility = Visibility.Visible;
            ButtonWebSocketPathSS.Visibility = Visibility.Visible;

            //显示域名
            TextBlockDomainSS.Visibility = Visibility.Visible;
            TextBoxDomainSS.Visibility = Visibility.Visible;
            //检测域名按钮
            ButtonDomain.Visibility = Visibility.Collapsed;
            //显示伪装网站
            TextBlockMaskSites.Visibility = Visibility.Visible;
            TextBoxMaskSites.Visibility = Visibility.Visible;

            //初始化密码
            TextBoxNewUUIDSS.Text = GenerateRandomUUID();

            //初始化端口

            //TextBoxServerListenPortSS.Text = GenerateRandomPort().ToString();

            //初始化Websocket Path
            TextBoxWebSocketPathSS.Text = GenerateRandomPath();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }

        #endregion

        //加密方法更改后的动作
        private void ComboBoxEncryptionMethodInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //传递加密方式
            MainWindow.ReceiveConfigurationParameters[3] = GetEncryptionMethodSS();

        }

        //产生随机的uuid
        private void ButtonNewUUID_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNewUUIDSS.Text = GenerateRandomUUID();
        }

        //产生随机服务端口
        private void ButtonServerListenPort_Click(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPortSS.Text = GenerateRandomPort().ToString();
        }
        //产生随机的Path
        private void ButtonPath_Click(object sender, RoutedEventArgs e)
        {
            TextBoxWebSocketPathSS.Text = GenerateRandomPath();
        }

        #region 相关参数生成函数
        //产生随机的UUID
        private string GenerateRandomUUID()
        {
            Guid uuid = Guid.NewGuid();
            return uuid.ToString();
        }
        //产生随机端口
        private int GenerateRandomPort()
        {
            Random random = new Random();
            return random.Next(30001, 50000);
        }
        //读取加密方式
        private string GetEncryptionMethodSS()
        {
            //string methodName;
            //object methodSelected;
            //methodSelected = ComboBoxEncryptionMethodInfo.SelectedValue;
            //methodName = methodSelected.ToString();
            //return methodName;
            return ComboBoxEncryptionMethodInfo.SelectedValue.ToString();
        }
        //产生随机的Path
        private string GenerateRandomPath()
        {
            Random random = new Random();
            int randomSerialNum = random.Next(0, 4);
            Guid uuid = Guid.NewGuid();
            string[] pathArray = uuid.ToString().Split('-');
            string path = pathArray[randomSerialNum];
            return $"/{path}";
        }

        #endregion

        //TextBox输入内容做预处理
        private string PreTrim(string preString)
        {
            return preString.Trim();
        }

        //域名检测是否为空
        //private bool TestDomainIsEmpty()
        //{
        //    if (string.IsNullOrEmpty(PreTrim(TextBoxDomainSS.Text)) == true)
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
    }
}
