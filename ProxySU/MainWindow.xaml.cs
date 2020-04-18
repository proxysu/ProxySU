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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Renci.SshNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Drawing;
using QRCoder;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Globalization;

namespace ProxySU
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string[] ReceiveConfigurationParameters { get; set; }
        //ReceiveConfigurationParameters[0]----模板类型
        //ReceiveConfigurationParameters[1]----服务端口
        //ReceiveConfigurationParameters[2]----uuid
        //ReceiveConfigurationParameters[3]----path
        //ReceiveConfigurationParameters[4]----domain
        //ReceiveConfigurationParameters[5]----伪装类型
        //ReceiveConfigurationParameters[6]----QUIC密钥
        //ReceiveConfigurationParameters[7]----伪装网站
        //public static ConnectionInfo ConnectionInfo;
        public MainWindow()
        {
            InitializeComponent();
            RadioButtonPasswordLogin.IsChecked = true;
            RadioButtonNoProxy.IsChecked = true;
            RadioButtonProxyNoLogin.IsChecked = true;
            RadioButtonSocks4.Visibility = Visibility.Collapsed;
            ReceiveConfigurationParameters = new string[8];
            

        }
        //远程主机连接信息
        private ConnectionInfo GenerateConnectionInfo()
        {
            ConnectionInfo connectionInfo;
            //ProgressBarSetUpProcessing.IsIndeterminate = true;
            #region 检测输入的内空是否有错，并读取内容
            if (string.IsNullOrEmpty(TextBoxHost.Text) == true || string.IsNullOrEmpty(TextBoxPort.Text) == true || string.IsNullOrEmpty(TextBoxUserName.Text) == true)
            {
                MessageBox.Show("主机地址、主机端口、用户名为必填项，不能为空");
                //exitProgram.Kill();
                return connectionInfo = null;
            }
            string sshHostName = TextBoxHost.Text.ToString();
            int sshPort = int.Parse(TextBoxPort.Text);
            string sshUser = TextBoxUserName.Text.ToString();
            if (RadioButtonPasswordLogin.IsChecked == true && string.IsNullOrEmpty(PasswordBoxHostPassword.Password) == true)
            {
                MessageBox.Show("登录密码为必填项，不能为空");
                return connectionInfo = null;
            }
            string sshPassword = PasswordBoxHostPassword.Password.ToString();
            if (RadioButtonCertLogin.IsChecked == true && string.IsNullOrEmpty(TextBoxCertFilePath.Text) == true)
            {
                MessageBox.Show("密钥文件为必填项，不能为空");
                return connectionInfo = null;
            }
            string sshPrivateKey = TextBoxCertFilePath.Text.ToString();
            ProxyTypes proxyTypes = new ProxyTypes();//默认为None
            //MessageBox.Show(proxyTypes.ToString());
            //proxyTypes = ProxyTypes.Socks5;
            if (RadioButtonHttp.IsChecked == true)
            {
                proxyTypes = ProxyTypes.Http;
            }
            else if (RadioButtonSocks4.IsChecked == true)
            {
                proxyTypes = ProxyTypes.Socks4;
            }
            else if (RadioButtonSocks5.IsChecked == true)
            {
                proxyTypes = ProxyTypes.Socks5;
            }
            else
            {
                proxyTypes = ProxyTypes.None;
            }

            //MessageBox.Show(proxyTypes.ToString());
            if (RadioButtonNoProxy.IsChecked == false && (string.IsNullOrEmpty(TextBoxProxyHost.Text) == true || string.IsNullOrEmpty(TextBoxProxyPort.Text) == true))
            {
                MessageBox.Show("如果选择了代理，则代理地址与端口不能为空");
                return connectionInfo = null;
            }
            string sshProxyHost = TextBoxProxyHost.Text.ToString();
            int sshProxyPort = int.Parse(TextBoxProxyPort.Text.ToString());
            if (RadioButtonNoProxy.IsChecked==false && RadiobuttonProxyYesLogin.IsChecked == true && (string.IsNullOrEmpty(TextBoxProxyUserName.Text) == true || string.IsNullOrEmpty(PasswordBoxProxyPassword.Password) == true))
            {
                MessageBox.Show("如果代理需要登录，则代理登录的用户名与密码不能为空");
                return connectionInfo = null;
            }
            string sshProxyUser = TextBoxProxyUserName.Text.ToString();
            string sshProxyPassword = PasswordBoxProxyPassword.Password.ToString();

            #endregion


            //var connectionInfo = new PasswordConnectionInfo(sshHostName, sshPort, sshUser, sshPassword);

            connectionInfo = new ConnectionInfo(
                                    sshHostName,
                                    sshPort,
                                    sshUser,
                                    proxyTypes,
                                    sshProxyHost,
                                    sshProxyPort,
                                    sshProxyUser,
                                    sshProxyPassword,
                                    new PasswordAuthenticationMethod(sshUser, sshPassword)
                                    //new PrivateKeyAuthenticationMethod(sshUser, new PrivateKeyFile(sshPrivateKey))
                                    );

            if (RadioButtonCertLogin.IsChecked == true)
            {
                connectionInfo = new ConnectionInfo(
                                        sshHostName,
                                        sshPort,
                                        sshUser,
                                        proxyTypes,
                                        sshProxyHost,
                                        sshProxyPort,
                                        sshProxyUser,
                                        sshProxyPassword,
                                        //new PasswordAuthenticationMethod(sshUser, sshPassword)
                                        new PrivateKeyAuthenticationMethod(sshUser, new PrivateKeyFile(sshPrivateKey))
                                        );

            }
            return connectionInfo;
        }

        //开始布署安装
        private void Button_Login_Click(object sender, RoutedEventArgs e)

        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if(connectionInfo==null)
            {
                MessageBox.Show("远程主机连接信息有误，请检查");
                return;
            }
            //using (var client = new SshClient(sshHostName, sshPort, sshUser, sshPassword))
            //Action<ConnectionInfo, TextBlock> startSetUpAction = new Action<ConnectionInfo, TextBlock>(StartSetUpRemoteHost);
            //string serverConfig = TextBoxJsonPath.Text.ToString().Replace("\\","\\\\");
            //读取模板配置
            //sed -i 's/PermitRootLogin no/PermitRootLogin yes/' /etc/v2ray/config.json
            string serverConfig="";  //服务端配置文件
            string clientConfig = "";   //生成的客户端配置文件
            string upLoadPath = "/etc/v2ray/config.json"; //服务端文件位置
            //生成客户端配置时，连接的服务主机的IP或者域名
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[4])==true)
            {
                ReceiveConfigurationParameters[4] = TextBoxHost.Text.ToString();
            }
            //选择模板
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[0]) == true)
            {
                MessageBox.Show("请先选择配置模板！");
                return;
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "TCP"))
            {
                serverConfig = "TemplateConfg\\tcp_server_config.json";
                clientConfig = "TemplateConfg\\tcp_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "TCPhttp"))
            {
                serverConfig = "TemplateConfg\\tcp_http_server_config.json";
                clientConfig = "TemplateConfg\\tcp_http_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "tcpTLS"))
            {
                serverConfig = "TemplateConfg\\tcp_TLS_server_config.json";
                clientConfig = "TemplateConfg\\tcp_TLS_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "tcpTLSselfSigned"))
            {
                serverConfig = "TemplateConfg\\tcpTLSselfSigned_server_config.json";
                clientConfig = "TemplateConfg\\tcpTLSselfSigned_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "webSocket"))
            {
                serverConfig = "TemplateConfg\\webSocket_server_config.json";
                clientConfig = "TemplateConfg\\webSocket_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS"))
            {
                serverConfig = "TemplateConfg\\WebSocket_TLS_server_config.json";
                clientConfig = "TemplateConfg\\WebSocket_TLS_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned"))
            {
                serverConfig = "TemplateConfg\\WebSocketTLS_selfSigned_server_config.json";
                clientConfig = "TemplateConfg\\WebSocketTLS_selfSigned_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web"))
            {
                serverConfig = "TemplateConfg\\WebSocketTLSWeb_server_config.json";
                clientConfig = "TemplateConfg\\WebSocketTLSWeb_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "Http2"))
            {
                serverConfig = "TemplateConfg\\http2_server_config.json";
                clientConfig = "TemplateConfg\\http2_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "http2Web"))
            {
                serverConfig = "TemplateConfg\\Http2Web_server_config.json";
                clientConfig = "TemplateConfg\\Http2Web_client_config.json";
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "http2selfSigned"))
            {
                serverConfig = "TemplateConfg\\Http2selfSigned_server_config.json";
                clientConfig = "TemplateConfg\\Http2selfSigned_client_config.json";
            }
            //else if (String.Equals(ReceiveConfigurationParameters[0], "MkcpNone")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2SRTP")||String.Equals(ReceiveConfigurationParameters[0], "mKCPuTP")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2WechatVideo")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2DTLS")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2WireGuard"))
            else if (ReceiveConfigurationParameters[0].Contains("mKCP"))
            {
                serverConfig = "TemplateConfg\\mkcp_server_config.json";
                clientConfig = "TemplateConfg\\mkcp_client_config.json";
            }

            // else if (String.Equals(ReceiveConfigurationParameters[0], "QuicNone") || String.Equals(ReceiveConfigurationParameters[0], "QuicSRTP") || String.Equals(ReceiveConfigurationParameters[0], "Quic2uTP") || String.Equals(ReceiveConfigurationParameters[0], "QuicWechatVideo") || String.Equals(ReceiveConfigurationParameters[0], "QuicDTLS") || String.Equals(ReceiveConfigurationParameters[0], "QuicWireGuard"))
            else if (ReceiveConfigurationParameters[0].Contains("Quic"))
            {
                serverConfig = "TemplateConfg\\quic_server_config.json";
                clientConfig = "TemplateConfg\\quic_client_config.json";
            }

            //Thread thread
            Thread thread = new Thread(() => StartSetUpRemoteHost(connectionInfo, TextBlockSetUpProcessing, ProgressBarSetUpProcessing, serverConfig, clientConfig, upLoadPath));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            // Task task = new Task(() => StartSetUpRemoteHost(connectionInfo, TextBlockSetUpProcessing, ProgressBarSetUpProcessing, serverConfig, clientConfig, upLoadPath));
            //task.Start();
            
        }

        #region 端口数字防错代码，密钥选择代码
        private void Button_canel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
       // private static readonly Regex _regex = new Regex("[^0-9]+");
        private void TextBoxPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxPort_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void ButtonOpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Cert Files (*.*)|*.*"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                TextBoxCertFilePath.Text = openFileDialog.FileName;
            }
        }
        #endregion

        #region 界面控件的有效无效控制代码块
        private void RadioButtonNoProxy_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockProxyHost.IsEnabled = false;
            TextBlockProxyHost.Visibility = Visibility.Collapsed;
            TextBoxProxyHost.IsEnabled = false;
            TextBoxProxyHost.Visibility = Visibility.Collapsed;
            TextBlockProxyPort.IsEnabled = false;
            TextBlockProxyPort.Visibility = Visibility.Collapsed;
            TextBoxProxyPort.IsEnabled = false;
            TextBoxProxyPort.Visibility = Visibility.Collapsed;
            RadioButtonProxyNoLogin.IsEnabled = false;
            RadioButtonProxyNoLogin.Visibility = Visibility.Collapsed;
            RadiobuttonProxyYesLogin.IsEnabled = false;
            RadiobuttonProxyYesLogin.Visibility = Visibility.Collapsed;
            TextBlockProxyUser.IsEnabled = false;
            TextBlockProxyUser.Visibility = Visibility.Collapsed;
            TextBoxProxyUserName.IsEnabled = false;
            TextBoxProxyUserName.Visibility = Visibility.Collapsed;
            TextBlockProxyPassword.IsEnabled = false;
            TextBlockProxyPassword.Visibility = Visibility.Collapsed;
            PasswordBoxProxyPassword.IsEnabled = false;
            PasswordBoxProxyPassword.Visibility = Visibility.Collapsed;
        }

        private void RadioButtonNoProxy_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBlockProxyHost.IsEnabled = true;
            TextBlockProxyHost.Visibility = Visibility.Visible;
            TextBoxProxyHost.IsEnabled = true;
            TextBoxProxyHost.Visibility = Visibility.Visible;
            TextBlockProxyPort.IsEnabled = true;
            TextBlockProxyPort.Visibility = Visibility.Visible;
            TextBoxProxyPort.IsEnabled = true;
            TextBoxProxyPort.Visibility = Visibility.Visible;
            RadioButtonProxyNoLogin.IsEnabled = true;
            RadioButtonProxyNoLogin.Visibility = Visibility.Visible;
            RadiobuttonProxyYesLogin.IsEnabled = true;
            RadiobuttonProxyYesLogin.Visibility = Visibility.Visible;
            if (RadioButtonProxyNoLogin.IsChecked == true)
            {
                TextBlockProxyUser.IsEnabled = false;
                TextBlockProxyUser.Visibility = Visibility.Collapsed;
                TextBlockProxyPassword.IsEnabled = false;
                TextBlockProxyPassword.Visibility = Visibility.Collapsed;
                TextBoxProxyUserName.IsEnabled = false;
                TextBoxProxyUserName.Visibility = Visibility.Collapsed;
                PasswordBoxProxyPassword.IsEnabled = false;
                PasswordBoxProxyPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                TextBlockProxyUser.IsEnabled = true;
                TextBlockProxyUser.Visibility = Visibility.Visible;
                TextBoxProxyUserName.IsEnabled = true;
                TextBoxProxyUserName.Visibility = Visibility.Visible;
                TextBlockProxyPassword.IsEnabled = true;
                TextBlockProxyPassword.Visibility = Visibility.Visible;
                PasswordBoxProxyPassword.IsEnabled = true;
                PasswordBoxProxyPassword.Visibility = Visibility.Visible;
            }
        }

        private void RadioButtonPasswordLogin_Checked(object sender, RoutedEventArgs e)
        {
            ButtonOpenFileDialog.IsEnabled = false;
            ButtonOpenFileDialog.Visibility = Visibility.Collapsed;
            TextBoxCertFilePath.IsEnabled = false;
            TextBoxCertFilePath.Visibility = Visibility.Collapsed;
            TextBlockPassword.Text = "密码：";
            //TextBlockPassword.Visibility = Visibility.Visible;
            PasswordBoxHostPassword.IsEnabled = true;
            PasswordBoxHostPassword.Visibility = Visibility.Visible;
        }

        private void RadioButtonCertLogin_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockPassword.Text = "密钥：";
            //TextBlockPassword.Visibility = Visibility.Collapsed;
            PasswordBoxHostPassword.IsEnabled = false;
            PasswordBoxHostPassword.Visibility = Visibility.Collapsed;
            ButtonOpenFileDialog.IsEnabled = true;
            ButtonOpenFileDialog.Visibility = Visibility.Visible;
            TextBoxCertFilePath.IsEnabled = true;
            TextBoxCertFilePath.Visibility = Visibility.Visible;
        }

        private void RadioButtonProxyNoLogin_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockProxyUser.IsEnabled = false;
            TextBlockProxyUser.Visibility = Visibility.Collapsed;
            TextBlockProxyPassword.IsEnabled = false;
            TextBlockProxyPassword.Visibility = Visibility.Collapsed;
            TextBoxProxyUserName.IsEnabled = false;
            TextBoxProxyUserName.Visibility = Visibility.Collapsed;
            PasswordBoxProxyPassword.IsEnabled = false;
            PasswordBoxProxyPassword.Visibility = Visibility.Collapsed;
        }

        private void RadiobuttonProxyYesLogin_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockProxyUser.IsEnabled = true;
            TextBlockProxyUser.Visibility = Visibility.Visible;
            TextBlockProxyPassword.IsEnabled = true;
            TextBlockProxyPassword.Visibility = Visibility.Visible;
            TextBoxProxyUserName.IsEnabled = true;
            TextBoxProxyUserName.Visibility = Visibility.Visible;
            PasswordBoxProxyPassword.IsEnabled = true;
            PasswordBoxProxyPassword.Visibility = Visibility.Visible;
        }
        #endregion

        //登录远程主机布署程序
        private void StartSetUpRemoteHost(ConnectionInfo connectionInfo,TextBlock textBlockName, ProgressBar progressBar, string serverConfig,string clientConfig,string upLoadPath)
        {
            string currentStatus = "正在登录远程主机......";
            Action<TextBlock, ProgressBar, string> updateAction = new Action<TextBlock, ProgressBar, string>(UpdateTextBlock);
            textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);

            try
            {
                #region 主机指纹，暂未启用
                //byte[] expectedFingerPrint = new byte[] {
                //                                0x66, 0x31, 0xaf, 0x00, 0x54, 0xb9, 0x87, 0x31,
                //                                0xff, 0x58, 0x1c, 0x31, 0xb1, 0xa2, 0x4c, 0x6b
                //                            };
                #endregion
                using (var client = new SshClient(connectionInfo))

                {
                    #region ssh登录验证主机指纹代码块，暂未启用
                    //    client.HostKeyReceived += (sender, e) =>
                    //    {
                    //        if (expectedFingerPrint.Length == e.FingerPrint.Length)
                    //        {
                    //            for (var i = 0; i < expectedFingerPrint.Length; i++)
                    //            {
                    //                if (expectedFingerPrint[i] != e.FingerPrint[i])
                    //                {
                    //                    e.CanTrust = false;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            e.CanTrust = false;
                    //        }
                    //    };
                    #endregion

                    client.Connect();
                    if (client.IsConnected == true)
                    {
                        currentStatus = "主机登录成功";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);
                    }
                    //检测是否安装有V2ray
                    currentStatus = "检测系统是否已经安装V2ray......";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);

                    //client.RunCommand("find / -name v2ray");
                    string cmdTestV2rayInstalled = @"find / -name v2ray";
                    //MessageBox.Show(cmdTestV2rayInstalled);
                    string resultCmdTestV2rayInstalled = client.RunCommand(cmdTestV2rayInstalled).Result;
                    //client.Disconnect();
                    //MessageBox.Show(resultCmdTestV2rayInstalled);
                    if (resultCmdTestV2rayInstalled.Contains("/usr/bin/v2ray") == true)
                    {
                        MessageBoxResult messageBoxResult = MessageBox.Show("远程主机已安装V2ray,是否强制重新安装？", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (messageBoxResult==MessageBoxResult.No)
                        {
                            currentStatus = "安装取消，退出";
                            textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                            Thread.Sleep(1000);
                            return;
                        }
                    }

                    //检测远程主机系统环境是否符合要求
                    currentStatus = "检测系统是否符合安装要求......";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);

                    var result = client.RunCommand("uname -r");
                    //var result = client.RunCommand("cat /root/test.ver");
                    string[] linuxKernelVerStr= result.Result.Split('-');

                    bool detectResult = DetectKernelVersion(linuxKernelVerStr[0]);
                    if (detectResult == false)
                    {
                        MessageBox.Show($"当前系统内核版本为{linuxKernelVerStr[0]}，V2ray要求内核为2.6.23及以上。请升级内核再安装！");
                        currentStatus = "系统内核版本不符合要求，安装失败！！";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);               
                    }

                    //检测系统是否支持yum 或 apt-get或zypper，且支持Systemd
                    //如果不存在组件，则命令结果为空，string.IsNullOrEmpty值为真，
                    bool getApt = String.IsNullOrEmpty(client.RunCommand("command -v apt-get").Result);
                    bool getYum = String.IsNullOrEmpty(client.RunCommand("command -v yum").Result);
                    bool getZypper = String.IsNullOrEmpty(client.RunCommand("command -v zypper").Result);
                    bool getSystemd = String.IsNullOrEmpty(client.RunCommand("command -v systemctl").Result);
                    bool getGetenforce = String.IsNullOrEmpty(client.RunCommand("command -v getenforce").Result);

                    //没有安装apt-get，也没有安装yum，也没有安装zypper,或者没有安装systemd的，不满足安装条件
                    //也就是apt-get ，yum, zypper必须安装其中之一，且必须安装Systemd的系统才能安装。
                    if ((getApt && getYum && getZypper) || getSystemd)
                    {
                        MessageBox.Show($"系统缺乏必要的安装组件如:apt-get||yum||zypper||Syetemd，主机系统推荐使用：CentOS 7/8,Debian 8/9/10,Ubuntu 16.04及以上版本");
                        currentStatus = "系统环境不满足要求，安装失败！！";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);
                        return;
                    }
                    //判断是否启用了SELinux,如果启用了，并且工作在Enforcing模式下，则改为Permissive模式
                    if (getGetenforce == false)
                    {
                        string testSELinux = client.RunCommand("getenforce").Result;
                        //MessageBox.Show(testSELinux);
                        if (testSELinux.Contains("Enforcing")==true)
                        {
                            //MessageBox.Show("Enforcing");
                            client.RunCommand("setenforce  0");//不重启改为Permissive模式
                            client.RunCommand("sed -i 's/SELINUX=enforcing/SELINUX=permissive/' /etc/selinux/config");//重启也工作在Permissive模式下
                        }
                        //else
                        //{
                        //    MessageBox.Show("非Enforcing");
                        //}
                    }

                    //校对时间
                    currentStatus = "校对时间......";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);
                    //获取远程主机的时间戳
                    long timeStampVPS = Convert.ToInt64(client.RunCommand("date +%s").Result.ToString());
                    //MessageBox.Show(timesStampVPS.ToString());
                    //获取本地时间戳
                    TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    long timeStampLocal = Convert.ToInt64(ts.TotalSeconds);
                    if (Math.Abs(timeStampLocal - timeStampVPS) >= 90)
                    {

                        MessageBox.Show("本地时间与远程主机时间相差超过限制(90秒)，请先用\"系统工具-->时间校对\"校对时间后再设置");
                        currentStatus = "时间较对失败......";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);
                        return;
                    }
                    //MessageBox.Show(timesStamp2.ToString());

                    //如果使用如果是WebSocket + TLS + Web模式，需要检测域名解析是否正确
                    if (serverConfig.Contains("WebSocketTLSWeb") == true || serverConfig.Contains("http2") == true)
                    {
                        currentStatus = "正在检测域名是否解析到当前VPS的IP上......";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);

                        string nativeIp = client.RunCommand("curl -4 ip.sb").Result.ToString();
                        string testDomainCmd = "ping " + ReceiveConfigurationParameters[4] + " -c 1 | grep -oE -m1 \"([0-9]{1,3}\\.){3}[0-9]{1,3}\"";
                        string resultCmd = client.RunCommand(testDomainCmd).Result.ToString();
                        //MessageBox.Show("nativeIp"+nativeIp);
                        //MessageBox.Show("resultCmd"+ resultCmd);
                        if (String.Equals(nativeIp, resultCmd) == true)
                        {
                            currentStatus = "解析正确！";
                            textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            currentStatus = "域名未能正确解析到当前VPS的IP上!安装失败！";
                            textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                            Thread.Sleep(1000);
                            MessageBox.Show("域名未能正确解析到当前VPS的IP上，请检查！若解析设置正确，请等待生效后再重试安装。如果域名使用了CDN，请先关闭！");
                            return;
                        }
                        
                    }
                    if (serverConfig.Contains("TLS") == true || serverConfig.Contains("http2") == true || serverConfig.Contains("Http2") == true) {
                        //检测是否安装lsof
                        if (string.IsNullOrEmpty(client.RunCommand("command -v lsof").Result) == true)
                        {
                            //为假则表示系统有相应的组件。
                            if (getApt == false)
                            {
                                client.RunCommand("apt-get -qq update");
                                client.RunCommand("apt-get -y -qq install lsof");
                            }
                            if (getYum == false)
                            {
                                client.RunCommand("yum -q makecache");
                                client.RunCommand("yum -y -q install lsof");
                            }
                            if (getZypper == false)
                            {
                                client.RunCommand("zypper ref");
                                client.RunCommand("zypper -y install lsof");
                            }
                        }
                        currentStatus = "正在检测端口占用情况......";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);
                        //MessageBox.Show(@"lsof -n -P -i :80 | grep LISTEN");
                        //MessageBox.Show(client.RunCommand(@"lsof -n -P -i :80 | grep LISTEN").Result);
                        if (String.IsNullOrEmpty(client.RunCommand(@"lsof -n -P -i :80 | grep LISTEN").Result) == false || String.IsNullOrEmpty(client.RunCommand(@"lsof -n -P -i :443 | grep LISTEN").Result) == false)
                        {
                            MessageBox.Show("80/443端口之一，或全部被占用，请先用系统工具中的“释放80/443端口”工具，释放出，再重新安装");
                            currentStatus = "端口被占用，安装失败......";
                            textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                            Thread.Sleep(1000);
                            return;
                        }
                    }
                    currentStatus = "符合安装要求,布署中......";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);
                    
                    //在相应系统内安装curl(如果没有安装curl)
                    if (string.IsNullOrEmpty(client.RunCommand("command -v curl").Result) == true)
                    {
                        //为假则表示系统有相应的组件。
                        if (getApt == false)
                        {
                            client.RunCommand("apt-get -qq update");
                            client.RunCommand("apt-get -y -qq install curl");
                        }
                        if (getYum == false)
                        {
                            client.RunCommand("yum -q makecache");
                            client.RunCommand("yum -y -q install curl");
                        }
                        if (getZypper == false)
                        {
                            client.RunCommand("zypper ref");
                            client.RunCommand("zypper -y install curl");
                        }
                    }


                    //下载官方安装脚本安装

                    client.RunCommand("curl -o /tmp/go.sh https://install.direct/go.sh");
                    client.RunCommand("bash /tmp/go.sh -f");
                    string installResult = client.RunCommand("find / -name v2ray").Result.ToString();

                    if (!installResult.Contains("/usr/bin/v2ray"))
                    {
                        MessageBox.Show("安装V2ray失败(官方脚本go.sh运行出错！");
                        client.Disconnect();
                        currentStatus = "安装V2ray失败(官方脚本go.sh运行出错！";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        return;
                    }
                    client.RunCommand("mv /etc/v2ray/config.json /etc/v2ray/config.json.1");

                    //上传配置文件
                    currentStatus = "V2ray程序安装完毕，配置文件上传中......";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);

                    //生成服务端配置
                    using (StreamReader reader = File.OpenText(serverConfig))
                    {
                        JObject serverJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                        //设置uuid
                        serverJson["inbounds"][0]["settings"]["clients"][0]["id"] = ReceiveConfigurationParameters[2];
                        //除WebSocketTLSWeb/http2Web模式外设置监听端口
                        if (serverConfig.Contains("WebSocketTLSWeb") == false && serverConfig.Contains("Http2Web") == false)
                        {
                            serverJson["inbounds"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        }
                        //TLS自签证书/http2Web模式下，使用v2ctl 生成自签证书
                        if (serverConfig.Contains("selfSigned") == true|| serverConfig.Contains("Http2Web") == true)
                        {
                            string selfSignedCa = client.RunCommand("/usr/bin/v2ray/v2ctl cert --ca").Result;
                            JObject selfSignedCaJObject = JObject.Parse(selfSignedCa);
                            serverJson["inbounds"][0]["streamSettings"]["tlsSettings"]["certificates"][0] = selfSignedCaJObject;
                        }
                        //如果是WebSocketTLSWeb/WebSocketTLS/WebSocketTLS(自签证书)模式，则设置路径
                        if (serverConfig.Contains("WebSocket") == true)
                        {
                            serverJson["inbounds"][0]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[3];
                        }
                        //如果是Http2模式下，设置路径
                        if (serverConfig.Contains("http2") == true|| serverConfig.Contains("Http2") == true)
                        {
                            serverJson["inbounds"][0]["streamSettings"]["httpSettings"]["path"] = ReceiveConfigurationParameters[3];
                        }
                        //如果是Http2Web模式下，设置host
                        if (serverConfig.Contains("Http2Web") == true)
                        {
                            serverJson["inbounds"][0]["streamSettings"]["httpSettings"]["path"] = ReceiveConfigurationParameters[3];
                            serverJson["inbounds"][0]["streamSettings"]["httpSettings"]["host"][0] = ReceiveConfigurationParameters[4];
                        }
                        //mkcp模式下，设置伪装类型
                        if (serverConfig.Contains("mkcp") == true)
                        {
                            serverJson["inbounds"][0]["streamSettings"]["kcpSettings"]["header"]["type"] = ReceiveConfigurationParameters[5];
                        }
                        //quic模式下设置伪装类型及密钥
                        if (serverConfig.Contains("quic") == true)
                        {
                            serverJson["inbounds"][0]["streamSettings"]["quicSettings"]["header"]["type"] = ReceiveConfigurationParameters[5];
                            serverJson["inbounds"][0]["streamSettings"]["quicSettings"]["key"] = ReceiveConfigurationParameters[6];
                        }

                        using (StreamWriter sw = new StreamWriter(@"config.json"))
                        {
                            sw.Write(serverJson.ToString());
                        }
                    }
                    UploadConfig(connectionInfo, @"config.json",upLoadPath);

                    File.Delete(@"config.json");

                    //打开防火墙端口
                    string openFireWallPort = ReceiveConfigurationParameters[1];
                    if (String.IsNullOrEmpty(client.RunCommand("command -v firewall-cmd").Result) == false)
                    {
                        if (String.Equals(openFireWallPort, "443"))
                        {
                            client.RunCommand("firewall-cmd --zone=public --add-port=80/tcp --permanent");
                            client.RunCommand("firewall-cmd --zone=public --add-port=443/tcp --permanent");
                            client.RunCommand("firewall-cmd --reload");
                        }
                        else
                        {
                            client.RunCommand($"firewall-cmd --zone=public --add-port={openFireWallPort}/tcp --permanent");
                            client.RunCommand($"firewall-cmd --zone=public --add-port={openFireWallPort}/udp --permanent");
                            client.RunCommand("firewall-cmd --reload");
                        }
                    }
                    if (String.IsNullOrEmpty(client.RunCommand("command -v ufw").Result) == false)
                    {
                        if (String.Equals(openFireWallPort, "443"))
                        {
                            client.RunCommand("ufw allow 80");
                            client.RunCommand("ufw allow 443");
                            client.RunCommand("ufw reset");
                        }
                        else
                        {
                            client.RunCommand($"ufw allow {openFireWallPort}/tcp");
                            client.RunCommand($"ufw allow {openFireWallPort}/udp");
                            client.RunCommand("ufw reset");
                        }
                    }

                    //如果是WebSocket + TLS + Web模式，需要安装Caddy
                    if (serverConfig.Contains("WebSocketTLSWeb")==true || serverConfig.Contains("Http2Web") == true)
                    {
                        currentStatus = "使用WebSocket+TLS+Web/HTTP2+TLS+Web模式，正在安装Caddy......";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);
                        
                        client.RunCommand("curl https://getcaddy.com -o getcaddy");
                        client.RunCommand("bash getcaddy personal hook.service");
                        client.RunCommand("mkdir -p /etc/caddy");
                        client.RunCommand("mkdir -p /var/www");

                        
                        currentStatus = "上传Caddy配置文件......";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);
                        if (serverConfig.Contains("WebSocketTLSWeb") == true)
                        {
                        serverConfig = "TemplateConfg\\WebSocketTLSWeb_server_config.caddyfile";
                        }
                        if (serverConfig.Contains("Http2Web") == true)
                        {
                            serverConfig = "TemplateConfg\\Http2Web_server_config.caddyfile";
                        }
                        upLoadPath = "/etc/caddy/Caddyfile";
                        UploadConfig(connectionInfo, serverConfig, upLoadPath);

                        //设置Caddyfile文件中的tls 邮箱
                        string sshCmdEmail = $"email={ReceiveConfigurationParameters[4]};email=${{email/./@}};echo $email";//结尾有回车符
                        string email = client.RunCommand(sshCmdEmail).Result.Replace("\n", "");//删除结尾的回车符
                        string sshCmd = $"sed -i 's/off/{email}/' {upLoadPath}";//设置Caddyfile中的邮箱
                        client.RunCommand(sshCmd);
                        //设置Path
                        sshCmd = $"sed -i 's/##path##/\\{ReceiveConfigurationParameters[3]}/' {upLoadPath}";
                        //MessageBox.Show(sshCmd);
                        client.RunCommand(sshCmd);
                        //设置域名
                        sshCmd = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}/' {upLoadPath}";
                        //MessageBox.Show(sshCmd);
                        client.RunCommand(sshCmd);
                        //设置伪装网站
                        if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7])==false)
                        {
                            sshCmd = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
                            //MessageBox.Show(sshCmd);
                            client.RunCommand(sshCmd);
                        }
                        Thread.Sleep(2000);
                       
                       //安装Caddy服务
                        sshCmd = $"caddy -service install -agree -conf /etc/caddy/Caddyfile -email {email}";
                        //MessageBox.Show(sshCmd);
                        client.RunCommand(sshCmd);
                       
                        
                        //启动Caddy服务
                        client.RunCommand("caddy -service restart");
                    }

                    if (serverConfig.Contains("http2") == true|| serverConfig.Contains("WebSocket_TLS") ==true|| serverConfig.Contains("tcp_TLS") == true)
                    {
                        currentStatus = "使用Http2/WebSocket+TLS/tcp+TLS模式，正在安装acme.sh......";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);

                        if (getApt == false)
                        {
                            //client.RunCommand("apt-get -qq update");
                            client.RunCommand("apt-get -y -qq install socat");
                        }
                        if (getYum == false)
                        {
                            //client.RunCommand("yum -q makecache");
                            client.RunCommand("yum -y -q install socat");
                        }
                        if (getZypper == false)
                        {
                           // client.RunCommand("zypper ref");
                            client.RunCommand("zypper -y install socat");
                        }
                        client.RunCommand("curl https://raw.githubusercontent.com/acmesh-official/acme.sh/master/acme.sh  | INSTALLONLINE=1  sh");
                        client.RunCommand("cd ~/.acme.sh/");
                        client.RunCommand("alias acme.sh=~/.acme.sh/acme.sh");

                        currentStatus = "申请域名证书......";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);

                        client.RunCommand("mkdir -p /etc/v2ray/ssl");
                        client.RunCommand($"/root/.acme.sh/acme.sh  --issue  --standalone  -d {ReceiveConfigurationParameters[4]}");

                        currentStatus = "安装证书到V2ray......";
                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                        Thread.Sleep(1000);
                        client.RunCommand($"/root/.acme.sh/acme.sh  --installcert  -d {ReceiveConfigurationParameters[4]}  --certpath /etc/v2ray/ssl/v2ray_ssl.crt --keypath /etc/v2ray/ssl/v2ray_ssl.key  --capath  /etc/v2ray/ssl/v2ray_ssl.crt  --reloadcmd  \"systemctl restart v2ray\"");
                    }

                    currentStatus = "正在启动V2ray......";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);
                    //启动V2ray服务
                    client.RunCommand("systemctl restart v2ray");

                    currentStatus = "V2ray启动成功！";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);


                    //生成客户端配置
                    currentStatus = "生成客户端配置......";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);
                    if (!Directory.Exists("config"))//如果不存在就创建file文件夹　　             　　              
                    {
                        Directory.CreateDirectory("config");//创建该文件夹　　   
                    }
                    //string clientConfig = "TemplateConfg\\tcp_client_config.json";
                    using (StreamReader reader = File.OpenText(clientConfig))
                    {
                        JObject clientJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                        clientJson["outbounds"][0]["settings"]["vnext"][0]["address"] = ReceiveConfigurationParameters[4];
                        clientJson["outbounds"][0]["settings"]["vnext"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        clientJson["outbounds"][0]["settings"]["vnext"][0]["users"][0]["id"] = ReceiveConfigurationParameters[2];
                        if (clientConfig.Contains("WebSocket")==true)
                        {
                            clientJson["outbounds"][0]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[3];
                        }
                        if (clientConfig.Contains("http2") == true|| clientConfig.Contains("Http2") == true)
                        {
                            clientJson["outbounds"][0]["streamSettings"]["httpSettings"]["path"] = ReceiveConfigurationParameters[3];
                        }
                        if (clientConfig.Contains("Http2Web") == true)
                        {
                            clientJson["outbounds"][0]["streamSettings"]["httpSettings"]["host"][0] = ReceiveConfigurationParameters[4];
                        }
                        if (clientConfig.Contains("mkcp")==true)
                        {
                            clientJson["outbounds"][0]["streamSettings"]["kcpSettings"]["header"]["type"] = ReceiveConfigurationParameters[5];
                        }
                        if (clientConfig.Contains("quic") == true)
                        {
                            clientJson["outbounds"][0]["streamSettings"]["quicSettings"]["header"]["type"] = ReceiveConfigurationParameters[5];
                            clientJson["outbounds"][0]["streamSettings"]["quicSettings"]["key"] = ReceiveConfigurationParameters[6];
                        }


                        using (StreamWriter sw = new StreamWriter(@"config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }

                    client.Disconnect();

                    currentStatus = "安装成功";
                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    Thread.Sleep(1000);

                    //显示服务端连接参数
                    //MessageBox.Show("用于V2ray官方客户端的配置文件已保存在config文件夹中");
                    ResultClientInformation resultClientInformation = new ResultClientInformation();
                    resultClientInformation.ShowDialog();

                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                //MessageBox.Show(ex1.Message);
                if (ex1.Message.Contains("连接尝试失败") == true)
                {
                    MessageBox.Show($"{ex1.Message}\n请检查主机地址及端口是否正确，如果通过代理，请检查代理是否正常工作");
                }

                else if (ex1.Message.Contains("denied (password)") == true)
                {
                    MessageBox.Show($"{ex1.Message}\n密码错误或用户名错误");
                }
                else if (ex1.Message.Contains("Invalid private key file") == true)
                {
                    MessageBox.Show($"{ex1.Message}\n所选密钥文件错误或者格式不对");
                }
                else if (ex1.Message.Contains("denied (publickey)") == true)
                {
                    MessageBox.Show($"{ex1.Message}\n使用密钥登录，密钥文件错误或用户名错误");
                }
                else if (ex1.Message.Contains("目标计算机积极拒绝") == true)
                {
                    MessageBox.Show($"{ex1.Message}\n主机地址错误，如果使用了代理，也可能是连接代理的端口错误");
                }
                else
                {
                    MessageBox.Show("发生错误");
                    MessageBox.Show(ex1.Message);
                }
                currentStatus = "主机登录失败";
                textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);

            }
            #endregion

        }
        //上传配置文件
        private void UploadConfig(ConnectionInfo connectionInfo,string uploadConfig,string upLoadPath)
        {
            try
            {
                using (var sftpClient = new SftpClient(connectionInfo))
                {
                    sftpClient.Connect();
                    //MessageBox.Show("sftp信息1" + sftpClient.ConnectionInfo.ServerVersion.ToString());
                    //sftpClient.UploadFile(File.OpenRead("TemplateConfg\tcp_server_config.json"), "/etc/v2ray/config.json", true);
                    FileStream openUploadConfigFile = File.OpenRead(uploadConfig);
                    sftpClient.UploadFile(openUploadConfigFile, upLoadPath, true);
                    openUploadConfigFile.Close();
                    //MessageBox.Show("sftp信息" + sftpClient.ConnectionInfo.ServerVersion.ToString());
                    sftpClient.Disconnect();
                }

            }
            catch (Exception ex2)
            {
                MessageBox.Show("sftp" + ex2.ToString());
                MessageBox.Show("sftp出现未知错误");
            }
        }
        //下载配置文件
        private void DownloadConfig(ConnectionInfo connectionInfo, string downloadConfig,string downloadPath)
        {
            try
            {
                using (var sftpClient = new SftpClient(connectionInfo))
                {
                    sftpClient.Connect();
                    //MessageBox.Show("sftp信息1" + sftpClient.ConnectionInfo.ServerVersion.ToString());
                    FileStream createDownloadConfig = File.Open(downloadConfig, FileMode.Create);
                    sftpClient.DownloadFile(downloadPath, createDownloadConfig);
                    createDownloadConfig.Close();
                    //MessageBox.Show("sftp信息" + sftpClient.ConnectionInfo.ServerVersion.ToString());
                    sftpClient.Disconnect();
                }

            }
            catch (Exception ex2)
            {
                MessageBox.Show("sftp" + ex2.ToString());
                MessageBox.Show("sftp出现未知错误");
            }
        }

        //更新UI显示内容
        private void UpdateTextBlock(TextBlock textBlockName, ProgressBar progressBar, string currentStatus)
        {
            textBlockName.Text = currentStatus;
            //if (currentStatus.Contains("正在登录远程主机") == true)
            //{
            //    progressBar.IsIndeterminate = true;
            //}
            //else if (currentStatus.Contains("主机登录成功") == true)
            //{
            //    progressBar.IsIndeterminate = true;
            //    //progressBar.Value = 100;
            //}
            //else if (currentStatus.Contains("检测系统是否符合安装要求") == true)
            //{
            //    progressBar.IsIndeterminate = true;
            //    //progressBar.Value = 100;
            //}
            //else if (currentStatus.Contains("布署中") == true)
            //{
            //    progressBar.IsIndeterminate = true;
            //    //progressBar.Value = 100;
            //}
            //else 
            if (currentStatus.Contains("安装成功") == true)
            {
                progressBar.IsIndeterminate = false;
                progressBar.Value = 100;
            }
            else if(currentStatus.Contains("失败") == true|| currentStatus.Contains("取消") == true)
            {
                progressBar.IsIndeterminate = false;
                progressBar.Value = 0;
            }
            else
            {
                progressBar.IsIndeterminate = true;
                //progressBar.Value = 0;
            }


        }
        //检测系统内核是否符合安装要求
        private static bool DetectKernelVersion(string kernelVer)
        {
            string[] linuxKernelCompared = kernelVer.Split('.');
            if (int.Parse(linuxKernelCompared[0]) > 2)
            {
                //MessageBox.Show($"当前系统内核版本为{result.Result}，符合安装要求！");
                return true;
            }
            else if (int.Parse(linuxKernelCompared[0]) < 2)
            {
                //MessageBox.Show($"当前系统内核版本为{result.Result}，V2ray要求内核为2.6.23及以上。请升级内核再安装！");
                return false;
            }
            else if (int.Parse(linuxKernelCompared[0]) == 2)
            {
                if (int.Parse(linuxKernelCompared[1]) > 6)
                {
                    //MessageBox.Show($"当前系统内核版本为{result.Result}，符合安装要求！");
                    return true;
                }
                else if (int.Parse(linuxKernelCompared[1]) < 6)
                {
                    //MessageBox.Show($"当前系统内核版本为{result.Result}，V2ray要求内核为2.6.23及以上。请升级内核再安装！");
                    return false;
                }
                else if (int.Parse(linuxKernelCompared[1]) == 6)
                {
                    if (int.Parse(linuxKernelCompared[2]) < 23)
                    {
                        //MessageBox.Show($"当前系统内核版本为{result.Result}，V2ray要求内核为2.6.23及以上。请升级内核再安装！");
                        return false;
                    }
                    else
                    {
                        //MessageBox.Show($"当前系统内核版本为{result.Result}，符合安装要求！");
                        return true;
                    }

                }
            }
            return false;

        }

        //打开模板设置窗口
        private void ButtonTemplateConfiguration_Click(object sender, RoutedEventArgs e)
        {
            //清空初始化模板参数
            for (int i = 0; i != ReceiveConfigurationParameters.Length; i++)

            {
                ReceiveConfigurationParameters[i] = "";
            }
            WindowTemplateConfiguration windowTemplateConfiguration = new WindowTemplateConfiguration();
            windowTemplateConfiguration.ShowDialog();
        }
        //打开系统工具中的校对时间窗口
        private void ButtonProofreadTime_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                MessageBox.Show("远程主机连接信息有误，请检查");
                return;
            }

            ProofreadTimeWindow proofreadTimeWindow = new ProofreadTimeWindow();
            ProofreadTimeWindow.ProfreadTimeReceiveConnectionInfo = connectionInfo;

            proofreadTimeWindow.ShowDialog();

        }
        //释放80/443端口
        private void ButtonClearOccupiedPorts_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("将强制停止占用80/443端口的程序?", "Stop application", MessageBoxButton.YesNo);
            if (dialogResult== MessageBoxResult.No)
            {
                return;
            }
            ConnectionInfo testconnect = GenerateConnectionInfo();
            try
            {
                using (var client = new SshClient(testconnect))
                {
                    client.Connect();
                    string cmdTestPort;
                    string cmdResult;
                    cmdTestPort = @"lsof -n -P -i :443 | grep LISTEN";
                    cmdResult = client.RunCommand(cmdTestPort).Result;
                    //MessageBox.Show(cmdTestPort);
                    if (String.IsNullOrEmpty(cmdResult) ==false)
                    {
                        //MessageBox.Show(cmdResult);
                        string[] cmdResultArry443 = cmdResult.Split(' ');
                        //MessageBox.Show(cmdResultArry443[3]);
                        client.RunCommand($"systemctl stop {cmdResultArry443[0]}");
                        client.RunCommand($"systemctl disable {cmdResultArry443[0]}");
                        client.RunCommand($"kill -9 {cmdResultArry443[3]}");
                    }
 
                    cmdTestPort = @"lsof -n -P -i :80 | grep LISTEN";
                    cmdResult = client.RunCommand(cmdTestPort).Result;
                    if (String.IsNullOrEmpty(cmdResult) == false)
                    {
                        string[] cmdResultArry80 = cmdResult.Split(' ');
                        client.RunCommand($"systemctl stop {cmdResultArry80[0]}");
                        client.RunCommand($"systemctl disable {cmdResultArry80[0]}");
                        client.RunCommand($"kill -9 {cmdResultArry80[3]}");
                    }
                    MessageBox.Show("执行完毕！");
                    client.Disconnect();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ButtonGuideConfiguration_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("尚未完善，敬请期待");
        }

        private void ButtonAdvancedConfiguration_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("尚未完善，敬请期待");
        }

        private void ButtonWebBrowserBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserResourcesAndTools.GoBack();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonWebBrowserForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserResourcesAndTools.GoForward();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonWebBrowserHomePage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserResourcesAndTools.Source=new Uri("https://github.com/proxysu/windows/wiki/ResourcesAndTools");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    string[] testString = new string[6];
        //    for (int i = 0; i != testString.Length; i++)

        //    {

        //        testString[i] = i.ToString();

        //    }
        //    foreach (string str in testString)
        //    {
        //        MessageBox.Show(str);
        //    }
        //}

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i != ReceiveConfigurationParameters.Length; i++)

            {
                MessageBox.Show(ReceiveConfigurationParameters[i]);
            }
        }

        //private void TestresultClientInform_Click(object sender, RoutedEventArgs e)
        //{
        //    ResultClientInformation resultClientInformation = new ResultClientInformation();
        //    resultClientInformation.ShowDialog();
        //}

        //private void TestPortOccupy_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBoxResult dialogResult = MessageBox.Show("将强制停止占用80/443端口的程序?", "Stop application", MessageBoxButton.YesNo);
        //    if (dialogResult == MessageBoxResult.No)
        //    {
        //        return;
        //    }
        //    ConnectionInfo testconnect = GenerateConnectionInfo();
        //    using (var client = new SshClient(testconnect))
        //    {
        //        client.Connect();
        //        string cmdTestPort = @"lsof -n -P -i :443 | grep LISTEN";
        //        MessageBox.Show(cmdTestPort);
        //        string cmdResult = client.RunCommand(cmdTestPort).Result;
        //        client.Disconnect();
        //        MessageBox.Show(cmdResult);
        //        string[] cmdResultArry = cmdResult.Split(' ');
        //        //foreach(string arry in cmdResultArry)
        //        //{
        //        //    MessageBox.Show(arry);
        //        //}
        //        MessageBox.Show(cmdResultArry[0]);//程序名字
        //        MessageBox.Show(cmdResultArry[3]);//程序PID
        //    }
        //}

        //private void TestInstalledV2ray_Click(object sender, RoutedEventArgs e)
        //{
        //    ConnectionInfo testconnect = GenerateConnectionInfo();
        //    using (var client = new SshClient(testconnect))
        //    {
        //        client.Connect();
        //        //string cmdTestPort = @"find / -name v2ray";
        //        //MessageBox.Show(cmdTestPort);
        //        //string cmdResult = client.RunCommand(cmdTestPort).Result;
        //        //设置Caddyfile文件中的tls 邮箱
        //        string upLoadPath = "/etc/caddy/Caddyfile.test";
        //        string emailAddress = ReceiveConfigurationParameters[4];
        //        string sshCmdEmail = $"email={emailAddress};email=${{email/./@}};echo $email";//结尾有回车符
        //        string email = client.RunCommand(sshCmdEmail).Result.Replace("\n","");
        //        MessageBox.Show(email);
        //        string sshCmd = $"sed -i 's/off/{email}/' {upLoadPath}";

        //        MessageBox.Show(sshCmd);
        //        client.RunCommand(sshCmd);
        //        sshCmd = $"sed -i 's/##path##/\\{ReceiveConfigurationParameters[3]}/' {upLoadPath}";
        //        MessageBox.Show(sshCmd);
        //        client.RunCommand(sshCmd);
        //        //sshCmd = "sed -i 's/##path##/\\" + ReceiveConfigurationParameters[3] + "/' " + upLoadPath;
        //        //MessageBox.Show(sshCmd);
        //        //client.RunCommand("sed -i 's/##path##/\\" + ReceiveConfigurationParameters[3] + "/' " + upLoadPath);
        //        sshCmd = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}/' {upLoadPath}";
        //        MessageBox.Show(sshCmd);
        //        client.RunCommand(sshCmd);
        //        //client.RunCommand("sed -i 's/##domain##/" + ReceiveConfigurationParameters[4] + "/' " + upLoadPath);
        //        if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7]) == false)
        //        {
        //            sshCmd = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
        //            //client.RunCommand("sed -i 's/##sites##/proxy \\/ " + ReceiveConfigurationParameters[7] + "/' " + upLoadPath);
        //            MessageBox.Show(sshCmd);
        //            client.RunCommand(sshCmd);
        //        }
        //        Thread.Sleep(2000);

        //        //生成安装服务命令中的邮箱
        //        //string sshCmdEmail = $"email={emailAddress};email=${{email/./@}};echo $email";
        //        //string email = client.RunCommand(sshCmdEmail).Result.ToString();

        //        //MessageBox.Show(email);

        //        //安装Caddy服务
        //        //sshCmd = "caddy -service install -agree -conf /etc/caddy/Caddyfile -email " + email;
        //        sshCmd = $"caddy -service install -agree -conf /etc/caddy/Caddyfile -email {email}";


        //        client.Disconnect();
        //        //MessageBox.Show(cmdResult);
        //        //if (cmdResult.Contains("/usr/bin/v2ray")==true)
        //        //{
        //        //    MessageBox.Show("已安装");
        //        //}
        //        //else
        //        //{
        //        //    MessageBox.Show("未安装");
        //        //}
        //        //string[] cmdResultArry = cmdResult.Split('\n');
        //        //foreach(string arry in cmdResultArry)
        //        //{
        //        //    MessageBox.Show(arry);
        //        //}
        //        //MessageBox.Show(cmdResultArry[0]);//程序名字
        //        //MessageBox.Show(cmdResultArry[3]);//程序PID
        //    }
        //}

        //private void TestsshCmd_Click(object sender, RoutedEventArgs e)
        //{
        //    ReceiveConfigurationParameters[3] = "https://tes.te.tt";
        //    ReceiveConfigurationParameters[7] = "http://77.77.77";
        //    string upLoadPath = "/etc/caddy/Caddyfile";
        //    string sshCmd = $"sed -i 's/##path##/\\{ReceiveConfigurationParameters[3]}/' {upLoadPath}";
        //    //MessageBox.Show(sshCmd);
        //    //sshCmd = "sed -i 's/##path##/\\" + ReceiveConfigurationParameters[3] + "/' " + upLoadPath;
        //    //MessageBox.Show(sshCmd);
        //    //sshCmd = $"sed -i 's/##path##/\\{ReceiveConfigurationParameters[3]}/' {upLoadPath}";
        //    //MessageBox.Show(sshCmd);
        //    //sshCmd = "sed -i 's/##path##/\\" + ReceiveConfigurationParameters[3] + "/' " + upLoadPath;
        //    //MessageBox.Show(sshCmd);
        //    //client.RunCommand("sed -i 's/##path##/\\" + ReceiveConfigurationParameters[3] + "/' " + upLoadPath);
        //    sshCmd = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}/' {upLoadPath}";
        //    MessageBox.Show(sshCmd);
        //    string testDomain = ReceiveConfigurationParameters[7].Substring(0,7);
        //    if (String.Equals(testDomain,"https:/")||String.Equals(testDomain,"http://"))
        //    {
        //        MessageBox.Show(testDomain);
        //        ReceiveConfigurationParameters[7]=ReceiveConfigurationParameters[7].Replace("/","\\/");
        //    }
        //    else
        //    {
        //        ReceiveConfigurationParameters[7] = "http:\\/\\/" + ReceiveConfigurationParameters[7];
        //    }
        //    sshCmd = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
        //    MessageBox.Show(sshCmd);
        //}



        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    ConnectionInfo testconnect = GenerateConnectionInfo();
        //    using (var client = new SshClient(testconnect))
        //    {
        //        client.Connect();
        //        bool getGetenforce = String.IsNullOrEmpty(client.RunCommand("command -v getenforce").Result);

        //        if (getGetenforce == false)
        //        {
        //            string testSELinux = client.RunCommand("getenforce").Result;
        //            MessageBox.Show(testSELinux);
        //            if (testSELinux.Contains("Enforcing") == true)
        //            {
        //                MessageBox.Show("Enforcing");
        //                client.RunCommand("setenforce  0");//不重启改为Permissive模式
        //                client.RunCommand("sed -i 's/SELINUX=enforcing/SELINUX=permissive/' /etc/selinux/config");//重启也工作在Permissive模式下
        //            }
        //            else
        //            {
        //                MessageBox.Show("非Enforcing");
        //            }
        //        }
        //        client.Disconnect();
        //    }
        //}
        //private void ButtonWebBrowserProxyGo_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        string urlStartchar = TextBoxWebBrowserProxyUrl.Text.Substring(0, 4);
        //        //MessageBox.Show(urlStartchar);
        //        if (String.Equals(urlStartchar,"http")==true)
        //        {
        //            WebBrowserResourcesAndTools.Source = new Uri(TextBoxWebBrowserProxyUrl.Text);
        //        }
        //        else
        //        {
        //            WebBrowserResourcesAndTools.Source = new Uri("http://" + TextBoxWebBrowserProxyUrl.Text);//如果没有前置代理，此处应为："http://" + TextBoxWebBrowserProxyUrl.Text
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        // }


    }
    
}
