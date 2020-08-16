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
//using System.Windows.Forms;

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
            //RadioButtonTCP.IsChecked = true;
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

        private void ButtondDecide_Click(object sender, RoutedEventArgs e)
        {
            //UncheckLayouts(TabControlTemplate);
            //TCP模式被选中
            if (RadioButtonTCP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TCP";

            }
            //TCP+http伪装模式被选中
            else if (RadioButtonTCPhttp.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TCPhttp";
                //伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "http";
            }
            //TCP+TLS模式被选中
            else if (RadioButtonTCP2TLS.IsChecked == true)
            {
                if (string.IsNullOrEmpty(TextBoxDomain.Text.ToString()) == true)
                {
                    MessageBox.Show("域名不能为空！");
                    return;
                }
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "tcpTLS";

                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = TextBoxDomain.Text.ToString();
               
            }
            //tcp+TLS(自签证书)模式被选中
            else if (RadioButtonTcpTLS2SelfSigned.IsChecked == true)
            {
               //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "tcpTLSselfSigned";

                //传递域名
               // MainWindow.ReceiveConfigurationParameters[4] = TextBoxDomain.Text.ToString();

            }
            //webSocket模式被选中
            else if (RadioButtonWebSocket.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "webSocket";

            }
            //WebSocket+TLS模式被选中
            else if (RadioButtonWebSocketTLS.IsChecked == true)
            {
                if (string.IsNullOrEmpty(TextBoxDomain.Text.ToString()) == true)
                {
                    MessageBox.Show("域名不能为空！");
                    return;
                }
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketTLS";
                //传递路径
                MainWindow.ReceiveConfigurationParameters[3] = TextBoxPath.Text.ToString();
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = TextBoxDomain.Text.ToString();

            }
            
            //WebSocket+TLS+Web模式被选中
            else if (RadioButtonWebSocketTLS2Web.IsChecked == true|| RadioButtonWebSocketTLS2WebHot.IsChecked==true)
            {
                if (string.IsNullOrEmpty(TextBoxDomain.Text.ToString()) == true)
                {
                    MessageBox.Show("域名不能为空！");
                    return;
                }
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketTLS2Web";
                //传递路径
                MainWindow.ReceiveConfigurationParameters[3] = TextBoxPath.Text.ToString();
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
            }
            //WebSocket+TLS(自签证书)模式被选中
            else if (RadioButtonWebSocketTLSselfSigned.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketTLSselfSigned";
                //传递路径
                MainWindow.ReceiveConfigurationParameters[3] = TextBoxPath.Text.ToString();
                //传递域名
                //MainWindow.ReceiveConfigurationParameters[4] = TextBoxDomain.Text.ToString();

            }
            //http2模式被选中
            else if (RadioButtonHTTP2.IsChecked == true)
            {
                if (string.IsNullOrEmpty(TextBoxDomain.Text.ToString()) == true)
                {
                    MessageBox.Show("域名不能为空！");
                    return;
                }
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "Http2";
                //传递路径
                MainWindow.ReceiveConfigurationParameters[3] = TextBoxPath.Text.ToString();
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = TextBoxDomain.Text.ToString();
               
            }
            //http2+TLS+Web模式被选中
            else if (RadioButtonHTTP2Web.IsChecked == true || RadioButtonHTTP2WebHot.IsChecked == true)
            {
                if (string.IsNullOrEmpty(TextBoxDomain.Text.ToString()) == true)
                {
                    MessageBox.Show("域名不能为空！");
                    return;
                }
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "http2Web";
                //传递路径
                MainWindow.ReceiveConfigurationParameters[3] = TextBoxPath.Text.ToString();
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
            }
            //http2(自签证书)模式被选中
            else if (RadioButtonHTTP2selfSigned.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "http2selfSigned";
                //传递路径
                MainWindow.ReceiveConfigurationParameters[3] = TextBoxPath.Text.ToString();
                //传递域名
                //MainWindow.ReceiveConfigurationParameters[4] = TextBoxDomain.Text.ToString();

            }
            //mKCP无伪装模式被选中
            else if (RadioButtonMkcpNone.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCPNone";
                MainWindow.ReceiveConfigurationParameters[5] = "none";
                if (String.IsNullOrEmpty(TextBoxQuicUUID.Text)==false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
                }
            }
            //mKCP+srtp伪装模式被选中
            else if (RadioButton2mKCP2SRTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2SRTP";
                MainWindow.ReceiveConfigurationParameters[5] = "srtp";
                if (String.IsNullOrEmpty(TextBoxQuicUUID.Text) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
                }
            }
            //mKCP+utp伪装模式被选中
            else if (RadioButton2mKCPuTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCPuTP";
                MainWindow.ReceiveConfigurationParameters[5] = "utp";
                if (String.IsNullOrEmpty(TextBoxQuicUUID.Text) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
                }
            }
            //mKCP+wechat-video伪装模式被选中
            else if (RadioButton2mKCP2WechatVideo.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2WechatVideo";
                MainWindow.ReceiveConfigurationParameters[5] = "wechat-video";
                if (String.IsNullOrEmpty(TextBoxQuicUUID.Text) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
                }
            }
            //mKCP+dtls伪装模式被选中
            else if (RadioButton2mKCP2DTLS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2DTLS";
                MainWindow.ReceiveConfigurationParameters[5] = "dtls";
                if (String.IsNullOrEmpty(TextBoxQuicUUID.Text) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
                }
            }
            //mKCP+wireguard伪装模式被选中
            else if (RadioButton2mKCP2WireGuard.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2WireGuard";
                MainWindow.ReceiveConfigurationParameters[5] = "wireguard";
                if (String.IsNullOrEmpty(TextBoxQuicUUID.Text) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
                }
            }
            //QUIC无伪装模式被选中
            else if (RadioButtonQuicNone.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicNone";
                MainWindow.ReceiveConfigurationParameters[5] = "none";
                MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
            }
            //QUIC+srtp伪装模式被选中
            else if (RadioButtonQuicSRTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicSRTP";
                MainWindow.ReceiveConfigurationParameters[5] = "srtp";
                MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
            }
            //QUIC+utp伪装模式被选中
            else if (RadioButtonQuic2uTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "Quic2uTP";
                MainWindow.ReceiveConfigurationParameters[5] = "utp";
                MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
            }
            //QUIC+wechat-video伪装模式被选中
            else if (RadioButtonQuicWechatVideo.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicWechatVideo";
                MainWindow.ReceiveConfigurationParameters[5] = "wechat-video";
                MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
            }
            //QUIC+dtls伪装模式被选中
            else if (RadioButtonQuicDTLS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicDTLS";
                MainWindow.ReceiveConfigurationParameters[5] = "dtls";
                MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
            }
            //QUIC+wireguard伪装模式被选中
            else if (RadioButtonQuicWireGuard.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicWireGuard";
                MainWindow.ReceiveConfigurationParameters[5] = "wireguard";
                MainWindow.ReceiveConfigurationParameters[6] = TextBoxQuicUUID.Text;
            }
            //默认模式为 TCP
            else
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TCP";
            }
            //传递服务端口
            MainWindow.ReceiveConfigurationParameters[1] = TextBoxServerListenPort.Text.ToString();
            //传递uuid
            MainWindow.ReceiveConfigurationParameters[2] = TextBoxNewUUID.Text.ToString();
       
            this.Close();
        }

        private void ButtondCancel_Click(object sender, RoutedEventArgs e) => Close();

        #region 其他设置中的界面控制
        private void RadioButtonTCP_Checked(object sender, RoutedEventArgs e)
        {
            //TextBlockServerListenPort.Visibility = Visibility.Visible;
            //TextBoxServerListenPort.Visibility = Visibility.Visible;
            //ButtonServerListenPort.Visibility = Visibility.Visible;
            //隐藏QUIC密钥
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicUUID.Visibility = Visibility.Collapsed;
            ButtonQuicUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
            //隐藏Path
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            ButtonPath.Visibility = Visibility.Collapsed;
            //隐藏域名
            TextBlockDomain.Visibility = Visibility.Collapsed;
            TextBoxDomain.Visibility = Visibility.Collapsed;
            ButtonDomain.Visibility = Visibility.Collapsed;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;


            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            Random random = new Random();
            int randomServerPort = random.Next(10000, 50000);
            TextBoxServerListenPort.Text = randomServerPort.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonTCPhttp_Checked(object sender, RoutedEventArgs e)
        {
            //TextBlockServerListenPort.Visibility = Visibility.Visible;
            //TextBoxServerListenPort.Visibility = Visibility.Visible;
            //ButtonServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Text = "80";
            //隐藏Path
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            ButtonPath.Visibility = Visibility.Collapsed;

            //隐藏域名
            TextBlockDomain.Visibility = Visibility.Collapsed;
            TextBoxDomain.Visibility = Visibility.Collapsed;
            ButtonDomain.Visibility = Visibility.Collapsed;
            //隐藏QUIC密钥
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicUUID.Visibility = Visibility.Collapsed;
            ButtonQuicUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;

            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonTCP2TLS_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";
            //隐藏Path
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            //TextBoxPath.Text = "/ray";
            ButtonPath.Visibility = Visibility.Collapsed;
            //显示域名
            TextBlockDomain.Visibility = Visibility.Visible;
            TextBoxDomain.Visibility = Visibility.Visible;
            //ButtonDomain.Visibility = Visibility.Visible;
            //隐藏QUIC密钥
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicUUID.Visibility = Visibility.Collapsed;
            ButtonQuicUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;

            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonTCP2TLSnoDomain_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";
            //隐藏Path
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            //TextBoxPath.Text = "/ray";
            ButtonPath.Visibility = Visibility.Collapsed;
            //隐藏域名
            TextBlockDomain.Visibility = Visibility.Collapsed;
            TextBoxDomain.Visibility = Visibility.Collapsed;
            //ButtonDomain.Visibility = Visibility.Visible;
            //隐藏QUIC密钥
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicUUID.Visibility = Visibility.Collapsed;
            ButtonQuicUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;

            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonWebSocketTLS2Web_Checked(object sender, RoutedEventArgs e)
        {
            //TextBlockServerListenPort.Visibility = Visibility.Visible;
            //TextBoxServerListenPort.Visibility = Visibility.Visible;
            //ButtonServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Text = "443";
            //显示Path
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            TextBoxPath.Text = "/ray";
            ButtonPath.Visibility = Visibility.Visible;
            //显示域名
            TextBlockDomain.Visibility = Visibility.Visible;
            TextBoxDomain.Visibility = Visibility.Visible;
            //ButtonDomain.Visibility = Visibility.Visible;
            //隐藏QUIC密钥
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicUUID.Visibility = Visibility.Collapsed;
            ButtonQuicUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
            //显示伪装网站
            //TextBlockMaskSites.Visibility = Visibility.Visible;
            //TextBoxMaskSites.Visibility = Visibility.Visible;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;

            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonWebSocketTLSselfSigned_Checked(object sender, RoutedEventArgs e)
        {
            //TextBlockServerListenPort.Visibility = Visibility.Visible;
            //TextBoxServerListenPort.Visibility = Visibility.Visible;
            //ButtonServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Text = "443";
            //显示Path
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            TextBoxPath.Text = "/ray";
            ButtonPath.Visibility = Visibility.Visible;
            //隐藏域名
            TextBlockDomain.Visibility = Visibility.Collapsed;
            TextBoxDomain.Visibility = Visibility.Collapsed;
            //TextBoxDomain.Tag = "可为空";
            //ButtonDomain.Visibility = Visibility.Visible;
            //隐藏QUIC密钥
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicUUID.Visibility = Visibility.Collapsed;
            ButtonQuicUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;

            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonHTTP2_Checked(object sender, RoutedEventArgs e)
        {
            //TextBlockServerListenPort.Visibility = Visibility.Visible;
            //TextBoxServerListenPort.Visibility = Visibility.Visible;
            //ButtonServerListenPort.Visibility = Visibility.Visible;
            TextBoxServerListenPort.Text = "443";
            //显示Path
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            TextBoxPath.Text = "/ray";
            ButtonPath.Visibility = Visibility.Visible;
            //显示域名
            TextBlockDomain.Visibility = Visibility.Visible;
            TextBoxDomain.Visibility = Visibility.Visible;
            //ButtonDomain.Visibility = Visibility.Visible;
            //隐藏QUIC密钥
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicUUID.Visibility = Visibility.Collapsed;
            ButtonQuicUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;

            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
         private void RadioButtonQuicNone_Checked(object sender, RoutedEventArgs e)
        {
            //显示QUIC密钥
            TextBlockQuicUUID.Visibility = Visibility.Visible;
            TextBoxQuicUUID.Visibility = Visibility.Visible;
            ButtonQuicUUID.Visibility = Visibility.Visible;
            TextBlockMkcpUUID.Visibility = Visibility.Visible;
            //隐藏Path
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            ButtonPath.Visibility = Visibility.Collapsed;
            //隐藏域名
            TextBlockDomain.Visibility = Visibility.Collapsed;
            TextBoxDomain.Visibility = Visibility.Collapsed;
            ButtonDomain.Visibility = Visibility.Collapsed;
            //隐藏伪装网站
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;

            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();

            uuid = Guid.NewGuid();
            TextBoxQuicUUID.Text = uuid.ToString();

            Random random = new Random();
            int randomServerPort = random.Next(10000, 50000);
            TextBoxServerListenPort.Text = randomServerPort.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        #endregion

        //产生随机的uuid
        private void ButtonNewUUID_Click(object sender, RoutedEventArgs e)
        {
            Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = uuid.ToString();
        }
        //产生QUIC密钥所用的UUID
        private void ButtonQuicUUID_Click(object sender, RoutedEventArgs e)
        {
            Guid uuid = Guid.NewGuid();
            TextBoxQuicUUID.Text = uuid.ToString();
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

        //private void ButtonTestChecked_Click(object sender, RoutedEventArgs e)
        //{
        //    UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);

        //}
    }
}
