using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Renci.SshNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Drawing;
using QRCoder;


namespace ProxySU
{
    /// <summary>
    /// ResultClientInformation.xaml 的交互逻辑
    /// </summary>
    public partial class ResultClientInformation : Window
    {
        private static string saveFileFolder = "";
        private static string server = MainWindow.ReceiveConfigurationParameters[4];
        public ResultClientInformation()
        {
            InitializeComponent();

            if (String.Equals(MainWindow.proxyType, "V2Ray"))
            {
                //显示V2Ray参数，隐藏其他
                GroupBoxV2rayClient.Visibility = Visibility.Visible;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                //主机地址
                TextBoxHostAddress.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxPort.Text = MainWindow.ReceiveConfigurationParameters[1];
                //用户ID(uuid)
                TextBoxUUID.Text = MainWindow.ReceiveConfigurationParameters[2];
                //额外ID
                TextBoxUUIDextra.Text = "16";
                //加密方式，一般都为auto
                TextBoxEncryption.Text = "auto";
                //传输协议
                TextBoxTransmission.Text = "";
                //伪装类型
                TextBoxCamouflageType.Text = MainWindow.ReceiveConfigurationParameters[5];
                //是否启用TLS
                TextBoxTLS.Text = "none";
                //TLS的Host
                TextBoxHost.Text = "";
                //路径Path
                TextBoxPath.Text = MainWindow.ReceiveConfigurationParameters[3];
                //QUIC密钥/mKCP Seed
                TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];

                if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLS2Web"))
                {
                    TextBoxTransmission.Text = "ws";        //传输协议
                    TextBoxCamouflageType.Text = "none";    //伪装类型
                    TextBoxTLS.Text = "tls";                //是否启用TLS
                    ShowPath();                             //显示路径
                    HideQuicKey();                          //隐藏Quic\mKCP密钥
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCP"))
                {
                    TextBoxTransmission.Text = "tcp";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "none";
                    HidePath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCPhttp"))
                {
                    TextBoxTransmission.Text = "tcp";
                    TextBoxCamouflageType.Text = "http";
                    TextBoxTLS.Text = "none";
                    HidePath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "tcpTLS"))
                {
                    TextBoxTransmission.Text = "tcp";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "tls";
                    HidePath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "tcpTLSselfSigned"))
                {
                    TextBoxTransmission.Text = "tcp";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "tls";
                    HidePath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb"))
                {
                    TextBoxTransmission.Text = "tcp";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxEncryption.Text = "none";
                    TextBoxTLS.Text = "tls";
                    HideAlterId();
                    HidePath();
                    HideQuicKey();
                    ImageShareQRcode.Visibility = Visibility.Collapsed;
                    TextBoxURL.Visibility = Visibility.Collapsed;
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "webSocket"))
                {
                    TextBoxTransmission.Text = "ws";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "none";
                    HidePath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLS"))
                {
                    TextBoxTransmission.Text = "ws";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "tls";
                    ShowPath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned"))
                {
                    TextBoxTransmission.Text = "ws";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "tls";
                    ShowPath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "Http2"))
                {
                    TextBoxTransmission.Text = "h2";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "tls";
                    ShowPath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "http2Web"))
                {
                    TextBoxTransmission.Text = "h2";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxHost.Text = MainWindow.ReceiveConfigurationParameters[4];
                    TextBoxTLS.Text = "tls";
                    ShowPath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "http2selfSigned"))
                {
                    TextBoxTransmission.Text = "h2";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "tls";
                    ShowPath();
                    HideQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPNone"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示mKCP Seed
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2SRTP"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "srtp";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示mKCP Seed
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPuTP"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "utp";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示mKCP Seed
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WechatVideo"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "wechat-video";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示mKCP Seed
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2DTLS"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "dtls";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示mKCP Seed
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WireGuard"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "wireguard";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示mKCP Seed
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicNone"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示QUIC密钥
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicSRTP"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "srtp";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示QUIC密钥
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "Quic2uTP"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "utp";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示QUIC密钥
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicWechatVideo"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "wechat-video";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示QUIC密钥
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicDTLS"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "dtls";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示QUIC密钥
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicWireGuard"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "wireguard";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();//显示QUIC密钥
                }

                else
                {
                    TextBoxTransmission.Text = "tcp";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxTLS.Text = "none";
                    HidePath();
                    HideQuicKey();
                }
                CheckDir("v2ray_config");

