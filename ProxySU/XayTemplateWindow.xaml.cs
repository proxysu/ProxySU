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
    public partial class XrayWindowTemplateConfiguration : Window
    {
        //QUIC 加密方法
        public class EncryptionMethodInfo
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public XrayWindowTemplateConfiguration()
        {
            InitializeComponent();

            #region 加密方法选择 初始设置为chacha20-poly1305
            List<EncryptionMethodInfo> methodList = new List<EncryptionMethodInfo>();

            methodList.Add(new EncryptionMethodInfo { Name = "chacha20-poly1305", Value = "chacha20-poly1305" });
            methodList.Add(new EncryptionMethodInfo { Name = "aes-128-gcm", Value = "aes-128-gcm" });
            methodList.Add(new EncryptionMethodInfo { Name = "none", Value = "none" });

            ComboBoxEncryptionMethodInfo.ItemsSource = methodList;

            ComboBoxEncryptionMethodInfo.DisplayMemberPath = "Name";//显示出来的值
            ComboBoxEncryptionMethodInfo.SelectedValuePath = "Value";//实际选中后获取的结果的值
            ComboBoxEncryptionMethodInfo.SelectedIndex = 0;

            DataContext = this;
            #endregion

            //隐藏QUIC密钥
            FirstQuicHideEncryption();
            RadioButtonVMESSmKCP.IsChecked = true;
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

        //传递所选择的参数与模板方案
        private void ButtondDecide_Click(object sender, RoutedEventArgs e)
        {
            bool preDomainMask = ClassModel.PreDomainMask(TextBoxMaskSites.Text);
            bool domainNotEmpty = true;

            #region TCP 传输协议(VMESS)

            //TCP模式被选中
            if (RadioButtonTCP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TCP";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonTCP.Content.ToString();

            }
            
            //TCP+http伪装模式被选中
            else if (RadioButtonTCPhttp.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "TCPhttp";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonTCPhttp.Content.ToString();
                //伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "http";
              
            }
            
            //TCP+TLS模式被选中
            else if (RadioButtonTCP2TLS.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);

                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "tcpTLS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonTCP2TLS.Content.ToString();

                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
               
            }
            
            //tcp+TLS(自签证书)模式被选中
            else if (RadioButtonTcpTLS2SelfSigned.IsChecked == true)
            {
               //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "tcpTLSselfSigned";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonTcpTLS2SelfSigned.Content.ToString();

                //传递域名
                // MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);

            }

            #endregion

            #region VLESS协议

            //VLESS+TCP+XTLS+Web模式选中
            else if (RadioButtonVlessXtlsTcp.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);

                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "VlessXtlsTcp";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonVlessXtlsTcp.Content.ToString();

                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));

            }

            //VLESS+TCP+TLS+Web模式选中
            else if (RadioButtonVlessTcpTlsWeb.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);
    
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "VlessTcpTlsWeb";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonVlessTcpTlsWeb.Content.ToString();

                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));

            }

            //VLESS+WebSocket+TLS+Web模式选中
            else if (RadioButtonVlessWebSocketTlsWeb.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);

                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "VlessWebSocketTlsWeb";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonVlessWebSocketTlsWeb.Content.ToString();

                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));
            }

            //VLESS+http2+TLS+Web模式选中
            else if (RadioButtonVlessHttp2Web.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);

                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "VlessHttp2Web";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonVlessHttp2Web.Content.ToString();

                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));
            }

            //VLESS+VMESS+Trojan+XTLS+TCP+WebSocket+Web模式被选中
            else if (RadioButtonVlessVmessXtlsTcpWebSocketHot.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "VlessVmessXtlsTcpWebSocketWeb";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonVlessVmessXtlsTcpWebSocketHot.Content.ToString();
                //传递路径
                MainWindow.ReceiveConfigurationParameters[3] = PreTrim(TextBoxPathVlessWS.Text);//VLESS ws Path
                MainWindow.ReceiveConfigurationParameters[9] = PreTrim(TextBoxPathVmessTcp.Text);//VMESS tcp Path
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPathVmessWS.Text);//VMESS ws Path

                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
                //传递混淆方式(VMESS TCP Path方式所用)
                MainWindow.ReceiveConfigurationParameters[5] = "http";
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));

            }

            #endregion

            #region WebSocket传输协议(VMESS)

            //webSocket模式被选中
            else if (RadioButtonWebSocket.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "webSocket";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonWebSocket.Content.ToString();

            }

            //WebSocket+TLS模式被选中
            else if (RadioButtonWebSocketTLS.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);

                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketTLS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonWebSocketTLS.Content.ToString();
                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);

            }
            
            //WebSocket+TLS+Web模式被选中
            else if (RadioButtonWebSocketTLS2Web.IsChecked == true|| RadioButtonWebSocketTLS2WebHot.IsChecked==true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);
 
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketTLS2Web";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonWebSocketTLS2Web.Content.ToString();

                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));

            }
            
            //WebSocket+TLS(自签证书)模式被选中
            else if (RadioButtonWebSocketTLSselfSigned.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "WebSocketTLSselfSigned";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonWebSocketTLSselfSigned.Content.ToString();

                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
                //传递域名
                //MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);

            }

            #endregion

            #region http2传输协议(VMESS)

            //http2模式被选中
            else if (RadioButtonHTTP2.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);

                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "Http2";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonHTTP2.Content.ToString();

                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
               
            }
            
            //http2+TLS+Web模式被选中
            else if (RadioButtonHTTP2Web.IsChecked == true || RadioButtonHTTP2WebHot.IsChecked == true)
            {
                domainNotEmpty = ClassModel.TestDomainIsEmpty(TextBoxDomain.Text);

                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "http2Web";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonHTTP2Web.Content.ToString();

                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
                //传递域名
                MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);
                //传递伪装网站
                MainWindow.ReceiveConfigurationParameters[7] = ClassModel.DisguiseURLprocessing(PreTrim(TextBoxMaskSites.Text));

            }
            
            //http2(自签证书)模式被选中
            else if (RadioButtonHTTP2selfSigned.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "http2selfSigned";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonHTTP2selfSigned.Content.ToString();

                //传递路径
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxPath.Text);
                //传递域名
                //MainWindow.ReceiveConfigurationParameters[4] = PreTrim(TextBoxDomain.Text);

            }

            #endregion

            #region mKCP 传输协议 (VMESS)

            //mKCP无伪装模式被选中
            else if (RadioButtonMkcpNone.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCPNone";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonMkcpNone.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "none";
                //传递mKCP Seed
                if (String.IsNullOrEmpty(PreTrim(TextBoxQuicAndMkcpSeedUUID.Text)) ==false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                }
            }
            
            //mKCP+srtp伪装模式被选中
            else if (RadioButton2mKCP2SRTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2SRTP";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButton2mKCP2SRTP.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "srtp";
                //传递mKCP Seed
                if (String.IsNullOrEmpty(PreTrim(TextBoxQuicAndMkcpSeedUUID.Text)) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                }
            }
            
            //mKCP+utp伪装模式被选中
            else if (RadioButton2mKCPuTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCPuTP";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButton2mKCPuTP.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "utp";
                //传递mKCP Seed
                if (String.IsNullOrEmpty(PreTrim(TextBoxQuicAndMkcpSeedUUID.Text)) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                }
            }
            
            //mKCP+wechat-video伪装模式被选中
            else if (RadioButton2mKCP2WechatVideo.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2WechatVideo";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButton2mKCP2WechatVideo.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "wechat-video";
                //传递mKCP Seed
                if (String.IsNullOrEmpty(PreTrim(TextBoxQuicAndMkcpSeedUUID.Text)) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                }
            }
            
            //mKCP+dtls伪装模式被选中
            else if (RadioButton2mKCP2DTLS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2DTLS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButton2mKCP2DTLS.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "dtls";
                //传递mKCP Seed
                if (String.IsNullOrEmpty(PreTrim(TextBoxQuicAndMkcpSeedUUID.Text)) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                }
            }
            
            //mKCP+wireguard伪装模式被选中
            else if (RadioButton2mKCP2WireGuard.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "mKCP2WireGuard";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButton2mKCP2WireGuard.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "wireguard";
                //传递mKCP Seed
                if (String.IsNullOrEmpty(PreTrim(TextBoxQuicAndMkcpSeedUUID.Text)) == false)
                {
                    MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                }
            }

            #endregion

            #region QUIC传输协议(VMESS)

            //QUIC无伪装模式被选中
            else if (RadioButtonQuicNone.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicNone";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonQuicNone.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "none";
                //QUIC 密钥
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                //QUIC加密方法
                MainWindow.ReceiveConfigurationParameters[3] = GetEncryptionMethodSS();
            }
            
            //QUIC+srtp伪装模式被选中
            else if (RadioButtonQuicSRTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicSRTP";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonQuicSRTP.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "srtp";
                //QUIC 密钥
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                //QUIC加密方法
                MainWindow.ReceiveConfigurationParameters[3] = GetEncryptionMethodSS();
            }
            
            //QUIC+utp伪装模式被选中
            else if (RadioButtonQuic2uTP.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "Quic2uTP";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonQuic2uTP.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "utp";
                //QUIC 密钥
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                //QUIC加密方法
                MainWindow.ReceiveConfigurationParameters[3] = GetEncryptionMethodSS();
            }
            
            //QUIC+wechat-video伪装模式被选中
            else if (RadioButtonQuicWechatVideo.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicWechatVideo";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonQuicWechatVideo.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "wechat-video";
                //QUIC 密钥
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                //QUIC加密方法
                MainWindow.ReceiveConfigurationParameters[3] = GetEncryptionMethodSS();
            }
            
            //QUIC+dtls伪装模式被选中
            else if (RadioButtonQuicDTLS.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicDTLS";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonQuicDTLS.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "dtls";
                //QUIC 密钥
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                //QUIC加密方法
                MainWindow.ReceiveConfigurationParameters[3] = GetEncryptionMethodSS();
            }
            
            //QUIC+wireguard伪装模式被选中
            else if (RadioButtonQuicWireGuard.IsChecked == true)
            {
                //传递模板类型
                MainWindow.ReceiveConfigurationParameters[0] = "QuicWireGuard";
                //传递方案名称
                MainWindow.ReceiveConfigurationParameters[8] = RadioButtonQuicWireGuard.Content.ToString();
                //传递伪装类型
                MainWindow.ReceiveConfigurationParameters[5] = "wireguard";
                //QUIC 密钥
                MainWindow.ReceiveConfigurationParameters[6] = PreTrim(TextBoxQuicAndMkcpSeedUUID.Text);
                //QUIC加密方法
                MainWindow.ReceiveConfigurationParameters[3] = GetEncryptionMethodSS();
            }

            #endregion

            //传递服务端口
            MainWindow.ReceiveConfigurationParameters[1] = PreTrim(TextBoxServerListenPort.Text);
            //传递uuid
            MainWindow.ReceiveConfigurationParameters[2] = PreTrim(TextBoxNewUUID.Text);

            if (RadioButtonVLESSmKCP.IsChecked == true)
            {
                MainWindow.mKCPvlessIsSet = true;
            }
            else
            {
                MainWindow.mKCPvlessIsSet = false;
            }
               
            if (domainNotEmpty == true && preDomainMask == true)
            {
                this.Close();
            }

        }

        //取消选择返回主窗口
        private void ButtondCancel_Click(object sender, RoutedEventArgs e) => Close();


        //伪装网站处理
        //DisguiseURLprocessing(string fakeUrl);
       // private string DisguiseURLprocessing(string fakeUrl)
        //{
            //var uri = new Uri(fakeUrl);
            //return uri.Host;
            //Console.WriteLine(uri.Host);

            ////处理伪装网站域名中的前缀
            //if (fakeUrl.Length >= 7)
            //{
            //    string testDomainMask = fakeUrl.Substring(0, 7);
            //    if (String.Equals(testDomainMask, "https:/") || String.Equals(testDomainMask, "http://"))
            //    {
            //        string[] tmpUrl = fakeUrl.Split('/');
            //        fakeUrl = tmpUrl[2];
            //    }

            //}

        //}

        #region 其他设置中的界面控制

        private void RadioButtonTCP_Checked(object sender, RoutedEventArgs e)
        {
            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();
         
            //隐藏Path
            HidePath();
            HideVlessVmessMultiplePath();

            //隐藏域名
            HideDomain();

            //隐藏伪装网站
            HideMaskSites();


            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
            //Random random = new Random();
            int randomServerPort = GetRandomPort();
            TextBoxServerListenPort.Text = randomServerPort.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonTCPhttp_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "80";

            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();

            //隐藏Path
            HidePath();
            HideVlessVmessMultiplePath();

            //隐藏域名
            HideDomain();

            //隐藏伪装网站
            HideMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonTCP2TLS_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";

            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();

            //隐藏Path
            HidePath();
            HideVlessVmessMultiplePath();

            //显示域名
            ShowDomain();

            //隐藏伪装网站
            HideMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonVlessTcpTlsWeb_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";

            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();

            //隐藏Path
            HidePath();
            HideVlessVmessMultiplePath();

            //显示域名
            ShowDomain();

            //显示伪装网站
            ShowMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonVlessVmessXtlsTcpWebSocketHot_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";

            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();

            //显示复合路径
            ShowVlessVmessMultiplePath();

            //显示域名
            ShowDomain();

            //显示伪装网站
            ShowMaskSites();

            //生成UUID
            TextBoxNewUUID.Text = GenerateRandomUUID();

            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        //单击TextBlockVlessVmessXtlsTcpWebSocket标签则选中RadioButtonVlessVmessXtlsTcpWebSocketHot
        private void TextBlockVlessVmessXtlsTcpWebSocket_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RadioButtonVlessVmessXtlsTcpWebSocketHot.IsChecked = true;
        }
        private void RadioButtonTCP2TLSnoDomain_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";

            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();

            //隐藏Path
            HidePath();
            HideVlessVmessMultiplePath();

            //隐藏域名
            HideDomain();

            //隐藏伪装网站
            HideMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonWebSocketTLS2Web_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";

            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();

            //显示Path
            ShowPath();

            //显示域名
            ShowDomain();

            //显示伪装网站
            ShowMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonWebSocketTLSselfSigned_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";

            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();

            //显示Path
            ShowPath();

            //隐藏域名
            HideDomain();

            //隐藏伪装网站
            HideMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
        private void RadioButtonHTTP2_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxServerListenPort.Text = "443";

            //隐藏mKCP项
            HideMkcpSeed();

            //隐藏QUIC密钥
            HideQuic();

            //显示Path
            ShowPath();

            //显示域名
            ShowDomain();

            //隐藏伪装网站
            HideMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }

        //mKCP显示界面
        private void RadioButtonMkcp_Checked(object sender, RoutedEventArgs e)
        {
            //隐藏QUIC密钥
            HideQuic();

            //显示mKCP Seed
            ShowMkcpSeed();

            //隐藏Path
            HidePath();
            HideVlessVmessMultiplePath();

            //隐藏域名
            HideDomain();

            //隐藏伪装网站
            HideMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();

            //uuid = Guid.NewGuid();
            TextBoxQuicAndMkcpSeedUUID.Text = GenerateRandomUUID();

            //Random random = new Random();
            int randomServerPort = GetRandomPort();
            TextBoxServerListenPort.Text = randomServerPort.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }

        //QUIC显示界面
        private void RadioButtonQuicNone_Checked(object sender, RoutedEventArgs e)
        {
            //隐藏mKCP项
            HideMkcpSeed();

            //显示QUIC密钥
            ShowQuic();

            //隐藏Path
            HidePath();
            HideVlessVmessMultiplePath();

            //隐藏域名
            HideDomain();

            //隐藏伪装网站
            HideMaskSites();

            //Guid uuid = Guid.NewGuid();
            TextBoxNewUUID.Text = GenerateRandomUUID();

            //uuid = Guid.NewGuid();
            TextBoxQuicAndMkcpSeedUUID.Text = GenerateRandomUUID();

            //Random random = new Random();
            int randomServerPort = GetRandomPort();
            TextBoxServerListenPort.Text = randomServerPort.ToString();
            //清除其他选项卡中的选项
            UncheckLayouts((TabItem)TabControlTemplate.SelectedItem);
        }
       

        //隐藏QUIC相关项
        private void HideQuic()
        {
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicAndMkcpSeedUUID.Visibility = Visibility.Collapsed;
            ButtonQuicAndmKcpSeedUUID.Visibility = Visibility.Collapsed;
            TextBlockQuicEncryption.Visibility = Visibility.Collapsed;
            ComboBoxEncryptionMethodInfo.Visibility = Visibility.Collapsed;
            //TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
        }
        //如果加密方法选择none，则隐藏只QUIC密钥
        private void NoneEncryptionHideQuicKey()
        {
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicAndMkcpSeedUUID.Visibility = Visibility.Collapsed;
            ButtonQuicAndmKcpSeedUUID.Visibility = Visibility.Collapsed;
        }
        //窗口初始化时，需要做一次隐藏QUIC加密方法
        private void FirstQuicHideEncryption()
        {
            TextBlockQuicEncryption.Visibility = Visibility.Collapsed;
            ComboBoxEncryptionMethodInfo.Visibility = Visibility.Collapsed;
        }
        //显示QUIC相关项
        private void ShowQuic()
        {
            TextBlockQuicUUID.Visibility = Visibility.Visible;
            TextBoxQuicAndMkcpSeedUUID.Visibility = Visibility.Visible;
            ButtonQuicAndmKcpSeedUUID.Visibility = Visibility.Visible;
            TextBlockQuicEncryption.Visibility = Visibility.Visible;
            ComboBoxEncryptionMethodInfo.Visibility = Visibility.Visible;

            //隐藏mKCP相关项
            TextBlockMkcpSeedUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;
        }
        //隐藏mKCP Seed相关项
        private void HideMkcpSeed()
        {
            TextBlockMkcpSeedUUID.Visibility = Visibility.Collapsed;
            TextBoxQuicAndMkcpSeedUUID.Visibility = Visibility.Collapsed;
            ButtonQuicAndmKcpSeedUUID.Visibility = Visibility.Collapsed;
            TextBlockMkcpUUID.Visibility = Visibility.Collapsed;

        }
        //显示mKCP Seed相关项
        private void ShowMkcpSeed()
        {
            TextBlockMkcpSeedUUID.Visibility = Visibility.Visible;
            TextBoxQuicAndMkcpSeedUUID.Visibility = Visibility.Visible;
            ButtonQuicAndmKcpSeedUUID.Visibility = Visibility.Visible;
            TextBlockMkcpUUID.Visibility = Visibility.Visible;
            //隐藏QUIC标示
            TextBlockQuicUUID.Visibility = Visibility.Collapsed;
        }
        //隐藏路径相关项
        private void HidePath()
        {
            //隐藏Path
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            ButtonPath.Visibility = Visibility.Collapsed;
        }
        //显示路径相关项
        private void ShowPath()
        {
            HideVlessVmessMultiplePath();//隐藏VLESS VMESS多种方案的路径Path
            //显示Path
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            TextBoxPath.Text = "/ray";
            ButtonPath.Visibility = Visibility.Visible;
        }
        //隐藏VLESS VMESS复合方案路径
        private void HideVlessVmessMultiplePath()
        {
            TextBlockPathVlessWs.Visibility = Visibility.Collapsed;
            TextBoxPathVlessWS.Visibility = Visibility.Collapsed;
            TextBlockPathVmessTcp.Visibility = Visibility.Collapsed;
            TextBoxPathVmessTcp.Visibility = Visibility.Collapsed;
            TextBlockPathVmessWs.Visibility = Visibility.Collapsed;
            TextBoxPathVmessWS.Visibility = Visibility.Collapsed;
            ButtonVlessVmessPath.Visibility = Visibility.Collapsed;
            TextBlockTrojanPassword.Visibility = Visibility.Collapsed;

        }
        //显示VLESS VMESS复合方案路径
        private void ShowVlessVmessMultiplePath()
        {
            HidePath();//隐藏普通路径Path
            TextBlockPathVlessWs.Visibility = Visibility.Visible;
            TextBoxPathVlessWS.Visibility = Visibility.Visible;
            TextBoxPathVlessWS.Text = "/vlessws";

            TextBlockPathVmessTcp.Visibility = Visibility.Visible;
            TextBoxPathVmessTcp.Visibility = Visibility.Visible;
            TextBoxPathVmessTcp.Text = "/vmesstcp";

            TextBlockPathVmessWs.Visibility = Visibility.Visible;
            TextBoxPathVmessWS.Visibility = Visibility.Visible;
            TextBoxPathVmessWS.Text = "/vmessws";

            ButtonVlessVmessPath.Visibility = Visibility.Visible;
            TextBlockTrojanPassword.Visibility = Visibility.Visible;
        }
        //隐藏域名相关项
        private void HideDomain()
        {
            //隐藏域名
            TextBlockDomain.Visibility = Visibility.Collapsed;
            TextBoxDomain.Visibility = Visibility.Collapsed;
            ButtonDomain.Visibility = Visibility.Collapsed;
        }
        //显示域名相关项
        private void ShowDomain()
        {
            //显示域名
            TextBlockDomain.Visibility = Visibility.Visible;
            TextBoxDomain.Visibility = Visibility.Visible;
            //ButtonDomain.Visibility = Visibility.Visible;
        }
        //隐藏伪装网站
        private void HideMaskSites()
        {
            TextBlockMaskSites.Visibility = Visibility.Collapsed;
            TextBoxMaskSites.Visibility = Visibility.Collapsed;
        }
        //显示伪装网站
        private void ShowMaskSites()
        {
            TextBlockMaskSites.Visibility = Visibility.Visible;
            TextBoxMaskSites.Visibility = Visibility.Visible;
        }
        #endregion

        //产生随机的uuid
        private void ButtonNewUUID_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNewUUID.Text = GenerateRandomUUID();
        }
       
        //产生QUIC密钥/mKCP Seed所用的UUID
        private void ButtonQuicAndMkcpSeedUUID_Click(object sender, RoutedEventArgs e)
        {
            TextBoxQuicAndMkcpSeedUUID.Text = GenerateRandomUUID();
        }
        
        //更新随机服务端口
        private void ButtonServerListenPort_Click(object sender, RoutedEventArgs e)
        {
            int randomServerPort = GetRandomPort();
            TextBoxServerListenPort.Text = randomServerPort.ToString();
        }
        
        //更新单方案随机的Path
        private void ButtonPath_Click(object sender, RoutedEventArgs e)
        {
            string path = GenerateRandomPath();
            TextBoxPath.Text = $"/{path}";
        }

        //更新多方案共存的Path
        private void ButtonVlessVmessPath_Click(object sender, RoutedEventArgs e)
        {
            string path = GenerateRandomPath();
            TextBoxPathVlessWS.Text = $"/{path}";

            path = GenerateRandomPath();
            TextBoxPathVmessTcp.Text = $"/{path}";

            path = GenerateRandomPath();
            TextBoxPathVmessWS.Text = $"/{path}";
        }
        //TextBox输入内容做预处理
        private string PreTrim(string preString)
        {
            return preString.Trim();
        }
        //生成随机端口
        private int GetRandomPort()
        {
            Random random = new Random();
            return random.Next(10001, 60000);
        }

       //生成随机UUID
       private string GenerateRandomUUID()
        {
            Guid uuid = Guid.NewGuid();
            return uuid.ToString();
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

        //加密方法更改后的动作
        private void ComboBoxEncryptionMethodInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string getMethond = GetEncryptionMethodSS();
            //传递加密方式
            MainWindow.ReceiveConfigurationParameters[3] = getMethond;
            if (String.Equals(getMethond,"none"))
            {
                NoneEncryptionHideQuicKey();
            }
            else
            {
                ShowQuic();
            }

        }

        //读取加密方式
        private string GetEncryptionMethodSS()
        {
            return ComboBoxEncryptionMethodInfo.SelectedValue.ToString();
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
