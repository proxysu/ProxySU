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
        private string saveFileFolder = "";
        public ResultClientInformation()
        {
            InitializeComponent();

            if (String.Equals(MainWindow.proxyType, "V2Ray"))
            {
                GroupBoxV2rayClient.Visibility = Visibility.Visible;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
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
                //QUIC密钥
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
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2SRTP"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "srtp";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPuTP"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "utp";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WechatVideo"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "wechat-video";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2DTLS"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "dtls";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WireGuard"))
                {
                    TextBoxTransmission.Text = "kcp";
                    TextBoxCamouflageType.Text = "wireguard";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicNone"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "none";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicSRTP"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "srtp";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "Quic2uTP"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "utp";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicWechatVideo"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "wechat-video";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicDTLS"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "dtls";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
                }
                else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicWireGuard"))
                {
                    TextBoxTransmission.Text = "quic";
                    TextBoxCamouflageType.Text = "wireguard";
                    TextBoxQuicKey.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTLS.Text = "none";
                    HidePath();
                    ShowQuicKey();
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

                TextBoxTrojanGoWSPath.Visibility = Visibility.Hidden;
                TextBlockTrojanGoWebSocketPath.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCaption.Visibility = Visibility.Hidden;

                TextBlockQrURLexplain.Text = "可用于ShadowRocket (ios)、igniter（Android）、Trojan-QT5 (windows) 扫码和导入url";

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

                //主机地址
                TextBoxTrojanServerHost.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxTrojanServerPort.Text = "443";
                //密钥（uuid）
                TextBoxTrojanServerPassword.Text = MainWindow.ReceiveConfigurationParameters[2];

                TextBlockQrURLexplain.Text = "可用于ShadowRocket (ios)、igniter（Android）、Trojan-QT5 (windows) 扫码和导入url";

                CheckDir("trojan_config");
                GenerateTrojanShareQRcodeAndBase64Url();
            }
            else if (String.Equals(MainWindow.proxyType, "NaiveProxy"))
            {
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Visible;

                TextBlockQrURLexplain.Text = "用于NaiveGUI(windows)的URL导入链接";

                TextBoxNaiveServerHost.Text = MainWindow.ReceiveConfigurationParameters[4];
                TextBoxNaiveUser.Text = MainWindow.ReceiveConfigurationParameters[3];
                TextBoxNaivePassword.Text = MainWindow.ReceiveConfigurationParameters[2];
                GenerateNaivePrxoyShareQRcodeAndBase64Url();
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
            
            if (TextBoxTransmission.Text.Contains("quic")==true)
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
            //MessageBox.Show(v2rayNjsonObject.ToString());
            string vmessUrl = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = vmessUrl;
            using (StreamWriter sw = new StreamWriter($"v2ray_config\\{saveFileFolder}\\url.txt"))
            {
                  sw.WriteLine(vmessUrl);

            }
            CreateQRCode(vmessUrl,"v2ray_config");

            if (File.Exists(@"v2ray_config\config.json"))
            {
                File.Move(@"v2ray_config\config.json", @"v2ray_config\" + saveFileFolder + @"\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"v2ray_config\\{saveFileFolder}\\说明.txt"))
            {
                sw.WriteLine("config.json");
                sw.WriteLine("此文件为v2ray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081");
                sw.WriteLine("v2ray官方网站：https://www.v2ray.com/");
                sw.WriteLine("v2ray官方程序下载地址：https://github.com/v2ray/v2ray-core/releases");
                sw.WriteLine("下载相应版本，Windows选择v2ray-windows-64.zip或者v2ray-windows-32.zip，解压后提取v2ctl.exe和v2ray.exe。与config.json放在同一目录，运行v2ray.exe即可。");
                sw.WriteLine("-----------------------------------------");
                sw.WriteLine("QR.bmp");
                sw.WriteLine("此文件为v2rayN、Trojan-QT5、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点");
                sw.WriteLine("v2rayN下载网址：https://github.com/2dust/v2rayN/releases");
                sw.WriteLine("Trojan-QT5：https://github.com/Trojan-Qt5/Trojan-Qt5");
                sw.WriteLine("v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases");
                sw.WriteLine("v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang");
                sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");

                sw.WriteLine("-----------------------------------------");
                sw.WriteLine("url.txt");
                sw.WriteLine("此文件为v2rayN、Trojan-QT5、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址");
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine($"地址(address)：{TextBoxHostAddress.Text}");
                sw.WriteLine($"端口(Port)：{TextBoxPort.Text}");
                sw.WriteLine($"用户ID(uuid)：{TextBoxUUID.Text}");
                sw.WriteLine($"额外ID：{TextBoxUUIDextra.Text}");
                sw.WriteLine($"加密方式：{TextBoxEncryption.Text}");
                sw.WriteLine($"传输协议：{TextBoxTransmission.Text}");
                sw.WriteLine($"伪装类型：{TextBoxCamouflageType.Text}");
                sw.WriteLine($"是否使用TLS：{TextBoxTLS.Text}");
                sw.WriteLine($"host：{TextBoxHostAddress.Text}");
                sw.WriteLine($"路径(Path)：{TextBoxPath.Text}");
                sw.WriteLine($"mKCP/QUIC密钥：{TextBoxQuicKey.Text}");
            }



        }

        //生成TrojanGo客户端资料
        private void GenerateTrojanGoShareQRcodeAndBase64Url()
        {

            string saveFileFolderFirst = TextBoxTrojanServerHost.Text;
            int num = 1;
            saveFileFolder = saveFileFolderFirst;
            CheckDir("trojan-go_config");
            while (Directory.Exists(@"trojan-go_config\" + saveFileFolder))
            {
                saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
                num++;
            }
            CheckDir(@"trojan-go_config\" + saveFileFolder);
            string trojanUrl = $"trojan://{TextBoxTrojanServerPassword.Text}@{TextBoxTrojanServerHost.Text}:{TextBoxTrojanServerPort.Text}?allowinsecure=0&tfo=0&sni=&mux=0&ws=0&group=#{TextBoxTrojanServerHost.Text}";
            //MessageBox.Show(v2rayNjsonObject.ToString());
            //string trojanUrl = "trojan://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = trojanUrl;
            using (StreamWriter sw = new StreamWriter($"trojan-go_config\\{saveFileFolder}\\url.txt"))
            {
                sw.WriteLine(trojanUrl);

            }
            CreateQRCode(trojanUrl, "trojan-go_config");

            //移动Trojan官方程序配置文件到相应目录
            if (File.Exists(@"trojan-go_config\config.json"))
            {
                File.Move(@"trojan-go_config\config.json", @"trojan-go_config\" + saveFileFolder + @"\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"trojan-go_config\\{saveFileFolder}\\说明.txt"))
            {
                sw.WriteLine("config.json");
                sw.WriteLine("此文件为Trojan-go官方程序所使用的客户端配置文件，配置为全局模式，http与socks5地址：127.0.0.1:1080");
                sw.WriteLine("Trojan-go官方网站：https://github.com/p4gefau1t/trojan-go");
                sw.WriteLine("Trojan-go官方程序下载地址：https://github.com/p4gefau1t/trojan-go/releases");
                sw.WriteLine("下载相应版本，Windows选择Trojan-x.xx-win.zip,解压后提取trojan.exe。与config.json放在同一目录，运行trojan.exe即可。");
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("QR.bmp");
                sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点（Trojan-Go的WebSocket模式暂不支持）");
                sw.WriteLine("Trojan-QT5 (windows)下载网址：https://github.com/TheWanderingCoel/Trojan-Qt5/releases");
                sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
                sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");

                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");
                sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)复制粘贴导入节点的网址（Trojan-Go的WebSocket模式暂不支持）");
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine($"地址(address)：{TextBoxTrojanServerHost.Text}");
                sw.WriteLine($"端口(Port)：{TextBoxTrojanServerPort.Text}");
                sw.WriteLine($"密钥：{TextBoxTrojanServerPassword.Text}");
                sw.WriteLine($"WebSocket路径：{TextBoxTrojanGoWSPath.Text}");

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
            CreateQRCode(trojanUrl, "trojan_config");

            //移动Trojan官方程序配置文件到相应目录
            if (File.Exists(@"trojan_config\config.json"))
            {
                File.Move(@"trojan_config\config.json", @"trojan_config\" + saveFileFolder + @"\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"trojan_config\\{saveFileFolder}\\说明.txt"))
            {
                sw.WriteLine("config.json");
                sw.WriteLine("此文件为Trojan官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080");
                sw.WriteLine("Trojan官方网站：https://trojan-gfw.github.io/trojan/");
                sw.WriteLine("Trojan官方程序下载地址：https://github.com/trojan-gfw/trojan/releases");
                sw.WriteLine("下载相应版本，Windows选择Trojan-x.xx-win.zip,解压后提取trojan.exe。与config.json放在同一目录，运行trojan.exe即可。");
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("QR.bmp");
                sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点");
                sw.WriteLine("Trojan-QT5 (windows)下载网址：https://github.com/TheWanderingCoel/Trojan-Qt5/releases");
                sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
                sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");

                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");
                sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)复制粘贴导入节点的网址");
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine($"地址(address)：{TextBoxTrojanServerHost.Text}");
                sw.WriteLine($"端口(Port)：{TextBoxTrojanServerPort.Text}");
                sw.WriteLine($"密钥：{TextBoxTrojanServerPassword.Text}");

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

            using (StreamWriter sw = new StreamWriter($"naive_config\\{saveFileFolder}\\说明.txt"))
            {
                sw.WriteLine("config.json");
                sw.WriteLine("此文件为NaiveProxy官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080");
                sw.WriteLine("NaiveProxy官方网站：https://github.com/klzgrad/naiveproxy");
                sw.WriteLine("NaiveProxy官方程序下载地址：https://github.com/klzgrad/naiveproxy/releases");
                sw.WriteLine("下载相应版本，Windows选择naiveproxy-x.xx-win.zip,解压后提取naive.exe。与config.json放在同一目录，运行naive.exe即可。");
                sw.WriteLine("-----------------------------------------\n");
                //sw.WriteLine("其他平台的客户端，暂未发布");
                //sw.WriteLine("QR.bmp");
                //sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点");
                //sw.WriteLine("Trojan-QT5 (windows)下载网址：https://github.com/TheWanderingCoel/Trojan-Qt5/releases");
                //sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
                //sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");

                //sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");
                sw.WriteLine("此文件为NaiveGUI(windows)复制粘贴导入节点的网址");
                sw.WriteLine("NaiveGUI(windows)下载网址：https://github.com/ExcitedCodes/NaiveGUI/releases");

                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine($"地址(address)：{TextBoxNaiveServerHost.Text}");
                sw.WriteLine($"用户名：{TextBoxNaiveUser.Text}");
                sw.WriteLine($"密钥：{TextBoxNaivePassword.Text}");

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

        //生成QRcoder图片
        private void CreateQRCode(string varBase64,string configPath)
        {
            //string varBase64 = varBase64;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(varBase64, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            IntPtr myImagePtr = qrCodeImage.GetHbitmap();
            BitmapSource imgsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(myImagePtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            ImageShareQRcode.Source = imgsource;
            //DeleteObject(myImagePtr);
            qrCodeImage.Save($"{configPath}\\{saveFileFolder}\\QR.bmp");
            //ImageShareQRcode.Source = @"config\v2rayN.bmp";
        }

        //判断目录是否存在，不存在则创建
        private static bool CheckDir(string folder)
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
        }
    }
}