                GenerateV2rayShareQRcodeAndBase64Url();
            }
            else if (String.Equals(MainWindow.proxyType, "TrojanGo"))
            {
                //GroupBoxTrojanClient.Header = "Trojan-go 客户端配置参数";
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Visible;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                TextBoxTrojanGoWSPath.Visibility = Visibility.Hidden;
                TextBlockTrojanGoWebSocketPath.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCaption.Visibility = Visibility.Hidden;

                //******"可用于ShadowRocket (ios)、igniter（Android）、Trojan-QT5 (windows) 扫码和导入url。注意：有的客户端可能不支持WebSocket模式。" ******
                TextBlockQrURLexplain.Text = Application.Current.FindResource("TextBlockQrURLexplainTrojan-go").ToString();

                //主机地址
                TextBoxTrojanGoServerHost.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxTrojanGoServerPort.Text = "443";
                //密钥（uuid）
                TextBoxTrojanGoServerPassword.Text = MainWindow.ReceiveConfigurationParameters[2];
                //WebSocket路径
                if (MainWindow.ReceiveConfigurationParameters[0].Equals("TrojanGoWebSocketTLS2Web"))
                {
                    TextBoxTrojanGoWSPath.Text = MainWindow.ReceiveConfigurationParameters[3];
                    TextBoxTrojanGoWSPath.Visibility = Visibility.Visible;
                    TextBlockTrojanGoWebSocketPath.Visibility = Visibility.Visible;
                    TextBlockTrojanGoCaption.Visibility = Visibility.Visible;

                }
                CheckDir("trojan-go_config");
                GenerateTrojanGoShareQRcodeAndBase64Url();
            }
            else if (String.Equals(MainWindow.proxyType, "Trojan"))
            {
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Visible;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                TextBlockQrURLexplain.Text = Application.Current.FindResource("TextBlockQrURLexplainTrojan").ToString();

                //主机地址
                TextBoxTrojanServerHost.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxTrojanServerPort.Text = "443";
                //密钥（uuid）
                TextBoxTrojanServerPassword.Text = MainWindow.ReceiveConfigurationParameters[2];

                CheckDir("trojan_config");
                GenerateTrojanShareQRcodeAndBase64Url();
            }
            else if (String.Equals(MainWindow.proxyType, "NaiveProxy"))
            {
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Visible;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                TextBlockQrURLexplain.Text = Application.Current.FindResource("TextBlockQrURLexplainNaiveProxy").ToString();

                TextBoxNaiveServerHost.Text = MainWindow.ReceiveConfigurationParameters[4];
                TextBoxNaiveUser.Text = MainWindow.ReceiveConfigurationParameters[3];
                TextBoxNaivePassword.Text = MainWindow.ReceiveConfigurationParameters[2];
                GenerateNaivePrxoyShareQRcodeAndBase64Url();
            }
            else if (String.Equals(MainWindow.proxyType, "SSR"))
            {
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Visible;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                TextBlockQrURLexplain.Text = Application.Current.FindResource("TextBlockQrURLexplainSSR").ToString();

                //主机地址
                TextBoxSSRHostAddress.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxSSRPort.Text = "443";
                //密码（uuid）
                TextBoxSSRUUID.Text = MainWindow.ReceiveConfigurationParameters[2];
                //加密方式
                TextBoxSSREncryption.Text = "none";
                //传输协议
                TextBoxSSRTransmission.Text = "auth_chain_a";
                //混淆
                TextBoxSSRCamouflageType.Text = "tls1.2_ticket_auth";

                //CheckDir("ssr_config");
                GenerateSSRShareQRcodeAndBase64Url();
            }
            else if (String.Equals(MainWindow.proxyType, "SS"))
            {
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Visible;

                //主机地址
                TextBoxHostAddressSS.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxPortSS.Text = MainWindow.ReceiveConfigurationParameters[1];
                //密码（uuid）
                TextBoxPasswordSS.Text = MainWindow.ReceiveConfigurationParameters[2];
                //加密方式
                TextBoxEncryptionSS.Text = MainWindow.ReceiveConfigurationParameters[6];
                //插件程序
                TextBoxPluginNameExplainSS.Text = MainWindow.ReceiveConfigurationParameters[5];
                //插件选项
                TextBoxPluginOptionExplainSS.Text = MainWindow.ReceiveConfigurationParameters[9];
                //创建ss配置文件总文件夹
                CheckDir("ss_config");

                if (String.IsNullOrEmpty(TextBoxPluginNameExplainSS.Text) == true)
                {
                    TextBlockPluginNameExplainSS.Visibility = Visibility.Collapsed;
                    TextBoxPluginNameExplainSS.Visibility = Visibility.Collapsed;

                    //TextBoxPluginNameExplainSSpc.Visibility = Visibility.Collapsed;
                    TextBlockPluginOptionExplainSS.Visibility = Visibility.Collapsed;
                    TextBoxPluginOptionExplainSS.Visibility = Visibility.Collapsed;
                    
                    TextBlockClientPromptSS.Visibility = Visibility.Collapsed;
                    RadioButtonMobile.Visibility = Visibility.Collapsed;
                    RadioButtonPC.Visibility = Visibility.Collapsed;
                    TextBlockQrURLexplain.Text = Application.Current.FindResource("TextBlockQrURLexplainSS").ToString();
                }
                else
                {
                    //显示插件程序与插件选项
                    TextBlockPluginNameExplainSS.Visibility = Visibility.Visible;
                    TextBoxPluginNameExplainSS.Visibility = Visibility.Visible;

                    TextBlockPluginOptionExplainSS.Visibility = Visibility.Visible;
                    TextBoxPluginOptionExplainSS.Visibility = Visibility.Visible;

                    TextBlockClientPromptSS.Visibility = Visibility.Visible;
                    RadioButtonMobile.Visibility = Visibility.Visible;
                    RadioButtonPC.Visibility = Visibility.Visible;

                    TextBlockQrURLexplain.Text = Application.Current.FindResource("TextBlockQrURLexplainSSmobile").ToString();
                    TextBlockQrURLexplainSSpc.Text = Application.Current.FindResource("TextBlockQrURLexplainSSpc").ToString();

                    //转换成PC版plugin格式
                    if (String.Equals(TextBoxPluginNameExplainSS.Text, "obfs-local"))
                    {
                        TextBoxPluginNameExplainSSpc.Text = @"plugins\obfs-local.exe";
                    }
                    else if (String.Equals(TextBoxPluginNameExplainSS.Text, "v2ray-plugin"))
                    {
                        TextBoxPluginNameExplainSSpc.Text = @"plugins\v2ray-plugin.exe";
                    }
                    else if (String.Equals(TextBoxPluginNameExplainSS.Text, "kcptun"))
                    {
                        TextBoxPluginNameExplainSSpc.Text = @"plugins\kcptun-client.exe";
                    }
                    else if (String.Equals(TextBoxPluginNameExplainSS.Text, "goquiet"))
                    {
                        TextBoxPluginNameExplainSSpc.Text = @"plugins\goquiet-client.exe";
                    }
                    else if (String.Equals(TextBoxPluginNameExplainSS.Text, "cloak"))
                    {
                        TextBoxPluginNameExplainSSpc.Text = @"plugins\cloak-client.exe";
                    }
                }

                GenerateShareQRcodeAndBase64UrlSS();
                //选择手机端做为默认显示
                if (String.IsNullOrEmpty(TextBoxPluginNameExplainSS.Text) == false)
                {
                    RadioButtonMobile.IsChecked = true;
                    if (String.Equals(TextBoxPluginNameExplainSS.Text, "goquiet"))
                    {
                        RadioButtonPC.IsChecked = true;
                        RadioButtonMobile.Visibility = Visibility.Collapsed;
                        TextBlockClientPromptSS.Visibility = Visibility.Collapsed;
                        //File.Delete($"ss_config\\{saveFileFolder}\\QR.bmp");
                        //File.Delete($"ss_config\\{saveFileFolder}\\url.txt");
                    }
                    else if (String.Equals(TextBoxPluginNameExplainSS.Text, "cloak"))
                    {
                        RadioButtonPC.IsChecked = true;
                        RadioButtonMobile.Visibility = Visibility.Collapsed;
                        TextBlockClientPromptSS.Visibility = Visibility.Collapsed;
                        //File.Delete($"ss_config\\{saveFileFolder}\\QR.bmp");
                        //File.Delete($"ss_config\\{saveFileFolder}\\url.txt");
                    }
                    
                }
               
            }

        }
        #region 界面控制相关
        private void HidePath()
        {
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            TextBlockPathExplain.Visibility = Visibility.Collapsed;

        }
        private void ShowPath()
        {
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            TextBlockPathExplain.Visibility = Visibility.Visible;

        }
        private void HideQuicKey()
        {
            TextBlockQuicKey.Visibility = Visibility.Collapsed;
            TextBoxQuicKey.Visibility = Visibility.Collapsed;
            TextBlockQuicKeyExplain.Visibility = Visibility.Collapsed;

        }
        private void ShowAlterId()
        {
            TextBlockUUIDextra.Visibility = Visibility.Visible;
            TextBoxUUIDextra.Visibility = Visibility.Visible;
            TextBlockUUIDextraExplanation.Visibility = Visibility.Visible;

        }
        private void HideAlterId()
        {
            TextBlockUUIDextra.Visibility = Visibility.Collapsed;
            TextBoxUUIDextra.Visibility = Visibility.Collapsed;
            TextBlockUUIDextraExplanation.Visibility = Visibility.Collapsed;

        }
        private void ShowQuicKey()
        {
            TextBlockQuicKey.Visibility = Visibility.Visible;
            TextBoxQuicKey.Visibility = Visibility.Visible;
            TextBlockQuicKeyExplain.Visibility = Visibility.Visible;

        }
        #endregion
       
        //生成v2rayN客户端导入文件
        private void GenerateV2rayShareQRcodeAndBase64Url()
        {
            //生成v2rayN的json文件
            string v2rayNjsonFile = @"
{
  ""v"": """",
  ""ps"": """",
  ""add"": """",
  ""port"": """",
  ""id"": """",
  ""aid"": """",
  ""net"": """",
  ""type"": """",
  ""host"": """",
  ""path"": """",
  ""tls"": """"
}";
            //MessageBox.Show(v2rayNjsonFile);
            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            v2rayNjsonObject["v"] = "2";
            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            if (TextBoxTransmission.Text.Contains("kcp") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKey.Text;//设置mKCP Seed
            }
            else if (TextBoxTransmission.Text.Contains("quic")==true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKey.Text;//设置quic密钥
                v2rayNjsonObject["host"] = "chacha20-poly1305";
            }
            else
            {
                v2rayNjsonObject["path"] = TextBoxPath.Text; //设置路径
                v2rayNjsonObject["host"] = TextBoxHost.Text;//设置TLS的Host
            }

            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            string saveFileFolderFirst = v2rayNjsonObject["ps"].ToString();
            int num = 1;
            saveFileFolder = saveFileFolderFirst;
            CheckDir(@"v2ray_config");
            while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            {
                saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
                num++;
            }
            CheckDir(@"v2ray_config\" + saveFileFolder);
            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            string vmessUrl = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = vmessUrl;
            using (StreamWriter sw = new StreamWriter($"v2ray_config\\{saveFileFolder}\\url.txt"))
            {
                if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == false)
                {
                    sw.WriteLine(vmessUrl);
                }
            }
            if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == false)
            {
                ImageShareQRcode.Source = CreateQRCode(vmessUrl, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            }
            

            if (File.Exists(@"v2ray_config\config.json"))
            {
                File.Move(@"v2ray_config\config.json", @"v2ray_config\" + saveFileFolder + @"\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"v2ray_config\\{saveFileFolder}\\readme.txt"))
            {
                sw.WriteLine("config.json");
                //****** "此文件为v2ray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine01").ToString());

                //****** "v2ray官方网站：https://www.v2ray.com/" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine02").ToString());

                //****** "v2ray官方程序下载地址：https://github.com/v2ray/v2ray-core/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine03").ToString());

                //****** "下载相应版本，Windows选择v2ray-windows-64.zip或者v2ray-windows-32.zip，解压后提取v2ctl.exe和v2ray.exe。与config.json放在同一目录，运行v2ray.exe即可。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine04").ToString());

                sw.WriteLine("-----------------------------------------");
                sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN、Trojan-QT5、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Trojan-QT5：https://github.com/Trojan-Qt5/Trojan-Qt5" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());
                
                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                sw.WriteLine("-----------------------------------------");
                sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Trojan-QT5、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());
                sw.WriteLine("-----------------------------------------\n");

                //****** "服务器通用连接配置参数" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine12").ToString());
                //sw.Write(Application.Current.FindResource("readmeTxtV2RayExplain").ToString());
                sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxHostAddress.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxPort.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockUserUUID").ToString() + $"{TextBoxUUID.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockV2RayAlterId").ToString() + $"{TextBoxUUIDextra.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockEncryption").ToString() + $"{TextBoxEncryption.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockTransferProtocol").ToString() + $"{TextBoxTransmission.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockCamouflageType").ToString() + $"{TextBoxCamouflageType.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockIsOrNotTLS").ToString() + $"{TextBoxTLS.Text}");
                sw.WriteLine("host:" + $"{TextBoxHostAddress.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockClientPath").ToString() + $"{TextBoxPath.Text}");
                sw.WriteLine(Application.Current.FindResource("TextBlockClientMkcpQuicKey").ToString() + $"{TextBoxQuicKey.Text}");
                
            }



        }

        //生成TrojanGo客户端资料
        private void GenerateTrojanGoShareQRcodeAndBase64Url()
        {

            string saveFileFolderFirst = TextBoxTrojanGoServerHost.Text;
            int num = 1;
            saveFileFolder = saveFileFolderFirst;
            CheckDir("trojan-go_config");
            while (Directory.Exists(@"trojan-go_config\" + saveFileFolder))
            {
                saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
                num++;
            }
            CheckDir(@"trojan-go_config\" + saveFileFolder);
            string trojanUrl = $"trojan://{TextBoxTrojanGoServerPassword.Text}@{TextBoxTrojanGoServerHost.Text}:{TextBoxTrojanGoServerPort.Text}?allowinsecure=0&tfo=0&sni=&mux=0&ws=0&group=#{TextBoxTrojanGoServerHost.Text}";
            //MessageBox.Show(v2rayNjsonObject.ToString());
            //string trojanUrl = "trojan://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = trojanUrl;
            using (StreamWriter sw = new StreamWriter($"trojan-go_config\\{saveFileFolder}\\url.txt"))
            {
                sw.WriteLine(trojanUrl);

            }
            ImageShareQRcode.Source = CreateQRCode(trojanUrl, $"trojan-go_config\\{saveFileFolder}\\QR.bmp");

            //移动Trojan官方程序配置文件到相应目录
            if (File.Exists(@"trojan-go_config\config.json"))
            {
                File.Move(@"trojan-go_config\config.json", @"trojan-go_config\" + saveFileFolder + @"\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"trojan-go_config\\{saveFileFolder}\\readme.txt"))
            {
                sw.WriteLine("config.json");

                //****** "此文件为Trojan-go官方程序所使用的客户端配置文件，配置为全局模式，http与socks5地址：127.0.0.1:1080" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine01").ToString());

                //****** "Trojan-go官方网站：https://github.com/p4gefau1t/trojan-go" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine02").ToString());

                //sw.WriteLine("Trojan-go官方程序下载地址：https://github.com/p4gefau1t/trojan-go/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine03").ToString());

                //sw.WriteLine("下载相应版本，Windows选择Trojan-go-x.xx-win.zip,解压后提取trojan-go.exe。与config.json放在同一目录，运行trojan-go.exe即可。");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine04").ToString());

                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("QR.bmp");

                //sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点（Trojan-Go的WebSocket模式暂不支持）");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine05").ToString());

                //sw.WriteLine("Trojan-QT5 (windows)下载网址：https://github.com/TheWanderingCoel/Trojan-Qt5/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine06").ToString());

                //sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine07").ToString());

                //sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine08").ToString());
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");

                //sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)复制粘贴导入节点的网址（Trojan-Go的WebSocket模式暂不支持）");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine09").ToString());
                sw.WriteLine("-----------------------------------------\n");

                //sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine10").ToString());

                //****** 服务器地址(address): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxTrojanGoServerHost.Text}");

                //****** 端口(port): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxTrojanGoServerPort.Text}");

                //****** 密码: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxTrojanGoServerPassword.Text}");

                //****** WebSocket路径: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoWebSocketPath").ToString() + $"{TextBoxTrojanGoWSPath.Text}");

            }

        }

        //生成Trojan客户端资料
        private void GenerateTrojanShareQRcodeAndBase64Url()
        {

            string saveFileFolderFirst = TextBoxTrojanServerHost.Text;
            int num = 1;
            saveFileFolder = saveFileFolderFirst;
            CheckDir("trojan_config");
            while (Directory.Exists(@"trojan_config\" + saveFileFolder))
            {
                saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
                num++;
            }
            CheckDir(@"trojan_config\" + saveFileFolder);
            string trojanUrl = $"trojan://{TextBoxTrojanServerPassword.Text}@{TextBoxTrojanServerHost.Text}:{TextBoxTrojanServerPort.Text}#{TextBoxTrojanServerHost.Text}";
            //MessageBox.Show(v2rayNjsonObject.ToString());
            //string trojanUrl = "trojan://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = trojanUrl;
            using (StreamWriter sw = new StreamWriter($"trojan_config\\{saveFileFolder}\\url.txt"))
            {
                sw.WriteLine(trojanUrl);

            }
            ImageShareQRcode.Source = CreateQRCode(trojanUrl, $"trojan_config\\{saveFileFolder}\\QR.bmp");

            //移动Trojan官方程序配置文件到相应目录
            if (File.Exists(@"trojan_config\config.json"))
            {
                File.Move(@"trojan_config\config.json", @"trojan_config\" + saveFileFolder + @"\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"trojan_config\\{saveFileFolder}\\readme.txt"))
            {
                sw.WriteLine("config.json");

                //****** "此文件为Trojan官方程序所使用的客户端配置文件，配置为全局模式，http与socks5地址：127.0.0.1:1080" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine01").ToString());

                //****** "Trojan官方网站：https://trojan-gfw.github.io/trojan/" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine02").ToString());

                //sw.WriteLine("Trojan官方程序下载地址：https://github.com/trojan-gfw/trojan/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine03").ToString());

                //sw.WriteLine("下载相应版本，Windows选择Trojan-x.xx-win.zip,解压后提取trojan.exe。与config.json放在同一目录，运行trojan.exe即可。");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine04").ToString());

                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("QR.bmp");

                //sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine05").ToString());

                //sw.WriteLine("Trojan-QT5 (windows)下载网址：https://github.com/TheWanderingCoel/Trojan-Qt5/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine06").ToString());

                //sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine07").ToString());

                //sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine08").ToString());
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");

                //sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)复制粘贴导入节点的网址");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine09").ToString());
                sw.WriteLine("-----------------------------------------\n");

                //sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine10").ToString());

                //****** 服务器地址(address): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxTrojanGoServerHost.Text}");

                //****** 端口(port): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxTrojanGoServerPort.Text}");

                //****** 密码: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxTrojanGoServerPassword.Text}");
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoWebSocketPath").ToString() + $"{TextBoxTrojanGoWSPath.Text}");


            }

        }

        //生成NaiveProxy客户端资料
        private void GenerateNaivePrxoyShareQRcodeAndBase64Url()
        {

            string saveFileFolderFirst = TextBoxNaiveServerHost.Text;
            int num = 1;
            saveFileFolder = saveFileFolderFirst;
            CheckDir("naive_config");
            while (Directory.Exists(@"naive_config\" + saveFileFolder))
            {
                saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
                num++;
            }
            CheckDir(@"naive_config\" + saveFileFolder);
            string naiveUrl = $"https://{TextBoxNaiveUser.Text}:{TextBoxNaivePassword.Text}@{TextBoxNaiveServerHost.Text}:443/?name={TextBoxNaiveServerHost.Text}&extra_headers=";
            //MessageBox.Show(v2rayNjsonObject.ToString());
            //string trojanUrl = "trojan://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = naiveUrl;
            using (StreamWriter sw = new StreamWriter($"naive_config\\{saveFileFolder}\\url.txt"))
            {
                sw.WriteLine(naiveUrl);

            }
            //CreateQRCode(trojanUrl);

            //移动NaiveProxy官方程序配置文件到相应目录
            if (File.Exists(@"naive_config\config.json"))
            {
                File.Move(@"naive_config\config.json", @"naive_config\" + saveFileFolder + @"\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"naive_config\\{saveFileFolder}\\readme.txt"))
            {
                sw.WriteLine("config.json");

                //****** "此文件为NaiveProxy官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine01").ToString());

                //****** "NaiveProxy官方网站：https://github.com/klzgrad/naiveproxy" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine02").ToString());

                //sw.WriteLine("NaiveProxy官方程序下载地址：https://github.com/klzgrad/naiveproxy/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine03").ToString());

                //sw.WriteLine("下载相应版本，Windows选择naiveproxy-x.xx-win.zip,解压后提取naive.exe。与config.json放在同一目录，运行naive.exe即可。");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine04").ToString());

                sw.WriteLine("-----------------------------------------\n");

                sw.WriteLine("url.txt");

                //sw.WriteLine("此文件为NaiveGUI(windows)复制粘贴导入节点的网址");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine05").ToString());

                //sw.WriteLine("NaiveGUI(windows)下载网址：https://github.com/ExcitedCodes/NaiveGUI/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine06").ToString());

                sw.WriteLine("-----------------------------------------\n");

                //sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine07").ToString());

                //****** 服务器地址(address): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxNaiveServerHost.Text}");

                //****** 用户名:******
                sw.WriteLine(Application.Current.FindResource("TextBlockHostUser").ToString() + $"{TextBoxNaiveUser.Text}");

                //****** 密码: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxNaivePassword.Text}");

            }

        }

        //生成SSR客户端资料
        private void GenerateSSRShareQRcodeAndBase64Url()
        {

            string saveFileFolderFirst = TextBoxSSRHostAddress.Text;
            int num = 1;
            saveFileFolder = saveFileFolderFirst;
            CheckDir("ssr_config");
            while (Directory.Exists(@"ssr_config\" + saveFileFolder))
            {
                saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
                num++;
            }
            CheckDir(@"ssr_config\" + saveFileFolder);

            string ssrUrl = GetSSRLinkForServer();
            TextBoxURL.Text = ssrUrl;
            using (StreamWriter sw = new StreamWriter($"ssr_config\\{saveFileFolder}\\url.txt"))
            {
                sw.WriteLine(ssrUrl);
            }
            ImageShareQRcode.Source = CreateQRCode(ssrUrl, $"ssr_config\\{saveFileFolder}\\QR.bmp");

            using (StreamWriter sw = new StreamWriter($"ssr_config\\{saveFileFolder}\\readme.txt"))
            {
               //sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("QR.bmp");

                //***"此文件为ShadowsocksR (windows)、SSRR（Android）、Shadowrocket(ios)扫码导入节点"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtSSRExplainLine05").ToString());

                //***"ShadowsocksR (windows)下载网址：https://github.com/shadowsocksrr/shadowsocksr-csharp/releases"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtSSRExplainLine06").ToString());

                //***"SSRR（Android）下载网址：https://github.com/shadowsocksrr/shadowsocksr-android/releases"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtSSRExplainLine07").ToString());

                //***"Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtSSRExplainLine08").ToString());
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");

                //***"此文件为ShadowsocksR (windows)、SSRR（Android）、Shadowrocket(ios)复制粘贴导入节点的网址"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtSSRExplainLine09").ToString());
                sw.WriteLine("-----------------------------------------\n");

                //***"服务器通用连接配置参数"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine10").ToString());

                //****** 服务器地址(address): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{ TextBoxSSRHostAddress.Text}");

                //****** 端口(port): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxSSRPort.Text}");

                //****** 密码: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxSSRUUID.Text}");

                //****** 加密方式: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockEncryption").ToString() + $"{TextBoxSSREncryption.Text}");

                //****** 传输协议: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockTransferProtocol").ToString() + $"{TextBoxSSRTransmission.Text}");

                //****** 混淆: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockCamouflageType").ToString() + $"{TextBoxSSRCamouflageType.Text}");

                //****** 密码: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoWebSocketPath").ToString() + $"{TextBoxTrojanGoWSPath.Text}");

            }
        }

        //生成SS客户端资料
        private void GenerateShareQRcodeAndBase64UrlSS()
        {
            //创建保存目录
            string configSavePath = CreateConfigSaveDir(@"ss_config", TextBoxHostAddressSS.Text);
            string ssUrl;

            //生成手机端的URL(无插件时，为电脑手机通用)
            if (String.Equals(TextBoxPluginNameExplainSS.Text, "goquiet")==false && String.Equals(TextBoxPluginNameExplainSS.Text, "cloak") == false)
            {
                ssUrl = GetSStoURL(TextBoxPluginNameExplainSS.Text);
                TextBoxURL.Text = ssUrl;
                using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
                {
                    sw.WriteLine(ssUrl);
                }
                ImageShareQRcode.Source = CreateQRCode(ssUrl, $"{configSavePath}\\QR.bmp");
            }

            if (String.IsNullOrEmpty(TextBoxPluginNameExplainSS.Text) == false)
            {
                //生成电脑端
                ssUrl = GetSStoURL(TextBoxPluginNameExplainSSpc.Text);
                TextBoxURLpcSS.Text = ssUrl;
                using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url_pc.txt"))
                {
                    sw.WriteLine(ssUrl);
                }
                ImageShareQRcodeSSpc.Source = CreateQRCode(ssUrl, $"{configSavePath}\\QR_pc.bmp");
            }
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                if (String.Equals(TextBoxPluginNameExplainSS.Text, "goquiet") == false && String.Equals(TextBoxPluginNameExplainSS.Text, "cloak") == false)
                {
                    //sw.WriteLine("-----------------------------------------\n");
                    sw.WriteLine("QR.bmp");
                    if (String.IsNullOrEmpty(TextBoxPluginNameExplainSS.Text) == true)
                    {
                        //***"此文件为Shadowsocks (windows)、shadowsocks（Android）、Shadowrocket(ios)扫码导入节点"***
                        sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS05").ToString());

                    }
                    else
                    {
                        //***"此文件为shadowsocks（Android）、Shadowrocket(ios)扫码导入节点"***
                        sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS05").ToString());
                    }

                    sw.WriteLine("-----------------------------------------\n");
                    sw.WriteLine("url.txt");
                    if (String.IsNullOrEmpty(TextBoxPluginNameExplainSS.Text) == true)
                    {
                        //***"此文件为Shadowsocks (windows)、shadowsocks（Android）、Shadowrocket(ios)复制粘贴导入节点的网址"***
                        sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS09").ToString());
                    }
                    else
                    {
                        //***"此文件为shadowsocks（Android）、Shadowrocket(ios)复制粘贴导入节点的网址"***
                        sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS09").ToString());
                    }
                    sw.WriteLine("");
                    //***"shadowsocks（Android）下载网址：https://github.com/shadowsocks/shadowsocks-android/releases"***
                    sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS07").ToString());

                    //***"Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。"***
                    sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS08").ToString());
                }
                if (String.IsNullOrEmpty(TextBoxPluginNameExplainSS.Text) == false)
                {
                    sw.WriteLine("");
                    sw.WriteLine("-----------------------------------------\n");
                    sw.WriteLine("QR_pc.bmp");

                    //***"此文件为Shadowsocks (windows)扫码导入节点"***
                    sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS01").ToString());
                    sw.WriteLine("-----------------------------------------\n");
                    sw.WriteLine("url_pc.txt");

                    //***"此文件为Shadowsocks (windows)复制粘贴导入节点的网址"***
                    sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS02").ToString());
                    sw.WriteLine("");

                }
                //***"Shadowsocks(windows)下载网址：https://github.com/shadowsocks/shadowsocks-windows/releases"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtExplainLineSS06").ToString());
                sw.WriteLine("-----------------------------------------\n");

                //***"服务器通用连接配置参数"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine10").ToString());

                //****** 服务器地址(address): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxHostAddressSS.Text}");

                //****** 端口(port): ******
                sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxPortSS.Text}");

                //****** 密码: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxPasswordSS.Text}");

                //****** 加密方式: ******
                sw.WriteLine(Application.Current.FindResource("TextBlockEncryption").ToString() + $"{TextBoxEncryptionSS.Text}");


                if (String.IsNullOrEmpty(TextBoxPluginNameExplainSS.Text) == false)
                {
                    //****** 插件程序:: ******
                    sw.WriteLine(Application.Current.FindResource("TextBlockPluginNameExplainSS").ToString() + $"{TextBoxPluginNameExplainSS.Text}");

                    //****** 插件选项: ******
                    sw.WriteLine(Application.Current.FindResource("TextBlockPluginOptionExplainSS").ToString() + $"{TextBoxPluginOptionExplainSS.Text}");

                    sw.WriteLine("-----------------------------------------\n");
                    //****** 插件使用说明 ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS").ToString());

                    //****** ProxySU默认所有插件，在Shadowsocks (windows)运行文件所在文件夹的子文件夹plugins下。 ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS01").ToString());

                    //****** 电脑端手动安装插件说明 ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS02").ToString());

                    //****** 先下载插件，各个插件Windows客户端下载地址为： ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS03").ToString());

                    //****** Simple-obfs: https://github.com/shadowsocks/simple-obfs/releases 只下载 obfs-local.zip  ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS04").ToString());

                    //****** V2ray-plugin: https://github.com/shadowsocks/v2ray-plugin/releases 64位系统选择：v2ray-plugin-windows-amd64-vx.x.x.tar.gz,32位系统选择：v2ray-plugin-windows-386-vx.x.x.tar.gz (x为数字，是版本号)******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS05").ToString());

                    //****** Kcptun-plugin: https://github.com/shadowsocks/kcptun/releases  64位系统选择：kcptun-windows-amd64-xxxxxx.tar.gz,32位系统选择：kcptun-plugin-windows-386-xxxxxx.tar.gz (x为数字，是版本号)******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS06").ToString());

                    ////****** GoQuiet-plugin: https://github.com/cbeuw/GoQuiet/releases  64位系统选择：gq-client-windows-amd64-x.x.x.exe,32位系统选择：gq-client-windows-386-x.x.x.exe(x为数字，是版本号)******
                    //sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS07").ToString());

                    ////****** Cloak-plugin: https://github.com/cbeuw/Cloak/releases 64位系统选择：ck-client-windows-amd64-x.x.x.exe,32位系统选择：ck-client-windows-386-x.x.x.exe(x为数字，是版本号) ******
                    //sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS08").ToString());

                    //****** 在Shadowsocks (windows)运行文件所在文件夹中，新建文件夹plugins，将obfs-local.zip解压出的文件（两个）全部复到plugins中，
                    //v2ray -plugin下载得到的文件，解压出的文件，复制到plugins中，并重命名为：v2ray-plugin.exe。
                    //Kcptun -plugin下载得到的文件，解压出两个文件，将其中的client_windows开头的文件，复制到plugins中，并重命名为：kcptun-client.exe。
                    //GoQuiet-plugin下载得到的文件，直接复制到plugin中，并重命名为：goquiet-client.exe。
                    //Cloak-plugin下载得到的文件，直接复制到plugin中，并重命名为：cloak-client.exe
                    //******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS09").ToString());
                    //******安装完毕 ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSS10").ToString());

                    sw.WriteLine("-----------------------------------------\n");

                    //******手机安卓客户端插件安装说明 ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSSandroid02").ToString());

                    //****** 先下载插件，各个插件安卓客户端下载地址为： ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSSandroid03").ToString());

                    //****** Simple-obfs: https://github.com/shadowsocks/simple-obfs-android/releases 只下载 obfs-local-nightly-x.x.x.apk(x为数字，是版本号)  ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSSandroid04").ToString());

                    //****** V2ray-plugin: https://github.com/shadowsocks/v2ray-plugin-android/releases 一般选择v2ray--universal-x.x.x.apk(x为数字，是版本号)******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSSandroid05").ToString());

                    //****** Kcptun-plugin: https://github.com/shadowsocks/kcptun-android/releases 一般选择kcptun--universal-x.x.x.apk(x为数字，是版本号)******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSSandroid06").ToString());
                    //******将上述apk文件传到手机，安装即可！ ******
                    sw.WriteLine(Application.Current.FindResource("readmeTxtPluginExplainSSandroid07").ToString());

                }

            }

        }

        //生成base64
        private string ToBase64Encode(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(textBytes);
        }

        //生成QRcoder图片，并按给出的路径与名称保存，返回窗口显示的图片源
        private BitmapSource CreateQRCode(string varBase64,string configPath)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(varBase64, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            IntPtr myImagePtr = qrCodeImage.GetHbitmap();
            BitmapSource imgsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(myImagePtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //ImageShareQRcode.Source = imgsource;
            //DeleteObject(myImagePtr);
            qrCodeImage.Save(configPath);
            return imgsource;

        }

        //判断目录是否存在，不存在则创建
        private bool CheckDir(string folder)
        {
            try
            {
                if (!Directory.Exists(folder))//如果不存在就创建file文件夹
                    Directory.CreateDirectory(folder);//创建该文件夹　　            
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //目录已存在则生成序号递增,并返回所创建的目录路径。
        private string CreateConfigSaveDir(string upperDir,string configDir)
        {
            try
            {
                //string saveFileFolderFirst = configDir;
                int num = 1;
                saveFileFolder = configDir;
                CheckDir(upperDir);
                while (Directory.Exists(upperDir + @"\" + saveFileFolder))
                {
                    saveFileFolder = configDir + "_copy_" + num.ToString();
                    num++;
                }
                CheckDir(upperDir + @"\" + saveFileFolder);
                return upperDir + @"\" + saveFileFolder;
            }
            catch (Exception)
            {
                saveFileFolder = "";
                return upperDir + @"\" + saveFileFolder;
            }
            
        }

        //打开文件夹
        private void ButtonOpenSaveDir_Click(object sender, RoutedEventArgs e)
        {
            if (String.Equals(MainWindow.proxyType, "V2Ray"))
            {
                string openFolderPath = @"v2ray_config\" + saveFileFolder;
                System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
                this.Close();
            }
            else if (String.Equals(MainWindow.proxyType, "TrojanGo"))
            {
                string openFolderPath = @"trojan-go_config\" + saveFileFolder;
                System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
                this.Close();
            }
            else if (String.Equals(MainWindow.proxyType, "Trojan"))
            {
                string openFolderPath = @"trojan_config\" + saveFileFolder;
                System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
                this.Close();
            }
            else if (String.Equals(MainWindow.proxyType, "NaiveProxy"))
            {
                string openFolderPath = @"naive_config\" + saveFileFolder;
                System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
                this.Close();
            }
            else if (String.Equals(MainWindow.proxyType, "SSR"))
            {
                string openFolderPath = @"ssr_config\" + saveFileFolder;
                System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
                this.Close();
            }
            else if (String.Equals(MainWindow.proxyType, "SS"))
            {
                string openFolderPath = @"ss_config\" + saveFileFolder;
                System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
                this.Close();
            }
        }

        //SSR生成URL链接
        private string GetSSRLinkForServer()
        {
            string server = TextBoxSSRHostAddress.Text;
            string server_port = TextBoxSSRPort.Text;
            string password = TextBoxSSRUUID.Text;
            string protocol = TextBoxSSRTransmission.Text;
            string method = TextBoxSSREncryption.Text;
            string obfs = TextBoxSSRCamouflageType.Text;

            string obfsparam="";
            string protocolparam = "";
            string remarks = TextBoxSSRHostAddress.Text;
            string group = TextBoxSSRHostAddress.Text;
            bool udp_over_tcp = true;
            int server_udp_port = 0;

            string main_part = server + ":" + server_port + ":" + protocol + ":" + method + ":" + obfs + ":" + EncodeUrlSafeBase64(password);
            string param_str = "obfsparam=" + EncodeUrlSafeBase64(obfsparam ?? "");
            if (!string.IsNullOrEmpty(protocolparam))
            {
                param_str += "&protoparam=" + EncodeUrlSafeBase64(protocolparam);
            }
            if (!string.IsNullOrEmpty(remarks))
            {
                param_str += "&remarks=" + EncodeUrlSafeBase64(remarks);
            }
            if (!string.IsNullOrEmpty(group))
            {
                param_str += "&group=" + EncodeUrlSafeBase64(group);
            }
            if (udp_over_tcp)
            {
                param_str += "&uot=" + "1";
            }
            if (server_udp_port > 0)
            {
                param_str += "&udpport=" + server_udp_port.ToString();
            }
            string base64 = EncodeUrlSafeBase64(main_part + "/?" + param_str);
            return "ssr://" + base64;
        }

        //SS生成URL链接，需要给出plugin的内容，用于生成手机端或电脑端的URL
        public string GetSStoURL(string plugin,bool legacyUrl = true)
        {
            string tag = string.Empty;
            string url = string.Empty;

            server = TextBoxHostAddressSS.Text;
            string server_port = TextBoxPortSS.Text;
            
            string password = TextBoxPasswordSS.Text;
            string method = TextBoxEncryptionSS.Text;

           // plugin = TextBoxPluginNameExplainSS.Text;
            string plugin_opts = TextBoxPluginOptionExplainSS.Text;

            string remarks = server;

            if (legacyUrl && string.IsNullOrWhiteSpace(plugin))
            {
                // For backwards compatiblity, if no plugin, use old url format
                string parts = $"{method}:{password}@{server}:{server_port}";
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(parts));
                url = base64;
            }
            else
            {
                // SIP002
                string parts = $"{method}:{password}";
                string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(parts));
                string websafeBase64 = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');

                url = string.Format(
                    "{0}@{1}:{2}/",
                    websafeBase64,
                    FormalHostName,
                    server_port
                    );

                if (!String.IsNullOrWhiteSpace(plugin))
                {

                    string pluginPart = plugin;
                    if (!String.IsNullOrWhiteSpace(plugin_opts))
                    {
                        pluginPart += ";" + plugin_opts;
                    }
                    string pluginQuery = "?plugin=" + HttpUtility.UrlEncode(pluginPart, Encoding.UTF8);
                    url += pluginQuery;
                }
            }

            if (!String.IsNullOrEmpty(remarks))
            {
                tag = $"#{HttpUtility.UrlEncode(remarks, Encoding.UTF8)}";
            }
            return $"ss://{url}{tag}";
        }

        //格式化主机名称
        private string FormalHostName
        {
            get
            {
                // CheckHostName() won't do a real DNS lookup
                switch (Uri.CheckHostName(server))
                {
                    case UriHostNameType.IPv6:  // Add square bracket when IPv6 (RFC3986)
                        return $"[{server}]";
                    default:    // IPv4 or domain name
                        return server;
                }
            }
        }
        //编码安全Base64
        private string EncodeUrlSafeBase64(byte[] val, bool trim)
        {
            if (trim)
                return Convert.ToBase64String(val).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            else
                return Convert.ToBase64String(val).Replace('+', '-').Replace('/', '_');
        }
        //编码安全Base64重载
        private string EncodeUrlSafeBase64(string val, bool trim = true)
        {
            return EncodeUrlSafeBase64(Encoding.UTF8.GetBytes(val), trim);
        }

        private void RadioButtonMobile_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxPluginNameExplainSS.Visibility = Visibility.Visible;
            GroupBoxClientQRandURL.Visibility = Visibility.Visible;
            
            TextBoxPluginNameExplainSSpc.Visibility = Visibility.Collapsed;
            GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
            
        }

        private void RadioButtonPC_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxPluginNameExplainSS.Visibility = Visibility.Collapsed;
            GroupBoxClientQRandURL.Visibility = Visibility.Collapsed;

            TextBoxPluginNameExplainSSpc.Visibility = Visibility.Visible;
            GroupBoxClientSSpc.Visibility = Visibility.Visible;
        }

    }

    
}
