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
using System.Runtime.InteropServices;


namespace ProxySU
{
    /// <summary>
    /// ResultClientInformation.xaml 的交互逻辑
    /// </summary>
    public partial class ResultClientInformation : Window
    {
        private static string saveFileFolder = "";
        private static string configDomainSavePath = "";
        private static string server = MainWindow.ReceiveConfigurationParameters[4];

        private static string TextBoxURLDefault;
        private static string TextBoxURLVlessTcp;
        private static string TextBoxURLVlessWs;
        private static string TextBoxURLVmessTcp;
        private static string TextBoxURLVmessWs;
        private static string TextBoxURLTrojanTcp;

        private static BitmapSource ImageShareQRcodeDefault;
        private static BitmapSource ImageShareQRcodeVlessTcp;
        private static BitmapSource ImageShareQRcodeVlessWs;
        private static BitmapSource ImageShareQRcodeVmessTcp;
        private static BitmapSource ImageShareQRcodeVmessWs;
        private static BitmapSource ImageShareQRcodeTrojanTcp;
      

        [DllImport("user32.dll")]
        private static extern int MessageBoxTimeoutA(IntPtr hWnd, string msg, string Caps, int type, int Id, int time);   //引用DLL
 
        public ResultClientInformation()
        {
            InitializeComponent();

            if (String.Equals(MainWindow.proxyType, "V2Ray"))
            {
                //显示V2Ray参数，隐藏其他
                GroupBoxClientMTProto.Visibility = Visibility.Collapsed;
                GroupBoxClientQRandURL.Visibility = Visibility.Visible;
                GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
                GroupBoxV2rayClient.Visibility = Visibility.Visible;
                GroupBoxXrayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                //主机地址
                TextBoxHostAddress.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxPort.Text = MainWindow.ReceiveConfigurationParameters[1];
                //用户ID(uuid)/Trojan密码
                TextBoxUUID.Text = MainWindow.ReceiveConfigurationParameters[2];
                //额外ID
                TextBoxUUIDextra.Text = "0";
                //加密方式，一般都为auto
                TextBoxEncryption.Text = "auto";

                //传输协议
                TextBoxTransmission.Text = "";
                //伪装类型
                TextBoxCamouflageType.Text = MainWindow.ReceiveConfigurationParameters[5];

                //TLS的Host /Quic 加密方式
                TextBoxHostQuicEncryption.Text = "";

                //QUIC密钥/mKCP Seed/路径Path
                TextBoxQuicKeyMkcpSeedPath.Text = MainWindow.ReceiveConfigurationParameters[6];

                //TLS设置
                TextBoxTLS.Text = "none";

                //初始化时，隐藏多方案客户端选择面板
                GroupBoxSelectVlessVmessXtlsTcpWs.Visibility = Visibility.Collapsed;
                //显示非Trojan的所有参数
                GridNotTrojanParameters.Visibility = Visibility.Visible;

                if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == false)
                {
                    #region 单模式方案
                    if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCP") == true)
                    {
                        TextBoxTransmission.Text = "tcp";
                        TextBoxCamouflageType.Text = "none";
                        TextBoxTLS.Text = "none";
                        ShowHostName();
                        ShowPathV2ray();

                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCPhttp") == true)
                    {
                        TextBoxTransmission.Text = "tcp";
                        TextBoxCamouflageType.Text = "http";
                        TextBoxTLS.Text = "none";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "tcpTLS") == true)
                    {
                        TextBoxEncryption.Text = "none";
                        TextBoxTransmission.Text = "tcp";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLS.Text = "tls";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "tcpTLSselfSigned") == true)
                    {
                        TextBoxTransmission.Text = "tcp";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLS.Text = "tls";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true)
                    {
                        TextBlockVmessOrVless.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();

                        TextBoxTransmission.Text = "tcp";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxEncryption.Text = "none";
                        TextBoxTLS.Text = "xtls";
                        HideAlterId();
                        ShowHostName();
                        ShowPathV2ray();
                        HideGroupBoxClientQRandURL();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true)
                    {
                        TextBlockVmessOrVless.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();

                        TextBoxTransmission.Text = "tcp";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxEncryption.Text = "none";
                        TextBoxTLS.Text = "tls";
                        HideAlterId();
                        ShowHostName();
                        ShowPathV2ray();
                        HideGroupBoxClientQRandURL();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true)
                    {
                        TextBlockVmessOrVless.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();

                        TextBoxTransmission.Text = "ws";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxEncryption.Text = "none";
                        TextBoxTLS.Text = "tls";
                        HideAlterId();
                        ShowHostName();
                        ShowPathV2ray();
                        HideGroupBoxClientQRandURL();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
                    {
                        TextBlockVmessOrVless.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();

                        TextBoxTransmission.Text = "h2";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxEncryption.Text = "none";
                        TextBoxTLS.Text = "tls";
                        HideAlterId();
                        ShowHostName();
                        ShowPathV2ray();
                        HideGroupBoxClientQRandURL();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "webSocket") == true)
                    {
                        TextBoxTransmission.Text = "ws";
                        TextBoxCamouflageType.Text = "none";
                        TextBoxTLS.Text = "none";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLS") == true)
                    {
                        TextBoxEncryption.Text = "none";
                        TextBoxTransmission.Text = "ws";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLS.Text = "tls";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true)
                    {
                        TextBoxEncryption.Text = "none";
                        TextBoxTransmission.Text = "ws";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLS.Text = "tls";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true)
                    {
                        TextBoxTransmission.Text = "ws";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLS.Text = "tls";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "Http2") == true)
                    {
                        TextBoxEncryption.Text = "none";
                        TextBoxTransmission.Text = "h2";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLS.Text = "tls";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "http2Web") == true)
                    {
                        TextBoxEncryption.Text = "none";
                        TextBoxTransmission.Text = "h2";
                        TextBoxCamouflageType.Text = "none";
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];//获取Host
                        TextBoxTLS.Text = "tls";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "http2selfSigned") == true)
                    {
                        TextBoxTransmission.Text = "h2";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLS.Text = "tls";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    else if (MainWindow.ReceiveConfigurationParameters[0].Contains("mKCP") == true)
                    {
                        if(MainWindow.mKCPvlessIsSet == true)
                        {
                            TextBlockVmessOrVless.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();
                            TextBoxEncryption.Text = "none";
                            HideAlterId();
                            HideGroupBoxClientQRandURL();
                        }
                        if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPNone") == true)
                        {
                            TextBoxCamouflageType.Text = "none";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2SRTP") == true)
                        {
                            TextBoxCamouflageType.Text = "srtp";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPuTP") == true)
                        {
                            TextBoxCamouflageType.Text = "utp";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WechatVideo") == true)
                        {
                            TextBoxCamouflageType.Text = "wechat-video";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2DTLS") == true)
                        {
                            TextBoxCamouflageType.Text = "dtls";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WireGuard") == true)
                        {
                            TextBoxCamouflageType.Text = "wireguard";
                        }

                        TextBoxTransmission.Text = "kcp";
                        TextBoxQuicKeyMkcpSeedPath.Text = MainWindow.ReceiveConfigurationParameters[6];//获取mkcp Seed
                        TextBoxTLS.Text = "none";
                        ShowHostName();
                        ShowMkcpSeed();
                    }
                    else if (MainWindow.ReceiveConfigurationParameters[0].Contains("Quic") == true)
                    {
                        if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicNone") == true)
                        {
                            TextBoxCamouflageType.Text = "none";

                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicSRTP") == true)
                        {
                            TextBoxCamouflageType.Text = "srtp";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "Quic2uTP") == true)
                        {
                            TextBoxCamouflageType.Text = "utp";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicWechatVideo") == true)
                        {
                            TextBoxCamouflageType.Text = "wechat-video";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicDTLS") == true)
                        {
                            TextBoxCamouflageType.Text = "dtls";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicWireGuard") == true)
                        {
                            TextBoxCamouflageType.Text = "wireguard";
                        }

                        TextBoxTransmission.Text = "quic";
                        TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[3];//获取Quic加密方式
                        TextBoxQuicKeyMkcpSeedPath.Text = MainWindow.ReceiveConfigurationParameters[6];//获取Quic加密密钥
                        TextBoxTLS.Text = "none";
                        ShowQuicEncryption();
                        ShowQuicKey();
                    }

                    else
                    {
                        TextBoxTransmission.Text = "tcp";
                        TextBoxCamouflageType.Text = "none";
                        TextBoxTLS.Text = "none";
                        ShowHostName();
                        ShowPathV2ray();
                    }
                    CheckDir("v2ray_config");

                    GenerateV2rayShareQRcodeAndBase64Url();
                    #endregion
                }
                else
                {
                    //显示多方案客户端选择面板
                    GroupBoxSelectVlessVmessXtlsTcpWs.Visibility = Visibility.Visible;

                    string proxyfolder = CheckDir("v2ray_config");
                    configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxHostAddress.Text);

                    V2raySetVlessTcpXtls();
                    //GenerateV2rayVlessTcpXtlsShareQRcodeAndBase64Url();

                    V2raySetVlessTcpTls();
                    GenerateV2rayVlessTcpTlsShareQRcodeAndBase64Url();

                    V2raySetVlessWsTls();
                    GenerateV2rayVlessWsTlsShareQRcodeAndBase64Url();

                    V2raySetVmessTcpTls();
                    GenerateV2rayVmessTcpTlsShareQRcodeAndBase64Url();

                    V2raySetVmessWsTls();
                    GenerateV2rayVmessWsTlsShareQRcodeAndBase64Url();

                    GenerateV2rayTrojanShareQRcodeAndBase64Url();
                    RadioButtonVlessTcpTls.IsChecked = true;
                }

            }
            else if (String.Equals(MainWindow.proxyType, "Xray"))
            {
                //显示Xray参数，隐藏其他
                GroupBoxClientMTProto.Visibility = Visibility.Collapsed;
                GroupBoxClientQRandURL.Visibility = Visibility.Visible;
                GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxXrayClient.Visibility = Visibility.Visible;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                //主机地址
                TextBoxHostAddressXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxPortXray.Text = MainWindow.ReceiveConfigurationParameters[1];
                //用户ID(uuid)/Trojan密码
                TextBoxUUIDXray.Text = MainWindow.ReceiveConfigurationParameters[2];
                //额外ID
                TextBoxUUIDextraXray.Text = "0";
                //加密方式，一般都为auto
                TextBoxEncryptionXray.Text = "auto";

                //传输协议
                TextBoxTransmissionXray.Text = "";
                //伪装类型
                TextBoxCamouflageTypeXray.Text = MainWindow.ReceiveConfigurationParameters[5];

                //TLS的Host /Quic 加密方式
                TextBoxHostQuicEncryptionXray.Text = "";

                //QUIC密钥/mKCP Seed/路径Path
                TextBoxQuicKeyMkcpSeedPathXray.Text = MainWindow.ReceiveConfigurationParameters[6];

                //TLS设置
                TextBoxTLSXray.Text = "none";

                //初始化时，隐藏多方案客户端选择面板
                GroupBoxSelectVlessVmessXtlsTcpWsXray.Visibility = Visibility.Collapsed;
                //显示非Trojan的所有参数
                GridNotTrojanParametersXray.Visibility = Visibility.Visible;

                if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == false)
                {
                    #region 单模式方案
                    if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCP") == true)
                    {
                        TextBoxTransmissionXray.Text = "tcp";
                        TextBoxCamouflageTypeXray.Text = "none";
                        TextBoxTLSXray.Text = "none";
                        ShowHostNameXray();
                        ShowPathXray();

                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCPhttp") == true)
                    {
                        TextBoxTransmissionXray.Text = "tcp";
                        TextBoxCamouflageTypeXray.Text = "http";
                        TextBoxTLSXray.Text = "none";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "tcpTLS") == true)
                    {
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTransmissionXray.Text = "tcp";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLSXray.Text = "tls";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "tcpTLSselfSigned") == true)
                    {
                        TextBoxTransmissionXray.Text = "tcp";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLSXray.Text = "tls";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true)
                    {
                        TextBlockVmessOrVlessXray.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();

                        TextBoxTransmissionXray.Text = "tcp";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTLSXray.Text = "xtls";
                        HideAlterIdXray();
                        ShowHostNameXray();
                        ShowPathXray();
                        HideGroupBoxClientQRandURL();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true)
                    {
                        TextBlockVmessOrVlessXray.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();

                        TextBoxTransmissionXray.Text = "tcp";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTLSXray.Text = "tls";
                        HideAlterIdXray();
                        ShowHostNameXray();
                        ShowPathXray();
                        HideGroupBoxClientQRandURL();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true)
                    {
                        TextBlockVmessOrVlessXray.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();

                        TextBoxTransmissionXray.Text = "ws";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTLSXray.Text = "tls";
                        HideAlterIdXray();
                        ShowHostNameXray();
                        ShowPathXray();
                        HideGroupBoxClientQRandURL();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
                    {
                        TextBlockVmessOrVlessXray.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();

                        TextBoxTransmissionXray.Text = "h2";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTLSXray.Text = "tls";
                        HideAlterIdXray();
                        ShowHostNameXray();
                        ShowPathXray();
                        HideGroupBoxClientQRandURL();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "webSocket") == true)
                    {
                        TextBoxTransmissionXray.Text = "ws";
                        TextBoxCamouflageTypeXray.Text = "none";
                        TextBoxTLSXray.Text = "none";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLS") == true)
                    {
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTransmissionXray.Text = "ws";
                        TextBoxCamouflageType.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLSXray.Text = "tls";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true)
                    {
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTransmissionXray.Text = "ws";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLSXray.Text = "tls";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true)
                    {
                        TextBoxTransmissionXray.Text = "ws";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLSXray.Text = "tls";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "Http2") == true)
                    {
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTransmissionXray.Text = "h2";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLSXray.Text = "tls";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "http2Web") == true)
                    {
                        TextBoxEncryptionXray.Text = "none";
                        TextBoxTransmissionXray.Text = "h2";
                        TextBoxCamouflageTypeXray.Text = "none";
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];//获取Host
                        TextBoxTLSXray.Text = "tls";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "http2selfSigned") == true)
                    {
                        TextBoxTransmissionXray.Text = "h2";
                        TextBoxCamouflageTypeXray.Text = "none";
                        //TLS的Host /Quic 加密方式
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
                        TextBoxTLSXray.Text = "tls";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    else if (MainWindow.ReceiveConfigurationParameters[0].Contains("mKCP") == true)
                    {
                        if (MainWindow.mKCPvlessIsSet == true)
                        {
                            TextBlockVmessOrVlessXray.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();
                            TextBoxEncryptionXray.Text = "none";
                            HideAlterIdXray();
                            HideGroupBoxClientQRandURL();
                        }
                        if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPNone") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "none";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2SRTP") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "srtp";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPuTP") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "utp";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WechatVideo") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "wechat-video";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2DTLS") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "dtls";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WireGuard") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "wireguard";
                        }

                        TextBoxTransmissionXray.Text = "kcp";
                        TextBoxQuicKeyMkcpSeedPathXray.Text = MainWindow.ReceiveConfigurationParameters[6];//获取mkcp Seed
                        TextBoxTLSXray.Text = "none";
                        ShowHostNameXray();
                        ShowMkcpSeedXray();
                    }
                    else if (MainWindow.ReceiveConfigurationParameters[0].Contains("Quic") == true)
                    {
                        if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicNone") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "none";

                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicSRTP") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "srtp";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "Quic2uTP") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "utp";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicWechatVideo") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "wechat-video";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicDTLS") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "dtls";
                        }
                        else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "QuicWireGuard") == true)
                        {
                            TextBoxCamouflageTypeXray.Text = "wireguard";
                        }

                        TextBoxTransmissionXray.Text = "quic";
                        TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[3];//获取Quic加密方式
                        TextBoxQuicKeyMkcpSeedPathXray.Text = MainWindow.ReceiveConfigurationParameters[6];//获取Quic加密密钥
                        TextBoxTLSXray.Text = "none";
                        ShowQuicEncryptionXray();
                        ShowQuicKeyXray();
                    }

                    else
                    {
                        TextBoxTransmissionXray.Text = "tcp";
                        TextBoxCamouflageTypeXray.Text = "none";
                        TextBoxTLSXray.Text = "none";
                        ShowHostNameXray();
                        ShowPathXray();
                    }
                    CheckDir("xray_config");

                    GenerateXrayShareQRcodeAndBase64Url();
                    #endregion
                }
                else
                {
                    //显示多方案客户端选择面板
                    GroupBoxSelectVlessVmessXtlsTcpWsXray.Visibility = Visibility.Visible;

                    string proxyfolder = CheckDir("xray_config");
                    configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxHostAddressXray.Text);

                    XraySetVlessTcpXtls();
                    GenerateXrayVlessTcpXtlsShareQRcodeAndBase64Url();

                    XraySetVlessTcpTls();
                    GenerateXrayVlessTcpTlsShareQRcodeAndBase64Url();

                    XraySetVlessWsTls();
                    GenerateXrayVlessWsTlsShareQRcodeAndBase64Url();

                    XraySetVmessTcpTls();
                    GenerateXrayVmessTcpTlsShareQRcodeAndBase64Url();

                    XraySetVmessWsTls();
                    GenerateXrayVmessWsTlsShareQRcodeAndBase64Url();

                    GenerateXrayTrojanShareQRcodeAndBase64Url();
                    RadioButtonVlessTcpXtlsXray.IsChecked = true;
                }

            }
            else if (String.Equals(MainWindow.proxyType, "TrojanGo"))
            {
                //GroupBoxTrojanClient.Header = "Trojan-go 客户端配置参数";
                GroupBoxClientMTProto.Visibility = Visibility.Collapsed;
                GroupBoxClientQRandURL.Visibility = Visibility.Visible;
                GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxXrayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Visible;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                TextBlockTrojanGoHost.Visibility = Visibility.Hidden;
                TextBoxTrojanGoHost.Visibility = Visibility.Hidden;
                TextBoxTrojanGoWSPath.Visibility = Visibility.Hidden;
                TextBlockTrojanGoWebSocketPath.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCaption.Visibility = Visibility.Hidden;

                TextBlockMuxSelect.Visibility = Visibility.Hidden;
                GridMuxSelect.Visibility = Visibility.Hidden;
                TextBlockExplainCheckBoxMuxSelectResult.Visibility = Visibility.Hidden;

                //******"可用于ShadowRocket (ios)、igniter（Android）、Qv2ray (windows) 扫码和导入url。注意：有的客户端可能不支持WebSocket模式。" ******
                TextBlockQrURLexplain.Text = Application.Current.FindResource("TextBlockQrURLexplainTrojan-go").ToString();

                //主机地址
                TextBoxTrojanGoServerHost.Text = MainWindow.ReceiveConfigurationParameters[4];
                //主机端口
                TextBoxTrojanGoServerPort.Text = "443";
                //密钥（uuid）
                TextBoxTrojanGoServerPassword.Text = MainWindow.ReceiveConfigurationParameters[2];
                //SNI
                TextBoxTrojanGoSNI.Text = MainWindow.ReceiveConfigurationParameters[4];
                //TrojanGo 使用类型
                TextBoxTrojanGoType.Text = "original";
                
                //WebSocket设置
                if (MainWindow.ReceiveConfigurationParameters[0].Equals("TrojanGoWebSocketTLS2Web"))
                {
                    TextBoxTrojanGoType.Text = "ws";
                    //Host
                    TextBoxTrojanGoHost.Text = MainWindow.ReceiveConfigurationParameters[4];
                    TextBlockTrojanGoHost.Visibility = Visibility.Visible;
                    TextBoxTrojanGoHost.Visibility = Visibility.Visible;
                    //Path
                    TextBoxTrojanGoWSPath.Text = MainWindow.ReceiveConfigurationParameters[6];
                    TextBoxTrojanGoWSPath.Visibility = Visibility.Visible;
                    TextBlockTrojanGoWebSocketPath.Visibility = Visibility.Visible;
                    TextBlockTrojanGoCaption.Visibility = Visibility.Visible;

                }
                if(String.Equals(MainWindow.ReceiveConfigurationParameters[9],"true") == true)
                {
                    TextBoxConcurrency.Text = MainWindow.ReceiveConfigurationParameters[3];
                    TextBoxIdle_timeout.Text = MainWindow.ReceiveConfigurationParameters[5];
                    TextBlockMuxSelect.Visibility = Visibility.Visible;
                    GridMuxSelect.Visibility = Visibility.Visible;
                    TextBlockExplainCheckBoxMuxSelectResult.Visibility = Visibility.Visible;
                }
                CheckDir("trojan-go_config");
                GenerateTrojanGoShareQRcodeAndBase64Url();
            }
            else if (String.Equals(MainWindow.proxyType, "Trojan"))
            {
                GroupBoxClientMTProto.Visibility = Visibility.Collapsed;
                GroupBoxClientQRandURL.Visibility = Visibility.Visible;
                GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxXrayClient.Visibility = Visibility.Collapsed;
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
                GroupBoxClientMTProto.Visibility = Visibility.Collapsed;
                GroupBoxClientQRandURL.Visibility = Visibility.Visible;
                GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxXrayClient.Visibility = Visibility.Collapsed;
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
                GroupBoxClientMTProto.Visibility = Visibility.Collapsed;
                GroupBoxClientQRandURL.Visibility = Visibility.Visible;
                GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxXrayClient.Visibility = Visibility.Collapsed;
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
                GroupBoxClientMTProto.Visibility = Visibility.Collapsed;
                GroupBoxClientQRandURL.Visibility = Visibility.Visible;
                GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxXrayClient.Visibility = Visibility.Collapsed;
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
                TextBoxEncryptionSS.Text = MainWindow.ReceiveConfigurationParameters[3];
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
            else if (String.Equals(MainWindow.proxyType, "MTProto"))
            {
                //显示MTProto参数，隐藏其他
                GroupBoxClientMTProto.Visibility = Visibility.Visible;
                GroupBoxClientQRandURL.Visibility = Visibility.Collapsed;
                GroupBoxClientSSpc.Visibility = Visibility.Collapsed;
                GroupBoxV2rayClient.Visibility = Visibility.Collapsed;
                GroupBoxXrayClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanGoClient.Visibility = Visibility.Collapsed;
                GroupBoxTrojanClient.Visibility = Visibility.Collapsed;
                GroupBoxNaiveProxyClient.Visibility = Visibility.Collapsed;
                GroupBoxSSRClient.Visibility = Visibility.Collapsed;
                GroupBoxClientSS.Visibility = Visibility.Collapsed;

                string proxyfolder = CheckDir("mtproto_config");
                configDomainSavePath = CreateConfigSaveDir(proxyfolder, MainWindow.ReceiveConfigurationParameters[4]);
                string configSavePath = configDomainSavePath;

                RadioButtonMtgIpv4.IsChecked = true;

                JObject jObjectJson = JObject.Parse(MainWindow.ReceiveConfigurationParameters[9]);

                if (jObjectJson["ipv6"]["tg_url"].ToString().Contains("nil") == false)
                {
                    RadioButtonMtgIpv6.Visibility = Visibility.Visible;
                    TextBoxURLMtgTgIpv6.Text = jObjectJson["ipv6"]["tg_url"].ToString();
                    ImageShareQRcodeMtgTgIpv6.Source = CreateQRCode(TextBoxURLMtgTgIpv6.Text, $"{configSavePath}\\QRIpv6Tg.bmp");
                    using (StreamWriter sw = new StreamWriter($"{configSavePath}\\urlIpv6Tg.txt"))
                    {
                        sw.WriteLine(TextBoxURLMtgTgIpv6.Text);
                    }

                    TextBoxURLMtgTmeIpv6.Text = jObjectJson["ipv6"]["tme_url"].ToString();
                    ImageShareQRcodeMtgTmeIpv6.Source = CreateQRCode(TextBoxURLMtgTmeIpv6.Text, $"{configSavePath}\\QRIpv6Tme.bmp");
                    using (StreamWriter sw = new StreamWriter($"{configSavePath}\\urlIpv6Tme.txt"))
                    {
                        sw.WriteLine(TextBoxURLMtgTmeIpv6.Text);
                    }
                    RadioButtonMtgIpv6.IsChecked = true;
                }
                else
                {
                    RadioButtonMtgIpv6.Visibility = Visibility.Collapsed;
                }

                if (jObjectJson["ipv4"]["tg_url"].ToString().Contains("nil") == false)
                {
                    RadioButtonMtgIpv4.Visibility = Visibility.Visible;
                    TextBoxURLMtgTgIpv4.Text = jObjectJson["ipv4"]["tg_url"].ToString();
                    ImageShareQRcodeMtgTgIpv4.Source = CreateQRCode(TextBoxURLMtgTgIpv4.Text, $"{configSavePath}\\QRIpv4Tg.bmp");
                    using (StreamWriter sw = new StreamWriter($"{configSavePath}\\urlIpv4Tg.txt"))
                    {
                        sw.WriteLine(TextBoxURLMtgTgIpv4.Text);
                    }

                    TextBoxURLMtgTmeIpv4.Text = jObjectJson["ipv4"]["tme_url"].ToString();
                    ImageShareQRcodeMtgTmeIpv4.Source = CreateQRCode(TextBoxURLMtgTmeIpv4.Text, $"{configSavePath}\\QRIpv4Tme.bmp");
                    using (StreamWriter sw = new StreamWriter($"{configSavePath}\\urlIpv4Tme.txt"))
                    {
                        sw.WriteLine(TextBoxURLMtgTmeIpv4.Text);
                    }
                    RadioButtonMtgIpv4.IsChecked = true;
                }
                else
                {
                    RadioButtonMtgIpv4.Visibility = Visibility.Collapsed;
                }

                using (StreamWriter sw = new StreamWriter($"{configSavePath}\\mtproto_info.json"))
                {
                    sw.Write(MainWindow.ReceiveConfigurationParameters[9].ToString());
                }

            }
        }

        #region V2Ray 相关

        #region V2ray参数设置函数

        //设置VLESS over TCP with XTLS
        private void V2raySetVlessTcpXtls()
        {
            GridNotTrojanParameters.Visibility = Visibility.Visible;
            TextBlockVmessOrVless.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();
            TextBlockVmessOrVless.Visibility = Visibility.Visible;
            //隐藏下面的二维码显示
            HideGroupBoxClientQRandURL();
            //ImageShareQRcode.Source = ImageShareQRcodeDefault;
            //TextBoxURL.Text = TextBoxURLDefault;

            TextBoxEncryption.Text = "none";
            TextBoxTransmission.Text = "tcp";
            TextBoxCamouflageType.Text = "none";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLS.Text = "xtls";
            HideAlterId();
            ShowHostName();
            ShowPathV2ray();
            TextBoxQuicKeyMkcpSeedPath.Text = "";


        }

        //设置VLESS over TCP with TLS
        private void V2raySetVlessTcpTls()
        {
            GridNotTrojanParameters.Visibility = Visibility.Visible;
            TextBlockVmessOrVless.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();
            TextBlockVmessOrVless.Visibility = Visibility.Visible;
            //隐藏下面的二维码显示
            HideGroupBoxClientQRandURL();
            //TextBoxURL.Text = TextBoxURLVlessTcp;
            //ImageShareQRcode.Source = ImageShareQRcodeVlessTcp;

            TextBoxEncryption.Text = "none";
            TextBoxTransmission.Text = "tcp";
            TextBoxCamouflageType.Text = "none";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLS.Text = "tls";
            HideAlterId();
            ShowHostName();
            ShowPathV2ray();
            TextBoxQuicKeyMkcpSeedPath.Text = "";
        }

        //设置VLESS over WS with TLS
        private void V2raySetVlessWsTls()
        {
            GridNotTrojanParameters.Visibility = Visibility.Visible;
            TextBlockVmessOrVless.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();
            TextBlockVmessOrVless.Visibility = Visibility.Visible;
            //隐藏下面的二维码显示
            HideGroupBoxClientQRandURL();
            //TextBoxURL.Text = TextBoxURLVlessWs;
            //ImageShareQRcode.Source = ImageShareQRcodeVlessWs;

            TextBoxEncryption.Text = "none";
            TextBoxTransmission.Text = "ws";
            TextBoxCamouflageType.Text = "none";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLS.Text = "tls";
            HideAlterId();
            ShowHostName();
            ShowPathV2ray();
            TextBoxQuicKeyMkcpSeedPath.Text = MainWindow.ReceiveConfigurationParameters[3];

        }

        //设置VMess over TCP with TLS
        private void V2raySetVmessTcpTls()
        {
            GridNotTrojanParameters.Visibility = Visibility.Visible;
            TextBoxEncryption.Text = "none";
            TextBoxTransmission.Text = "tcp";
            TextBoxCamouflageType.Text = "http";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLS.Text = "tls";
            ShowAlterId();
            ShowHostName();
            ShowPathV2ray();
            TextBoxQuicKeyMkcpSeedPath.Text = MainWindow.ReceiveConfigurationParameters[9];
            TextBlockVmessOrVless.Visibility = Visibility.Collapsed;
            //显示下面的二维码显示。
            //HideGroupBoxClientQRandURL();
            ShowGroupBoxClientQRandURL();
            TextBoxURL.Text = TextBoxURLVmessTcp;
            ImageShareQRcode.Source = ImageShareQRcodeVmessTcp;
        }

        //设置VMess over WS with TLS
        private void V2raySetVmessWsTls()
        {
            GridNotTrojanParameters.Visibility = Visibility.Visible;
            TextBoxEncryption.Text = "none";
            TextBoxTransmission.Text = "ws";
            TextBoxCamouflageType.Text = "none";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryption.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLS.Text = "tls";
            ShowAlterId();
            ShowHostName();
            ShowPathV2ray();
            TextBoxQuicKeyMkcpSeedPath.Text = MainWindow.ReceiveConfigurationParameters[6];
            TextBlockVmessOrVless.Visibility = Visibility.Collapsed;
            ShowGroupBoxClientQRandURL();
            TextBoxURL.Text = TextBoxURLVmessWs;
            ImageShareQRcode.Source = ImageShareQRcodeVmessWs;
        }

        //设置Trojan over TCP with TLS
        private void V2raySetTrojanTcpTls()
        {
            //隐藏所有不是Trojan的参数
            GridNotTrojanParameters.Visibility = Visibility.Collapsed;
            //显示下面的二维码与分享链接
            ShowGroupBoxClientQRandURL();
            TextBoxURL.Text = TextBoxURLTrojanTcp;
            ImageShareQRcode.Source = ImageShareQRcodeTrojanTcp;
        }
        #endregion

        #region 界面控制相关

        //显示Quic 加密方式
        private void ShowQuicEncryption()
        {
            TextBlockQuicEncryption.Visibility = Visibility.Visible;
            TextBlockHost.Visibility = Visibility.Collapsed;
        }
        
        //显示Host隐藏Quic加密方式
        private void ShowHostName()
        {
            TextBlockHost.Visibility = Visibility.Visible;
            TextBlockQuicEncryption.Visibility = Visibility.Collapsed;
        }
        
        //显示路径Path,隐藏mKCP/Quic Key/复制按钮
        private void ShowPathV2ray()
        {
            TextBlockPath.Visibility = Visibility.Visible;
            TextBlockMkcpSeed.Visibility = Visibility.Collapsed;
            TextBlockQuicKey.Visibility = Visibility.Collapsed;
        }
        
        //显示mKCP Seed/复制按钮，隐藏Path/Quic Key
        private void ShowMkcpSeed()
        {
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBlockMkcpSeed.Visibility = Visibility.Visible;
            TextBlockQuicKey.Visibility = Visibility.Collapsed;
        }

        //显示Quic Key/复制按钮 隐藏Path/mKcp Seed
        private void ShowQuicKey()
        {
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBlockMkcpSeed.Visibility = Visibility.Collapsed;
            TextBlockQuicKey.Visibility = Visibility.Visible;
        }

        //显示额外ID
        private void ShowAlterId()
        {
            TextBlockUUIDextra.Visibility = Visibility.Visible;
            TextBoxUUIDextra.Visibility = Visibility.Visible;
            TextBlockUUIDextraExplanation.Visibility = Visibility.Visible;

        }

        //隐藏额外ID
        private void HideAlterId()
        {
            TextBlockUUIDextra.Visibility = Visibility.Collapsed;
            TextBoxUUIDextra.Visibility = Visibility.Collapsed;
            TextBlockUUIDextraExplanation.Visibility = Visibility.Collapsed;

        }

        //显示加密方式
        private void ShowEncryption()
        {
            TextBlockEncryption.Visibility = Visibility.Visible;
            TextBoxEncryption.Visibility = Visibility.Visible;
        }
        //隐藏加密方式
        private void HideEncryption()
        {
            TextBlockEncryption.Visibility = Visibility.Collapsed;
            TextBoxEncryption.Visibility = Visibility.Collapsed;
        }

        //显示传输协议
        private void ShowTransferProtocol()
        {
            TextBlockTransferProtocol.Visibility = Visibility.Visible;
            TextBoxTransmission.Visibility = Visibility.Visible;
        }
        //隐藏传输协议
        private void HideTransferProtocol()
        {
            TextBlockTransferProtocol.Visibility = Visibility.Collapsed;
            TextBoxTransmission.Visibility = Visibility.Collapsed;
        }
        //显示伪装类型
        private void ShowTextBlockCamouflageType()
        {
            TextBlockCamouflageType.Visibility = Visibility.Visible;
            TextBoxCamouflageType.Visibility = Visibility.Visible;
        }
        //隐藏伪装类型
        private void HideTextBlockCamouflageType()
        {
            TextBlockCamouflageType.Visibility = Visibility.Collapsed;
            TextBoxCamouflageType.Visibility = Visibility.Collapsed;
        }

        //显示二维码与链接分享
        private void ShowGroupBoxClientQRandURL()
        {
            GroupBoxClientQRandURL.Visibility = Visibility.Visible;
        }
        //隐藏二维码与链接分享
        private void HideGroupBoxClientQRandURL()
        {
            GroupBoxClientQRandURL.Visibility = Visibility.Hidden;
        }

        //以下几个为对RadioButton按钮的选中后，界面变化与参数显示
        private void RadioButtonVlessTcpXtls_Checked(object sender, RoutedEventArgs e)
        {
            V2raySetVlessTcpXtls();
        }

        private void RadioButtonVlessTcpTls_Checked(object sender, RoutedEventArgs e)
        {
            V2raySetVlessTcpTls();
        }

        private void RadioButtonVlessWsTls_Checked(object sender, RoutedEventArgs e)
        {
            V2raySetVlessWsTls();
        }

        private void RadioButtonVmessTcpTls_Checked(object sender, RoutedEventArgs e)
        {
            V2raySetVmessTcpTls();
        }

        private void RadioButtonVmessWsTls_Checked(object sender, RoutedEventArgs e)
        {
            V2raySetVmessWsTls();
        }

        private void RadioButtonTrojanTcpTls_Checked(object sender, RoutedEventArgs e)
        {
            V2raySetTrojanTcpTls();
        }
        #endregion

        #region 复制参数到剪贴板中

        //复制内容到剪贴板函数
        private void CopyToClipboard(string content)
        {
            if (content != "")
            {
                Clipboard.SetDataObject(content);
                //MessageBox.Show(Application.Current.FindResource("MessageBoxShow_V2RayUUIDcopyedToClip").ToString());
                string message = Application.Current.FindResource("MessageBoxShow_V2RayUUIDcopyedToClip").ToString();
                MessageBoxTimeoutA((IntPtr)0, message, "", 0, 0, 600);    // 直接调用  0.6秒后自动关闭 
            }
            else
            {
                //MessageBox.Show(Application.Current.FindResource("MessageBoxShow_V2RayEmptyToClip").ToString());
                string message = Application.Current.FindResource("MessageBoxShow_V2RayEmptyToClip").ToString();
                MessageBoxTimeoutA((IntPtr)0, message, "", 0, 0, 600);    // 直接调用  0.6秒后自动关闭 
            }
        }

        //复制服务器地址到剪贴板
        private void TextBoxHostAddress_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxHostAddress.Text);
        }


        //复制服务器端口到剪贴板
        private void TextBoxPort_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxPort.Text);
        }

        //复制UUID到剪贴板
        private void TextBoxUUID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxUUID.Text);
        }

        //复制额外ID到剪贴板
        private void TextBoxUUIDextra_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxUUIDextra.Text);
        }

        //复制加密方式到剪贴板
        private void TextBoxEncryption_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxEncryption.Text);
        }

        //复制传输协议到剪贴板
        private void TextBoxTransmission_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTransmission.Text);
        }

        //复制伪装方式到剪贴板
        private void TextBoxCamouflageType_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxCamouflageType.Text);
        }

        //复制Host/Quic加密方法到剪贴板
        private void TextBoxHostQuicEncryption_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxHostQuicEncryption.Text);
        }

        //复制Quic Key/mKCP Seed/路径Path 到剪贴板中
        private void TextBoxQuicKeyMkcpSeedPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxQuicKeyMkcpSeedPath.Text);
        }

        //复制TLS 到剪贴板中
        private void TextBoxTLS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTLS.Text);
        }

        //复制URL链接到剪贴板中
        private void TextBoxURL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxURL.Text);
        }

        #endregion

        #region V2Ray客户端生成
        //生成单方案v2rayN客户端导入文件
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
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
                v2rayNjsonObject["host"] = "";//设置Host
            }
            else if (TextBoxTransmission.Text.Contains("quic")==true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            }
            else
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            }

            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //MessageBox.Show(v2rayNjsonObject["v"].ToString());
            //MessageBox.Show("step1");
            string proxyfolder = CheckDir("v2ray_config");
            //MessageBox.Show("step2");
            configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxHostAddress.Text);
            //MessageBox.Show("step3");
            string configSavePath = configDomainSavePath;

            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            string vmessUrl = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = vmessUrl;
            if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessXtlsTcp") == false
                && String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == false
                && String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == false
                && String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessHttp2Web") == false
                && MainWindow.mKCPvlessIsSet == false)
            {
                using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
                {
                    sw.WriteLine(vmessUrl);
                }
                ImageShareQRcode.Source = CreateQRCode(vmessUrl, $"{configSavePath}\\QR.bmp");
            }
    
            if (File.Exists(@"v2ray_config\config.json"))
            {
                File.Move(@"v2ray_config\config.json", $"{configSavePath}\\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                ReadmeTxtWriteGenerate(sw);
                //sw.WriteLine("config.json");
                ////****** "此文件为v2ray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine01").ToString());

                ////****** "v2ray官方网站：https://www.v2ray.com/" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine02").ToString());

                ////****** "v2ray官方程序下载地址：https://github.com/v2ray/v2ray-core/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine03").ToString());

                ////****** "下载相应版本，Windows选择v2ray-windows-64.zip或者v2ray-windows-32.zip，解压后提取v2ctl.exe和v2ray.exe。与config.json放在同一目录，运行v2ray.exe即可。" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine04").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("QR.bmp");

                ////****** "此文件为v2rayN(windows)、Qv2ray(windows)、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                ////****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                ////****** "Qv2ray下载网址：https://github.com/Qv2ray/Qv2ray/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());
                
                ////****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                ////****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                ////****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                ////****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                ////写入通用配置参数
                //TxtWriteGeneralParameters(sw);
            }
        }

        #region VLESS VMESS XTLS WS共存方案生成链接与说明文件

        //生成VLESS Vmess Tcp Xtls Ws 配置保存（暂未有分享链接与二维码）
        private void GenerateV2rayVlessVmessTcpXtlsWsShareQRcodeAndBase64Url(string plainSavePath)
        {
            #region 暂时不用内容
            //            //生成v2rayN的json文件
            //            string v2rayNjsonFile = @"
            //{
            //  ""v"": """",
            //  ""ps"": """",
            //  ""add"": """",
            //  ""port"": """",
            //  ""id"": """",
            //  ""aid"": """",
            //  ""net"": """",
            //  ""type"": """",
            //  ""host"": """",
            //  ""path"": """",
            //  ""tls"": """"
            //}";
            //            //MessageBox.Show(v2rayNjsonFile);
            //            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            //            v2rayNjsonObject["v"] = "2";
            //            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            //            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            //            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            //            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            //            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            //            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            //            if (TextBoxTransmission.Text.Contains("kcp") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            //            }
            //            else if (TextBoxTransmission.Text.Contains("quic") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            //            }
            //            else
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            //            }

            //            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            //            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            //string saveFileFolderFirst = TextBoxHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir(@"v2ray_config");
            //while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"v2ray_config\" + saveFileFolder);

            #endregion

            //创建保存目录
            //string plainSavePath = @"";

            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            //string vmessUrl = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = vmessUrl;
            //using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            //{
            //    if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == false)
            //    {
            //        sw.WriteLine(vmessUrl);
            //    }
            //}
            //if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == false)
            //{
            //    ImageShareQRcode.Source = CreateQRCode(vmessUrl, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            //}


            if (File.Exists($"v2ray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"v2ray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"v2ray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
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
                //sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray：https://github.com/Qv2ray/Qv2ray/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());
                
                //写入通用配置参数
                TxtWriteGeneralParameters(sw);
            }
        }


        //生成VLESS over TCP with XTLS的配置保存（暂未有分享链接与二维码）
        private void GenerateV2rayVlessTcpXtlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
            //            //生成v2rayN的json文件
            //            string v2rayNjsonFile = @"
            //{
            //  ""v"": """",
            //  ""ps"": """",
            //  ""add"": """",
            //  ""port"": """",
            //  ""id"": """",
            //  ""aid"": """",
            //  ""net"": """",
            //  ""type"": """",
            //  ""host"": """",
            //  ""path"": """",
            //  ""tls"": """"
            //}";
            //            //MessageBox.Show(v2rayNjsonFile);
            //            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            //            v2rayNjsonObject["v"] = "2";
            //            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            //            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            //            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            //            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            //            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            //            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            //            if (TextBoxTransmission.Text.Contains("kcp") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            //            }
            //            else if (TextBoxTransmission.Text.Contains("quic") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            //            }
            //            else
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            //            }

            //            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            //            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            //string saveFileFolderFirst = TextBoxHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir(@"v2ray_config");
            //while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"v2ray_config\" + saveFileFolder);

            #endregion

            //创建保存目录
            string plainSavePath = @"vless_tcp_xtls_client_config";

            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            //TextBoxURLDefault = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLDefault;
            //using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            //{

            //        sw.WriteLine(TextBoxURLDefault);

            //}

            // ImageShareQRcodeDefault = CreateQRCode(TextBoxURLDefault, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            // ImageShareQRcode.Source = ImageShareQRcodeDefault;


            if (File.Exists($"v2ray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"v2ray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"v2ray_config\\{plainSavePath}");
            }
            
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
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

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray：https://github.com/Qv2ray/Qv2ray/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                //写入通用配置参数--
                TxtWriteGeneralParameters(sw,false);

            }
        }

        //生成VLESS over TCP with TLS的配置保存（暂未有分享链接与二维码）
        private void GenerateV2rayVlessTcpTlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
            //            //生成v2rayN的json文件
            //            string v2rayNjsonFile = @"
            //{
            //  ""v"": """",
            //  ""ps"": """",
            //  ""add"": """",
            //  ""port"": """",
            //  ""id"": """",
            //  ""aid"": """",
            //  ""net"": """",
            //  ""type"": """",
            //  ""host"": """",
            //  ""path"": """",
            //  ""tls"": """"
            //}";
            //            //MessageBox.Show(v2rayNjsonFile);
            //            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            //            v2rayNjsonObject["v"] = "2";
            //            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            //            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            //            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            //            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            //            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            //            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            //            if (TextBoxTransmission.Text.Contains("kcp") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            //            }
            //            else if (TextBoxTransmission.Text.Contains("quic") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            //            }
            //            else
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            //            }

            //            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            //            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            //string saveFileFolderFirst = TextBoxHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir(@"v2ray_config");
            //while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"v2ray_config\" + saveFileFolder);

            #endregion

            //创建保存目录
            string plainSavePath = @"vless_tcp_tls_client_config";
            //v2ray_config\${域名IP}
            //string configDomainSavePath = CreateConfigSaveDir(@"v2ray_config", TextBoxHostAddressSS.Text);
            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            //TextBoxURLVlessTcp = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLVlessTcp;
            //using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            //{

            //        sw.WriteLine(TextBoxURLVlessTcp);

            //}

            //   ImageShareQRcodeVlessTcp = CreateQRCode(TextBoxURLVlessTcp, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            //  ImageShareQRcode.Source = ImageShareQRcodeVlessTcp;


            if (File.Exists($"v2ray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"v2ray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"v2ray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
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

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray：https://github.com/Qv2ray/Qv2ray/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                //写入通用配置参数--
                TxtWriteGeneralParameters(sw,false);

            }
        }

        //生成VLESS over WS with TLS的配置保存（暂未有分享链接与二维码）
        private void GenerateV2rayVlessWsTlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
            //            //生成v2rayN的json文件
            //            string v2rayNjsonFile = @"
            //{
            //  ""v"": """",
            //  ""ps"": """",
            //  ""add"": """",
            //  ""port"": """",
            //  ""id"": """",
            //  ""aid"": """",
            //  ""net"": """",
            //  ""type"": """",
            //  ""host"": """",
            //  ""path"": """",
            //  ""tls"": """"
            //}";
            //            //MessageBox.Show(v2rayNjsonFile);
            //            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            //            v2rayNjsonObject["v"] = "2";
            //            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            //            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            //            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            //            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            //            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            //            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            //            if (TextBoxTransmission.Text.Contains("kcp") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            //            }
            //            else if (TextBoxTransmission.Text.Contains("quic") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            //            }
            //            else
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            //            }

            //            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            //            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            //string saveFileFolderFirst = TextBoxHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir(@"v2ray_config");
            //while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"v2ray_config\" + saveFileFolder);

            #endregion

            //创建保存目录
            string plainSavePath = @"vless_ws_tls_client_config";
            //v2ray_config\${域名IP}
            //string configDomainSavePath = CreateConfigSaveDir(@"v2ray_config", TextBoxHostAddressSS.Text);
            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            //TextBoxURLVlessWs = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLVlessWs;
            //using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            //{

            //        sw.WriteLine(TextBoxURLVlessWs);

            //}

            // ImageShareQRcodeVlessWs = CreateQRCode(TextBoxURLVlessWs, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            //ImageShareQRcode.Source = ImageShareQRcodeVlessWs;


            if (File.Exists($"v2ray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"v2ray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"v2ray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
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

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray：https://github.com/Qv2ray/Qv2ray/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                //写入通用配置参数--
                TxtWriteGeneralParameters(sw,false);

            }
        }

        //生成VMess over TCP with TLS的配置保存
        private void GenerateV2rayVmessTcpTlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
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
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
                v2rayNjsonObject["host"] = "";//设置Host
            }
            else if (TextBoxTransmission.Text.Contains("quic") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            }
            else
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            }

            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注


            #endregion

            //创建保存目录
            string plainSavePath = @"vmess_tcp_tls_client_config";
            //v2ray_config\${域名IP}
            //string configDomainSavePath = CreateConfigSaveDir(@"v2ray_config", TextBoxHostAddressSS.Text);
            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            TextBoxURLVmessTcp = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLVmessTcp;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(TextBoxURLVmessTcp);
            }

            ImageShareQRcodeVmessTcp = CreateQRCode(TextBoxURLVmessTcp, $"{configSavePath}\\QR.bmp");
            //ImageShareQRcode.Source = ImageShareQRcodeVmessTcp;


            if (File.Exists($"v2ray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"v2ray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"v2ray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                ReadmeTxtWriteGenerate(sw);

            }
        }

        //生成VMess over WS with TLS的配置保存
        private void GenerateV2rayVmessWsTlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
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
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            }
            else if (TextBoxTransmission.Text.Contains("quic") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            }
            else
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            }

            v2rayNjsonObject["tls"] = "tls";  //设置是否启用TLS
            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注

            #endregion

            //创建保存目录
            string plainSavePath = @"vmess_ws_tls_client_config";
            //v2ray_config\${域名IP}
            //string configDomainSavePath = CreateConfigSaveDir(@"v2ray_config", TextBoxHostAddressSS.Text);
            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            TextBoxURLVmessWs = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLVmessWs;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {

                sw.WriteLine(TextBoxURLVmessWs);

            }

            ImageShareQRcodeVmessWs = CreateQRCode(TextBoxURLVmessWs, $"{configSavePath}\\QR.bmp");
            //ImageShareQRcode.Source = ImageShareQRcodeVmessWs;

            if (File.Exists($"v2ray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"v2ray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"v2ray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                ReadmeTxtWriteGenerate(sw);
               
            }
        }

        //生成Trojan over TCP with TLS的配置保存
        private void GenerateV2rayTrojanShareQRcodeAndBase64Url()
        {
            //创建保存目录
            string plainSavePath = @"trojan_tcp_tls_client_config";
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");

            TextBoxURLTrojanTcp = $"trojan://{TextBoxUUID.Text}@{TextBoxHostAddress.Text}:{TextBoxPort.Text}#{TextBoxHostAddress.Text}";

            //TextBoxURL.Text = TextBoxURLTrojanTcp;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(TextBoxURLTrojanTcp);

            }
            //ImageShareQRcode.Source = CreateQRCode(TextBoxURLTrojanTcp, $"{configSavePath}\\QR.bmp");
            ImageShareQRcodeTrojanTcp = CreateQRCode(TextBoxURLTrojanTcp, $"{configSavePath}\\QR.bmp");
            //ImageShareQRcode.Source = ImageShareQRcodeTrojanTcp;
            //移动官方程序配置文件到相应目录
            if (File.Exists($"v2ray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"v2ray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"v2ray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                //ReadmeTxtWriteGenerate(sw);
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

                //****** "此文件为v2rayN(windows)、Qv2ray(windows)、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray下载网址：https://github.com/Qv2ray/Qv2ray/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                sw.WriteLine("-----------------------------------------");
                sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                sw.WriteLine("-----------------------------------------\n");

                string strApplicat = "";
                string strParam = "";
                int strLenth = 20;
                //****** "服务器通用连接配置参数:" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine12").ToString());
                sw.WriteLine("");

                strApplicat = "TextBlockServerAddress";
                strParam = TextBoxHostAddress.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                strApplicat = "TextBlockServerPort";
                strParam = TextBoxPort.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                strApplicat = "TextBlockUserUUID";
                strParam = TextBoxUUID.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            }

        }

        //写入VMESS的readme.txt文件
        private void ReadmeTxtWriteGenerate(StreamWriter sw)
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

            //****** "此文件为v2rayN(windows)、Qv2ray(windows)、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

            //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

            //****** "Qv2ray下载网址：https://github.com/Qv2ray/Qv2ray/releases" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

            //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

            //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

            //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

            sw.WriteLine("-----------------------------------------");
            sw.WriteLine("url.txt");

            //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

            //写入通用配置参数
            TxtWriteGeneralParameters(sw);
        }
        //TXT文件中写入通用配置参数---
        private void TxtWriteGeneralParameters(StreamWriter sw,bool alterId = true)
        {
            sw.WriteLine("-----------------------------------------\n");

            string strApplicat = "";
            string strParam = "";
            int strLenth = 20;
            //****** "服务器通用连接配置参数:" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine12").ToString());
            sw.WriteLine("");

            strApplicat = "TextBlockServerAddress";
            strParam = TextBoxHostAddress.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockServerPort";
            strParam = TextBoxPort.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockUserUUID";
            strParam = TextBoxUUID.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            if (alterId == true)
            {
                strApplicat = "TextBlockV2RayAlterId";
                strParam = TextBoxUUIDextra.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);
            }

            strApplicat = "TextBlockEncryption";
            strParam = TextBoxEncryption.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockTransferProtocol";
            strParam = TextBoxTransmission.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockCamouflageType";
            strParam = TextBoxCamouflageType.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockIsOrNotTLS";
            strParam = TextBoxTLS.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            if (MainWindow.ReceiveConfigurationParameters[0].Contains("Quic") == true)
            {
                strParam = TextBoxHostQuicEncryption.Text;
                sw.WriteLine(AlignmentStrFunc(TextBlockQuicEncryption.Text, strLenth) + strParam);

                strParam = TextBoxQuicKeyMkcpSeedPath.Text;
                sw.WriteLine(AlignmentStrFunc(TextBlockQuicKey.Text, strLenth) + strParam);

            }
            else if (MainWindow.ReceiveConfigurationParameters[0].Contains("mKCP") == true)
            {
                strParam = TextBoxHostQuicEncryption.Text;
                sw.WriteLine(AlignmentStrFunc("host:", strLenth) + strParam);

                strParam = TextBoxQuicKeyMkcpSeedPath.Text;
                sw.WriteLine(AlignmentStrFunc(TextBlockMkcpSeed.Text, strLenth) + strParam);

            }
            else
            {
                strParam = TextBoxHostQuicEncryption.Text;
                sw.WriteLine(AlignmentStrFunc("host:", strLenth) + strParam);

                strParam = TextBoxQuicKeyMkcpSeedPath.Text;
                sw.WriteLine(AlignmentStrFunc(TextBlockPath.Text, strLenth) + strParam);
            }
        }
        #endregion

        #endregion

        #endregion

        #region Xray 相关

        #region Xray参数设置函数

        //设置VLESS over TCP with XTLS
        private void XraySetVlessTcpXtls()
        {
            GridNotTrojanParametersXray.Visibility = Visibility.Visible;
            TextBlockVmessOrVlessXray.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();
            TextBlockVmessOrVlessXray.Visibility = Visibility.Visible;
            //隐藏下面的二维码显示
            HideGroupBoxClientQRandURL();

            TextBoxEncryptionXray.Text = "none";
            TextBoxTransmissionXray.Text = "tcp";
            TextBoxCamouflageTypeXray.Text = "none";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLSXray.Text = "xtls";
            HideAlterIdXray();
            ShowHostNameXray();
            ShowPathXray();
            TextBoxQuicKeyMkcpSeedPathXray.Text = "";


        }

        //设置VLESS over TCP with TLS
        private void XraySetVlessTcpTls()
        {
            GridNotTrojanParametersXray.Visibility = Visibility.Visible;
            TextBlockVmessOrVlessXray.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();
            TextBlockVmessOrVlessXray.Visibility = Visibility.Visible;
            //隐藏下面的二维码显示
            HideGroupBoxClientQRandURL();

            TextBoxEncryptionXray.Text = "none";
            TextBoxTransmissionXray.Text = "tcp";
            TextBoxCamouflageTypeXray.Text = "none";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLSXray.Text = "tls";
            HideAlterIdXray();
            ShowHostNameXray();
            ShowPathXray();
            TextBoxQuicKeyMkcpSeedPathXray.Text = "";
        }

        //设置VLESS over WS with TLS
        private void XraySetVlessWsTls()
        {
            GridNotTrojanParametersXray.Visibility = Visibility.Visible;
            TextBlockVmessOrVlessXray.Text = Application.Current.FindResource("TabItemHeaderV2RayVlessProtocol").ToString();
            TextBlockVmessOrVlessXray.Visibility = Visibility.Visible;
            //隐藏下面的二维码显示
            HideGroupBoxClientQRandURL();

            TextBoxEncryptionXray.Text = "none";
            TextBoxTransmissionXray.Text = "ws";
            TextBoxCamouflageTypeXray.Text = "none";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLSXray.Text = "tls";
            HideAlterIdXray();
            ShowHostNameXray();
            ShowPathXray();
            TextBoxQuicKeyMkcpSeedPathXray.Text = MainWindow.ReceiveConfigurationParameters[3];

        }

        //设置VMess over TCP with TLS
        private void XraySetVmessTcpTls()
        {
            GridNotTrojanParametersXray.Visibility = Visibility.Visible;
            TextBoxEncryptionXray.Text = "none";
            TextBoxTransmissionXray.Text = "tcp";
            TextBoxCamouflageTypeXray.Text = "http";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLSXray.Text = "tls";
            ShowAlterIdXray();
            ShowHostNameXray();
            ShowPathXray();
            TextBoxQuicKeyMkcpSeedPathXray.Text = MainWindow.ReceiveConfigurationParameters[9];
            TextBlockVmessOrVlessXray.Visibility = Visibility.Collapsed;
            //显示下面的二维码显示。
            //HideGroupBoxClientQRandURL();
            ShowGroupBoxClientQRandURL();
            TextBoxURL.Text = TextBoxURLVmessTcp;
            ImageShareQRcode.Source = ImageShareQRcodeVmessTcp;
        }

        //设置VMess over WS with TLS
        private void XraySetVmessWsTls()
        {
            GridNotTrojanParametersXray.Visibility = Visibility.Visible;
            TextBoxEncryptionXray.Text = "none";
            TextBoxTransmissionXray.Text = "ws";
            TextBoxCamouflageTypeXray.Text = "none";
            //TLS的Host /Quic 加密方式
            TextBoxHostQuicEncryptionXray.Text = MainWindow.ReceiveConfigurationParameters[4];
            TextBoxTLSXray.Text = "tls";
            ShowAlterIdXray();
            ShowHostNameXray();
            ShowPathXray();
            TextBoxQuicKeyMkcpSeedPathXray.Text = MainWindow.ReceiveConfigurationParameters[6];
            TextBlockVmessOrVlessXray.Visibility = Visibility.Collapsed;
            //显示下面的二维码与分享链接
            ShowGroupBoxClientQRandURL();
            TextBoxURL.Text = TextBoxURLVmessWs;
            ImageShareQRcode.Source = ImageShareQRcodeVmessWs;
        }

        //设置Trojan over TCP with TLS
        private void XraySetTrojanTcpTls()
        {
            //隐藏所有不是Trojan的参数
            GridNotTrojanParametersXray.Visibility = Visibility.Collapsed;
            //显示下面的二维码与分享链接
            ShowGroupBoxClientQRandURL();
            TextBoxURL.Text = TextBoxURLTrojanTcp;
            ImageShareQRcode.Source = ImageShareQRcodeTrojanTcp;
        }
        #endregion

        #region 界面控制相关

        //显示Quic 加密方式
        private void ShowQuicEncryptionXray()
        {
            TextBlockQuicEncryptionXray.Visibility = Visibility.Visible;
            TextBlockHostXray.Visibility = Visibility.Collapsed;
        }

        //显示Host隐藏Quic加密方式
        private void ShowHostNameXray()
        {
            TextBlockHostXray.Visibility = Visibility.Visible;
            TextBlockQuicEncryptionXray.Visibility = Visibility.Collapsed;
        }

        //显示路径Path,隐藏mKCP/Quic Key/复制按钮
        private void ShowPathXray()
        {
            TextBlockPathXray.Visibility = Visibility.Visible;
            TextBlockMkcpSeedXray.Visibility = Visibility.Collapsed;
            TextBlockQuicKeyXray.Visibility = Visibility.Collapsed;
        }

        //显示mKCP Seed/复制按钮，隐藏Path/Quic Key
        private void ShowMkcpSeedXray()
        {
            TextBlockPathXray.Visibility = Visibility.Collapsed;
            TextBlockMkcpSeedXray.Visibility = Visibility.Visible;
            TextBlockQuicKeyXray.Visibility = Visibility.Collapsed;
        }

        //显示Quic Key/复制按钮 隐藏Path/mKcp Seed
        private void ShowQuicKeyXray()
        {
            TextBlockPathXray.Visibility = Visibility.Collapsed;
            TextBlockMkcpSeedXray.Visibility = Visibility.Collapsed;
            TextBlockQuicKeyXray.Visibility = Visibility.Visible;
        }

        //显示额外ID
        private void ShowAlterIdXray()
        {
            TextBlockUUIDextraXray.Visibility = Visibility.Visible;
            TextBoxUUIDextraXray.Visibility = Visibility.Visible;
            TextBlockUUIDextraExplanationXray.Visibility = Visibility.Visible;

        }

        //隐藏额外ID
        private void HideAlterIdXray()
        {
            TextBlockUUIDextraXray.Visibility = Visibility.Collapsed;
            TextBoxUUIDextraXray.Visibility = Visibility.Collapsed;
            TextBlockUUIDextraExplanationXray.Visibility = Visibility.Collapsed;

        }

        //显示加密方式
        private void ShowEncryptionXray()
        {
            TextBlockEncryptionXray.Visibility = Visibility.Visible;
            TextBoxEncryptionXray.Visibility = Visibility.Visible;
        }
        //隐藏加密方式
        private void HideEncryptionXray()
        {
            TextBlockEncryptionXray.Visibility = Visibility.Collapsed;
            TextBoxEncryptionXray.Visibility = Visibility.Collapsed;
        }

        //显示传输协议
        private void ShowTransferProtocolXray()
        {
            TextBlockTransferProtocolXray.Visibility = Visibility.Visible;
            TextBoxTransmissionXray.Visibility = Visibility.Visible;
        }
        //隐藏传输协议
        private void HideTransferProtocolXray()
        {
            TextBlockTransferProtocolXray.Visibility = Visibility.Collapsed;
            TextBoxTransmissionXray.Visibility = Visibility.Collapsed;
        }
        //显示伪装类型
        private void ShowTextBlockCamouflageTypeXray()
        {
            TextBlockCamouflageTypeXray.Visibility = Visibility.Visible;
            TextBoxCamouflageTypeXray.Visibility = Visibility.Visible;
        }
        //隐藏伪装类型
        private void HideTextBlockCamouflageTypeXray()
        {
            TextBlockCamouflageTypeXray.Visibility = Visibility.Collapsed;
            TextBoxCamouflageTypeXray.Visibility = Visibility.Collapsed;
        }


        //以下几个为对RadioButton按钮的选中后，界面变化与参数显示
        private void RadioButtonVlessTcpXtlsXray_Checked(object sender, RoutedEventArgs e)
        {
            XraySetVlessTcpXtls();
        }

        private void RadioButtonVlessTcpTlsXray_Checked(object sender, RoutedEventArgs e)
        {
            XraySetVlessTcpTls();
        }

        private void RadioButtonVlessWsTlsXray_Checked(object sender, RoutedEventArgs e)
        {
            XraySetVlessWsTls();
        }

        private void RadioButtonVmessTcpTlsXray_Checked(object sender, RoutedEventArgs e)
        {
            XraySetVmessTcpTls();
        }

        private void RadioButtonVmessWsTlsXray_Checked(object sender, RoutedEventArgs e)
        {
            XraySetVmessWsTls();
        }

        private void RadioButtonTrojanTcpTlsXray_Checked(object sender, RoutedEventArgs e)
        {
            XraySetTrojanTcpTls();
        }
        #endregion

        #region 复制参数到剪贴板中



        //复制服务器地址到剪贴板
        private void TextBoxHostAddressXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxHostAddressXray.Text);
        }


        //复制服务器端口到剪贴板
        private void TextBoxPortXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxPortXray.Text);
        }

        //复制UUID到剪贴板
        private void TextBoxUUIDXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxUUIDXray.Text);
        }

        //复制额外ID到剪贴板
        private void TextBoxUUIDextraXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxUUIDextraXray.Text);
        }

        //复制加密方式到剪贴板
        private void TextBoxEncryptionXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxEncryptionXray.Text);
        }

        //复制传输协议到剪贴板
        private void TextBoxTransmissionXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTransmissionXray.Text);
        }

        //复制伪装方式到剪贴板
        private void TextBoxCamouflageTypeXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxCamouflageTypeXray.Text);
        }

        //复制Host/Quic加密方法到剪贴板
        private void TextBoxHostQuicEncryptionXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxHostQuicEncryptionXray.Text);
        }

        //复制Quic Key/mKCP Seed/路径Path 到剪贴板中
        private void TextBoxQuicKeyMkcpSeedPathXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxQuicKeyMkcpSeedPathXray.Text);
        }

        //复制TLS 到剪贴板中
        private void TextBoxTLSXray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTLSXray.Text);
        }

        #endregion

        #region Xray客户端生成
        //生成单方案v2rayN客户端导入文件
        private void GenerateXrayShareQRcodeAndBase64Url()
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
            v2rayNjsonObject["add"] = TextBoxHostAddressXray.Text; //设置域名
            v2rayNjsonObject["port"] = TextBoxPortXray.Text; //设置端口
            v2rayNjsonObject["id"] = TextBoxUUIDXray.Text; //设置uuid
            v2rayNjsonObject["aid"] = TextBoxUUIDextraXray.Text; //设置额外ID
            v2rayNjsonObject["net"] = TextBoxTransmissionXray.Text; //设置传输模式
            v2rayNjsonObject["type"] = TextBoxCamouflageTypeXray.Text; //设置伪装类型

            if (TextBoxTransmissionXray.Text.Contains("kcp") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text;//设置mKCP Seed
                v2rayNjsonObject["host"] = "";//设置Host
            }
            else if (TextBoxTransmissionXray.Text.Contains("quic") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text;//设置quic密钥
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryptionXray.Text;//Quic加密方式
            }
            else
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text; //设置路径
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryptionXray.Text;//设置TLS的Host
            }

            v2rayNjsonObject["tls"] = TextBoxTLSXray.Text;  //设置是否启用TLS
            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //MessageBox.Show(v2rayNjsonObject["v"].ToString());
            //MessageBox.Show("step1");
            string proxyfolder = CheckDir("xray_config");
            //MessageBox.Show("step2");
            configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxHostAddressXray.Text);
            //MessageBox.Show("step3");
            string configSavePath = configDomainSavePath;

            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            string vmessUrl = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = vmessUrl;
            if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessXtlsTcp") == false
                && String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == false
                && String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == false
                && String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessHttp2Web") == false
                && MainWindow.mKCPvlessIsSet == false)
            {
                using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
                {
                    sw.WriteLine(vmessUrl);
                }
                ImageShareQRcode.Source = CreateQRCode(vmessUrl, $"{configSavePath}\\QR.bmp");
            }

            if (File.Exists(@"xray_config\config.json"))
            {
                File.Move(@"xray_config\config.json", $"{configSavePath}\\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                ReadmeTxtWriteGenerateXray(sw);

            }
        }

        #region VLESS VMESS XTLS WS共存方案生成链接与说明文件

        //生成VLESS Vmess Tcp Xtls Ws 配置保存（暂未有分享链接与二维码）
        private void GenerateXrayVlessVmessTcpXtlsWsShareQRcodeAndBase64Url(string plainSavePath)
        {
            #region 暂时不用内容
            //            //生成v2rayN的json文件
            //            string v2rayNjsonFile = @"
            //{
            //  ""v"": """",
            //  ""ps"": """",
            //  ""add"": """",
            //  ""port"": """",
            //  ""id"": """",
            //  ""aid"": """",
            //  ""net"": """",
            //  ""type"": """",
            //  ""host"": """",
            //  ""path"": """",
            //  ""tls"": """"
            //}";
            //            //MessageBox.Show(v2rayNjsonFile);
            //            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            //            v2rayNjsonObject["v"] = "2";
            //            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            //            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            //            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            //            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            //            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            //            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            //            if (TextBoxTransmission.Text.Contains("kcp") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            //            }
            //            else if (TextBoxTransmission.Text.Contains("quic") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            //            }
            //            else
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            //            }

            //            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            //            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            //string saveFileFolderFirst = TextBoxHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir(@"v2ray_config");
            //while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"v2ray_config\" + saveFileFolder);

            #endregion

            //创建保存目录
            //string plainSavePath = @"";

            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            //string vmessUrl = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = vmessUrl;
            //using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            //{
            //    if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == false)
            //    {
            //        sw.WriteLine(vmessUrl);
            //    }
            //}
            //if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == false)
            //{
            //    ImageShareQRcode.Source = CreateQRCode(vmessUrl, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            //}


            if (File.Exists($"xray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"xray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"xray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                sw.WriteLine("config.json");
                //****** "此文件为v2ray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine01").ToString());

                //****** "v2ray官方网站：https://www.v2ray.com/" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine02").ToString());

                //****** "v2ray官方程序下载地址：https://github.com/v2ray/v2ray-core/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine03").ToString());

                //****** "下载相应版本，Windows选择v2ray-windows-64.zip或者v2ray-windows-32.zip，解压后提取v2ctl.exe和v2ray.exe。与config.json放在同一目录，运行v2ray.exe即可。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine04").ToString());

                sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray：https://github.com/Qv2ray/Qv2ray/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                //写入通用配置参数
                TxtWriteGeneralParametersXray(sw);
            }
        }


        //生成VLESS over TCP with XTLS的配置保存（暂未有分享链接与二维码）
        private void GenerateXrayVlessTcpXtlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
            //            //生成v2rayN的json文件
            //            string v2rayNjsonFile = @"
            //{
            //  ""v"": """",
            //  ""ps"": """",
            //  ""add"": """",
            //  ""port"": """",
            //  ""id"": """",
            //  ""aid"": """",
            //  ""net"": """",
            //  ""type"": """",
            //  ""host"": """",
            //  ""path"": """",
            //  ""tls"": """"
            //}";
            //            //MessageBox.Show(v2rayNjsonFile);
            //            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            //            v2rayNjsonObject["v"] = "2";
            //            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            //            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            //            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            //            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            //            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            //            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            //            if (TextBoxTransmission.Text.Contains("kcp") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            //            }
            //            else if (TextBoxTransmission.Text.Contains("quic") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            //            }
            //            else
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            //            }

            //            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            //            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            //string saveFileFolderFirst = TextBoxHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir(@"v2ray_config");
            //while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"v2ray_config\" + saveFileFolder);

            #endregion

            //创建保存目录
            string plainSavePath = @"vless_tcp_xtls_client_config";

            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            //TextBoxURLDefault = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLDefault;
            //using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            //{

            //        sw.WriteLine(TextBoxURLDefault);

            //}

            //   ImageShareQRcodeDefault = CreateQRCode(TextBoxURLDefault, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            // ImageShareQRcode.Source = ImageShareQRcodeDefault;


            if (File.Exists($"xray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"xray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"xray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                sw.WriteLine("config.json");
                //****** "此文件为Xray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine01").ToString());

                //****** "Xray官方网站：https://github.com/XTLS/Xray-core" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine02").ToString());

                //****** "Xray官方程序下载地址：https://github.com/XTLS/Xray-core/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine03").ToString());

                //****** "下载相应版本，Windows选择xray-windows-64.zip或者xray-windows-32.zip，解压后提取xray.exe。与config.json放在同一目录，运行xray.exe即可。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine04").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN(windows)、Qv2ray(windows)、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray下载网址：https://github.com/Qv2ray/Qv2ray/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                //写入通用配置参数--
                TxtWriteGeneralParametersXray(sw, false);

            }
        }

        //生成VLESS over TCP with TLS的配置保存（暂未有分享链接与二维码）
        private void GenerateXrayVlessTcpTlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
            //            //生成v2rayN的json文件
            //            string v2rayNjsonFile = @"
            //{
            //  ""v"": """",
            //  ""ps"": """",
            //  ""add"": """",
            //  ""port"": """",
            //  ""id"": """",
            //  ""aid"": """",
            //  ""net"": """",
            //  ""type"": """",
            //  ""host"": """",
            //  ""path"": """",
            //  ""tls"": """"
            //}";
            //            //MessageBox.Show(v2rayNjsonFile);
            //            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            //            v2rayNjsonObject["v"] = "2";
            //            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            //            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            //            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            //            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            //            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            //            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            //            if (TextBoxTransmission.Text.Contains("kcp") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            //            }
            //            else if (TextBoxTransmission.Text.Contains("quic") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            //            }
            //            else
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            //            }

            //            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            //            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            //string saveFileFolderFirst = TextBoxHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir(@"v2ray_config");
            //while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"v2ray_config\" + saveFileFolder);

            #endregion

            //创建保存目录
            string plainSavePath = @"vless_tcp_tls_client_config";
            //v2ray_config\${域名IP}
            //string configDomainSavePath = CreateConfigSaveDir(@"v2ray_config", TextBoxHostAddressSS.Text);
            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            //TextBoxURLVlessTcp = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLVlessTcp;
            //using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            //{

            //        sw.WriteLine(TextBoxURLVlessTcp);

            //}

            //   ImageShareQRcodeVlessTcp = CreateQRCode(TextBoxURLVlessTcp, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            //  ImageShareQRcode.Source = ImageShareQRcodeVlessTcp;


            if (File.Exists($"xray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"xray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"xray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                sw.WriteLine("config.json");
                //****** "此文件为Xray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine01").ToString());

                //****** "Xray官方网站：https://github.com/XTLS/Xray-core" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine02").ToString());

                //****** "Xray官方程序下载地址：https://github.com/XTLS/Xray-core/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine03").ToString());

                //****** "下载相应版本，Windows选择xray-windows-64.zip或者xray-windows-32.zip，解压后提取v2ray.exe。与config.json放在同一目录，运行xray.exe即可。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine04").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN(windows)、Qv2ray(windows)、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray下载网址：https://github.com/Qv2ray/Qv2ray/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                //写入通用配置参数--
                TxtWriteGeneralParametersXray(sw, false);

            }
        }

        //生成VLESS over WS with TLS的配置保存（暂未有分享链接与二维码）
        private void GenerateXrayVlessWsTlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
            //            //生成v2rayN的json文件
            //            string v2rayNjsonFile = @"
            //{
            //  ""v"": """",
            //  ""ps"": """",
            //  ""add"": """",
            //  ""port"": """",
            //  ""id"": """",
            //  ""aid"": """",
            //  ""net"": """",
            //  ""type"": """",
            //  ""host"": """",
            //  ""path"": """",
            //  ""tls"": """"
            //}";
            //            //MessageBox.Show(v2rayNjsonFile);
            //            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            //            v2rayNjsonObject["v"] = "2";
            //            v2rayNjsonObject["add"] = TextBoxHostAddress.Text; //设置域名
            //            v2rayNjsonObject["port"] = TextBoxPort.Text; //设置端口
            //            v2rayNjsonObject["id"] = TextBoxUUID.Text; //设置uuid
            //            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text; //设置额外ID
            //            v2rayNjsonObject["net"] = TextBoxTransmission.Text; //设置传输模式
            //            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text; //设置伪装类型

            //            if (TextBoxTransmission.Text.Contains("kcp") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置mKCP Seed
            //            }
            //            else if (TextBoxTransmission.Text.Contains("quic") == true)
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text;//设置quic密钥
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//Quic加密方式
            //            }
            //            else
            //            {
            //                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPath.Text; //设置路径
            //                v2rayNjsonObject["host"] = TextBoxHostQuicEncryption.Text;//设置TLS的Host
            //            }

            //            v2rayNjsonObject["tls"] = TextBoxTLS.Text;  //设置是否启用TLS
            //            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            //string saveFileFolderFirst = TextBoxHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir(@"v2ray_config");
            //while (Directory.Exists(@"v2ray_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"v2ray_config\" + saveFileFolder);

            #endregion

            //创建保存目录
            string plainSavePath = @"vless_ws_tls_client_config";
            //v2ray_config\${域名IP}
            //string configDomainSavePath = CreateConfigSaveDir(@"v2ray_config", TextBoxHostAddressSS.Text);
            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            //TextBoxURLVlessWs = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLVlessWs;
            //using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            //{

            //        sw.WriteLine(TextBoxURLVlessWs);

            //}

            // ImageShareQRcodeVlessWs = CreateQRCode(TextBoxURLVlessWs, $"v2ray_config\\{saveFileFolder}\\QR.bmp");
            //ImageShareQRcode.Source = ImageShareQRcodeVlessWs;


            if (File.Exists($"xray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"xray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"xray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                sw.WriteLine("config.json");
                //****** "此文件为Xray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine01").ToString());

                //****** "Xray官方网站：https://github.com/XTLS/Xray-core" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine02").ToString());

                //****** "Xray官方程序下载地址：https://github.com/XTLS/Xray-core/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine03").ToString());

                //****** "下载相应版本，Windows选择xray-windows-64.zip或者xray-windows-32.zip，解压后提取v2ray.exe。与config.json放在同一目录，运行xray.exe即可。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine04").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN(windows)、Qv2ray(windows)、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray下载网址：https://github.com/Qv2ray/Qv2ray/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                //sw.WriteLine("-----------------------------------------");
                //sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                //sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                //写入通用配置参数--
                TxtWriteGeneralParametersXray(sw, false);

            }
        }

        //生成VMess over TCP with TLS的配置保存
        private void GenerateXrayVmessTcpTlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
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
            v2rayNjsonObject["add"] = TextBoxHostAddressXray.Text; //设置域名
            v2rayNjsonObject["port"] = TextBoxPortXray.Text; //设置端口
            v2rayNjsonObject["id"] = TextBoxUUIDXray.Text; //设置uuid
            v2rayNjsonObject["aid"] = TextBoxUUIDextraXray.Text; //设置额外ID
            v2rayNjsonObject["net"] = TextBoxTransmissionXray.Text; //设置传输模式
            v2rayNjsonObject["type"] = TextBoxCamouflageTypeXray.Text; //设置伪装类型

            if (TextBoxTransmissionXray.Text.Contains("kcp") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text;//设置mKCP Seed
                v2rayNjsonObject["host"] = "";//设置Host
            }
            else if (TextBoxTransmissionXray.Text.Contains("quic") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text;//设置quic密钥
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryptionXray.Text;//Quic加密方式
            }
            else
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text; //设置路径
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryptionXray.Text;//设置TLS的Host
            }

            v2rayNjsonObject["tls"] = TextBoxTLSXray.Text;  //设置是否启用TLS
            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注


            #endregion

            //创建保存目录
            string plainSavePath = @"vmess_tcp_tls_client_config";
            //v2ray_config\${域名IP}
            //string configDomainSavePath = CreateConfigSaveDir(@"v2ray_config", TextBoxHostAddressSS.Text);
            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL，跳过VlessTcpTlsWeb暂时未有URL标准
            TextBoxURLVmessTcp = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLVmessTcp;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(TextBoxURLVmessTcp);
            }

            ImageShareQRcodeVmessTcp = CreateQRCode(TextBoxURLVmessTcp, $"{configSavePath}\\QR.bmp");
            //ImageShareQRcode.Source = ImageShareQRcodeVmessTcp;


            if (File.Exists($"xray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"xray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"xray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                ReadmeTxtWriteGenerateXray(sw);

            }
        }

        //生成VMess over WS with TLS的配置保存
        private void GenerateXrayVmessWsTlsShareQRcodeAndBase64Url()
        {
            #region 暂时不用内容
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
            v2rayNjsonObject["add"] = TextBoxHostAddressXray.Text; //设置域名
            v2rayNjsonObject["port"] = TextBoxPortXray.Text; //设置端口
            v2rayNjsonObject["id"] = TextBoxUUIDXray.Text; //设置uuid
            v2rayNjsonObject["aid"] = TextBoxUUIDextraXray.Text; //设置额外ID
            v2rayNjsonObject["net"] = TextBoxTransmissionXray.Text; //设置传输模式
            v2rayNjsonObject["type"] = TextBoxCamouflageTypeXray.Text; //设置伪装类型

            if (TextBoxTransmissionXray.Text.Contains("kcp") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text;//设置mKCP Seed
            }
            else if (TextBoxTransmissionXray.Text.Contains("quic") == true)
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text;//设置quic密钥
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryptionXray.Text;//Quic加密方式
            }
            else
            {
                v2rayNjsonObject["path"] = TextBoxQuicKeyMkcpSeedPathXray.Text; //设置路径
                v2rayNjsonObject["host"] = TextBoxHostQuicEncryptionXray.Text;//设置TLS的Host
            }

            v2rayNjsonObject["tls"] = "tls";  //设置是否启用TLS
            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注

            #endregion

            //创建保存目录
            string plainSavePath = @"vmess_ws_tls_client_config";
            //v2ray_config\${域名IP}
            //string configDomainSavePath = CreateConfigSaveDir(@"v2ray_config", TextBoxHostAddressSS.Text);
            //v2ray_config\${域名IP}\vless_tcp_xtls_client_config
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");


            //生成二维码与URL
            TextBoxURLVmessWs = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            //TextBoxURL.Text = TextBoxURLVmessWs;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(TextBoxURLVmessWs);
            }

            ImageShareQRcodeVmessWs = CreateQRCode(TextBoxURLVmessWs, $"{configSavePath}\\QR.bmp");
            //ImageShareQRcode.Source = ImageShareQRcodeVmessWs;

            if (File.Exists($"xray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"xray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"xray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                ReadmeTxtWriteGenerateXray(sw);

            }
        }

        //生成Trojan over TCP with TLS的配置保存
        private void GenerateXrayTrojanShareQRcodeAndBase64Url()
        {
            //创建保存目录
            string plainSavePath = @"trojan_tcp_tls_client_config";
            string configSavePath = CheckDir($"{configDomainSavePath}\\{plainSavePath}");

            TextBoxURLTrojanTcp = $"trojan://{TextBoxUUIDXray.Text}@{TextBoxHostAddressXray.Text}:{TextBoxPortXray.Text}#{TextBoxHostAddressXray.Text}";

            //TextBoxURL.Text = TextBoxURLTrojanTcp;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(TextBoxURLTrojanTcp);

            }
            //ImageShareQRcode.Source = CreateQRCode(TextBoxURLTrojanTcp, $"{configSavePath}\\QR.bmp");
            ImageShareQRcodeTrojanTcp = CreateQRCode(TextBoxURLTrojanTcp, $"{configSavePath}\\QR.bmp");
            //ImageShareQRcode.Source = ImageShareQRcodeTrojanTcp;
            //移动官方程序配置文件到相应目录
            if (File.Exists($"xray_config\\{plainSavePath}\\config.json"))
            {
                File.Move($"xray_config\\{plainSavePath}\\config.json", $"{configSavePath}\\config.json");
                Directory.Delete($"xray_config\\{plainSavePath}");
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
            {
                //ReadmeTxtWriteGenerate(sw);
                sw.WriteLine("config.json");
                //****** "此文件为Xray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine01").ToString());

                //****** "Xray官方网站：https://github.com/XTLS/Xray-core" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine02").ToString());

                //****** "Xray官方程序下载地址：https://github.com/XTLS/Xray-core/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine03").ToString());

                //****** "下载相应版本，Windows选择Xray-windows-64.zip或者Xray-windows-32.zip，解压后提取v2ctl.exe和v2ray.exe。与config.json放在同一目录，运行v2ray.exe即可。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine04").ToString());

                sw.WriteLine("-----------------------------------------");
                sw.WriteLine("QR.bmp");

                //****** "此文件为v2rayN(windows)、Qv2ray(windows)、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

                //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

                //****** "Qv2ray下载网址：https://github.com/Qv2ray/Qv2ray/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

                //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

                //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

                //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

                sw.WriteLine("-----------------------------------------");
                sw.WriteLine("url.txt");

                //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

                sw.WriteLine("-----------------------------------------\n");

                string strApplicat = "";
                string strParam = "";
                int strLenth = 20;
                //****** "服务器通用连接配置参数:" ******
                sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine12").ToString());
                sw.WriteLine("");

                strApplicat = "TextBlockServerAddress";
                strParam = TextBoxHostAddressXray.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                strApplicat = "TextBlockServerPort";
                strParam = TextBoxPortXray.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                strApplicat = "TextBlockUserUUID";
                strParam = TextBoxUUIDXray.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            }

        }

        //写入VMESS的readme.txt文件
        private void ReadmeTxtWriteGenerateXray(StreamWriter sw)
        {
            sw.WriteLine("config.json");
            //****** "此文件为Xray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine01").ToString());

            //****** "Xray官方网站：https://github.com/XTLS/Xray-core" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine02").ToString());

            //****** "Xray官方程序下载地址：https://github.com/XTLS/Xray-core/releases" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine03").ToString());

            //****** "下载相应版本，Windows选择xray-windows-64.zip或者xray-windows-32.zip，解压后提取xray.exe。与config.json放在同一目录，运行xray.exe即可。" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtXrayExplainLine04").ToString());

            sw.WriteLine("-----------------------------------------");
            sw.WriteLine("QR.bmp");

            //****** "此文件为v2rayN(windows)、Qv2ray(windows)、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine05").ToString());

            //****** "v2rayN下载网址：https://github.com/2dust/v2rayN/releases" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine06").ToString());

            //****** "Qv2ray下载网址：https://github.com/Qv2ray/Qv2ray/releases" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine07").ToString());

            //****** "v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine08").ToString());

            //****** "v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine09").ToString());

            //****** "Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine10").ToString());

            sw.WriteLine("-----------------------------------------");
            sw.WriteLine("url.txt");

            //****** "此文件为v2rayN、Qv2ray、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine11").ToString());

            //写入通用配置参数
            TxtWriteGeneralParametersXray(sw);
        }
        //TXT文件中写入通用配置参数---
        private void TxtWriteGeneralParametersXray(StreamWriter sw, bool alterId = true)
        {
            sw.WriteLine("-----------------------------------------\n");

            string strApplicat = "";
            string strParam = "";
            int strLenth = 20;
            //****** "服务器通用连接配置参数:" ******
            sw.WriteLine(Application.Current.FindResource("readmeTxtV2RayExplainLine12").ToString());
            sw.WriteLine("");

            strApplicat = "TextBlockServerAddress";
            strParam = TextBoxHostAddressXray.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockServerPort";
            strParam = TextBoxPortXray.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockUserUUID";
            strParam = TextBoxUUIDXray.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            if (alterId == true)
            {
                strApplicat = "TextBlockV2RayAlterId";
                strParam = TextBoxUUIDextraXray.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);
            }

            strApplicat = "TextBlockEncryption";
            strParam = TextBoxEncryptionXray.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockTransferProtocol";
            strParam = TextBoxTransmissionXray.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockCamouflageType";
            strParam = TextBoxCamouflageTypeXray.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            strApplicat = "TextBlockIsOrNotTLS";
            strParam = TextBoxTLSXray.Text;
            sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            if (MainWindow.ReceiveConfigurationParameters[0].Contains("Quic") == true)
            {
                strParam = TextBoxHostQuicEncryptionXray.Text;
                sw.WriteLine(AlignmentStrFunc(TextBlockQuicEncryptionXray.Text, strLenth) + strParam);

                strParam = TextBoxQuicKeyMkcpSeedPathXray.Text;
                sw.WriteLine(AlignmentStrFunc(TextBlockQuicKeyXray.Text, strLenth) + strParam);

            }
            else if (MainWindow.ReceiveConfigurationParameters[0].Contains("mKCP") == true)
            {
                strParam = TextBoxHostQuicEncryptionXray.Text;
                sw.WriteLine(AlignmentStrFunc("host:", strLenth) + strParam);

                strParam = TextBoxQuicKeyMkcpSeedPathXray.Text;
                sw.WriteLine(AlignmentStrFunc(TextBlockMkcpSeedXray.Text, strLenth) + strParam);

            }
            else
            {
                strParam = TextBoxHostQuicEncryptionXray.Text;
                sw.WriteLine(AlignmentStrFunc("host:", strLenth) + strParam);

                strParam = TextBoxQuicKeyMkcpSeedPathXray.Text;
                sw.WriteLine(AlignmentStrFunc(TextBlockPathXray.Text, strLenth) + strParam);
            }
        }
        #endregion

        #endregion

        #endregion

        #region TrojanGo 相关
        //生成TrojanGo客户端资料
        private void GenerateTrojanGoShareQRcodeAndBase64Url()
        {
            string proxyfolder = CheckDir("trojan-go_config");
            configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxTrojanGoServerHost.Text);
            string configSavePath = configDomainSavePath;

            string trojanGoUrl = GetTrojanGoUrl();

            TextBoxURL.Text = trojanGoUrl;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(trojanGoUrl);

            }
            ImageShareQRcode.Source = CreateQRCode(trojanGoUrl, $"{configSavePath}\\QR.bmp");

            //移动Trojan官方程序配置文件到相应目录
            if (File.Exists(@"trojan-go_config\config.json"))
            {
                File.Move(@"trojan-go_config\config.json", $"{configSavePath}\\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
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

                //sw.WriteLine("此文件为Qv2ray (windows)、igniter（Android）扫码导入节点");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine05").ToString());

                //sw.WriteLine("Qv2ray (windows)下载网址：https://github.com/Qv2ray/Qv2ray/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine06").ToString());

                //sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine07").ToString());

                //sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");
               // sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine08").ToString());
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");

                //sw.WriteLine("此文件为Qv2ray (windows)、igniter（Android）复制粘贴导入节点的网址");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine09").ToString());
                sw.WriteLine("-----------------------------------------\n");

                string strApplicat = "";
                string strParam = "";
                int strLenth = 20;

                //sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojan-goExplainLine10").ToString());
                sw.WriteLine("");

                //****** 服务器地址(address): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxTrojanGoServerHost.Text}");
                strApplicat = "TextBlockServerAddress";
                strParam = TextBoxTrojanGoServerHost.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 端口(port): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxTrojanGoServerPort.Text}");
                strApplicat = "TextBlockServerPort";
                strParam = TextBoxTrojanGoServerPort.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 密码: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxTrojanGoServerPassword.Text}");
                strApplicat = "TextBlockTrojanGoPassword";
                strParam = TextBoxTrojanGoServerPassword.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** Type: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxTrojanGoServerPassword.Text}");
                //strApplicat = "TextBlockTrojanGoPassword";
                strParam = TextBoxTrojanGoType.Text;
                sw.WriteLine(AlignmentStrFunc("Type:", strLenth) + strParam);
                //sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** WebSocket路径: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoWebSocketPath").ToString() + $"{TextBoxTrojanGoWSPath.Text}");
                strApplicat = "TextBlockTrojanGoWebSocketPath";
                strParam = TextBoxTrojanGoWSPath.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** Mux多路复用 ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoWebSocketPath").ToString() + $"{TextBoxTrojanGoWSPath.Text}");
                strApplicat = "CheckBoxMuxSelect";
                strParam = ":";
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** concurrency: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoWebSocketPath").ToString() + $"{TextBoxTrojanGoWSPath.Text}");
                strApplicat = "concurrency:";
                strParam = TextBoxConcurrency.Text;
                sw.WriteLine(AlignmentStrFunc(strApplicat, strLenth) + strParam);

                //****** idle_timeout: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoWebSocketPath").ToString() + $"{TextBoxTrojanGoWSPath.Text}");
                strApplicat = "idle_timeout:";
                strParam = TextBoxIdle_timeout.Text;
                sw.WriteLine(AlignmentStrFunc(strApplicat, strLenth) + strParam);

            }

        }
        
        #region TrojanGo内容双击复制到剪贴板
        private void TextBoxTrojanGoServerHost_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanGoServerHost.Text);
        }

        private void TextBoxTrojanGoServerPort_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanGoServerPort.Text);
        }

        private void TextBoxTrojanGoServerPassword_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanGoServerPassword.Text);
        }
        private void TextBoxTrojanGoSNI_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanGoSNI.Text);
        }
        private void TextBoxTrojanGoType_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanGoType.Text);
        }
        private void TextBoxTrojanGoHost_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanGoHost.Text);
        }
        private void TextBoxTrojanGoWSPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanGoWSPath.Text);
        }
        private void TextBoxConcurrency_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxConcurrency.Text);
        }

        private void TextBoxIdle_timeout_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxIdle_timeout.Text);
        }
        #endregion

        //生成TrojanGo的分享链接
        private string GetTrojanGoUrl()
        {
            string trojanGoPassword = EncodeURIComponent(TextBoxTrojanGoServerPassword.Text);
            string trojanGoHost = TextBoxTrojanGoServerHost.Text;
            string trojanGoPort = TextBoxTrojanGoServerPort.Text;

            string trojanGoSni = EncodeURIComponent(TextBoxTrojanGoSNI.Text);
            string trojanGoType= EncodeURIComponent(TextBoxTrojanGoType.Text);
            string trojanGohostName = EncodeURIComponent(TextBoxTrojanGoHost.Text);

            string trojanGoPath = EncodeURIComponent(TextBoxTrojanGoWSPath.Text);
            string trojanGoEncryption = EncodeURIComponent("");
            string trojanGoPlugin = EncodeURIComponent("");

            string trojanGoRemarks = EncodeURIComponent(trojanGoHost);

            //分享链接规范：https://github.com/p4gefau1t/trojan-go/issues/132
            //trojan-go://$(trojan-password)@trojan-host:port/?sni=$(update.microsoft.com)&type=$(original|ws|h2|h2+ws)&host=$(update-01.microsoft.com)&path=$(/update/whatever)&encryption=$(ss;aes-256-gcm:ss-password)&plugin=$(...)#$(descriptive-text)
            //string trojanGoUrl = $"trojan-go://{trojanGoPassword}@{trojanGoHost}:{trojanGoPort}/?sni={trojanGoSni}&type={trojanGoType}&host={trojanGohostName}&path={trojanGoPath}&encryption={trojanGoEncryption}&plugin={trojanGoPlugin}#{trojanGoRemarks}";

            //&path={trojanGoPath}
            //&encryption={trojanGoEncryption}
            //&plugin={trojanGoPlugin}
            //#{trojanGoRemarks}
            string trojanGoUrl = $"trojan-go://{trojanGoPassword}@{trojanGoHost}:{trojanGoPort}/?sni={trojanGoSni}&type={trojanGoType}";

            if (String.IsNullOrEmpty(trojanGohostName) == false)
            {
                trojanGoUrl += $"&host={trojanGohostName}";
            }
            if (String.IsNullOrEmpty(trojanGoPath) == false)
            {
                trojanGoUrl += $"&path={trojanGoPath}";
            }
            if (String.IsNullOrEmpty(trojanGoEncryption) == false)
            {
                trojanGoUrl += $"&encryption={trojanGoEncryption}";
            }
            if (String.IsNullOrEmpty(trojanGoPlugin) == false)
            {
                trojanGoUrl += $"&plugin={trojanGoPlugin}";
            }

            trojanGoUrl += $"#{trojanGoRemarks}";
            return trojanGoUrl;
        }
        private string EncodeURIComponent(string initialUri)
        {
            return Uri.EscapeDataString(initialUri);
        }

        //生成Trojan客户端资料
        private void GenerateTrojanShareQRcodeAndBase64Url()
        {
            string proxyfolder = CheckDir("trojan_config");
            configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxTrojanServerHost.Text);
            string configSavePath = configDomainSavePath;

            //string saveFileFolderFirst = TextBoxTrojanServerHost.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir("trojan_config");
            //while (Directory.Exists(@"trojan_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"trojan_config\" + saveFileFolder);
            string trojanUrl = $"trojan://{TextBoxTrojanServerPassword.Text}@{TextBoxTrojanServerHost.Text}:{TextBoxTrojanServerPort.Text}#{TextBoxTrojanServerHost.Text}";
            //MessageBox.Show(v2rayNjsonObject.ToString());
            //string trojanUrl = "trojan://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxURL.Text = trojanUrl;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(trojanUrl);

            }
            ImageShareQRcode.Source = CreateQRCode(trojanUrl, $"{configSavePath}\\QR.bmp");

            //移动Trojan官方程序配置文件到相应目录
            if (File.Exists(@"trojan_config\config.json"))
            {
                File.Move(@"trojan_config\config.json", $"{configSavePath}\\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
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

                //sw.WriteLine("此文件为Qv2ray (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine05").ToString());

                //sw.WriteLine("Qv2ray (windows)下载网址：https://github.com/Qv2ray/Qv2ray/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine06").ToString());

                //sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine07").ToString());

                //sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine08").ToString());
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");

                //sw.WriteLine("此文件为Qv2ray (windows)、igniter（Android）、Shadowrocket(ios)复制粘贴导入节点的网址");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine09").ToString());
                sw.WriteLine("-----------------------------------------\n");

                string strApplicat = "";
                string strParam = "";
                int strLenth = 20;

                //sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine10").ToString());
                sw.WriteLine("");

                //****** 服务器地址(address): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxTrojanGoServerHost.Text}");
                strApplicat = "TextBlockServerAddress";
                strParam = TextBoxTrojanServerHost.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 端口(port): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxTrojanGoServerPort.Text}");
                strApplicat = "TextBlockServerPort";
                strParam = TextBoxTrojanServerPort.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 密码: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxTrojanGoServerPassword.Text}");
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoWebSocketPath").ToString() + $"{TextBoxTrojanGoWSPath.Text}");
                strApplicat = "TextBlockTrojanGoPassword";
                strParam = TextBoxTrojanServerPassword.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);


            }

        }
       
        #region Trojan内容双击复制到剪贴板

        private void TextBoxTrojanServerHost_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanServerHost.Text);
        }

        private void TextBoxTrojanServerPort_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanServerPort.Text);
        }

        private void TextBoxTrojanServerPassword_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxTrojanServerPassword.Text);
        }

        #endregion

        #endregion

        #region NaiveProxy 相关
        //生成NaiveProxy客户端资料
        private void GenerateNaivePrxoyShareQRcodeAndBase64Url()
        {
            string proxyfolder = CheckDir("naive_config");
            configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxNaiveServerHost.Text);
            string configSavePath = configDomainSavePath;

            string naiveUrl = $"naive+https://{TextBoxNaiveUser.Text}:{TextBoxNaivePassword.Text}@{TextBoxNaiveServerHost.Text}:443?padding=true#{TextBoxNaiveServerHost.Text}";

            TextBoxURL.Text = naiveUrl;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(naiveUrl);

            }
            ImageShareQRcode.Source = CreateQRCode(naiveUrl, $"{configSavePath}\\QR.bmp");

            //移动NaiveProxy官方程序配置文件到相应目录
            if (File.Exists(@"naive_config\config.json"))
            {
                File.Move(@"naive_config\config.json", $"{configSavePath}\\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
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

                sw.WriteLine("-----------------------------------------");
                sw.WriteLine("QR.bmp");

                //sw.WriteLine("此文件为NaiveSharp(windows)复制粘贴导入节点的网址");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine05").ToString());

                //sw.WriteLine("NaiveSharp(windows)下载网址：https://github.com/KevinZonda/NaiveSharp/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine06").ToString());

                sw.WriteLine("-----------------------------------------\n");

                sw.WriteLine("url.txt");

                //sw.WriteLine("此文件为NaiveSharp(windows)复制粘贴导入节点的网址");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine05").ToString());

                //sw.WriteLine("NaiveSharp(windows)下载网址：https://github.com/KevinZonda/NaiveSharp/releases");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine06").ToString());

                sw.WriteLine("-----------------------------------------\n");

                string strApplicat = "";
                string strParam = "";
                int strLenth = 20;

                //sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine(Application.Current.FindResource("readmeTxtNaiveProxyExplainLine07").ToString());
                sw.WriteLine("");

                //****** 服务器地址(address): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxNaiveServerHost.Text}");
                strApplicat = "TextBlockServerAddress";
                strParam = TextBoxNaiveServerHost.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 用户名:******
                //sw.WriteLine(Application.Current.FindResource("TextBlockHostUser").ToString() + $"{TextBoxNaiveUser.Text}");
                strApplicat = "TextBlockHostUser";
                strParam = TextBoxNaiveUser.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 密码: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxNaivePassword.Text}");
                strApplicat = "TextBlockTrojanGoPassword";
                strParam = TextBoxNaivePassword.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            }

        }
        
        #region NaiveProxy内容双击复制到剪贴板
        private void TextBoxNaiveServerHost_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxNaiveServerHost.Text);
        }

        private void TextBoxNaivePort_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxNaivePort.Text);
        }

        private void TextBoxNaiveUser_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxNaiveUser.Text);
        }

        private void TextBoxNaivePassword_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxNaivePassword.Text);
        }



        #endregion

        #endregion

        #region SSR 相关
        //生成SSR客户端资料
        private void GenerateSSRShareQRcodeAndBase64Url()
        {
            string proxyfolder = CheckDir("ssr_config");
            configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxSSRHostAddress.Text);
            string configSavePath = configDomainSavePath;
            //string saveFileFolderFirst = TextBoxSSRHostAddress.Text;
            //int num = 1;
            //saveFileFolder = saveFileFolderFirst;
            //CheckDir("ssr_config");
            //while (Directory.Exists(@"ssr_config\" + saveFileFolder))
            //{
            //    saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
            //    num++;
            //}
            //CheckDir(@"ssr_config\" + saveFileFolder);

            string ssrUrl = GetSSRLinkForServer();
            TextBoxURL.Text = ssrUrl;
            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\url.txt"))
            {
                sw.WriteLine(ssrUrl);
            }
            ImageShareQRcode.Source = CreateQRCode(ssrUrl, $"{configSavePath}\\QR.bmp");

            using (StreamWriter sw = new StreamWriter($"{configSavePath}\\readme.txt"))
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

                string strApplicat = "";
                string strParam = "";
                int strLenth = 20;

                //***"服务器通用连接配置参数"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine10").ToString());
                sw.WriteLine("");

                //****** 服务器地址(address): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{ TextBoxSSRHostAddress.Text}");
                strApplicat = "TextBlockServerAddress";
                strParam = TextBoxSSRHostAddress.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 端口(port): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxSSRPort.Text}");
                strApplicat = "TextBlockServerPort";
                strParam = TextBoxSSRPort.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 密码: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxSSRUUID.Text}");
                strApplicat = "TextBlockTrojanGoPassword";
                strParam = TextBoxSSRUUID.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 加密方式: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockEncryption").ToString() + $"{TextBoxSSREncryption.Text}");
                strApplicat = "TextBlockEncryption";
                strParam = TextBoxSSREncryption.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 传输协议: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTransferProtocol").ToString() + $"{TextBoxSSRTransmission.Text}");
                strApplicat = "TextBlockTransferProtocol";
                strParam = TextBoxSSRTransmission.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 混淆: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockCamouflageType").ToString() + $"{TextBoxSSRCamouflageType.Text}");
                strApplicat = "TextBlockCamouflageType";
                strParam = TextBoxSSRCamouflageType.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

            }
        }
        
        #region SSR内容双击复制到剪贴板

        private void TextBoxSSRHostAddress_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxSSRHostAddress.Text);
        }

        private void TextBoxSSRPort_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxSSRPort.Text);
        }

        private void TextBoxSSRUUID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxSSRUUID.Text);
        }

        private void TextBoxSSREncryption_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxSSREncryption.Text);
        }

        private void TextBoxSSRTransmission_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxSSRTransmission.Text);
        }

        private void TextBoxSSRCamouflageType_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxSSRCamouflageType.Text);
        }
        #endregion
        
        //生成SS客户端资料
        private void GenerateShareQRcodeAndBase64UrlSS()
        {
            //创建保存目录
            string proxyfolder = CheckDir("ss_config");
            configDomainSavePath = CreateConfigSaveDir(proxyfolder, TextBoxHostAddressSS.Text);
            string configSavePath = configDomainSavePath;
            //string configSavePath = CreateConfigSaveDir(@"ss_config", TextBoxHostAddressSS.Text);
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

                string strApplicat = "";
                string strParam = "";
                int strLenth = 20;

                //***"服务器通用连接配置参数"***
                sw.WriteLine(Application.Current.FindResource("readmeTxtTrojanExplainLine10").ToString());
                sw.WriteLine("");

                //****** 服务器地址(address): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerAddress").ToString() + $"{TextBoxHostAddressSS.Text}");
                strApplicat = "TextBlockServerAddress";
                strParam = TextBoxHostAddressSS.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 端口(port): ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockServerPort").ToString() + $"{TextBoxPortSS.Text}");
                strApplicat = "TextBlockServerPort";
                strParam = TextBoxPortSS.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 密码: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockTrojanGoPassword").ToString() + $"{TextBoxPasswordSS.Text}");
                strApplicat = "TextBlockTrojanGoPassword";
                strParam = TextBoxPasswordSS.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                //****** 加密方式: ******
                //sw.WriteLine(Application.Current.FindResource("TextBlockEncryption").ToString() + $"{TextBoxEncryptionSS.Text}");
                strApplicat = "TextBlockEncryption";
                strParam = TextBoxEncryptionSS.Text;
                sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);


                if (String.IsNullOrEmpty(TextBoxPluginNameExplainSS.Text) == false)
                {
                    //****** 插件程序:: ******
                    //sw.WriteLine(Application.Current.FindResource("TextBlockPluginNameExplainSS").ToString() + $"{TextBoxPluginNameExplainSS.Text}");
                    strApplicat = "TextBlockPluginNameExplainSS";
                    strParam = TextBoxPluginNameExplainSS.Text;
                    sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

                    //****** 插件选项: ******
                    //sw.WriteLine(Application.Current.FindResource("TextBlockPluginOptionExplainSS").ToString() + $"{TextBoxPluginOptionExplainSS.Text}");
                    strApplicat = "TextBlockPluginOptionExplainSS";
                    strParam = TextBoxPluginOptionExplainSS.Text;
                    sw.WriteLine(AlignmentStrFunc(Application.Current.FindResource($"{strApplicat}").ToString(), strLenth) + strParam);

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
        
        #region SS内容双击复制到剪贴板
        private void TextBoxHostAddressSS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxHostAddressSS.Text);
        }

        private void TextBoxPortSS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxPortSS.Text);
        }

        private void TextBoxPasswordSS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxPasswordSS.Text);
        }

        private void TextBoxEncryptionSS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxEncryptionSS.Text);
        }

        private void TextBoxPluginNameExplainSS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxPluginNameExplainSS.Text);
        }
        private void TextBoxPluginNameExplainSSpc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxPluginNameExplainSSpc.Text);
        }

        private void TextBoxPluginOptionExplainSS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxPluginOptionExplainSS.Text);
        }
        
        private void TextBoxURLpcSS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxURLpcSS.Text);
        }

        #endregion

        #endregion

        #region MTProto 界面控制
        private void RadioButtonMtgIpv4_Checked(object sender, RoutedEventArgs e)
        {
            GridMtgIpv4.Visibility = Visibility.Visible;
            GridMtgIpv6.Visibility = Visibility.Collapsed;
        }

        private void RadioButtonMtgIpv6_Checked(object sender, RoutedEventArgs e)
        {
            GridMtgIpv4.Visibility = Visibility.Collapsed;
            GridMtgIpv6.Visibility = Visibility.Visible;
        }
        private void TextBoxURLMtgTgIpv4_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxURLMtgTgIpv4.Text);
        }

        private void TextBoxURLMtgTmeIpv4_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxURLMtgTmeIpv4.Text);
        }

        private void TextBoxURLMtgTgIpv6_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxURLMtgTgIpv6.Text);
        }

        private void TextBoxURLMtgTmeIpv6_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CopyToClipboard(TextBoxURLMtgTmeIpv6.Text);
        }

        #endregion

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
        private string CheckDir(string folder)
        {
            try
            {
                if (!Directory.Exists(folder))//如果不存在就创建file文件夹
                    Directory.CreateDirectory(folder);//创建该文件夹　　            
                return folder;
            }
            catch (Exception)
            {
                return "";
            }
        }

        //目录已存在则生成序号递增,并返回所创建的目录路径。
        private string CreateConfigSaveDir(string upperDir,string configDir)
        {
            try
            {
                //string saveFileFolderFirst = configDir;
                int num = 1;
                //saveFileFolder = EncodeURIComponent(configDir);
                saveFileFolder = configDir.Replace(":","_");
                CheckDir(upperDir);
                while (Directory.Exists(upperDir + @"\" + saveFileFolder) == true)
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
            string openFolderPath = configDomainSavePath;
            System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
            this.Close();
            
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

        ///<summary>
        ///生成固定长度的空格字符串
        ///</summary>
        ///<paramname="length"></param>
        ///<returns></returns>

        private string SpaceStrFunc(int length)
        {
            string strReturn = string.Empty;
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    strReturn += " ";
                }
            }
            return strReturn;
        }

        ///<summary>
        ///将字符串生转化为固定长度左对齐，右补空格
        ///</summary>
        ///<paramname="strTemp"></param>需要补齐的字符串
        ///<paramname="length"></param>补齐后的长度
        ///<returns></returns>

        private string AlignmentStrFunc(string strTemp, int length)
        {
            byte[] byteStr = System.Text.Encoding.Default.GetBytes(strTemp.Trim());
            int iLength = byteStr.Length;
            int iNeed = length - iLength;

            byte[] spaceLen = Encoding.Default.GetBytes(" "); //一个空格的长度
            iNeed = iNeed / spaceLen.Length;

            string spaceString = SpaceStrFunc(iNeed);
            //return strTemp + spaceString;
            return spaceString + strTemp;
        }

      
    }


}
