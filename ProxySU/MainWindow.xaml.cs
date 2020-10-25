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
using System.ComponentModel;
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
        //多语言参数
        public class LanguageInfo
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public static string[] ReceiveConfigurationParameters { get; set; }
        //ReceiveConfigurationParameters[0]----模板类型
        //ReceiveConfigurationParameters[1]----服务端口
        //ReceiveConfigurationParameters[2]----V2Ray uuid/(naive/Trojan-go/Trojan/SSR/SS)' Password
        //ReceiveConfigurationParameters[3]----QUIC加密方式/SSR 加密方法/SS 加密方式/naive'user/VLESS ws Path
        //ReceiveConfigurationParameters[4]----Domain
        //ReceiveConfigurationParameters[5]----伪装类型/插件名称
        //ReceiveConfigurationParameters[6]----V2Ray&Trojan-go&SS--Websocket'Path/http2'Path/QUIC密钥/mKCP Seed/VMESS ws Path
        //ReceiveConfigurationParameters[7]----伪装网站
        //ReceiveConfigurationParameters[8]----方案名称
        //ReceiveConfigurationParameters[9]----插件参数选项/VMESS tcp Path/MTProto Parameters
        //public static ConnectionInfo ConnectionInfo;
        public static string proxyType = "V2Ray";                   //代理类型标识: V2Ray\TrojanGo\Trojan\NaiveProxy
        public static readonly string pwdir = AppDomain.CurrentDomain.BaseDirectory; //执行文件所在目录
        static bool testDomain = false;                             //设置标识--域名是否需要检测解析，初始化为不需要
        static string ipv4 = String.Empty;                          //保存获取的ipv4地址
        static string ipv6 = String.Empty;                          //保存获取的ipv6地址
        static bool onlyIpv6 = false;                               //主机是否基于纯ipv6地址
        //static string scriptGithubUrl = "raw.githubusercontent.com";//安装脚本下载地址
        //static string apiGithubCom = "api.github.com";              //github api接口
        //static string githubCom = "github.com";                     //github 主站网址
        static bool functionResult = true;                          //标示功能函数是否执行状态(true无错误发生/false有错误发生)
        static string sshShellCommand = String.Empty;               //定义保存执行的命令
        static string currentStatus = String.Empty;                 //定议保存要显示的状态
        static string currentShellCommandResult = String.Empty;     //定义Shell命令执行结果保存变量
        static bool getApt = false;                                 //判断系统软件管理，下同
        static bool getDnf = false;
        static bool getYum = false;
        static string sshCmdUpdate = String.Empty;                  //保存软件安装所用更新软件库命令
        static string sshCmdInstall = String.Empty;                 //保存软件安装所用命令格式
        static int randomCaddyListenPort = 8800;                    //Caddy做伪装网站所监听的端口，随机10001-60000
        static int installationDegree = 0;                          //安装进度条显示的百分比

        //******  ******
        //  Application.Current.FindResource("").ToString()
        // <!--  -->
        // {DynamicResource }
        //SetUpProgressBarProcessing(0);
        public MainWindow()
        {
            InitializeComponent();

            #region 多语言选择设置 初始设置为auto
            List<LanguageInfo> languageList = new List<LanguageInfo>();

            languageList.Add(new LanguageInfo { Name = "auto", Value = "auto" });
            languageList.Add(new LanguageInfo { Name = "English", Value = "en-US" });
            languageList.Add(new LanguageInfo { Name = "简体中文", Value = "zh-CN" });
            languageList.Add(new LanguageInfo { Name = "正體中文", Value = "zh-TW" });

            ComboBoxLanguage.ItemsSource = languageList;

            ComboBoxLanguage.DisplayMemberPath = "Name";//显示出来的值
            ComboBoxLanguage.SelectedValuePath = "Value";//实际选中后获取的结果的值
            ComboBoxLanguage.SelectedIndex = 0;

            DataContext = this;
            string Culture = System.Globalization.CultureInfo.InstalledUICulture.Name;
            //Culture = "en-US";
            ResourcesLoad(Culture);
            #endregion

            //初始化参数设置数组
            ReceiveConfigurationParameters = new string[10];

            //初始化选定密码登录
            RadioButtonPasswordLogin.IsChecked = true;
            //初始化选定无代理
            RadioButtonNoProxy.IsChecked = true;
            //初始化代理无需登录
            RadioButtonProxyNoLogin.IsChecked = true;
            //初始化隐藏Socks4代理，
            RadioButtonSocks4.Visibility = Visibility.Collapsed;

            //初始化Trojan的密码
            TextBoxTrojanPassword.Text = RandomUUID();

            //初始化NaiveProxy的用户名和密码
            TextBoxNaivePassword.Text = RandomUUID();
            TextBoxNaiveUser.Text = RandomUserName();

            //初始化SSR的密码
            TextBoxSSRPassword.Text = RandomUUID();

            //初始化所选方案面板为不显示
            GridV2rayCurrentlyPlan.Visibility = Visibility.Hidden;

            //初始化三合一的所有内容
            //TextBoxV2rayUUID3in1.Text = RandomUUID();
            //TextBoxV2rayPath3in1.Text = "/ray";

            //TextBoxTrojanPassword3in1.Text= RandomUUID();

            //TextBoxNaiveUser3in1.Text = RandomUserName();
            //TextBoxNaivePassword3in1.Text= RandomUUID();

            //自动检查ProxySU是否有新版本发布，有则显示更新提示
            Thread thread = new Thread(() => TestLatestVersionProxySU(TextBlockLastVersionProxySU, TextBlockNewVersionReminder, ButtonUpgradeProxySU));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        #region 检测新版本代码

        //检测ProxySU新版本
        private void TestLatestVersionProxySU(TextBlock TextBlockLastVersionProxySU,TextBlock TextBlockNewVersionReminder,Button ButtonUpgradeProxySU)
        {
            string strJson = GetLatestJson(@"https://api.github.com/repos/proxysu/windows/releases/latest");
            if (String.IsNullOrEmpty(strJson) == false)
            {
                JObject lastVerJsonObj = JObject.Parse(strJson);
                string lastVersion = (string)lastVerJsonObj["tag_name"];//得到远程版本信息

                string lastVersionNoV = lastVersion.Replace("v", String.Empty);

                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                //MessageBox.Show(version.ToString());
                string cerversion = version.ToString().Substring(0, 6); //获取本地版本信息
                //MessageBox.Show(cerversion);
                string[] lastVerComp = lastVersionNoV.Split('.');
                string[] localVerComp = cerversion.Split('.');

                //版本信息不相同，则认为新版本发布，显示出新版本信息及更新提醒，下载按钮
                if (int.Parse(lastVerComp[0]) > int.Parse(localVerComp[0]))
                {
                    TextBlockLastVersionProxySU.Dispatcher.BeginInvoke(updateNewVersionProxySUAction, TextBlockLastVersionProxySU, TextBlockNewVersionReminder, ButtonUpgradeProxySU, lastVersion);
                    return;
                }
                else if (int.Parse(lastVerComp[0]) == int.Parse(localVerComp[0]))
                {
                    if (int.Parse(lastVerComp[1]) > int.Parse(localVerComp[1]))
                    {
                        TextBlockLastVersionProxySU.Dispatcher.BeginInvoke(updateNewVersionProxySUAction, TextBlockLastVersionProxySU, TextBlockNewVersionReminder, ButtonUpgradeProxySU, lastVersion);
                        return;
                    }
                    else if (int.Parse(lastVerComp[1]) == int.Parse(localVerComp[1]))
                    {
                        if (int.Parse(lastVerComp[2]) > int.Parse(localVerComp[2]))
                        {
                            TextBlockLastVersionProxySU.Dispatcher.BeginInvoke(updateNewVersionProxySUAction, TextBlockLastVersionProxySU, TextBlockNewVersionReminder, ButtonUpgradeProxySU, lastVersion);
                            return;
                        }
                    }
                }
            }
        }

        //下载最新版ProxySU
        private void ButtonUpgradeProxySU_Click(object sender, RoutedEventArgs e)
        {
            TextBlockNewVersionReminder.Visibility = Visibility.Hidden;
            TextBlockNewVersionDown.Visibility = Visibility.Visible;
            //TextBlockNewVersionReminder.Text = Application.Current.FindResource("TextBlockNewVersionDown").ToString();
            try
            {
                string strJson = GetLatestJson(@"https://api.github.com/repos/proxysu/windows/releases/latest");
                if (String.IsNullOrEmpty(strJson) == false)
                {
                    JObject lastVerJsonObj = JObject.Parse(strJson);
                    string lastVersion = (string)lastVerJsonObj["tag_name"];//得到远程版本信息
                    string latestVerDownUrl = (string)lastVerJsonObj["assets"][0]["browser_download_url"];//得到最新版文件下载地址
                    string latestNewVerFileName = (string)lastVerJsonObj["assets"][0]["name"];//得到最新版文件名
                    string latestNewVerExplanation = (string)lastVerJsonObj["body"];//得到更新说明

                    WebClient webClientNewVer = new WebClient();
                    webClientNewVer.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    //webClientNewVer.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    if (CheckDir(lastVersion))
                    {
                        Uri latestVerDownUri = new Uri(latestVerDownUrl);
                        webClientNewVer.DownloadFileAsync(latestVerDownUri, lastVersion + @"\" + latestNewVerFileName);
                        //webClientNewVer.DownloadFile(latestVerDownUrl, latestNewVerFileName);

                        using (StreamWriter sw = new StreamWriter(lastVersion + @"\" + "readme.txt"))
                        {
                            sw.WriteLine(latestNewVerExplanation);
                        }
                    }
                }
            }
            catch (Exception ex1)
            {
               // MessageBox.Show(ex1.ToString());

                return;
            }
        }
        //文件下载完处理方法
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                MessageBox.Show(e.Error.ToString());
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorDownProxyFail").ToString());
                //label1.Text = "Download cancelled!";
            }
            else
            {
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorDownProxySuccess").ToString());
            }
        }

        //获取LatestJson
        private string GetLatestJson(string theLatestUrl)
        {
            try
            {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | (SecurityProtocolType)3072 | (SecurityProtocolType)768 | SecurityProtocolType.Tls;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                // ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00 | 0x30);
                //string url = "https://api.github.com/repos/proxysu/windows/releases/latest";
                Uri uri = new Uri(theLatestUrl);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                req.Accept = @"application/json";
                req.UserAgent = @"Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:74.0) Gecko/20100101 Firefox/74.0";
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                string strJson = sr.ReadToEnd();
                return strJson;
            }
            catch (Exception ex1)
            {
                return null;
            }
        }

       
        //更新新版本提醒显示
        Action<TextBlock, TextBlock, Button, string> updateNewVersionProxySUAction = new Action<TextBlock, TextBlock, Button, string>(UpdateNewVersionProxySU);
        private static void UpdateNewVersionProxySU(TextBlock TextBlockLastVersionProxySU, TextBlock TextBlockNewVersionReminder, Button ButtonUpgradeProxySU, string theLatestVersion)
        {
            TextBlockLastVersionProxySU.Text = theLatestVersion;
            TextBlockLastVersionProxySU.Visibility = Visibility.Visible;
            TextBlockNewVersionReminder.Visibility = Visibility.Visible;
            ButtonUpgradeProxySU.Visibility = Visibility.Visible;
        }

        #endregion

        #region 安装过程信息显示 端口数字防错代码，密钥选择代码 

        //更新状态条显示
        Action<TextBlock, ProgressBar, string> updateAction = new Action<TextBlock, ProgressBar, string>(UpdateTextBlock);
        private static void UpdateTextBlock(TextBlock textBlockName, ProgressBar progressBar, string currentStatus)
        {
            textBlockName.Text = currentStatus;

            //if (currentStatus.Contains("成功") == true || currentStatus.ToLower().Contains("success") == true)
            //{
            //    progressBar.IsIndeterminate = false;
            //    progressBar.Value = 100;
            //}
            //else 
            if (currentStatus.Contains("失败") == true || currentStatus.Contains("取消") == true || currentStatus.Contains("退出") == true || currentStatus.ToLower().Contains("fail") == true || currentStatus.ToLower().Contains("cancel") == true || currentStatus.ToLower().Contains("exit") == true)
            {
                progressBar.IsIndeterminate = false;
                progressBar.Value = 0;
            }
            //else
            //{
            //    progressBar.IsIndeterminate = true;
            //    //progressBar.Value = 0;
            //}


        }
        
        //进度条更新百分比
        private void SetUpProgressBarProcessing(int max)
        {
           
            for (int i = installationDegree; i <= max; i++)
            {
                Thread.Sleep(100);
                Dispatcher.Invoke(new Action<System.Windows.DependencyProperty, object>(ProgressBarSetUpProcessing.SetValue), System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, Convert.ToDouble(i) });
            }
            installationDegree = max;
        }

       
        //更新监视窗内的显示
        //结尾自动添加一个换行符
        Action<TextBox, string> updateMonitorAction = new Action<TextBox, string>(UpdateTextBox);
        private static void UpdateTextBox(TextBox textBoxName, string currentResult)
        {
            textBoxName.Text = textBoxName.Text + currentResult + Environment.NewLine;
            textBoxName.ScrollToEnd();
        }
        
        //结尾不添加换行符
        Action<TextBox, string> updateMonitorActionNoWrap = new Action<TextBox, string>(UpdateTextBoxNoWrap);
        private static void UpdateTextBoxNoWrap(TextBox textBoxName, string currentResult)
        {
            textBoxName.Text = textBoxName.Text + currentResult;
            textBoxName.ScrollToEnd();
        }

        //退出主程序
        private void Button_canel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        // private static readonly Regex _regex = new Regex("[^0-9]+");

        //检测数字输入
        private void TextBoxPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //检测字符串是否只由数字组成
        private bool IsOnlyNumber(string value)
        {
            value = value.Trim();
            Regex r = new Regex(@"^[0-9]+$");
            return r.Match(value).Success;
        }

        //端口输入框是否可使用粘贴命令
        private void TextBoxPort_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = false;//true为不能使用粘贴命令，false为可以使用粘贴命令
            }
        }

        //打开证书浏览
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

        #region 主界面控件的有效无效控制代码块及界面语言
   
        //加载语言资源文件
        private void ResourcesLoad(string Culture)
        {
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            string requestedCulture = string.Format(@"Translations\ProxySU.{0}.xaml", Culture);
            //string requestedCulture = string.Format(@"Translations\ProxySU.{0}.xaml", "default");
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            if (resourceDictionary == null)
            {
                requestedCulture = @"Translations\ProxySU.en-US.xaml";
                resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            }
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
        }

        //界面语言处理
        private void ComboBoxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string languageCulture;
            object languageSelected;
            languageSelected = ComboBoxLanguage.SelectedValue;
            languageCulture = languageSelected.ToString();
            if (languageCulture.Equals("auto"))
            {
                languageCulture = System.Globalization.CultureInfo.InstalledUICulture.Name;
                ResourcesLoad(languageCulture);
            }
            else
            {
                ResourcesLoad(languageCulture);
            }
            //display.Text = language;
        }
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
            TextBlockCert.Visibility = Visibility.Collapsed;
            TextBlockPassword.Visibility = Visibility.Visible;
            PasswordBoxHostPassword.IsEnabled = true;
            PasswordBoxHostPassword.Visibility = Visibility.Visible;
        }

        private void RadioButtonCertLogin_Checked(object sender, RoutedEventArgs e)
        {
            //TextBlockPassword.Text = "密钥：";
            TextBlockCert.Visibility = Visibility.Visible;
            TextBlockPassword.Visibility = Visibility.Collapsed;
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

        //远程主机连接信息
        private ConnectionInfo GenerateConnectionInfo()
        {
            ConnectionInfo connectionInfo;

            #region 检测输入的内容是否有错，并读取内容
            if (string.IsNullOrEmpty(TextBoxHost.Text) == true || string.IsNullOrEmpty(TextBoxPort.Text) == true || string.IsNullOrEmpty(TextBoxUserName.Text) == true)
            {
                //******"主机地址、主机端口、用户名为必填项，不能为空"******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostPortUserNotEmpty").ToString());

                return connectionInfo = null;
            }
            string sshHostName = TextBoxHost.Text.ToString();
            int sshPort = 22;

            if (IsOnlyNumber(TextBoxPort.Text) == true)
            {
                TextBoxPort.Text = TextBoxPort.Text.Trim();
                sshPort = int.Parse(TextBoxPort.Text);
            }
            else
            {
                //******"连接端口含有非数字字符！"******
                MessageBox.Show("Host Port" + Application.Current.FindResource("MessageBoxShow_ErrorHostPortErr").ToString());
                return connectionInfo = null;
            }

            string sshUser = TextBoxUserName.Text.ToString();

            if (RadioButtonPasswordLogin.IsChecked == true && string.IsNullOrEmpty(PasswordBoxHostPassword.Password) == true)
            {
                //****** "登录密码为必填项，不能为空!!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostPasswordNotEmpty").ToString());
                return connectionInfo = null;
            }
            string sshPassword = PasswordBoxHostPassword.Password.ToString();
            if (RadioButtonCertLogin.IsChecked == true && string.IsNullOrEmpty(TextBoxCertFilePath.Text) == true)
            {
                //****** "密钥文件为必填项，不能为空!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostKeyNotEmpty").ToString());
                return connectionInfo = null;
            }
            string sshPrivateKey = TextBoxCertFilePath.Text.ToString();
            ProxyTypes proxyTypes = new ProxyTypes();//默认为None

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
                //****** "如果选择了代理，则代理地址与端口不能为空!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorProxyAddressPortNotEmpty").ToString());
                return connectionInfo = null;
            }
            string sshProxyHost = TextBoxProxyHost.Text.ToString();

            int sshProxyPort = 1080;
            if (IsOnlyNumber(TextBoxProxyPort.Text) == true)
            {
                TextBoxProxyPort.Text = TextBoxProxyPort.Text.Trim();
                sshProxyPort = int.Parse(TextBoxProxyPort.Text);
            }
            else
            {
                //******"连接端口含有非数字字符！"******
                MessageBox.Show("Proxy Port" + Application.Current.FindResource("MessageBoxShow_ErrorHostPortErr").ToString());
                return connectionInfo = null;
            }

            if (RadioButtonNoProxy.IsChecked==false && RadiobuttonProxyYesLogin.IsChecked == true && (string.IsNullOrEmpty(TextBoxProxyUserName.Text) == true || string.IsNullOrEmpty(PasswordBoxProxyPassword.Password) == true))
            {
                //****** "如果代理需要登录，则代理登录的用户名与密码不能为空!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorProxyUserPasswordNotEmpty").ToString());
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

        //登录主机过程中出现的异常处理
        private void ProcessException(string exceptionMessage)
        {
            //下面代码需要保留，以备将来启用
            //if (exceptionMessage.Contains("连接尝试失败") == true)
            //{
            //    //****** "请检查主机地址及端口是否正确，如果通过代理，请检查代理是否正常工作!" ******
            //    MessageBox.Show($"{exceptionMessage}\n" +
            //        Application.Current.FindResource("MessageBoxShow_ErrorLoginHostOrPort").ToString());
            //}

            //else if (exceptionMessage.Contains("denied (password)") == true)
            //{
            //    //****** "密码错误或用户名错误" ******
            //    MessageBox.Show($"{exceptionMessage}\n" +
            //        Application.Current.FindResource("MessageBoxShow_ErrorLoginUserOrPassword").ToString());
            //}
            //else if (exceptionMessage.Contains("Invalid private key file") == true)
            //{
            //    //****** "所选密钥文件错误或者格式不对!" ******
            //    MessageBox.Show($"{exceptionMessage}\n" +
            //        Application.Current.FindResource("MessageBoxShow_ErrorLoginKey").ToString());
            //}
            //else if (exceptionMessage.Contains("denied (publickey)") == true)
            //{
            //    //****** "使用密钥登录，密钥文件错误或用户名错误" ******
            //    MessageBox.Show($"{exceptionMessage}\n" +
            //        Application.Current.FindResource("MessageBoxShow_ErrorLoginKeyOrUser").ToString());
            //}
            //else if (exceptionMessage.Contains("目标计算机积极拒绝") == true)
            //{
            //    //****** "主机地址错误，如果使用了代理，也可能是连接代理的端口错误" ******
            //    MessageBox.Show($"{exceptionMessage}\n" +
            //        Application.Current.FindResource("MessageBoxShow_ErrorLoginHostOrProxyPort").ToString());
            //}
            //else
            //{
                //****** "发生错误" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorLoginOccurred").ToString());
                MessageBox.Show(exceptionMessage);
            //}

        }

        #region V2Ray相关

        //打开v2ray模板设置窗口
        private void ButtonTemplateConfiguration_Click(object sender, RoutedEventArgs e)
        {
            //清空初始化模板参数
            for (int i = 0; i != ReceiveConfigurationParameters.Length; i++)

            {
                ReceiveConfigurationParameters[i] = "";
            }
            WindowTemplateConfiguration windowTemplateConfiguration = new WindowTemplateConfiguration();
            windowTemplateConfiguration.Closed += windowTemplateConfigurationClosed;
            windowTemplateConfiguration.ShowDialog();
        }
       
        //V2Ray模板设置窗口关闭后，触发事件，将所选的方案与其参数显示在UI上
        private void windowTemplateConfigurationClosed(object sender, System.EventArgs e)
        {
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[0]) == true)
            {
                //显示"未选择方案！"
                TextBlockCurrentlySelectedPlan.Text = Application.Current.FindResource("TextBlockCurrentlySelectedPlanNo").ToString();

                GridV2rayCurrentlyPlan.Visibility = Visibility.Hidden;

                return;
            }
            else
            {
                GridV2rayCurrentlyPlan.Visibility = Visibility.Visible;
            }
            TextBlockCurrentlySelectedPlan.Text = ReceiveConfigurationParameters[8];            //所选方案名称
            TextBlockCurrentlySelectedPlanPort.Text = ReceiveConfigurationParameters[1];        //服务器端口
            TextBlockCurrentlySelectedPlanUUID.Text = ReceiveConfigurationParameters[2];        //UUID
            TextBlockCurrentlySelectedPlanPathSeedKey.Text = ReceiveConfigurationParameters[6]; //mKCP Seed\Quic Key\Path
            
            TextBlockCurrentlySelectedPlanFakeWebsite.Text = ReceiveConfigurationParameters[7]; //伪装网站

            if (String.Equals(ReceiveConfigurationParameters[0],"TCP") == true
                || String.Equals(ReceiveConfigurationParameters[0], "TCPhttp") == true
                || String.Equals(ReceiveConfigurationParameters[0], "tcpTLSselfSigned") == true
                || String.Equals(ReceiveConfigurationParameters[0], "webSocket") == true)
            {
                //隐藏Path/mKCP Seed/Quic Key
                HideV2RayPathSeedKey();
                HideVlessVmessXtlsTcpWs();

                //隐藏域名/Quic加密方式
                HideV2RayDomainQuicEncrypt();

                //隐藏伪装网站
                HideV2RayMaskSites();

            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "tcpTLS") == true)
            {
                //隐藏Path/mKCP Seed/Quic Key
                HideV2RayPathSeedKey();
                HideVlessVmessXtlsTcpWs();

                //显示域名
                ShowV2RayDomainQuicEncrypt();

                //隐藏伪装网站
                HideV2RayMaskSites();
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == true)
            {
                //显示复合方案路径
                ShowVlessVmessXtlsTcpWs();

                //显示域名
                ShowV2RayDomainQuicEncrypt();
              
                //显示伪装网站(暂时不显示)
                ShowV2RayMaskSites();

            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true
              || String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true)
            {
                //隐藏Path/mKCP Seed/Quic Key
                HideV2RayPathSeedKey();
                HideVlessVmessXtlsTcpWs();

                //显示域名
                ShowV2RayDomainQuicEncrypt();
               
                //显示伪装网站(暂时不显示)
                ShowV2RayMaskSites();
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS") == true
                || String.Equals(ReceiveConfigurationParameters[0], "Http2") == true)
            {
                //显示Path
                ShowV2RayPathSeedKey();
                TextBlockV2RayShowPathSeedKey.Text = "Path:";
                TextBlockCurrentlySelectedPlanPathSeedKey.Text = ReceiveConfigurationParameters[6]; //mKCP Seed\Quic Key\Path


                //显示域名
                ShowV2RayDomainQuicEncrypt();
            
                //显示伪装网站(暂时不显示)
                HideV2RayMaskSites();
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true
                || String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true
                || String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true
                || String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
            {
                //显示Path
                ShowV2RayPathSeedKey();
                TextBlockV2RayShowPathSeedKey.Text = "Path:";
                TextBlockCurrentlySelectedPlanPathSeedKey.Text = ReceiveConfigurationParameters[6]; //mKCP Seed\Quic Key\Path

                //显示域名
                ShowV2RayDomainQuicEncrypt();
             
                //显示伪装网站(暂时不显示)
                ShowV2RayMaskSites();
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true
                || String.Equals(ReceiveConfigurationParameters[0], "http2selfSigned") == true)
            { 
                //显示Path
                ShowV2RayPathSeedKey();
                TextBlockV2RayShowPathSeedKey.Text = "Path:";
                TextBlockCurrentlySelectedPlanPathSeedKey.Text = ReceiveConfigurationParameters[6]; //mKCP Seed\Quic Key\Path

                //隐藏域名/Quic加密方式
                HideV2RayDomainQuicEncrypt();

                //隐藏伪装网站
                HideV2RayMaskSites();
            }
            else if (ReceiveConfigurationParameters[0].Contains("mKCP") == true)
            {
                //显示mKCP Seed
                ShowV2RayPathSeedKey();
                TextBlockV2RayShowPathSeedKey.Text = "mKCP Seed:";
                TextBlockCurrentlySelectedPlanPathSeedKey.Text = ReceiveConfigurationParameters[6]; //mKCP Seed\Quic Key\Path

                //隐藏域名/Quic加密方式
                HideV2RayDomainQuicEncrypt();

                //隐藏伪装网站
                HideV2RayMaskSites();
            }
            else if (ReceiveConfigurationParameters[0].Contains("Quic") == true)
            {
                //显示QUIC Key
                ShowV2RayPathSeedKey();
                TextBlockV2RayShowPathSeedKey.Text = "QUIC Key:";
                TextBlockCurrentlySelectedPlanPathSeedKey.Text = ReceiveConfigurationParameters[6]; //mKCP Seed\Quic Key\Path

                //显示Quic加密方式
                ShowV2RayDomainQuicEncrypt();
                TextBlockV2RayShowCurrentlySelectedPlanDomain.Text = Application.Current.FindResource("TextBlockQuicEncryption").ToString();
                TextBlockCurrentlySelectedPlanDomain.Text = ReceiveConfigurationParameters[3];      //Quic加密方式
                if (String.Equals(TextBlockCurrentlySelectedPlanDomain.Text,"none")==true)
                {
                    HideV2RayPathSeedKey();
                }


                //隐藏伪装网站
                HideV2RayMaskSites();
            }
        }

        #region 当前方案界面控制
        //显示端口与UUID
        private void ShowV2RayCurrentPortUUID()
        {
            TextBlockV2RayShowPort.Visibility = Visibility.Visible;
            TextBlockCurrentlySelectedPlanPort.Visibility = Visibility.Visible;

            TextBlockV2RayShowUUID.Visibility = Visibility.Visible;
            TextBlockCurrentlySelectedPlanUUID.Visibility = Visibility.Visible;
        }

        //显示Path/mKCP Seed/Quic Key
        private void ShowV2RayPathSeedKey()
        {
            HideVlessVmessXtlsTcpWs();
            TextBlockV2RayShowPathSeedKey.Visibility = Visibility.Visible;
            TextBlockCurrentlySelectedPlanPathSeedKey.Visibility = Visibility.Visible;
        }

        //隐藏Path/mKCP Seed/Quic Key
        private void HideV2RayPathSeedKey()
        {
            TextBlockV2RayShowPathSeedKey.Visibility = Visibility.Hidden;
            TextBlockCurrentlySelectedPlanPathSeedKey.Visibility = Visibility.Hidden;
        }

        //显示VLESS VMESS XTLS TCP WS 复合方案
        private void ShowVlessVmessXtlsTcpWs()
        {
            HideV2RayPathSeedKey();
            GridVlessVmessXtlsTcpWs.Visibility = Visibility.Visible;
            TextBlockBoxPathVlessWS.Text = ReceiveConfigurationParameters[3];
            TextBlockBoxPathVmessTcp.Text = ReceiveConfigurationParameters[9];
            TextBlockBoxPathVmessWS.Text = ReceiveConfigurationParameters[6];
        }

        //隐藏VLESS VMESS XTLS TCP WS 复合方案
        private void HideVlessVmessXtlsTcpWs()
        {
            GridVlessVmessXtlsTcpWs.Visibility = Visibility.Collapsed;
        }

        //显示域名/Quic加密方式
        private void ShowV2RayDomainQuicEncrypt()
        {
            TextBlockV2RayShowCurrentlySelectedPlanDomain.Visibility = Visibility.Visible;
            TextBlockCurrentlySelectedPlanDomain.Visibility = Visibility.Visible;
            TextBlockV2RayShowCurrentlySelectedPlanDomain.Text = Application.Current.FindResource("TextBlockV2RayDomain").ToString();
            TextBlockCurrentlySelectedPlanDomain.Text = ReceiveConfigurationParameters[4];      //域名

        }

        //隐藏域名/Quic加密方式
        private void HideV2RayDomainQuicEncrypt()
        {
            TextBlockV2RayShowCurrentlySelectedPlanDomain.Visibility = Visibility.Hidden;
            TextBlockCurrentlySelectedPlanDomain.Visibility = Visibility.Hidden;
        }
        //显示伪装网站(暂时不显示)
        private void ShowV2RayMaskSites()
        {
            TextBlockV2RayShowCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
            TextBlockCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
        }

        //隐藏伪装网站
        private void HideV2RayMaskSites()
        {
            TextBlockV2RayShowCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
            TextBlockCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
        }
        #endregion

        //传送V2Ray模板参数,启动V2Ray安装进程
        private void Button_Login_Click(object sender, RoutedEventArgs e)

        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if(connectionInfo==null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());    
                return;
            }

            
            //生成客户端配置时，连接的服务主机的IP或者域名
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[4])==true)
            {
                ReceiveConfigurationParameters[4] = TextBoxHost.Text.ToString();
                testDomain = false;
            }
            //选择模板
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[0]) == true)
            {
                //******"请先选择配置模板！"******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ChooseTemplate").ToString());
                return;
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "TCP") == true
                || String.Equals(ReceiveConfigurationParameters[0], "TCPhttp") == true
                || String.Equals(ReceiveConfigurationParameters[0], "tcpTLSselfSigned") == true
                || String.Equals(ReceiveConfigurationParameters[0], "webSocket") == true
                || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true
                || String.Equals(ReceiveConfigurationParameters[0], "http2selfSigned") == true
                || ReceiveConfigurationParameters[0].Contains("mKCP") == true
                || ReceiveConfigurationParameters[0].Contains("Quic") == true)
            {
                testDomain = false;

            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "tcpTLS") == true
                || String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true
                || String.Equals(ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true
                || String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true
                || String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true
                || String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == true
                || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true
                || String.Equals(ReceiveConfigurationParameters[0], "Http2") == true
                || String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true)
            {
                testDomain = true;

            }


            //Thread thread
            //SetUpProgressBarProcessing(0); //重置安装进度
            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            //Thread thread = new Thread(() => StartSetUpV2ray(connectionInfo, TextBlockSetUpProcessing, ProgressBarSetUpProcessing));
            Thread thread = new Thread(() => StartSetUpV2ray(connectionInfo));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            
        }

        //登录远程主机布署V2ray程序
        private void StartSetUpV2ray(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;   
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);                                                                                                                                 //Thread.Sleep(1000);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测是否已安装代理 8--10
                    functionResult = SoftInstalledIsNoYes(client, "v2ray", @"/usr/local/bin/v2ray");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测系统是否符合安装要求 11--30
                    //检测关闭Selinux及系统组件是否齐全（apt/yum/dnf/systemctl）
                    //安装依赖软件，检测端口，防火墙开启端口
                    functionResult = ShutDownSelinuxAndSysComponentsDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测校对时间 31--33
                    functionResult = CheckProofreadingTime(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测域名解析是否正确 34---36
                    if (testDomain == true)
                    {
                        functionResult = DomainResolutionCurrentIPDetect(client);
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
                    }

                    //下载安装脚本安装 37--40
                    functionResult = V2RayInstallScript(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //程序是否安装成功检测 41--43
                    functionResult = SoftInstalledSuccessOrFail(client, "v2ray", @"/usr/local/bin/v2ray");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //生成服务端配置 44--46
                    functionResult = GenerateServerConfigurationV2Ray(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //****** "上传配置文件......" ****** 47--50
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadSoftConfig").ToString();
                    MainWindowsShowInfo(currentStatus);
                    string serverRemoteConfig = @"/usr/local/etc/v2ray/config.json";
                    UploadConfig(connectionInfo, @"config.json", serverRemoteConfig);
                    File.Delete(@"config.json");
                    SetUpProgressBarProcessing(50);

                    //如果使用http2/WebSocketTLS/tcpTLS/VlessTcpTlsWeb/VLESS+TCP+XTLS+Web模式，先要安装acme.sh,申请证书
                    if (String.Equals(ReceiveConfigurationParameters[0], "Http2") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "tcpTLS") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == true)
                    {
                        //安装acme.sh与申请证书 51--57
                        functionResult = AcmeShInstall(client);
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                        //安装证书到V2Ray 58--60
                        functionResult = CertInstallToV2ray(client);
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    }


                    #region Caddy安装 61--70

                    //如果是VLESS+TCP+XTLS+Web/VLESS+TCP+TLS+Web/VLESS+WebSocket+TLS+Web/VLESS+http2+TLS+Web/WebSocket+TLS+Web/http2Web模式，需要安装Caddy
                    if (String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == true)
                    {
                        //安装Caddy 61--66
                        functionResult = CaddyInstall(client);
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                        #region 上传Caddy配置文件 67--70

                        //******"上传Caddy配置文件"******
                        SetUpProgressBarProcessing(67);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfig").ToString();
                        MainWindowsShowInfo(currentStatus);

                        string serverConfig = "";
                        sshShellCommand = @"mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.bak";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == true)
                        {
                            serverConfig = $"{pwdir}" + @"TemplateConfg\v2ray\caddy\vlessTcpTlsWeb.caddyfile";
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true)
                        {
                            serverConfig = $"{pwdir}" + @"TemplateConfg\v2ray\caddy\WebSocketTLSWeb.caddyfile";
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true)
                        {
                            serverConfig = $"{pwdir}" + @"TemplateConfg\v2ray\caddy\Http2Web.caddyfile";
                        }

                        string upLoadPath = "/etc/caddy/Caddyfile";
                        client.RunCommand("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.bak");
                        UploadConfig(connectionInfo, serverConfig, upLoadPath);

                        //设置Caddyfile文件中的tls 邮箱,在caddy2中已经不需要设置。

                        //设置Caddy监听的随机端口
                        string randomCaddyListenPortStr = randomCaddyListenPort.ToString();

                        sshShellCommand = $"sed -i 's/8800/{randomCaddyListenPortStr}/' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //设置域名
                        sshShellCommand = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}/g' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //设置Path
                        sshShellCommand = $"sed -i 's/##path##/\\{ReceiveConfigurationParameters[6]}/' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //设置伪装网站
                        if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7]) == false)
                        {
                            sshShellCommand = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        }
                        //****** "Caddy配置文件上传成功,OK!" ******32
                        SetUpProgressBarProcessing(70);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfigOK").ToString();
                        MainWindowsShowInfo(currentStatus);
                        #endregion

                        //启动Caddy服务
                        functionResult = SoftStartDetect(client, "caddy", @"/usr/bin/caddy");
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
                    }
                    #endregion

                    //****** "正在启动V2ray......" ******35
                    functionResult = SoftStartDetect(client, "v2ray", @"/usr/local/bin/v2ray");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //测试BBR条件，若满足提示是否启用
                    functionResult = DetectBBRandEnable(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    client.Disconnect();//断开服务器ssh连接

                    //生成客户端配置 96--98
                    functionResult = GenerateClientConfigurationV2Ray();

                    //****** "V2Ray安装成功,祝你玩的愉快！！" ******40
                    SetUpProgressBarProcessing(100);
                    currentStatus = "V2Ray" + Application.Current.FindResource("DisplayInstallInfo_ProxyInstalledOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    Thread.Sleep(1000);

                    //显示服务端连接参数
                    proxyType = "V2Ray";
                    ResultClientInformation resultClientInformation = new ResultClientInformation();
                    resultClientInformation.ShowDialog();

                    return;

                }
            }
            catch (Exception ex1)//例外处理   
            {
                ProcessException(ex1.Message);

                //****** "安装失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
        }

        #region V2Ray专用调用函数

        //下载安装脚本安装V2Ray 37--40
        //functionResult = V2RayInstallScript(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool V2RayInstallScript(SshClient client)
        {
            //****** "系统环境检测完毕，符合安装要求,开始布署......" ******17
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstalling").ToString();
            MainWindowsShowInfo(currentStatus);

            //下载官方安装脚本安装
            //****** "正在安装V2Ray......" ******19
            SetUpProgressBarProcessing(37);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + "V2Ray......";
            MainWindowsShowInfo(currentStatus);

            sshShellCommand = $"curl -o /tmp/go.sh https://raw.githubusercontent.com/v2fly/fhs-install-v2ray/master/install-release.sh";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = @"yes | bash /tmp/go.sh -f";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = @"rm -f /tmp/go.sh";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            SetUpProgressBarProcessing(40);
            return true;
        }

        //生成V2Ray服务端配置 44--46
        //functionResult = GenerateServerConfiguration(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool GenerateServerConfigurationV2Ray(SshClient client)
        {
            SetUpProgressBarProcessing(44);
            //备份原来的文件
            sshShellCommand = @"mv /usr/local/etc/v2ray/config.json /usr/local/etc/v2ray/config.json.1";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            //读取配置文件各个模块
            string logConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\00_log\00_log.json";
            string apiConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\01_api\01_api.json";
            string dnsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\02_dns\02_dns.json";
            string routingConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\03_routing\03_routing.json";
            string policyConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\04_policy\04_policy.json";
            string inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\05_inbounds.json";
            string outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\06_outbounds\06_outbounds.json";
            string transportConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\07_transport\07_transport.json";
            string statsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\08_stats\08_stats.json";
            string reverseConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\09_reverse\09_reverse.json";
            string baseConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\base.json";

            //配置文件模块合成
            using (StreamReader reader = File.OpenText(baseConfigJson))
            {
                JObject serverJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                //读取"log"
                using (StreamReader readerJson = File.OpenText(logConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["log"] = jObjectJson["log"];
                }
                //读取"api"
                using (StreamReader readerJson = File.OpenText(apiConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["api"] = jObjectJson["api"];
                }
                //读取"dns"
                using (StreamReader readerJson = File.OpenText(dnsConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["dns"] = jObjectJson["dns"];
                }
                //读取"routing"
                using (StreamReader readerJson = File.OpenText(routingConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["routing"] = jObjectJson["routing"];
                }
                //读取"policy"
                using (StreamReader readerJson = File.OpenText(policyConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["policy"] = jObjectJson["policy"];
                }
                //读取"inbounds"
                using (StreamReader readerJson = File.OpenText(inboundsConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["inbounds"] = jObjectJson["inbounds"];
                }
                //读取"outbounds"
                using (StreamReader readerJson = File.OpenText(outboundsConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["outbounds"] = jObjectJson["outbounds"];
                }
                //读取"transport"
                using (StreamReader readerJson = File.OpenText(transportConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["transport"] = jObjectJson["transport"];
                }
                //读取"stats"
                using (StreamReader readerJson = File.OpenText(statsConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["stats"] = jObjectJson["stats"];
                }
                //读取"reverse"
                using (StreamReader readerJson = File.OpenText(reverseConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    serverJson["reverse"] = jObjectJson["reverse"];
                }

                //依据安装模式读取相应模板
                if (String.Equals(ReceiveConfigurationParameters[0], "TCP") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\tcp_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "TCPhttp") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\tcp_http_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "tcpTLS") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\tcp_TLS_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "tcpTLSselfSigned") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\tcpTLSselfSigned_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\vless_tcp_xtls_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\vless_tcp_tls_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\vless_ws_tls_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\vless_http2_tls_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\vless_vmess_xtls_tcp_websocket_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "webSocket") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\webSocket_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\WebSocket_TLS_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\WebSocketTLS_selfSigned_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\WebSocketTLSWeb_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "Http2") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\http2_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\Http2Web_server_config.json";
                }
                else if (String.Equals(ReceiveConfigurationParameters[0], "http2selfSigned") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\Http2selfSigned_server_config.json";
                }
                //else if (String.Equals(ReceiveConfigurationParameters[0], "MkcpNone")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2SRTP")||String.Equals(ReceiveConfigurationParameters[0], "mKCPuTP")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2WechatVideo")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2DTLS")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2WireGuard"))
                else if (ReceiveConfigurationParameters[0].Contains("mKCP") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\mkcp_server_config.json";
                }

                // else if (String.Equals(ReceiveConfigurationParameters[0], "QuicNone") || String.Equals(ReceiveConfigurationParameters[0], "QuicSRTP") || String.Equals(ReceiveConfigurationParameters[0], "Quic2uTP") || String.Equals(ReceiveConfigurationParameters[0], "QuicWechatVideo") || String.Equals(ReceiveConfigurationParameters[0], "QuicDTLS") || String.Equals(ReceiveConfigurationParameters[0], "QuicWireGuard"))
                else if (ReceiveConfigurationParameters[0].Contains("Quic") == true)
                {
                    inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\quic_server_config.json";
                }

                //读取"inbounds"
                using (StreamReader readerJson = File.OpenText(inboundsConfigJson))
                {
                    JObject jObjectJsonTmp = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    var jObjectJson = (dynamic)jObjectJsonTmp;

                    //Padavan路由固件服务端设置（因为客户端分流有问题所以在服务端弥补）加上后会影响一定的速度

                    //string sniffingAddServer = $"{pwdir}" + @"TemplateConfg\v2ray\server\05_inbounds\00_padavan_router.json";
                    //using (StreamReader readerSniffingJson = File.OpenText(sniffingAddServer))
                    //{
                    //    JObject jObjectSniffingJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerSniffingJson));
                    //    jObjectJson["inbounds"][0]["sniffing"] = jObjectSniffingJson["sniffing"];
                    //}

                    //设置uuid
                    jObjectJson["inbounds"][0]["settings"]["clients"][0]["id"] = ReceiveConfigurationParameters[2];

                    //除WebSocketTLSWeb/http2Web/VLESS+WebSocket+TLS+Web/VLESS+http2+TLS+Web模式外设置监听端口
                    if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == false
                        && String.Equals(ReceiveConfigurationParameters[0], "http2Web") == false
                        && String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == false
                        && String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == false)
                    {
                        jObjectJson["inbounds"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                    }

                    //设置VLESS协议的回落端口，指向Caddy
                    if (String.Equals(ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == true)
                    {
                        //设置Caddy随机监听的端口
                        randomCaddyListenPort = GetRandomPort();

                        //指向Caddy监听的随机端口
                        jObjectJson["inbounds"][0]["settings"]["fallbacks"][0]["dest"] = randomCaddyListenPort;
                    }

                    //
                    if (String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == true)
                    {
                        //设置其他模式的UUID
                        jObjectJson["inbounds"][1]["settings"]["clients"][0]["id"] = ReceiveConfigurationParameters[2];
                        jObjectJson["inbounds"][2]["settings"]["clients"][0]["id"] = ReceiveConfigurationParameters[2];
                        jObjectJson["inbounds"][3]["settings"]["clients"][0]["id"] = ReceiveConfigurationParameters[2];

                        //设置Vless回落与分流的Path
                        jObjectJson["inbounds"][0]["settings"]["fallbacks"][1]["path"] = ReceiveConfigurationParameters[3];
                        jObjectJson["inbounds"][0]["settings"]["fallbacks"][2]["path"] = ReceiveConfigurationParameters[9];
                        jObjectJson["inbounds"][0]["settings"]["fallbacks"][3]["path"] = ReceiveConfigurationParameters[6];

                        //设置Vless ws Path
                        jObjectJson["inbounds"][1]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[3];
                        //设置Vmess tcp Path
                        jObjectJson["inbounds"][2]["streamSettings"]["tcpSettings"]["header"]["request"]["path"][0] = ReceiveConfigurationParameters[9];
                        //设置Vmess ws Path
                        jObjectJson["inbounds"][3]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[6];

                    }

                    //TLS自签证书/WebSocketTLS(自签证书)/http2自签证书模式下，使用v2ctl 生成自签证书
                    if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "tcpTLSselfSigned") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "http2selfSigned") == true)
                    {
                        string selfSignedCa = client.RunCommand("/usr/local/bin/v2ctl cert --ca").Result;
                        JObject selfSignedCaJObject = JObject.Parse(selfSignedCa);
                        jObjectJson["inbounds"][0]["streamSettings"]["tlsSettings"]["certificates"][0] = selfSignedCaJObject;
                    }

                    //如果是WebSocketTLSWeb/WebSocketTLS/WebSocketTLS(自签证书)/VLESS+WebSocket+TLS+Web模式，则设置路径
                    if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true)
                    {
                        jObjectJson["inbounds"][0]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[6];
                    }

                    //如果是Http2/http2Web/http2自签/VLESS+http2+TLS+Web模式下，设置路径
                    if (String.Equals(ReceiveConfigurationParameters[0], "Http2") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "http2selfSigned") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
                    {
                        jObjectJson["inbounds"][0]["streamSettings"]["httpSettings"]["path"] = ReceiveConfigurationParameters[6];
                    }

                    //如果是Http2+Web/VLESS+http2+TLS+Web模式下，设置host
                    if (String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
                    {
                        jObjectJson["inbounds"][0]["streamSettings"]["httpSettings"]["host"][0] = ReceiveConfigurationParameters[4];
                    }

                    //mkcp模式下，设置伪装类型
                    if (ReceiveConfigurationParameters[0].Contains("mKCP") == true)
                    {
                        jObjectJson["inbounds"][0]["streamSettings"]["kcpSettings"]["header"]["type"] = ReceiveConfigurationParameters[5];
                        if (String.IsNullOrEmpty(ReceiveConfigurationParameters[6]) == false)
                        {
                            jObjectJson["inbounds"][0]["streamSettings"]["kcpSettings"]["seed"] = ReceiveConfigurationParameters[6];
                        }
                    }

                    //quic模式下设置伪装类型及密钥
                    if (ReceiveConfigurationParameters[0].Contains("Quic") == true)
                    {
                        jObjectJson["inbounds"][0]["streamSettings"]["quicSettings"]["header"]["type"] = ReceiveConfigurationParameters[5];
                        jObjectJson["inbounds"][0]["streamSettings"]["quicSettings"]["security"] = ReceiveConfigurationParameters[3];

                        if (String.Equals(ReceiveConfigurationParameters[3], "none") == true)
                        {
                            ReceiveConfigurationParameters[6] = "";
                        }
                        jObjectJson["inbounds"][0]["streamSettings"]["quicSettings"]["key"] = ReceiveConfigurationParameters[6];
                    }

                    serverJson["inbounds"] = jObjectJson["inbounds"];
                }

                using (StreamWriter sw = new StreamWriter(@"config.json"))
                {
                    sw.Write(serverJson.ToString());
                }
            }
            SetUpProgressBarProcessing(46);
            return true;
        }

        //安装证书到V2Ray 58--60
        //functionResult = CertInstallToV2ray(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool CertInstallToV2ray(SshClient client)
        {
            //****** "安装证书到V2ray......" ******26
            SetUpProgressBarProcessing(58);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoft").ToString() + "V2ray......";
            MainWindowsShowInfo(currentStatus);

            sshShellCommand = @"mkdir -p /usr/local/etc/v2ray/ssl";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = $"/root/.acme.sh/acme.sh  --installcert  -d {ReceiveConfigurationParameters[4]}  --certpath /usr/local/etc/v2ray/ssl/v2ray_ssl.crt --keypath /usr/local/etc/v2ray/ssl/v2ray_ssl.key  --capath  /usr/local/etc/v2ray/ssl/v2ray_ssl.crt  --reloadcmd  \"systemctl restart v2ray\"";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = @"if [ ! -f ""/usr/local/etc/v2ray/ssl/v2ray_ssl.key"" ]; then echo ""0""; else echo ""1""; fi | head -n 1";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            if (currentShellCommandResult.Contains("1") == true)
            {
                //****** "证书成功安装到V2ray！" ******27
                SetUpProgressBarProcessing(60);
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoftOK").ToString() + "V2Ray!";
                MainWindowsShowInfo(currentStatus);
            }
            else
            {
                //****** "证书安装到V2ray失败，原因未知，可以向开发者提问！" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoftFail").ToString() +
                                "V2Ray" +
                                Application.Current.FindResource("DisplayInstallInfo_InstallCertFailAsk").ToString();
                MainWindowsShowInfo(currentStatus);
                return false;
            }

            //设置私钥权限
            sshShellCommand = @"chmod 644 /usr/local/etc/v2ray/ssl/v2ray_ssl.key";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            return true;
        }

        //生成V2Ray客户端配置 96--98
        //functionResult = GenerateClientConfiguration();
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool GenerateClientConfigurationV2Ray()
        {
            //****** "生成客户端配置......" ******39
            SetUpProgressBarProcessing(96);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_GenerateClientConfig").ToString();
            MainWindowsShowInfo(currentStatus);

            string logConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\00_log\00_log.json";
            string apiConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\01_api\01_api.json";
            string dnsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\02_dns\02_dns.json";
            string routingConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\03_routing\03_routing.json";
            string policyConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\04_policy\04_policy.json";
            string inboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\05_inbounds\05_inbounds.json";
            string outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\06_outbounds.json";
            string transportConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\07_transport\07_transport.json";
            string statsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\08_stats\08_stats.json";
            string reverseConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\09_reverse\09_reverse.json";
            string baseConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\base.json";
            //Thread.Sleep(1000);
            if (!Directory.Exists("v2ray_config"))//如果不存在就创建file文件夹　　             　　              
            {
                Directory.CreateDirectory("v2ray_config");//创建该文件夹　　   
            }

            using (StreamReader reader = File.OpenText(baseConfigJson))
            {
                JObject clientJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                //读取"log"
                using (StreamReader readerJson = File.OpenText(logConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["log"] = jObjectJson["log"];
                }
                //读取"api"
                using (StreamReader readerJson = File.OpenText(apiConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["api"] = jObjectJson["api"];
                }
                //读取"dns"
                using (StreamReader readerJson = File.OpenText(dnsConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["dns"] = jObjectJson["dns"];
                }
                //读取"routing"
                using (StreamReader readerJson = File.OpenText(routingConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["routing"] = jObjectJson["routing"];
                }
                //读取"policy"
                using (StreamReader readerJson = File.OpenText(policyConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["policy"] = jObjectJson["policy"];
                }
                //读取"inbounds"
                using (StreamReader readerJson = File.OpenText(inboundsConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["inbounds"] = jObjectJson["inbounds"];
                }
                //读取"outbounds"
                using (StreamReader readerJson = File.OpenText(outboundsConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["outbounds"] = jObjectJson["outbounds"];
                }
                //读取"transport"
                using (StreamReader readerJson = File.OpenText(transportConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["transport"] = jObjectJson["transport"];
                }
                //读取"stats"
                using (StreamReader readerJson = File.OpenText(statsConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["stats"] = jObjectJson["stats"];
                }
                //读取"reverse"
                using (StreamReader readerJson = File.OpenText(reverseConfigJson))
                {
                    JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));
                    clientJson["reverse"] = jObjectJson["reverse"];
                }

                //根据不同的安装方案，选择相应的客户端模板
                if (String.Equals(ReceiveConfigurationParameters[0], "VlessVmessXtlsTcpWebSocketWeb") == false)
                {
                    #region 单模式方案
                    //根据选择的不同模式，选择相应的配置文件
                    if (String.Equals(ReceiveConfigurationParameters[0], "TCP") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\tcp_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "TCPhttp") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\tcp_http_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "tcpTLS") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\tcp_TLS_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "tcpTLSselfSigned") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\tcpTLSselfSigned_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\vless_tcp_xtls_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "VlessTcpTlsWeb") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\vless_tcp_tls_caddy_cilent_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\vless_ws_tls_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\vless_http2_tls_server_config.json";
                    }

                    else if (String.Equals(ReceiveConfigurationParameters[0], "webSocket") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\webSocket_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\WebSocket_TLS_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\WebSocketTLS_selfSigned_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\WebSocketTLSWeb_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "Http2") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\http2_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\Http2Web_client_config.json";
                    }
                    else if (String.Equals(ReceiveConfigurationParameters[0], "http2selfSigned") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\Http2selfSigned_client_config.json";
                    }
                    //else if (String.Equals(ReceiveConfigurationParameters[0], "MkcpNone")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2SRTP")||String.Equals(ReceiveConfigurationParameters[0], "mKCPuTP")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2WechatVideo")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2DTLS")|| String.Equals(ReceiveConfigurationParameters[0], "mKCP2WireGuard"))
                    else if (ReceiveConfigurationParameters[0].Contains("mKCP") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\mkcp_client_config.json";
                    }
                    // else if (String.Equals(ReceiveConfigurationParameters[0], "QuicNone") || String.Equals(ReceiveConfigurationParameters[0], "QuicSRTP") || String.Equals(ReceiveConfigurationParameters[0], "Quic2uTP") || String.Equals(ReceiveConfigurationParameters[0], "QuicWechatVideo") || String.Equals(ReceiveConfigurationParameters[0], "QuicDTLS") || String.Equals(ReceiveConfigurationParameters[0], "QuicWireGuard"))
                    else if (ReceiveConfigurationParameters[0].Contains("Quic") == true)
                    {
                        outboundsConfigJson = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\quic_client_config.json";
                    }


                    //读取"相应模板的outbounds"
                    using (StreamReader readerJson = File.OpenText(outboundsConfigJson))
                    {
                        JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));

                        //设置客户端的地址/端口/id
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["address"] = ReceiveConfigurationParameters[4];
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["users"][0]["id"] = ReceiveConfigurationParameters[2];


                        //设置WebSocket模式下的path
                        if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSselfSigned") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLS2Web") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "VlessWebSocketTlsWeb") == true)
                        {
                            jObjectJson["outbounds"][0]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[6];
                        }

                        //设置http2模式下的path
                        if (String.Equals(ReceiveConfigurationParameters[0], "Http2") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "http2selfSigned") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
                        {
                            jObjectJson["outbounds"][0]["streamSettings"]["httpSettings"]["path"] = ReceiveConfigurationParameters[6];
                        }

                        //设置http2+TLS+Web/VLESS+http2+TLS+Web模式下的host
                        if (String.Equals(ReceiveConfigurationParameters[0], "http2Web") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "VlessHttp2Web") == true)
                        {
                            jObjectJson["outbounds"][0]["streamSettings"]["httpSettings"]["host"][0] = ReceiveConfigurationParameters[4];
                        }

                        //设置VLESS+TCP+XTLS+Web模式下的serverName
                        //if (String.Equals(ReceiveConfigurationParameters[0], "VlessXtlsTcp") == true)
                        //{
                        //    jObjectJson["outbounds"][0]["streamSettings"]["xtlsSettings"]["serverName"] = ReceiveConfigurationParameters[4];
                        //}

                        //设置mkcp
                        if (ReceiveConfigurationParameters[0].Contains("mKCP") == true)
                        {
                            jObjectJson["outbounds"][0]["streamSettings"]["kcpSettings"]["header"]["type"] = ReceiveConfigurationParameters[5];
                            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[6]) == false)
                            {
                                jObjectJson["outbounds"][0]["streamSettings"]["kcpSettings"]["seed"] = ReceiveConfigurationParameters[6];
                            }
                        }

                        //设置QUIC
                        if (ReceiveConfigurationParameters[0].Contains("Quic") == true)
                        {
                            jObjectJson["outbounds"][0]["streamSettings"]["quicSettings"]["header"]["type"] = ReceiveConfigurationParameters[5];
                            jObjectJson["outbounds"][0]["streamSettings"]["quicSettings"]["security"] = ReceiveConfigurationParameters[3];
                            if (String.Equals(ReceiveConfigurationParameters[3], "none") == true)
                            {
                                ReceiveConfigurationParameters[6] = "";
                            }
                            jObjectJson["outbounds"][0]["streamSettings"]["quicSettings"]["key"] = ReceiveConfigurationParameters[6];
                        }

                        clientJson["outbounds"] = jObjectJson["outbounds"];

                    }

                    using (StreamWriter sw = new StreamWriter(@"v2ray_config\config.json"))
                    {
                        sw.Write(clientJson.ToString());
                    }

                    #endregion

                }
                else
                {
                    //复合方案所需要的配置文件
                    //VLESS over TCP with XTLS模式
                    string outboundsConfigJsonVlessXtls = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\vless_tcp_xtls_client_config.json";
                    using (StreamReader readerJson = File.OpenText(outboundsConfigJsonVlessXtls))
                    {
                        JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));

                        //设置客户端的地址/端口/id
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["address"] = ReceiveConfigurationParameters[4];
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["users"][0]["id"] = ReceiveConfigurationParameters[2];

                        clientJson["outbounds"] = jObjectJson["outbounds"];
                        if (!Directory.Exists(@"v2ray_config\vless_tcp_xtls_client_config"))//如果不存在就创建file文件夹　　             　　              
                        {
                            Directory.CreateDirectory(@"v2ray_config\vless_tcp_xtls_client_config");//创建该文件夹　　   
                        }
                        using (StreamWriter sw = new StreamWriter(@"v2ray_config\vless_tcp_xtls_client_config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }

                    //VLESS over TCP with TLS模式
                    string outboundsConfigJsonVlessTcpTls = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\vless_tcp_tls_caddy_cilent_config.json";
                    using (StreamReader readerJson = File.OpenText(outboundsConfigJsonVlessTcpTls))
                    {
                        JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));

                        //设置客户端的地址/端口/id
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["address"] = ReceiveConfigurationParameters[4];
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["users"][0]["id"] = ReceiveConfigurationParameters[2];

                        clientJson["outbounds"] = jObjectJson["outbounds"];
                        if (!Directory.Exists(@"v2ray_config\vless_tcp_tls_client_config"))//如果不存在就创建file文件夹　　             　　              
                        {
                            Directory.CreateDirectory(@"v2ray_config\vless_tcp_tls_client_config");//创建该文件夹　　   
                        }
                        using (StreamWriter sw = new StreamWriter(@"v2ray_config\vless_tcp_tls_client_config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }

                    //VLESS over WS with TLS 模式
                    string outboundsConfigJsonVlessWsTls = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\vless_ws_tls_client_config.json";
                    using (StreamReader readerJson = File.OpenText(outboundsConfigJsonVlessWsTls))
                    {
                        JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));

                        //设置客户端的地址/端口/id
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["address"] = ReceiveConfigurationParameters[4];
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["users"][0]["id"] = ReceiveConfigurationParameters[2];
                        jObjectJson["outbounds"][0]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[3];

                        clientJson["outbounds"] = jObjectJson["outbounds"];
                        if (!Directory.Exists(@"v2ray_config\vless_ws_tls_client_config"))//如果不存在就创建file文件夹　　             　　              
                        {
                            Directory.CreateDirectory(@"v2ray_config\vless_ws_tls_client_config");//创建该文件夹　　   
                        }
                        using (StreamWriter sw = new StreamWriter(@"v2ray_config\vless_ws_tls_client_config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }

                    //VMess over TCP with TLS模式
                    string outboundsConfigJsonVmessTcpTls = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\vmess_tcp_tls_client_config.json";
                    using (StreamReader readerJson = File.OpenText(outboundsConfigJsonVmessTcpTls))
                    {
                        JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));

                        //设置客户端的地址/端口/id
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["address"] = ReceiveConfigurationParameters[4];
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["users"][0]["id"] = ReceiveConfigurationParameters[2];
                        jObjectJson["outbounds"][0]["streamSettings"]["tcpSettings"]["header"]["request"]["path"][0] = ReceiveConfigurationParameters[9];

                        clientJson["outbounds"] = jObjectJson["outbounds"];
                        if (!Directory.Exists(@"v2ray_config\vmess_tcp_tls_client_config"))//如果不存在就创建file文件夹　　             　　              
                        {
                            Directory.CreateDirectory(@"v2ray_config\vmess_tcp_tls_client_config");//创建该文件夹　　   
                        }
                        using (StreamWriter sw = new StreamWriter(@"v2ray_config\vmess_tcp_tls_client_config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }

                    //VMess over WS with TLS模式
                    string outboundsConfigJsonVmessWsTls = $"{pwdir}" + @"TemplateConfg\v2ray\client\06_outbounds\WebSocketTLSWeb_client_config.json";
                    using (StreamReader readerJson = File.OpenText(outboundsConfigJsonVmessWsTls))
                    {
                        JObject jObjectJson = (JObject)JToken.ReadFrom(new JsonTextReader(readerJson));

                        //设置客户端的地址/端口/id
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["address"] = ReceiveConfigurationParameters[4];
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        jObjectJson["outbounds"][0]["settings"]["vnext"][0]["users"][0]["id"] = ReceiveConfigurationParameters[2];
                        jObjectJson["outbounds"][0]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[6];

                        clientJson["outbounds"] = jObjectJson["outbounds"];
                        if (!Directory.Exists(@"v2ray_config\vmess_ws_tls_client_config"))//如果不存在就创建file文件夹　　             　　              
                        {
                            Directory.CreateDirectory(@"v2ray_config\vmess_ws_tls_client_config");//创建该文件夹　　   
                        }
                        using (StreamWriter sw = new StreamWriter(@"v2ray_config\vmess_ws_tls_client_config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }

                }
            }
            SetUpProgressBarProcessing(98);
            return true;
        }


        #endregion

        //检测升级远程主机端的V2Ray版本
        private void ButtonUpdateV2ray_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }
            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => UpdateV2ray(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        //升级V2ray主程序
        private void UpdateV2ray(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                    //******"检测系统是否已经安装V2ray......"******
                    SetUpProgressBarProcessing(20);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "V2ray......";
                    MainWindowsShowInfo(currentStatus);

                    //Thread.Sleep(1000);
                    //检测是否安装V2Ray
                    sshShellCommand = @"find / -name v2ray";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/local/bin/v2ray") == false)
                    {
                        //******"退出！原因：远程主机未安装V2ray"******
                        MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorUpgradeSoftNotInstall").ToString() + "V2Ray!");
                        //******"退出！原因：远程主机未安装V2ray"******
                        currentStatus = Application.Current.FindResource("MessageBoxShow_ErrorUpgradeSoftNotInstall").ToString() + "V2Ray!";
                        MainWindowsShowInfo(currentStatus);

                        client.Disconnect();
                        return;

                    }
                    
                    //sshcmd = @"/usr/local/bin/v2ray -version | head -n 1 | cut -d "" "" -f2";
                    sshShellCommand = @"/usr/local/bin/v2ray -version | head -n 1 | cut -d "" "" -f2";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string v2rayCurrentVersion = currentShellCommandResult;//不含字母v

                    //sshcmd = @"curl -H ""Accept: application/json"" -H ""User-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:74.0) Gecko/20100101 Firefox/74.0"" -s ""https://api.github.com/repos/v2fly/v2ray-core/releases/latest"" --connect-timeout 10| grep 'tag_name' | cut -d\"" -f4";
                    sshShellCommand = @"curl -H ""Accept: application/json"" -H ""User-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:74.0) Gecko/20100101 Firefox/74.0"" -s ""https://api.github.com/repos/v2fly/v2ray-core/releases/latest"" --connect-timeout 10 | grep 'tag_name' | cut -d\"" -f4";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string v2rayNewVersion = currentShellCommandResult;//包含字母v

                    if (v2rayNewVersion.Contains(v2rayCurrentVersion) == false)
                    {
                        MessageBoxResult messageBoxResult = MessageBox.Show(
                            //****** "远程主机当前版本为：v" ******
                            Application.Current.FindResource("DisplayInstallInfo_CurrentVersion").ToString() + 
                            $"{v2rayCurrentVersion}\n" +
                            //****** "最新版本为：" ******
                            Application.Current.FindResource("DisplayInstallInfo_NewVersion").ToString() + 
                            $"{v2rayNewVersion}\n" +
                            //****** "是否升级为最新版本？" ******
                            Application.Current.FindResource("DisplayInstallInfo_IsOrNoUpgradeNewVersion").ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            //****** "正在升级到最新版本......" ******
                            SetUpProgressBarProcessing(60);
                            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartUpgradeNewVersion").ToString();
                            MainWindowsShowInfo(currentStatus);

                            //client.RunCommand(@"bash <(curl -L -s https://raw.githubusercontent.com/v2fly/fhs-install-v2ray/master/install-release.sh)");
                            sshShellCommand = $"bash <(curl -L -s https://raw.githubusercontent.com/v2fly/fhs-install-v2ray/master/install-release.sh)";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            SetUpProgressBarProcessing(80);
                            //sshcmd = @"/usr/local/bin/v2ray -version | head -n 1 | cut -d "" "" -f2";
                            sshShellCommand = @"/usr/local/bin/v2ray -version | head -n 1 | cut -d "" "" -f2";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            v2rayCurrentVersion = currentShellCommandResult;//不含字母v
                            if (v2rayNewVersion.Contains(v2rayCurrentVersion) == true)
                            {
                                //****** "升级成功！当前已是最新版本！" ******
                                MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionOK").ToString());
                                //****** "升级成功！当前已是最新版本！" ******
                                SetUpProgressBarProcessing(100);
                                currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionOK").ToString();
                                MainWindowsShowInfo(currentStatus);
                            }
                            else
                            {
                                //****** "升级失败，原因未知，请向开发者提问！" ******
                                MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionFail").ToString());
                                //****** "升级失败，原因未知，请向开发者提问！" ******
                                currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionFail").ToString();
                                MainWindowsShowInfo(currentStatus);
                            }
                        }
                        else
                        {
                            //****** "升级取消，退出!" ******
                            currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeVersionCancel").ToString();
                            MainWindowsShowInfo(currentStatus);

                            client.Disconnect();
                            return;
                        }
                    }
                    else
                    {
                        //****** "远程主机当前已是最新版本：" ******
                        SetUpProgressBarProcessing(100);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_IsNewVersion").ToString() +
                            $"{v2rayNewVersion}\n" +
                            //******  "无需升级！退出！" ******
                            Application.Current.FindResource("DisplayInstallInfo_NotUpgradeVersion").ToString();
                        MessageBox.Show(currentStatus);
                        MainWindowsShowInfo(currentStatus);
                    }

                    client.Disconnect();
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);
   
                //****** "主机登录失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }
        #endregion

        #region Trojan-go相关

        //打开设置TrojanGo参数窗口
        private void ButtonTrojanGoTemplate_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i != ReceiveConfigurationParameters.Length; i++)

            {
                ReceiveConfigurationParameters[i] = "";
            }
            TrojanGoTemplateWindow windowTrojanGoTemplateConfiguration = new TrojanGoTemplateWindow();
            windowTrojanGoTemplateConfiguration.Closed += windowTrojanGoTemplateConfigurationClosed;
            windowTrojanGoTemplateConfiguration.ShowDialog();
        }
        //TrojanGo模板设置窗口关闭后，触发事件，将所选的方案与其参数显示在UI上
        private void windowTrojanGoTemplateConfigurationClosed(object sender, System.EventArgs e)
        {
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[0]) == true)
            {
                //显示"未选择方案！"
                TextBlockCurrentlySelectedPlan.Text = Application.Current.FindResource("TextBlockCurrentlySelectedPlanNo").ToString();

                TextBlockTrojanGoShowPort.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCurrentlySelectedPlanPort.Visibility = Visibility.Hidden;

                TextBlockTrojanGoShowPassword.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCurrentlySelectedPlanPassword.Visibility = Visibility.Hidden;

                TextBlockTrojanGoShowPath.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCurrentlySelectedPlanPathSeedKey.Visibility = Visibility.Hidden;

                TextBlockTrojanGoShowCurrentlySelectedPlanDomain.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCurrentlySelectedPlanDomain.Visibility = Visibility.Hidden;

                TextBlockTrojanGoShowCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
                return;
            }
            TextBlockTrojanGoCurrentlySelectedPlan.Text = ReceiveConfigurationParameters[8];            //所选方案名称
            TextBlockTrojanGoCurrentlySelectedPlanPort.Text = ReceiveConfigurationParameters[1];        //服务器端口
            TextBlockTrojanGoCurrentlySelectedPlanPassword.Text = ReceiveConfigurationParameters[2];        //UUID
            TextBlockTrojanGoCurrentlySelectedPlanPathSeedKey.Text = ReceiveConfigurationParameters[6]; //mKCP Seed\Quic Key\Path
            TextBlockTrojanGoCurrentlySelectedPlanDomain.Text = ReceiveConfigurationParameters[4];      //域名
            TextBlockTrojanGoCurrentlySelectedPlanFakeWebsite.Text = ReceiveConfigurationParameters[7]; //伪装网站

            if (String.Equals(ReceiveConfigurationParameters[0], "TrojanGoTLS2Web"))
            {
                TextBlockTrojanGoShowPort.Visibility = Visibility.Visible;
                TextBlockTrojanGoCurrentlySelectedPlanPort.Visibility = Visibility.Visible;

                TextBlockTrojanGoShowPassword.Visibility = Visibility.Visible;
                TextBlockTrojanGoCurrentlySelectedPlanPassword.Visibility = Visibility.Visible;

                TextBlockTrojanGoShowPath.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCurrentlySelectedPlanPathSeedKey.Visibility = Visibility.Hidden;

                TextBlockTrojanGoShowCurrentlySelectedPlanDomain.Visibility = Visibility.Visible;
                TextBlockTrojanGoCurrentlySelectedPlanDomain.Visibility = Visibility.Visible;

                TextBlockTrojanGoShowCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;

            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "TrojanGoWebSocketTLS2Web"))
            {
                TextBlockTrojanGoShowPort.Visibility = Visibility.Visible;
                TextBlockTrojanGoCurrentlySelectedPlanPort.Visibility = Visibility.Visible;

                TextBlockTrojanGoShowPassword.Visibility = Visibility.Visible;
                TextBlockTrojanGoCurrentlySelectedPlanPassword.Visibility = Visibility.Visible;

                TextBlockTrojanGoShowPath.Text = "WebSocket Path:";
                TextBlockTrojanGoCurrentlySelectedPlanPathSeedKey.Text = ReceiveConfigurationParameters[6]; //mKCP Seed\Quic Key\Path


                TextBlockTrojanGoShowPath.Visibility = Visibility.Visible;
                TextBlockTrojanGoCurrentlySelectedPlanPathSeedKey.Visibility = Visibility.Visible;

                TextBlockTrojanGoShowCurrentlySelectedPlanDomain.Visibility = Visibility.Visible;
                TextBlockTrojanGoCurrentlySelectedPlanDomain.Visibility = Visibility.Visible;

                TextBlockTrojanGoShowCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
                TextBlockTrojanGoCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
            }
           
        }

        //传递TrojanGo参数
        private void ButtonTrojanGoSetUp_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }

            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[0]) == true)
            {
                //******"请先选择配置模板！"******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ChooseTemplate").ToString());
                return;
            }
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[4]) == true)
            {
                //****** "域名不能为空，请检查相关参数设置！" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_DomainNotEmpty").ToString());
                return;
            }
            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => StartSetUpTrojanGo(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        //登录远程主机布署Trojan-Go程序
        private void StartSetUpTrojanGo(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);

                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测是否已安装代理 8--10
                    functionResult = SoftInstalledIsNoYes(client, "trojan-go", @"/usr/local/bin/trojan-go");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测关闭Selinux及系统组件是否齐全（apt/yum/dnf/systemctl）11--30
                    //安装依赖软件，检测端口，防火墙开启端口
                    functionResult = ShutDownSelinuxAndSysComponentsDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测域名是否解析到当前IP上 34---36
                    functionResult = DomainResolutionCurrentIPDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //下载脚本安装Trojan-go 37--40
                    functionResult = TrojanGoInstall(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //程序是否安装成功检测并设置开机启动 41--43
                    functionResult = SoftInstalledSuccessOrFail(client, "trojan-go", @"/usr/local/bin/trojan-go");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //生成Trojan-go服务端配置 44--46
                    functionResult = GenerateServerConfigurationTrojanGo(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //上传配置文件
                    string upLoadPath = "/usr/local/etc/trojan-go/config.json";
                    UploadConfig(connectionInfo, @"config.json", upLoadPath);
                    File.Delete(@"config.json");

                    //acme.sh安装与申请证书 51--57
                    functionResult = AcmeShInstall(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    
                    //****** "安装证书到Trojan-go......" ******
                    SetUpProgressBarProcessing(58);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoft").ToString() + "Trojan-go......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = $"/root/.acme.sh/acme.sh  --installcert  -d {ReceiveConfigurationParameters[4]}  --certpath /usr/local/etc/trojan-go/trojan-go.crt --keypath /usr/local/etc/trojan-go/trojan-go.key  --capath  /usr/local/etc/trojan-go/trojan-go.crt  --reloadcmd  \"systemctl restart trojan-go\"";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"if [ ! -f ""/usr/local/etc/trojan-go/trojan-go.key"" ]; then echo ""0""; else echo ""1""; fi | head -n 1";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("1") == true)
                    {
                        //****** "证书成功安装到Trojan-go！" ******
                        SetUpProgressBarProcessing(60);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoftOK").ToString() + "Trojan-go!";
                        MainWindowsShowInfo(currentStatus);
                    }
                    else
                    {
                        //****** "证书安装到Trojan-go失败，原因未知，可以向开发者提问！" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoftFail").ToString() +
                                        "Trojan-go" +
                                        Application.Current.FindResource("DisplayInstallInfo_InstallCertFailAsk").ToString();
                        MainWindowsShowInfo(currentStatus);
                        client.Disconnect();
                        return;
                    }

                    //设置证书权限
                    sshShellCommand = @"chmod 644 /usr/local/etc/trojan-go/trojan-go.key";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //Caddy安装与检测安装是否成功 61--66
                    functionResult = CaddyInstall(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                   
                    //****** "上传Caddy配置文件......" ******
                    SetUpProgressBarProcessing(67);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.bak";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string caddyConfig = $"{pwdir}" + @"TemplateConfg\trojan-go\trojan-go.caddyfile";

                    upLoadPath = "/etc/caddy/Caddyfile";
                    UploadConfig(connectionInfo, caddyConfig, upLoadPath);

                    //设置Caddyfile文件中的tls 邮箱

                    //设置Caddy监听的随机端口
                    string randomCaddyListenPortStr = randomCaddyListenPort.ToString();

                    sshShellCommand = $"sed -i 's/8800/{randomCaddyListenPortStr}/' {upLoadPath}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //设置域名
                    sshShellCommand = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}/g' {upLoadPath}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //设置伪装网站
                    if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7]) == false)
                    {
                        sshShellCommand = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }
                    //****** "Caddy配置文件上传成功,OK!" ******
                    SetUpProgressBarProcessing(70);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfigOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //程序启动检测Caddy
                    functionResult = SoftStartDetect(client, "caddy", @"/usr/bin/caddy");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //程序启动检测Trojan-go
                    functionResult = SoftStartDetect(client, "trojan-go", @"/usr/local/bin/trojan-go");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测BBR，满足条件并启动 90--95
                    functionResult = DetectBBRandEnable(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    client.Disconnect();//断开服务器ssh连接


                    //****** "生成客户端配置......" ******
                    SetUpProgressBarProcessing(96);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_GenerateClientConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    if (!Directory.Exists("trojan-go_config"))//如果不存在就创建file文件夹　　             　　              
                    {
                        Directory.CreateDirectory("trojan-go_config");//创建该文件夹　　   
                    }

                    string clientConfig = $"{pwdir}" + @"TemplateConfg\trojan-go\trojan-go_all_config.json"; //生成的客户端配置文件

                    using (StreamReader reader = File.OpenText(clientConfig))
                    {
                        JObject clientJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                        clientJson["run_type"] = "client";
                        clientJson["local_addr"] = "127.0.0.1";
                        clientJson["local_port"] = 1080;
                        clientJson["remote_addr"] = ReceiveConfigurationParameters[4];
                        clientJson["remote_port"] = 443;
                        //设置密码
                        clientJson["password"][0] = ReceiveConfigurationParameters[2];
                        //如果是WebSocket协议则设置路径
                        if (String.Equals(ReceiveConfigurationParameters[0], "TrojanGoWebSocketTLS2Web"))
                        {
                            clientJson["websocket"]["enabled"] = true;
                            clientJson["websocket"]["path"] = ReceiveConfigurationParameters[6];
                        }

                        using (StreamWriter sw = new StreamWriter(@"trojan-go_config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }

                    //****** "Trojan-go安装成功,祝你玩的愉快！！" ******
                    SetUpProgressBarProcessing(100);
                    currentStatus = "Trojan-go" + Application.Current.FindResource("DisplayInstallInfo_ProxyInstalledOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //显示服务端连接参数
                    proxyType = "TrojanGo";

                    ResultClientInformation resultClientInformation = new ResultClientInformation();
                    resultClientInformation.ShowDialog();
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "安装失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }

        //下载脚本安装Trojan-go 37--40
        //functionResult = TrojanGoInstall(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool TrojanGoInstall(SshClient client)
        {
            //****** "系统环境检测完毕，符合安装要求,开始布署......" ******
            SetUpProgressBarProcessing(37);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstalling").ToString();
            MainWindowsShowInfo(currentStatus);

            //****** "正在安装Trojan-go......" ******
            SetUpProgressBarProcessing(38);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + "Trojan-go......";
            MainWindowsShowInfo(currentStatus);

            sshShellCommand = $"curl -o /tmp/trojan-go.sh https://raw.githubusercontent.com/proxysu/shellscript/master/trojan-go.sh";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = @"yes | bash /tmp/trojan-go.sh -f";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = @"rm -f /tmp/trojan-go.sh";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            SetUpProgressBarProcessing(40);
            return true;
        }

        //生成Trojan-go服务端配置 44--46
        //functionResult = GenerateServerConfigurationTrojanGo(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool GenerateServerConfigurationTrojanGo(SshClient client)
        {
            //备份原配置文件
            sshShellCommand = @"mv /etc/trojan-go/config.json /etc/trojan-go/config.json.1";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            //****** "安装完毕，上传配置文件......" ******
            SetUpProgressBarProcessing(44);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadSoftConfig").ToString();
            MainWindowsShowInfo(currentStatus);

            string serverConfig = $"{pwdir}" + @"TemplateConfg\trojan-go\trojan-go_all_config.json";  //服务端配置文件
            //string upLoadPath = @"/usr/local/etc/trojan-go/config.json"; //服务端文件位置

            //生成服务端配置
            using (StreamReader reader = File.OpenText(serverConfig))
            {
                //设置Caddy随机监听的端口，用于Trojan-go,Trojan,V2Ray vless TLS
                //Random random = new Random();
                randomCaddyListenPort = GetRandomPort();

                JObject serverJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                serverJson["run_type"] = "server";
                serverJson["local_addr"] = "0.0.0.0";
                serverJson["local_port"] = 443;
                serverJson["remote_addr"] = "127.0.0.1";
                serverJson["remote_port"] = randomCaddyListenPort;
                //设置密码
                serverJson["password"][0] = ReceiveConfigurationParameters[2];
                //设置证书
                serverJson["ssl"]["cert"] = "/usr/local/etc/trojan-go/trojan-go.crt";
                serverJson["ssl"]["key"] = "/usr/local/etc/trojan-go/trojan-go.key";
                //serverJson["ssl"]["sni"] = ReceiveConfigurationParameters[4];

                if (String.Equals(ReceiveConfigurationParameters[0], "TrojanGoWebSocketTLS2Web"))
                {
                    serverJson["websocket"]["enabled"] = true;
                    serverJson["websocket"]["path"] = ReceiveConfigurationParameters[6];
                }

                using (StreamWriter sw = new StreamWriter(@"config.json"))
                {
                    sw.Write(serverJson.ToString());
                }
            }


            SetUpProgressBarProcessing(46);
            return true;
        }


        //检测升级Trojan-Go版本传递参数
        private void ButtonUpdateTrojanGo_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }
            installationDegree = 0;
            Thread thread = new Thread(() => UpdateTojanGo(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        //升级Trojan-go主程序
        private void UpdateTojanGo(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                    //******"检测系统是否已经安装Trojan-go......"******
                    SetUpProgressBarProcessing(20);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "Trojan-go......";
                    MainWindowsShowInfo(currentStatus);

                    //string cmdTestTrojanInstalled = @"find / -name trojan-go";

                    sshShellCommand = @"find / -name trojan-go";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/local/bin/trojan-go") == false)
                    {
                        //******"退出！原因：远程主机未安装Trojan-go"******
                        MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorUpgradeSoftNotInstall").ToString() + "Trojan-go!");
                        //******"退出！原因：远程主机未安装Trojan-go"******
                        currentStatus = Application.Current.FindResource("MessageBoxShow_ErrorUpgradeSoftNotInstall").ToString() + "Trojan-go!";
                        MainWindowsShowInfo(currentStatus);

                        client.Disconnect();
                        return;

                    }
                    SetUpProgressBarProcessing(40);
                    //获取当前安装的版本
                    //string sshcmd = @"echo ""$(/usr/local/bin/trojan-go -version)"" | head -n 1 | cut -d "" "" -f2";
                    sshShellCommand = @"echo ""$(/usr/local/bin/trojan-go -version)"" | head -n 1 | cut -d "" "" -f2";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    string trojanCurrentVersion = currentShellCommandResult;//含字母v

                    //获取最新版本
                    //sshcmd = @"curl -s https://api.github.com/repos/p4gefau1t/trojan-go/tags | grep 'name' | cut -d\"" -f4 | head -1";
                    sshShellCommand = @"curl -s https://api.github.com/repos/p4gefau1t/trojan-go/tags | grep 'name' | cut -d\"" -f4 | head -1";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    string trojanNewVersion = currentShellCommandResult;//含字母v

                    if (trojanNewVersion.Equals(trojanCurrentVersion) == false)
                    {
                        MessageBoxResult messageBoxResult = MessageBox.Show(
                             //****** "远程主机当前版本为：v" ******
                             Application.Current.FindResource("DisplayInstallInfo_CurrentVersion").ToString() +
                             $"{trojanCurrentVersion}\n" +
                             //****** "最新版本为：" ******
                             Application.Current.FindResource("DisplayInstallInfo_NewVersion").ToString() +
                             $"{trojanNewVersion}\n" +
                             //****** "是否升级为最新版本？" ******
                             Application.Current.FindResource("DisplayInstallInfo_IsOrNoUpgradeNewVersion").ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            //****** "正在升级到最新版本......" ******
                            SetUpProgressBarProcessing(60);
                            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartUpgradeNewVersion").ToString();
                            MainWindowsShowInfo(currentStatus);

                            //备份配置文件
                            //sshcmd = @"mv /usr/local/etc/trojan/config.json /usr/local/etc/trojan/config.json.bak";
                            //client.RunCommand(sshcmd);
                            //升级Trojan-Go主程序
                            //client.RunCommand("curl -o /tmp/trojan-go.sh https://raw.githubusercontent.com/proxysu/shellscript/master/trojan-go.sh");
                            //client.RunCommand("yes | bash /tmp/trojan-go.sh -f");
                            sshShellCommand = $"curl -o /tmp/trojan-go.sh https://raw.githubusercontent.com/proxysu/shellscript/master/trojan-go.sh";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            sshShellCommand = @"yes | bash /tmp/trojan-go.sh -f";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            sshShellCommand = @"rm -f /tmp/trojan-go.sh";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            SetUpProgressBarProcessing(80);

                            //获取升级后的版本
                            //sshcmd = @"echo ""$(/usr/local/bin/trojan-go -version)"" | head -n 1 | cut -d "" "" -f2";
                            sshShellCommand = @"echo ""$(/usr/local/bin/trojan-go -version)"" | head -n 1 | cut -d "" "" -f2";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            trojanCurrentVersion = currentShellCommandResult;//含字母v
                            if (trojanNewVersion.Equals(trojanCurrentVersion) == true)
                            {
                                //恢复原来的配置文件备份
                                //sshcmd = @"rm -f /usr/local/etc/trojan/config.json";
                                //client.RunCommand(sshcmd);
                                //sshcmd = @"mv /usr/local/etc/trojan/config.json.bak /usr/local/etc/trojan/config.json";
                                //client.RunCommand(sshcmd);

                                //****** "升级成功！当前已是最新版本！" ******
                                MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionOK").ToString());
                                //****** "升级成功！当前已是最新版本！" ******
                                SetUpProgressBarProcessing(100);
                                currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionOK").ToString();
                                MainWindowsShowInfo(currentStatus);
                            }
                            else
                            {
                                //****** "升级失败，原因未知，请向开发者提问！" ******
                                MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionFail").ToString());
                                //****** "升级失败，原因未知，请向开发者提问！" ******
                                currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionFail").ToString();
                                MainWindowsShowInfo(currentStatus);

                                client.Disconnect();
                                return;
                            }
                        }

                        else
                        {
                            //****** "升级取消，退出!" ******
                            currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeVersionCancel").ToString();
                            MainWindowsShowInfo(currentStatus);

                            client.Disconnect();
                            return;
                        }
                    }
                    else
                    {
                        //****** "远程主机当前已是最新版本：" ******
                        SetUpProgressBarProcessing(100);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_IsNewVersion").ToString() +
                            $"{trojanNewVersion}\n" +
                            //******  "无需升级！退出！" ******
                            Application.Current.FindResource("DisplayInstallInfo_NotUpgradeVersion").ToString();
                        MessageBox.Show(currentStatus);
                        MainWindowsShowInfo(currentStatus);
                    }

                    client.Disconnect();
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "主机登录失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }
        #endregion

        #region Trojan相关

        //Trojan参数传递
        private void ButtonTrojanSetUp_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }
            //清空参数空间
            for (int i = 0; i != ReceiveConfigurationParameters.Length; i++)

            {
                ReceiveConfigurationParameters[i] = "";
            }
            if (string.IsNullOrEmpty(TextBoxTrojanHostDomain.Text.ToString()) == true)
            {
                //****** "域名不能为空，请检查相关参数设置！" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_DomainNotEmpty").ToString());
                return;
            }
            //传递模板类型
            ReceiveConfigurationParameters[0] = "TrojanTLS2Web";

            //传递域名
            ReceiveConfigurationParameters[4] = TextBoxTrojanHostDomain.Text.ToString();
            //传递伪装网站
            ReceiveConfigurationParameters[7] = TextBoxTrojanSites.Text.ToString();
            //处理伪装网站域名中的前缀
            if (TextBoxTrojanSites.Text.ToString().Length >= 7)
            {
                string testDomain = TextBoxTrojanSites.Text.Substring(0, 7);
                if (String.Equals(testDomain, "https:/") || String.Equals(testDomain, "http://"))
                {
                    //MessageBox.Show(testDomain);
                    ReceiveConfigurationParameters[7] = TextBoxTrojanSites.Text.Replace("/", "\\/");
                }
                else
                {
                    ReceiveConfigurationParameters[7] = "http:\\/\\/" + TextBoxTrojanSites.Text;
                }
            }
            //传递服务端口
            ReceiveConfigurationParameters[1] = "443";
            //传递密码(uuid)
            ReceiveConfigurationParameters[2] = TextBoxTrojanPassword.Text.ToString();

            //启动布署进程                                 
            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => StartSetUpTrojan(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        
        //登录远程主机布署Trojan程序
        private void StartSetUpTrojan(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测是否已安装代理 8--10
                    functionResult = SoftInstalledIsNoYes(client, "trojan", @"/usr/local/bin/trojan");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测关闭Selinux及系统组件是否齐全（apt/yum/dnf/systemctl）11--30
                    //安装依赖软件，检测端口，防火墙开启端口
                    functionResult = ShutDownSelinuxAndSysComponentsDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                    //检测是否为64位系统
                    sshShellCommand = @"uname -m";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    string resultCmd = currentShellCommandResult;
                    if (resultCmd.Contains("x86_64") == false)
                    {
                        //******"请在x86_64系统中安装Trojan" ******
                        MessageBox.Show(Application.Current.FindResource("MessageBoxShow_PleaseInstallSoftAtX64").ToString() + "NaiveProxy......");
                        //****** "系统环境不满足要求，安装失败！！" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_MissingSystemComponents").ToString();
                        MainWindowsShowInfo(currentStatus);
                        return;
                    }

                    //检测域名是否解析到当前IP上 34---36
                    functionResult = DomainResolutionCurrentIPDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //下载安装脚本安装
                    //****** "正在安装Trojan......" ******
                    SetUpProgressBarProcessing(37);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + "Trojan......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = $"curl -o /tmp/trojan-quickstart.sh https://raw.githubusercontent.com/trojan-gfw/trojan-quickstart/master/trojan-quickstart.sh";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"yes | bash /tmp/trojan-quickstart.sh";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"rm -f /tmp/trojan-quickstart.sh";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //程序是否安装成功检测并设置开机启动 41--43
                    functionResult = SoftInstalledSuccessOrFail(client, "trojan", @"/usr/local/bin/trojan");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //****** "安装完毕，上传配置文件......" ******
                    SetUpProgressBarProcessing(44);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadSoftConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"mv /usr/local/etc/trojan/config.json /usr/local/etc/trojan/config.json.1";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string serverConfig = $"{pwdir}" + @"TemplateConfg\trojan\trojan_server_config.json";  //服务端配置文件
                    string upLoadPath = @"/usr/local/etc/trojan/config.json"; //服务端文件位置

                    //生成服务端配置
                    using (StreamReader reader = File.OpenText(serverConfig))
                    {
                        JObject serverJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                        //设置密码
                        serverJson["password"][0] = ReceiveConfigurationParameters[2];

                        //设置Caddy随机监听的端口，用于Trojan-go,Trojan,V2Ray vless TLS
                        //Random random = new Random();
                        randomCaddyListenPort = GetRandomPort();

                        //设置转发到Caddy的随机监听端口
                        serverJson["remote_port"] = randomCaddyListenPort;

                        using (StreamWriter sw = new StreamWriter(@"config.json"))
                        {
                            sw.Write(serverJson.ToString());
                        }
                    }
                    UploadConfig(connectionInfo, @"config.json", upLoadPath);

                    File.Delete(@"config.json");

                    //acme.sh安装与申请证书 51--57
                    functionResult = AcmeShInstall(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //****** "安装证书到Trojan......" ******
                    SetUpProgressBarProcessing(58);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoft").ToString() + "Trojan......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = $"/root/.acme.sh/acme.sh  --installcert  -d {ReceiveConfigurationParameters[4]}  --certpath /usr/local/etc/trojan/trojan_ssl.crt --keypath /usr/local/etc/trojan/trojan_ssl.key  --capath  /usr/local/etc/trojan/trojan_ssl.crt  --reloadcmd  \"systemctl restart trojan\"";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"if [ ! -f ""/usr/local/etc/trojan/trojan_ssl.key"" ]; then echo ""0""; else echo ""1""; fi | head -n 1";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("1") == true)
                    {
                        //****** "证书成功安装到Trojan！" ******
                        SetUpProgressBarProcessing(60);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoftOK").ToString() + "Trojan!";
                        MainWindowsShowInfo(currentStatus);
                    }
                    else
                    {
                        //****** "证书安装到Trojan失败，原因未知，可以向开发者提问！" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_IntallCertToSoftFail").ToString() +
                                        "Trojan" +
                                        Application.Current.FindResource("DisplayInstallInfo_InstallCertFailAsk").ToString();
                        MainWindowsShowInfo(currentStatus);
                        return;
                    }

                    //设置证书权限
                    sshShellCommand = @"chmod 644 /usr/local/etc/trojan/trojan_ssl.key";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //Caddy安装 61--66
                    functionResult = CaddyInstall(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //****** "上传Caddy配置文件......" ******
                    SetUpProgressBarProcessing(67);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.bak";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string caddyConfig = $"{pwdir}" + @"TemplateConfg\trojan\trojan.caddyfile";
                    upLoadPath = @"/etc/caddy/Caddyfile";

                    UploadConfig(connectionInfo, caddyConfig, upLoadPath);

                    //设置Caddy监听的随机端口
                    string randomCaddyListenPortStr = randomCaddyListenPort.ToString();

                    sshShellCommand = $"sed -i 's/8800/{randomCaddyListenPortStr}/' {upLoadPath}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //设置域名

                    sshShellCommand = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}/g' {upLoadPath}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //设置伪装网站
                    if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7]) == false)
                    {
                        sshShellCommand = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }
                    //****** "Caddy配置文件上传成功,OK!" ******
                    SetUpProgressBarProcessing(70);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfigOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //程序启动检测Caddy
                    functionResult = SoftStartDetect(client, "caddy", @"/usr/bin/caddy");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //程序启动检测Trojan
                    functionResult = SoftStartDetect(client, "trojan", @"/usr/local/bin/trojan");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测BBR，满足条件并启动 90--95
                    functionResult = DetectBBRandEnable(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    client.Disconnect();//断开服务器ssh连接

                    //****** "生成客户端配置......" ******
                    SetUpProgressBarProcessing(96);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_GenerateClientConfig").ToString();
                    MainWindowsShowInfo(currentStatus);
                    if (!Directory.Exists("trojan_config"))//如果不存在就创建file文件夹　　             　　              
                    {
                        Directory.CreateDirectory("trojan_config");//创建该文件夹　　   
                    }

                    string clientConfig = $"{pwdir}" + @"TemplateConfg\trojan\trojan_client_config.json";   //生成的客户端配置文件
                    using (StreamReader reader = File.OpenText(clientConfig))
                    {
                        JObject clientJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                        clientJson["remote_addr"] = ReceiveConfigurationParameters[4];
                        clientJson["remote_port"] = int.Parse(ReceiveConfigurationParameters[1]);
                        clientJson["password"][0] = ReceiveConfigurationParameters[2];
                       
                        using (StreamWriter sw = new StreamWriter(@"trojan_config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }

                    //****** "Trojan安装成功,祝你玩的愉快！！" ******
                    SetUpProgressBarProcessing(100);
                    currentStatus = "Trojan" + Application.Current.FindResource("DisplayInstallInfo_ProxyInstalledOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //显示服务端连接参数
                    proxyType = "Trojan";

                    ResultClientInformation resultClientInformation = new ResultClientInformation();
                    resultClientInformation.ShowDialog();
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "安装失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }
        
        //检测升级远程主机Trojan版本传递参数
        private void ButtonUpdateTrojan_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }
            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => UpdateTojan(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        
        //升级Trojan主程序
        private void UpdateTojan(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                    //******"检测系统是否已经安装Trojan......"******
                    SetUpProgressBarProcessing(20);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "Trojan......";
                    MainWindowsShowInfo(currentStatus);


                    //string cmdTestTrojanInstalled = @"find / -name trojan";
                    //string resultCmdTestTrojanInstalled = client.RunCommand(cmdTestTrojanInstalled).Result;
                    sshShellCommand = @"find / -name trojan";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/local/bin/trojan") == false)
                    {
                        //******"退出！原因：远程主机未安装Trojan"******
                        MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorUpgradeSoftNotInstall").ToString() + "Trojan!");
                        //******"退出！原因：远程主机未安装Trojan"******
                        currentStatus = Application.Current.FindResource("MessageBoxShow_ErrorUpgradeSoftNotInstall").ToString() + "Trojan!";
                        MainWindowsShowInfo(currentStatus);

                        client.Disconnect();
                        return;

                    }

                    SetUpProgressBarProcessing(40);

                    //获取当前安装的版本
                    //string sshcmd = @"echo ""$(/usr/local/bin/trojan -v 2>&1)"" | head -n 1 | cut -d "" "" -f4";
                    //string trojanCurrentVersion = client.RunCommand(sshcmd).Result;//不含字母v
                    sshShellCommand = @"echo ""$(/usr/local/bin/trojan -v 2>&1)"" | head -n 1 | cut -d "" "" -f4";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string trojanCurrentVersion = currentShellCommandResult;//不含字母v
              

                    //sshcmd = @"curl -fsSL https://api.github.com/repos/trojan-gfw/trojan/releases/latest | grep tag_name | sed -E 's/.*""v(.*)"".*/\1/'";
                    //获取最新版本

                    sshShellCommand = @"curl -fsSL https://api.github.com/repos/trojan-gfw/trojan/releases/latest | grep tag_name | sed -E 's/.*""v(.*)"".*/\1/'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string trojanNewVersion = currentShellCommandResult;//不含字母v

                    if (trojanNewVersion.Equals(trojanCurrentVersion) == false)
                    {
                        MessageBoxResult messageBoxResult = MessageBox.Show(
                             //****** "远程主机当前版本为：v" ******
                             Application.Current.FindResource("DisplayInstallInfo_CurrentVersion").ToString() +
                             $"{trojanCurrentVersion}\n" +
                             //****** "最新版本为：" ******
                             Application.Current.FindResource("DisplayInstallInfo_NewVersion").ToString() +
                             $"{trojanNewVersion}\n" +
                             //****** "是否升级为最新版本？" ******
                             Application.Current.FindResource("DisplayInstallInfo_IsOrNoUpgradeNewVersion").ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            //****** "正在升级到最新版本......" ******
                            SetUpProgressBarProcessing(60);
                            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartUpgradeNewVersion").ToString();
                            MainWindowsShowInfo(currentStatus);

                            //****** "备份Trojan配置文件......" ******
                            SetUpProgressBarProcessing(80);
                            currentStatus = Application.Current.FindResource("DisplayInstallInfo_BackTrojanConfig").ToString();
                            MainWindowsShowInfo(currentStatus);

                            //string sshcmd = @"mv /usr/local/etc/trojan/config.json /usr/local/etc/trojan/config.json.bak";
                            //client.RunCommand(sshcmd);
                            sshShellCommand = @"mv /usr/local/etc/trojan/config.json /usr/local/etc/trojan/config.json.bak";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            //升级Trojan主程序
                            //client.RunCommand("curl -o /tmp/trojan-quickstart.sh https://raw.githubusercontent.com/trojan-gfw/trojan-quickstart/master/trojan-quickstart.sh");
                            //client.RunCommand("yes | bash /tmp/trojan-quickstart.sh");
                            sshShellCommand = $"curl -o /tmp/trojan-quickstart.sh https://raw.githubusercontent.com/trojan-gfw/trojan-quickstart/master/trojan-quickstart.sh";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            sshShellCommand = @"yes | bash /tmp/trojan-quickstart.sh";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            sshShellCommand = @"rm -f /tmp/trojan-quickstart.sh";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            //sshcmd = @"echo ""$(/usr/local/bin/trojan -v 2>&1)"" | head -n 1 | cut -d "" "" -f4";
                            sshShellCommand = @"echo ""$(/usr/local/bin/trojan -v 2>&1)"" | head -n 1 | cut -d "" "" -f4";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            trojanCurrentVersion = currentShellCommandResult;//不含字母v
                            //trojanCurrentVersion = client.RunCommand(sshcmd).Result;//不含字母v
                            if (trojanNewVersion.Equals(trojanCurrentVersion) == true)
                            {
                                //****** "恢复Trojan配置文件......" ******
                                SetUpProgressBarProcessing(90);
                                currentStatus = Application.Current.FindResource("DisplayInstallInfo_RestoreTrojanConfig").ToString();
                                MainWindowsShowInfo(currentStatus);

                                //sshcmd = @"rm -f /usr/local/etc/trojan/config.json";
                                //client.RunCommand(sshcmd);
                                sshShellCommand = @"rm -f /usr/local/etc/trojan/config.json";
                                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                                //sshcmd = @"mv /usr/local/etc/trojan/config.json.bak /usr/local/etc/trojan/config.json";
                                //client.RunCommand(sshcmd);
                                sshShellCommand = @"mv /usr/local/etc/trojan/config.json.bak /usr/local/etc/trojan/config.json";
                                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                                //****** "升级成功！当前已是最新版本！" ******
                                SetUpProgressBarProcessing(100);
                                MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionOK").ToString());
                                //****** "升级成功！当前已是最新版本！" ******
                                currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionOK").ToString();
                                MainWindowsShowInfo(currentStatus);

                            }
                            else
                            {
                                //****** "升级失败，原因未知，请向开发者提问！" ******
                                MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionFail").ToString());
                                //****** "升级失败，原因未知，请向开发者提问！" ******
                                currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeNewVersionFail").ToString();
                                MainWindowsShowInfo(currentStatus);
                            }
                        }

                        else
                        {
                            //****** "升级取消，退出!" ******
                            currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeVersionCancel").ToString();
                            MainWindowsShowInfo(currentStatus);

                            client.Disconnect();
                            return;
                        }
                    }
                    else
                    {
                        //****** "远程主机当前已是最新版本：" ******
                        SetUpProgressBarProcessing(100);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_IsNewVersion").ToString() +
                            $"{trojanNewVersion}\n" +
                            //******  "无需升级！退出！" ******
                            Application.Current.FindResource("DisplayInstallInfo_NotUpgradeVersion").ToString();
                        MessageBox.Show(currentStatus);
                        MainWindowsShowInfo(currentStatus);
                    }

                    client.Disconnect();
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);
                //****** "主机登录失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }

        //更新Trojan的密码
        private void ButtonTrojanPassword_Click(object sender, RoutedEventArgs e)
        {
            TextBoxTrojanPassword.Text = RandomUUID();
        }
        #endregion

        #region NaiveProxy相关

        //NaiveProxy一键安装开始传递参数
        private void ButtonNavieSetUp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxNaiveHostDomain.Text) == true)
            {
                //****** "域名不能为空，请检查相关参数设置！" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_DomainNotEmpty").ToString());
                return;
            }

            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }
         
            //传递参数
            ReceiveConfigurationParameters[4] = TextBoxNaiveHostDomain.Text;//传递域名
            ReceiveConfigurationParameters[3] = TextBoxNaiveUser.Text;//传递用户名
            ReceiveConfigurationParameters[2] = TextBoxNaivePassword.Text;//传递密码
            ReceiveConfigurationParameters[7] = TextBoxNaiveSites.Text;//传递伪装网站
            if (TextBoxNaiveSites.Text.ToString().Length >= 7)
            {
                string testDomain = TextBoxNaiveSites.Text.Substring(0, 7);
                if (String.Equals(testDomain, "https:/") || String.Equals(testDomain, "http://"))
                {
                    //MessageBox.Show(testDomain);
                    ReceiveConfigurationParameters[7] = TextBoxNaiveSites.Text.Replace("/", "\\/");
                }
                else
                {
                    ReceiveConfigurationParameters[7] = "http:\\/\\/" + TextBoxNaiveSites.Text;
                }
            }
            //启动布署进程
            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => StartSetUpNaive(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        //登录远程主机布署NaiveProxy程序
        private void StartSetUpNaive(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);

                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测是否已安装代理 8--10
                    functionResult = SoftInstalledIsNoYes(client, "caddy", @"/usr/bin/caddy");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测关闭Selinux及系统组件是否齐全（apt/yum/dnf/systemctl）11--30
                    //安装依赖软件，检测端口，防火墙开启端口
                    functionResult = ShutDownSelinuxAndSysComponentsDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测域名是否解析到当前IP上 34---36
                    functionResult = DomainResolutionCurrentIPDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    

                    //****** "系统环境检测完毕，符合安装要求,开始布署......" ******
                    SetUpProgressBarProcessing(60);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstalling").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //Caddy安装与检测安装是否成功 61--66
                    functionResult = CaddyInstall(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                    //使用带插件的Caddy替换
                    //****** "正在为NaiveProxy升级服务端！" ******
                    //SetUpProgressBarProcessing(76);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeNaiveProxy").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = $"curl -o /tmp/caddy.zip https://raw.githubusercontent.com/proxysu/Resources/master/Caddy2/caddy20200816.zip";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"yes | unzip -o /tmp/caddy.zip";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"chmod +x ./caddy";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"systemctl stop caddy;rm -f /usr/bin/caddy";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"cp caddy /usr/bin/";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"rm -f  /tmp/caddy.zip caddy";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //****** "升级完毕，OK！" ******
                    //SetUpProgressBarProcessing(79);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UpgradeNaiveProxyOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //****** "上传Caddy配置文件......" ******
                    SetUpProgressBarProcessing(67);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //生成服务端配置
                    string caddyConfig = $"{pwdir}" + @"TemplateConfg\naive\naive_server_config.json";
                    using (StreamReader reader = File.OpenText(caddyConfig))
                    {
                        JObject serverJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                        serverJson["apps"]["http"]["servers"]["srv0"]["routes"][0]["handle"][0]["auth_user"] = ReceiveConfigurationParameters[3];//----用户名
                        serverJson["apps"]["http"]["servers"]["srv0"]["routes"][0]["handle"][0]["auth_pass"] = ReceiveConfigurationParameters[2]; //----密码

                        serverJson["apps"]["http"]["servers"]["srv0"]["routes"][1]["match"][0]["host"][0] = ReceiveConfigurationParameters[4]; //----域名

                        serverJson["apps"]["http"]["servers"]["srv0"]["tls_connection_policies"][0]["match"]["sni"][0] = ReceiveConfigurationParameters[4];  //----域名

                        serverJson["apps"]["tls"]["automation"]["policies"][0]["subjects"][0] = ReceiveConfigurationParameters[4];  //-----域名
                        serverJson["apps"]["tls"]["automation"]["policies"][0]["issuer"]["email"] = $"user@{ReceiveConfigurationParameters[4]}";  //-----邮箱
                        //保存配置文件
                        using (StreamWriter sw = new StreamWriter(@"config.json"))
                        {
                            sw.Write(serverJson.ToString());
                        }
                    }
                    string upLoadPath = @"/etc/caddy/config.json";
                    UploadConfig(connectionInfo, @"config.json", upLoadPath);

                    File.Delete(@"config.json");

                    //****** Caddy配置文件上传成功,OK! ******
                    SetUpProgressBarProcessing(70);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfigOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"sed -i 's/Caddyfile/config.json/' /lib/systemd/system/caddy.service";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"systemctl daemon-reload";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //程序启动检测Caddy
                    functionResult = SoftStartDetect(client, "caddy", @"/usr/bin/caddy");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    ////设置伪装网站
                    //if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7]) == false)
                    //{
                    //    sshCmd = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
                    //    //MessageBox.Show(sshCmd);
                    //    client.RunCommand(sshCmd);
                    //}
                    //Thread.Sleep(2000);

                    //****** "正在优化网络参数......" ******
                    SetUpProgressBarProcessing(80);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_OptimizeNetwork").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //优化网络参数
                    sshShellCommand = @"bash -c 'echo ""fs.file-max = 51200"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.core.rmem_max = 67108864"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.core.wmem_max = 67108864"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.core.rmem_default = 65536"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.core.wmem_default = 65536"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.core.netdev_max_backlog = 4096"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.core.somaxconn = 4096"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_syncookies = 1"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_tw_reuse = 1"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_tw_recycle = 0"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_fin_timeout = 30"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_keepalive_time = 1200"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.ip_local_port_range = 10000 65000"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_max_syn_backlog = 4096"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_max_tw_buckets = 5000"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_rmem = 4096 87380 67108864"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_wmem = 4096 65536 67108864"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_mtu_probing = 1"" >> /etc/sysctl.conf'";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"sysctl -p";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //****** "优化网络参数,OK!" ******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_OptimizeNetworkOK").ToString(); ;
                    MainWindowsShowInfo(currentStatus);

                    //检测BBR，满足条件并启动 90--95
                    functionResult = DetectBBRandEnable(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    client.Disconnect();//断开服务器ssh连接

                    //****** "生成客户端配置......" ******
                    SetUpProgressBarProcessing(96);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_GenerateClientConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    if (!Directory.Exists("naive_config"))//如果不存在就创建file文件夹　　             　　              
                    {
                        Directory.CreateDirectory("naive_config");//创建该文件夹　　   
                    }

                    string clientConfig = $"{pwdir}" + @"TemplateConfg\naive\naive_client_config.json";   //生成的客户端配置文件
                    using (StreamReader reader = File.OpenText(clientConfig))
                    {
                        JObject clientJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                        clientJson["proxy"] = $"https://{ReceiveConfigurationParameters[3]}:{ReceiveConfigurationParameters[2]}@{ReceiveConfigurationParameters[4]}";
                        using (StreamWriter sw = new StreamWriter(@"naive_config\config.json"))
                        {
                            sw.Write(clientJson.ToString());
                        }
                    }
                    

                    //****** "NaiveProxy安装成功,祝你玩的愉快！！" ******
                    SetUpProgressBarProcessing(100);
                    currentStatus = "NaiveProxy" + Application.Current.FindResource("DisplayInstallInfo_ProxyInstalledOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //显示服务端连接参数
                    proxyType = "NaiveProxy";
                    ResultClientInformation resultClientInformation = new ResultClientInformation();
                    resultClientInformation.ShowDialog();
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "安装失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }

        //更新NaiveProxy的密码
        private void ButtonNaivePassword_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNaivePassword.Text = RandomUUID();
        }
        
        //生成随机UUID
        private string RandomUUID()
        {
            Guid uuid = Guid.NewGuid();
            //TextBoxNaivePassword.Text = uuid.ToString();
            return uuid.ToString();
        }
        
        //NaiveProxy产生随机用户名
        private string RandomUserName()
        {
            Random random = new Random();
            int randomSerialNum = random.Next(0, 4);
            Guid uuid = Guid.NewGuid();
            string[] pathArray = uuid.ToString().Split('-');
            string path = pathArray[randomSerialNum];
            return path;
            // TextBoxPath.Text = $"/{path}";
            //MessageBox.Show(path);
        }
        
        //NaiveProxy更改用户名，随机方式
        private void ButtonNaiveUser_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNaiveUser.Text = RandomUserName();
        }

        #endregion

        #region SSR+TLS+Caddy相关

        //SSR参数传递
        private void ButtonSSRSetUp_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }
            //清空参数空间
            for (int i = 0; i != ReceiveConfigurationParameters.Length; i++)

            {
                ReceiveConfigurationParameters[i] = "";
            }
            if (string.IsNullOrEmpty(TextBoxSSRHostDomain.Text.ToString()) == true)
            {
                //****** "域名不能为空，请检查相关参数设置！" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_DomainNotEmpty").ToString());
                return;
            }

            //传递域名
            ReceiveConfigurationParameters[4] = TextBoxSSRHostDomain.Text.ToString();
            //传递伪装网站
            ReceiveConfigurationParameters[7] = TextBoxSSRSites.Text.ToString();
            //处理伪装网站域名中的前缀
            if (TextBoxSSRSites.Text.ToString().Length >= 7)
            {
                string testDomain = TextBoxSSRSites.Text.Substring(0, 7);
                if (String.Equals(testDomain, "https:/") || String.Equals(testDomain, "http://"))
                {
                    //MessageBox.Show(testDomain);
                    ReceiveConfigurationParameters[7] = TextBoxSSRSites.Text.Replace("/", "\\/");
                }
                else
                {
                    ReceiveConfigurationParameters[7] = "http:\\/\\/" + TextBoxSSRSites.Text;
                }
            }
            //传递服务端口
            ReceiveConfigurationParameters[1] = "443";
            //传递密码(uuid)
            ReceiveConfigurationParameters[2] = TextBoxSSRPassword.Text.ToString();


            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => StartSetUpSSR(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        //登录远程主机布署SSR+TLS+Caddy程序
        private void StartSetUpSSR(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测是否已安装代理 8--10
                    functionResult = SoftInstalledIsNoYes(client, "server.py", @"/usr/local/shadowsocks/server.py");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测关闭Selinux及系统组件是否齐全（apt/yum/dnf/systemctl）11--30
                    //安装依赖软件，检测端口，防火墙开启端口
                    functionResult = ShutDownSelinuxAndSysComponentsDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测域名是否解析到当前IP上 34---36
                    functionResult = DomainResolutionCurrentIPDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                    //****** "系统环境检测完毕，符合安装要求,开始布署......" ******
                    SetUpProgressBarProcessing(37);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstalling").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //下载安装脚本安装
                    //****** "正在安装SSR......" ******
                    SetUpProgressBarProcessing(38);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + "SSR......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = $"curl -o /tmp/ssr.sh https://raw.githubusercontent.com/proxysu/shellscript/master/ssr/ssr.sh";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"yes | bash /tmp/ssr.sh";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"rm -f /tmp/ssr.sh";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //程序是否安装成功检测并设置开机启动 41--43
                    functionResult = SoftInstalledSuccessOrFail(client, "server.py", @"/usr/local/shadowsocks/server.py");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    
                    //****** "安装完毕，上传配置文件......" ******
                    SetUpProgressBarProcessing(44);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadSoftConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //生成服务端配置
                    string upLoadPath = @"/etc/shadowsocks.json";

                    //设置指向Caddy监听的随机端口
                    randomCaddyListenPort = GetRandomPort();
                    string randomSSRListenPortStr = randomCaddyListenPort.ToString();

                    sshShellCommand = $"sed -i 's/8800/{randomSSRListenPortStr}/' {upLoadPath}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //设置密码
                    sshShellCommand = $"sed -i 's/##password##/{ReceiveConfigurationParameters[2]}/' {upLoadPath}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //Caddy安装与检测安装是否成功 61--66
                    functionResult = CaddyInstall(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //****** "上传Caddy配置文件......" ******
                    SetUpProgressBarProcessing(67);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.bak";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string caddyConfig = $"{pwdir}" + @"TemplateConfg\ssr\ssr_tls.caddyfile";
                    upLoadPath = @"/etc/caddy/Caddyfile";

                    UploadConfig(connectionInfo, caddyConfig, upLoadPath);

                    //设置Caddy监听的随机端口
                    string randomCaddyListenPortStr = randomCaddyListenPort.ToString();

                    sshShellCommand = $"sed -i 's/8800/{randomCaddyListenPortStr}/' {upLoadPath}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //设置域名
                    sshShellCommand = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}/g' {upLoadPath}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    //设置伪装网站
                    if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7]) == false)
                    {
                        sshShellCommand = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }
                    //****** "Caddy配置文件上传成功,OK!" ******
                    SetUpProgressBarProcessing(70);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfigOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //程序启动检测Caddy
                    functionResult = SoftStartDetect(client, "caddy", @"/usr/bin/caddy");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //程序启动检测SSR
                    functionResult = SoftStartDetect(client, "ssr", @"/usr/bin/ssr");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测BBR，满足条件并启动 90--95
                    functionResult = DetectBBRandEnable(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    client.Disconnect();//断开服务器ssh连接

                    //****** "生成客户端配置......" ******
                    SetUpProgressBarProcessing(96);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_GenerateClientConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    if (!Directory.Exists("ssr_config"))//如果不存在就创建file文件夹　　             　　              
                    {
                        Directory.CreateDirectory("ssr_config");//创建该文件夹　　   
                    }


                    //****** "SSR+TLS+Caddy安装成功,祝你玩的愉快！！" ******
                    SetUpProgressBarProcessing(100);
                    currentStatus = "SSR+TLS+Caddy" + Application.Current.FindResource("DisplayInstallInfo_ProxyInstalledOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //显示服务端连接参数
                    proxyType = "SSR";

                    ResultClientInformation resultClientInformation = new ResultClientInformation();
                    resultClientInformation.ShowDialog();
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "安装失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }

        //更新SSR的密码
        private void ButtonSSRPassword_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSSRPassword.Text = RandomUUID();
        }


        #endregion

        #region SS相关

        //打开SS Plugin设置窗口
        private void ButtonTemplateConfigurationSS_Click(object sender, RoutedEventArgs e)
        {
            //清空初始化模板参数
            for (int i = 0; i != ReceiveConfigurationParameters.Length; i++)

            {
                ReceiveConfigurationParameters[i] = "";
            }
            SSpluginWindow windowTemplateConfigurationSS = new SSpluginWindow();
            windowTemplateConfigurationSS.Closed += windowTemplateConfigurationSSClosed;
            windowTemplateConfigurationSS.ShowDialog();
        }
        //SS Plugin设置窗口关闭后，触发事件，将所选的方案与其参数显示在UI上
        private void windowTemplateConfigurationSSClosed(object sender, System.EventArgs e)
        {
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[0]) == true)
            {
                //显示"未选择方案！"
                TextBlockCurrentlySelectedPlanSS.Text = Application.Current.FindResource("TextBlockCurrentlySelectedPlanNo").ToString();

                TextBlockShowPortSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanPortSS.Visibility = Visibility.Hidden;

                TextBlockShowUUIDSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanUUIDSS.Visibility = Visibility.Hidden;

                TextBlockShowMethodSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanMethodSS.Visibility = Visibility.Hidden;

                TextBlockShowDomainSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanDomainSS.Visibility = Visibility.Hidden;

                TextBlockShowPathSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanPathSS.Visibility = Visibility.Hidden;

                TextBlockShowFakeWebsiteSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanFakeWebsite.Visibility = Visibility.Hidden;
                return;
            }
            TextBlockCurrentlySelectedPlanSS.Text = ReceiveConfigurationParameters[8];              //所选方案名称
            TextBlockCurrentlySelectedPlanPortSS.Text = ReceiveConfigurationParameters[1];          //服务器端口
            TextBlockCurrentlySelectedPlanUUIDSS.Text = ReceiveConfigurationParameters[2];          //密码
            TextBlockCurrentlySelectedPlanMethodSS.Text = ReceiveConfigurationParameters[3];        //加密方法
            TextBlockCurrentlySelectedPlanDomainSS.Text = ReceiveConfigurationParameters[4];        //域名
            TextBlockCurrentlySelectedPlanPathSS.Text = ReceiveConfigurationParameters[6];          //WebSocket Path
            TextBlockCurrentlySelectedPlanFakeWebsite.Text = ReceiveConfigurationParameters[7];     //伪装网站

            if (String.Equals(ReceiveConfigurationParameters[0], "NonePluginSS")
                || String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpWebSS")
                || String.Equals(ReceiveConfigurationParameters[0], "WebSocketSS")
                || String.Equals(ReceiveConfigurationParameters[0], "KcptunPluginSS")
                )
            {
                //显示端口
                TextBlockShowPortSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanPortSS.Visibility = Visibility.Visible;
                //显示密码
                TextBlockShowUUIDSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanUUIDSS.Visibility = Visibility.Visible;
                //显示加密方式
                TextBlockShowMethodSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanMethodSS.Visibility = Visibility.Visible;
                //隐藏域名
                TextBlockShowDomainSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanDomainSS.Visibility = Visibility.Hidden;
                //隐藏WebSocket路径
                TextBlockShowPathSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanPathSS.Visibility = Visibility.Hidden;
                //隐藏伪装网站
                TextBlockShowFakeWebsiteSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanFakeWebsiteSS.Visibility = Visibility.Hidden;

            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpsWebSS")
                || String.Equals(ReceiveConfigurationParameters[0], "QuicSS")
                || String.Equals(ReceiveConfigurationParameters[0], "GoQuietPluginSS")
                || String.Equals(ReceiveConfigurationParameters[0], "CloakPluginSS")
                )
            {
                //显示端口
                TextBlockShowPortSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanPortSS.Visibility = Visibility.Visible;
                //显示密码
                TextBlockShowUUIDSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanUUIDSS.Visibility = Visibility.Visible;
                //显示加密方式
                TextBlockShowMethodSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanMethodSS.Visibility = Visibility.Visible;
                //显示域名
                TextBlockShowDomainSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanDomainSS.Visibility = Visibility.Visible;
                //隐藏WebSocket路径
                TextBlockShowPathSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanPathSS.Visibility = Visibility.Hidden;
                //隐藏伪装网站
                TextBlockShowFakeWebsiteSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanFakeWebsiteSS.Visibility = Visibility.Hidden;
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSWebFrontSS"))
            {
                //显示端口
                TextBlockShowPortSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanPortSS.Visibility = Visibility.Visible;
                //显示密码
                TextBlockShowUUIDSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanUUIDSS.Visibility = Visibility.Visible;
                //显示加密方式
                TextBlockShowMethodSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanMethodSS.Visibility = Visibility.Visible;
                //显示域名
                TextBlockShowDomainSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanDomainSS.Visibility = Visibility.Visible;
                //隐藏WebSocket路径
                TextBlockShowPathSS.Visibility = Visibility.Visible;
                TextBlockCurrentlySelectedPlanPathSS.Visibility = Visibility.Visible;
                //隐藏伪装网站
                TextBlockShowFakeWebsiteSS.Visibility = Visibility.Hidden;
                TextBlockCurrentlySelectedPlanFakeWebsiteSS.Visibility = Visibility.Hidden;
            }
            
        }

        //传送SS参数,启动SS安装进程
        private void Button_LoginSS_Click(object sender, RoutedEventArgs e)

        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }

            //读取模板配置

            //生成客户端配置时，连接的服务主机的IP或者域名
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[4]) == true)
            {
                ReceiveConfigurationParameters[4] = TextBoxHost.Text.ToString();
                testDomain = false;
            }
            //选择模板
            if (String.IsNullOrEmpty(ReceiveConfigurationParameters[0]) == true)
            {
                //******"请先选择配置模板！"******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ChooseTemplate").ToString());
                return;
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "NonePluginSS")
                || String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpWebSS")
                || String.Equals(ReceiveConfigurationParameters[0], "WebSocketSS")
                || String.Equals(ReceiveConfigurationParameters[0], "KcptunPluginSS")
                )
            {
                testDomain = false;
            }
            else if (String.Equals(ReceiveConfigurationParameters[0], "GoQuietPluginSS")
                || String.Equals(ReceiveConfigurationParameters[0], "CloakPluginSS")
                || String.Equals(ReceiveConfigurationParameters[0], "QuicSS")
                || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSWebFrontSS")
                || String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpsWebSS")
                )
            {
                testDomain = true;
            }

            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => StartSetUpSS(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

        }

        //登录远程主机布署SS程序
        private void StartSetUpSS(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测是否已安装代理 8--10
                    functionResult = SoftInstalledIsNoYes(client, "ss-server", @"/usr/local/bin/ss-server");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测关闭Selinux及系统组件是否齐全（apt/yum/dnf/systemctl）11--30
                    //安装依赖软件，检测端口，防火墙开启端口
                    functionResult = ShutDownSelinuxAndSysComponentsDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //如果使用是TLS模式，需要检测域名解析是否正确
                    if (testDomain == true)
                    {
                        //检测域名是否解析到当前IP上 34---36
                        functionResult = DomainResolutionCurrentIPDetect(client);
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    }
                    //****** "系统环境检测完毕，符合安装要求,开始布署......" ******
                    SetUpProgressBarProcessing(37);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstalling").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //下载安装脚本安装
                    //****** "正在安装SS，使用编译方式，时间稍长，请耐心等待............" ******
                    SetUpProgressBarProcessing(38);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + "SS，" + Application.Current.FindResource("DisplayInstallInfo_ExplainBuildSS").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = $"curl -o /tmp/install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/ss/ss-install.sh";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"yes | bash /tmp/install.sh";
                    TextBoxMonitorCommandResults.Dispatcher.BeginInvoke(updateMonitorAction, TextBoxMonitorCommandResults, sshShellCommand);//显示执行的命令

                    //****** "编译中,请耐心等待............" ******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_CompilingSS").ToString();
                    MainWindowsShowInfo(currentStatus);

                    Thread threadWaitSScompile = new Thread(() => MonitorCompileSSprocess());
                    threadWaitSScompile.SetApartmentState(ApartmentState.STA);
                    threadWaitSScompile.Start();

                    //开始编译。。。
                    currentShellCommandResult = client.RunCommand(sshShellCommand).Result;
                    compileSSend = true;  //中止显示"**"的线程
                    threadWaitSScompile.Abort();
                    TextBoxMonitorCommandResults.Dispatcher.BeginInvoke(updateMonitorAction, TextBoxMonitorCommandResults, currentShellCommandResult);//显示命令执行的结果

                    sshShellCommand = @"rm -f /tmp/install.sh";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //程序是否安装成功检测并设置开机启动 41--43
                    functionResult = SoftInstalledSuccessOrFail(client, "ss-server", @"/usr/local/bin/ss-server");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //****** "上传配置文件......" ******
                    SetUpProgressBarProcessing(44);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadSoftConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"mv /etc/shadowsocks-libev/config.json /etc/shadowsocks-libev/config.json.1";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //string getIpv4 = client.RunCommand(@"curl -4 ip.sb").Result;
                    //string getIpv6 = client.RunCommand(@"wget -qO- -t1 -T2 ipv6.icanhazip.com").Result;

                    //生成服务端配置

                    string serverConfig = $"{pwdir}" + @"TemplateConfg\ss\ss_server_config.json";
                    string ssPluginType = "";
                    using (StreamReader reader = File.OpenText(serverConfig))
                    {
                        JObject serverJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                        //设置服务器监听地址
                        //if (String.IsNullOrEmpty(getIpv6) == true)
                        //{
                        serverJson["server"] = "0.0.0.0";
                        //}
                        //else
                        //{
                        //    JArray serverIp = JArray.Parse(@"[""[::0]"", ""0.0.0.0""]");
                        //    serverJson["server"] = serverIp;
                        //}

                        //设置密码
                        serverJson["password"] = ReceiveConfigurationParameters[2];
                        //设置监听端口
                       serverJson["server_port"]= int.Parse(ReceiveConfigurationParameters[1]);
                        //设置加密方式
                        serverJson["method"] = ReceiveConfigurationParameters[3];
                        //产生伪装Web的监听端口
                        randomCaddyListenPort = GetRandomPort();
                        
                        string failoverPort = randomCaddyListenPort.ToString();
                        //obfs http模式
                        if (String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpWebSS") == true)
                        {
                            serverJson["plugin"] = @"obfs-server";
                            serverJson["plugin_opts"] = $"obfs=http;failover=127.0.0.1:{failoverPort}";
                            //客户端项
                            ReceiveConfigurationParameters[5] = @"obfs-local";
                            ReceiveConfigurationParameters[9] = "obfs=http;obfs-host=www.bing.com";

                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpsWebSS") == true)
                        {
                            serverJson["plugin"] = @"obfs-server";
                            serverJson["plugin_opts"] = $"obfs=tls;failover=127.0.0.1:{failoverPort}";
                            //客户端项
                            ReceiveConfigurationParameters[5] = @"obfs-local";
                            ReceiveConfigurationParameters[9] = $"obfs=tls;obfs-host={ReceiveConfigurationParameters[4]}";
                            
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketSS") == true)
                        {
                            serverJson["plugin"] = @"v2ray-plugin";
                            serverJson["plugin_opts"] = $"server";
                            //客户端项
                            ReceiveConfigurationParameters[5] = @"v2ray-plugin";
                            ReceiveConfigurationParameters[9] = "";
                            
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSWebFrontSS") == true)
                        {
                            serverJson["server_port"] = 10000;
                            serverJson["plugin"] = @"v2ray-plugin";
                            serverJson["plugin_opts"] = $"server;host={ReceiveConfigurationParameters[4]};path={ReceiveConfigurationParameters[6]}";
                            
                            //客户端项
                            ReceiveConfigurationParameters[5] = @"v2ray-plugin";
                            ReceiveConfigurationParameters[9] = $"tls;host={ReceiveConfigurationParameters[4]};path={ReceiveConfigurationParameters[6]}";
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "QuicSS") == true)
                        {
                            serverJson["mode"] = "tcp";
                            serverJson["plugin"] = @"v2ray-plugin";
                            serverJson["plugin_opts"] = $"server;mode=quic;host={ReceiveConfigurationParameters[4]}";
                            //客户端项
                            ReceiveConfigurationParameters[5] = @"v2ray-plugin";
                            ReceiveConfigurationParameters[9] = $"mode=quic;host={ReceiveConfigurationParameters[4]}";
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "KcptunPluginSS") == true)
                        {
                            serverJson["mode"] = "tcp";
                            serverJson["plugin"] = @"kcptun-plugin-server";
                            serverJson["plugin_opts"] = $"key={ReceiveConfigurationParameters[2]}";
                            //客户端项
                            ReceiveConfigurationParameters[5] = @"kcptun";
                            ReceiveConfigurationParameters[9] = $"key={ReceiveConfigurationParameters[2]}";
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "GoQuietPluginSS") == true)
                        {
                            serverJson["plugin"] = @"goquiet-plugin-server";
                            serverJson["plugin_opts"] = $"WebServerAddr=127.0.0.1:{failoverPort};Key={ReceiveConfigurationParameters[2]}";
                            //客户端项
                            ReceiveConfigurationParameters[5] = @"goquiet";
                            ReceiveConfigurationParameters[9] = $"ServerName={ReceiveConfigurationParameters[4]};Key={ReceiveConfigurationParameters[2]};Browser=chrome";
                           
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "CloakPluginSS") == true)
                        {
                            //****** "正在安装 Cloak-Plugin......" ******
                            SetUpProgressBarProcessing(48);
                            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + " Cloak-Plugin......";
                            MainWindowsShowInfo(currentStatus);

                            sshShellCommand = $"curl -o /tmp/install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/ss/ss-plugins/cloak-plugin-install.sh";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            sshShellCommand = @"yes | bash /tmp/install.sh";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            sshShellCommand = @"rm -f /tmp/install.sh";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                            //程序是否安装成功检测并设置开机启动 41--43
                            functionResult = SoftInstalledSuccessOrFail(client, "cloak-plugin-server", @"/usr/local/bin/cloak-plugin-server");
                            if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                            string bypassUID = client.RunCommand(@"/usr/local/bin/cloak-plugin-server -u").Result.TrimEnd('\r', '\n');
                            string generateKey = client.RunCommand(@"/usr/local/bin/cloak-plugin-server -k").Result.TrimEnd('\r', '\n');
                            string[] keyCloak = generateKey.Split(new char[] { ',' });
                            string publicKey  = keyCloak[0];
                            string privateKey = keyCloak[1];

                            serverJson["plugin"] = @"cloak-plugin-server";
                            serverJson["plugin_opts"] = $"BypassUID={bypassUID};PrivateKey={privateKey};RedirAddr=127.0.0.1:{failoverPort};DatabasePath=/etc/shadowsocks-libev/userinfo.db;StreamTimeout=300";
                            //客户端项
                            ReceiveConfigurationParameters[5] = @"cloak";
                            ReceiveConfigurationParameters[9] = $"UID={bypassUID};PublicKey={publicKey};Transport=direct;ServerName={ReceiveConfigurationParameters[4]};BrowserSig=chrome;NumConn=4;EncryptionMethod=plain;StreamTimeout=300";

                        }
                        ssPluginType = (string)serverJson["plugin"];
                        using (StreamWriter sw = new StreamWriter(@"config.json"))
                        {
                            sw.Write(serverJson.ToString());
                        }
                    }

                    string upLoadPath = @"/etc/shadowsocks-libev/config.json";
                    UploadConfig(connectionInfo, @"config.json", upLoadPath);

                    File.Delete(@"config.json");

                    //安装所使用的插件
                    if (String.Equals(ssPluginType, "obfs-server"))
                    {
                        //****** "正在安装 Simple-obfs Plugin......" ******
                        SetUpProgressBarProcessing(48);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + " Simple-obfs Plugin......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = $"curl -o /tmp/install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/ss/ss-plugins/obfs-install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"yes | bash /tmp/install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -f /tmp/install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //程序是否安装成功检测并设置开机启动 41--43
                        functionResult = SoftInstalledSuccessOrFail(client, "obfs-server", @"/usr/local/bin/obfs-server");
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                       
                    }
                    else if (String.Equals(ssPluginType, "v2ray-plugin"))
                    {
                        //****** "正在安装 V2Ray-Plugin......" ******
                        SetUpProgressBarProcessing(48);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + " V2Ray-Plugin......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = $"curl -o /tmp/install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/ss/ss-plugins/v2ray-plugin-install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"yes | bash /tmp/install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -f /tmp/install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //程序是否安装成功检测并设置开机启动 41--43
                        functionResult = SoftInstalledSuccessOrFail(client, "v2ray-plugin", @"/usr/local/bin/v2ray-plugin");
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    }
                    else if (String.Equals(ssPluginType, "kcptun-plugin-server"))
                    {
                        //****** "正在安装 Kcptun-Plugin......" ******
                        SetUpProgressBarProcessing(48);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + " Kcptun-Plugin......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = $"curl -o /tmp/install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/ss/ss-plugins/kcptun-plugin-install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"yes | bash /tmp/install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -f /tmp/install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //程序是否安装成功检测并设置开机启动 41--43
                        functionResult = SoftInstalledSuccessOrFail(client, "kcptun-plugin-server", @"/usr/local/bin/kcptun-plugin-server");
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    }
                    else if (String.Equals(ssPluginType, "goquiet-plugin-server"))
                    {
                        //****** "正在安装 GoQuiet-Plugin......" ******
                        SetUpProgressBarProcessing(48);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + " GoQuiet-Plugin......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = $"curl -o /tmp/install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/ss/ss-plugins/goquiet-plugin-install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"yes | bash /tmp/install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -f /tmp/install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //程序是否安装成功检测并设置开机启动 41--43
                        functionResult = SoftInstalledSuccessOrFail(client, "goquiet-plugin-server", @"/usr/local/bin/goquiet-plugin-server");
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    }
                    else if (String.Equals(ssPluginType, "cloak-plugin-server"))
                    {
                        
                    }
                

                    //如果使用v2ray-plugin Quic模式，先要安装acme.sh,申请证书
                    if (String.Equals(ReceiveConfigurationParameters[0], "QuicSS") == true )
                    {
                        //acme.sh安装与申请证书 51--57
                        functionResult = AcmeShInstall(client);
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    }

                    //如果使用Web伪装网站模式，需要安装Caddy
                    if (String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpWebSS") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpsWebSS") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSWebFrontSS") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "GoQuietPluginSS") == true
                        || String.Equals(ReceiveConfigurationParameters[0], "CloakPluginSS") == true
                        )
                    {
                        //Caddy安装 61--66
                        functionResult = CaddyInstall(client);
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                        //****** "上传Caddy配置文件......" ******
                        SetUpProgressBarProcessing(67);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfig").ToString();
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.bak";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (String.Equals( ReceiveConfigurationParameters[0], "ObfsPluginHttpWebSS") == true)
                        {
                            serverConfig = $"{pwdir}" + @"TemplateConfg\ss\ss_obfs_http_web_config.caddyfile";
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "ObfsPluginHttpsWebSS") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "GoQuietPluginSS") == true
                            || String.Equals(ReceiveConfigurationParameters[0], "CloakPluginSS") == true)
                        {
                            serverConfig = $"{pwdir}" + @"TemplateConfg\ss\ss_tls_caddy_config.caddyfile";
                        }
                        else if (String.Equals(ReceiveConfigurationParameters[0], "WebSocketTLSWebFrontSS") == true)
                        {
                            serverConfig = $"{pwdir}" + @"TemplateConfg\ss\WebSocketTLSWeb.caddyfile";
                        }

                        upLoadPath = @"/etc/caddy/Caddyfile";
                       
                        UploadConfig(connectionInfo, serverConfig, upLoadPath);

                        //设置Caddyfile文件中的tls 邮箱,在caddy2中已经不需要设置。

                        //设置Caddy监听的随机端口
                        string randomCaddyListenPortStr = randomCaddyListenPort.ToString();

                        sshShellCommand = $"sed -i 's/8800/{randomCaddyListenPortStr}/' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //设置域名
                        sshShellCommand = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}/g' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //设置Path
                        sshShellCommand = $"sed -i 's/##path##/\\{ReceiveConfigurationParameters[3]}/' {upLoadPath}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //设置伪装网站
                        if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7]) == false)
                        {
                            sshShellCommand = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
                            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        }
                        //****** "Caddy配置文件上传成功,OK!" ******
                        SetUpProgressBarProcessing(70);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_UploadCaddyConfigOK").ToString();
                        MainWindowsShowInfo(currentStatus);

                        //程序启动检测Caddy
                        functionResult = SoftStartDetect(client, "caddy", @"/usr/bin/caddy");
                        if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    }

                    //程序启动检测SS
                    functionResult = SoftStartDetect(client, "ss-server", @"/usr/local/bin/ss-server");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测BBR，满足条件并启动 90--95
                    functionResult = DetectBBRandEnable(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    
                    client.Disconnect();//断开服务器ssh连接

                    //****** "生成客户端配置......" ******
                    SetUpProgressBarProcessing(96);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_GenerateClientConfig").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //****** "SS安装成功,祝你玩的愉快！！" ******
                    SetUpProgressBarProcessing(100);
                    currentStatus = "SS" + Application.Current.FindResource("DisplayInstallInfo_ProxyInstalledOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //显示服务端连接参数
                    proxyType = "SS";
                    ResultClientInformation resultClientInformation = new ResultClientInformation();
                    resultClientInformation.ShowDialog();

                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "安装失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion
        }

        //编译SS时，中间监视窗口显示"****"不断增长进度，直到编译结束
        private static bool compileSSend = false;
        private void MonitorCompileSSprocess()
        {
            currentShellCommandResult = "**";
            while (compileSSend==false)
            {
                TextBoxMonitorCommandResults.Dispatcher.BeginInvoke(updateMonitorActionNoWrap, TextBoxMonitorCommandResults, currentShellCommandResult);//显示命令执行的结果
                Thread.Sleep(1000);
            }
        }

        #endregion

        #region MTProto相关
        //传递参数启动安装进程
        private void ButtonMtgSetUp_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }
            //清空参数空间
            for (int i = 0; i != ReceiveConfigurationParameters.Length; i++)

            {
                ReceiveConfigurationParameters[i] = "";
            }
            //传递服务器地址
            ReceiveConfigurationParameters[4] = TextBoxHost.Text.ToString();
            //传递服务端口
            ReceiveConfigurationParameters[1] = TextBoxMtgHostDomain.Text;
            //传递伪装域名
            if (String.IsNullOrEmpty(TextBoxMtgSites.Text) == true)
            {
                ReceiveConfigurationParameters[7] = "azure.microsoft.com";
            }
            else
            {
                ReceiveConfigurationParameters[7] = TextBoxMtgSites.Text;
            }
            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => StartSetUpMtg(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        //Mtg安装进程
        private void StartSetUpMtg(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;   
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(3);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测是否已安装代理 8--10
                    functionResult = SoftInstalledIsNoYes(client, "mtg", @"/usr/local/bin/mtg");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测关闭Selinux及系统组件是否齐全（apt/yum/dnf/systemctl）11--30
                    //安装依赖软件，检测端口，防火墙开启端口
                    functionResult = ShutDownSelinuxAndSysComponentsDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //检测是否为64位系统
                    sshShellCommand = @"uname -m";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    string resultCmd = currentShellCommandResult;
                    if (resultCmd.Contains("x86_64") == false)
                    {
                        //******"请在x86_64系统中安装MTProto" ******
                        MessageBox.Show(Application.Current.FindResource("MessageBoxShow_PleaseInstallSoftAtX64").ToString() + "MTProto......");
                        //****** "系统环境不满足要求，安装失败！！" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_MissingSystemComponents").ToString();
                        MainWindowsShowInfo(currentStatus);
                        return;
                    }

                    //下载安装脚本安装MTProto 37--40
                    functionResult = MTProtoInstall(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //程序是否安装成功检测并设置开机启动 41--43
                    functionResult = SoftInstalledSuccessOrFail(client, "mtg", @"/usr/local/bin/mtg");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //****** "正在启动MTProto......" ******
                    SetUpProgressBarProcessing(80);
                    //currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartSoft").ToString() + "MTProto......";
                    //MainWindowsShowInfo(currentStatus);

                    //启动MTProto服务
                    functionResult = SoftStartDetect(client, "mtg", @"/usr/local/bin/mtg");
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }


                    //检测BBR，满足条件并启动 90--95
                    functionResult = DetectBBRandEnable(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
                                       

                    //****** "生成客户端配置......" ******
                    SetUpProgressBarProcessing(96);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_GenerateClientConfig").ToString();
                    MainWindowsShowInfo(currentStatus);
                    //读取生成的代理参数
                    sshShellCommand = @"cat /usr/local/etc/mtg_info.json";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    ReceiveConfigurationParameters[9] = currentShellCommandResult;
                    if (currentShellCommandResult.Contains(@"No such file or directory"))
                    {
                        sshShellCommand = @"systemctl stop mtg";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        sshShellCommand = @"cat /usr/local/etc/mtg.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        sshShellCommand = currentShellCommandResult;
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        sshShellCommand = @"cat /usr/local/etc/mtg_info.json";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        ReceiveConfigurationParameters[9] = currentShellCommandResult;
                        sshShellCommand = @"pkill mtg";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        sshShellCommand = @"systemctl restart mtg";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }

                    client.Disconnect();//断开服务器ssh连接

                    //Thread.Sleep(1000);
                    //if (!Directory.Exists("mtproto_config"))//如果不存在就创建file文件夹　　             　　              
                    //{
                    //    Directory.CreateDirectory("mtproto_config");//创建该文件夹　　   
                    //}
                    //using (StreamWriter sw = new StreamWriter(@"mtproto_config\mtproto_info.json"))
                    //{
                    //    sw.Write(currentShellCommandResult.ToString());
                    //}

                    //****** "MTProto+TLS安装成功,祝你玩的愉快！！" ******
                    SetUpProgressBarProcessing(100);
                    currentStatus = "MTProto+TLS" + Application.Current.FindResource("DisplayInstallInfo_ProxyInstalledOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //显示服务端连接参数
                    proxyType = "MTProto";

                    ResultClientInformation resultClientInformation = new ResultClientInformation();
                    resultClientInformation.ShowDialog();
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "安装失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }
        
        //下载安装脚本安装MTProto 37--40
        //functionResult = MTProtoInstall(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool MTProtoInstall(SshClient client)
        {
            //****** "系统环境检测完毕，符合安装要求,开始布署......" ******
            SetUpProgressBarProcessing(37);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstalling").ToString();
            MainWindowsShowInfo(currentStatus);

            //下载安装脚本安装
            //****** "正在安装MTProto......" ******
            SetUpProgressBarProcessing(38);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallSoft").ToString() + "MTProto......";
            MainWindowsShowInfo(currentStatus);

            sshShellCommand = $"curl -o /tmp/mtg_install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/MTProto/mtg_install.sh";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = $"yes | bash /tmp/mtg_install.sh {ReceiveConfigurationParameters[1]} {ReceiveConfigurationParameters[7]}";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = @"rm -f /tmp/mtg_install.sh";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            SetUpProgressBarProcessing(40);

            return true;
        }

        #endregion

        #region 其他功能函数及系统工具相关
        //产生随机端口
        private int GetRandomPort()
        {
            Random random = new Random();
            return random.Next(10001, 60000);
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

        //目录已存在则生成序号递增,并返回所创建的目录路径。
        private string CreateConfigSaveDir(string upperDir, string configDir)
        {
            try
            {
                //string saveFileFolderFirst = configDir;
                int num = 1;
                //string saveFileFolder;
                //saveFileFolder = EncodeURIComponent(configDir);
                string saveFileFolder = configDir.Replace(":", "_");
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
                //string saveFileFolder = "";
                //return upperDir + @"\" + saveFileFolder;
                return upperDir;
            }

        }


        //上传配置文件
        private void UploadConfig(ConnectionInfo connectionInfo, string uploadConfig, string upLoadPath)
        {
            try
            {
                using (var sftpClient = new SftpClient(connectionInfo))
                {
                    sftpClient.Connect();
                    FileStream openUploadConfigFile = File.OpenRead(uploadConfig);
                    sftpClient.UploadFile(openUploadConfigFile, upLoadPath, true);
                    openUploadConfigFile.Close();
                    sftpClient.Disconnect();
                }

            }
            catch (Exception ex2)
            {
                MessageBox.Show("sftp" + ex2.ToString());
                //MessageBox.Show("sftp出现未知错误,上传文件失败，请重试！");
                return;
            }
        }

        //下载配置文件
        private void DownloadConfig(ConnectionInfo connectionInfo, string localConfigSavePathAndFileName, string remoteConfigPathAndFileName)
        {
            try
            {
                using (var sftpClient = new SftpClient(connectionInfo))
                {
                    sftpClient.Connect();
                    FileStream createDownloadConfig = File.Open(localConfigSavePathAndFileName, FileMode.Create);
                    sftpClient.DownloadFile(remoteConfigPathAndFileName, createDownloadConfig);
                    createDownloadConfig.Close();
                  
                    sftpClient.Disconnect();
                }

            }
            catch (Exception ex2)
            {
                MessageBox.Show("sftp" + ex2.ToString());
                //MessageBox.Show("sftp出现未知错误,下载文件失败，请重试！");
                return;
            }
        }

        #region 检测系统内核是否符合安装要求
        //private static bool DetectKernelVersion(string kernelVer)
        //{
        //    string[] linuxKernelCompared = kernelVer.Split('.');
        //    if (int.Parse(linuxKernelCompared[0]) > 2)
        //    {
        //        //MessageBox.Show($"当前系统内核版本为{result.Result}，符合安装要求！");
        //        return true;
        //    }
        //    else if (int.Parse(linuxKernelCompared[0]) < 2)
        //    {
        //        //MessageBox.Show($"当前系统内核版本为{result.Result}，V2ray要求内核为2.6.23及以上。请升级内核再安装！");
        //        return false;
        //    }
        //    else if (int.Parse(linuxKernelCompared[0]) == 2)
        //    {
        //        if (int.Parse(linuxKernelCompared[1]) > 6)
        //        {
        //            //MessageBox.Show($"当前系统内核版本为{result.Result}，符合安装要求！");
        //            return true;
        //        }
        //        else if (int.Parse(linuxKernelCompared[1]) < 6)
        //        {
        //            //MessageBox.Show($"当前系统内核版本为{result.Result}，V2ray要求内核为2.6.23及以上。请升级内核再安装！");
        //            return false;
        //        }
        //        else if (int.Parse(linuxKernelCompared[1]) == 6)
        //        {
        //            if (int.Parse(linuxKernelCompared[2]) < 23)
        //            {
        //                //MessageBox.Show($"当前系统内核版本为{result.Result}，V2ray要求内核为2.6.23及以上。请升级内核再安装！");
        //                return false;
        //            }
        //            else
        //            {
        //                //MessageBox.Show($"当前系统内核版本为{result.Result}，符合安装要求！");
        //                return true;
        //            }

        //        }
        //    }
        //    return false;

        //}
        #endregion

        //打开系统工具中的校对时间窗口
        private void ButtonProofreadTime_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }

            ProofreadTimeWindow proofreadTimeWindow = new ProofreadTimeWindow();
            ProofreadTimeWindow.ProfreadTimeReceiveConnectionInfo = connectionInfo;

            proofreadTimeWindow.ShowDialog();

        }
        //释放80/443端口
        private void ButtonClearOccupiedPorts_Click(object sender, RoutedEventArgs e)
        {
            //****** "80/443端口之一，或全部被占用，将强制停止占用80/443端口的程序?" ******
            MessageBoxResult dialogResult = MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorPortUsed").ToString(), "Stop application", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.No)
            {
                return;
            }

            ConnectionInfo testconnect = GenerateConnectionInfo();
            try
            {
                using (var client = new SshClient(testconnect))
                {
                    client.Connect();

                    //检测是否运行在root权限下
                    string testRootAuthority = client.RunCommand(@"id -u").Result;
                    if (testRootAuthority.Equals("0\n") == false)
                    {
                        //******"请使用具有root权限的账户登录主机！！"******
                        MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorRootPermission").ToString());
                        client.Disconnect();
                        return;
                    }
                    string cmdTestPort;
                    string cmdResult;
                    cmdTestPort = @"lsof -n -P -i :443 | grep LISTEN";
                    cmdResult = client.RunCommand(cmdTestPort).Result;

                    if (String.IsNullOrEmpty(cmdResult) == false)
                    {

                        string[] cmdResultArry443 = cmdResult.Split(' ');
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
                    //****** "80/443端口释放完毕！" ******
                    MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_ReleasePortOK").ToString());
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //启用BBR
        private void ButtonTestAndEnableBBR_Click(object sender, RoutedEventArgs e)
        {
            ConnectionInfo connectionInfo = GenerateConnectionInfo();
            if (connectionInfo == null)
            {
                //****** "远程主机连接信息有误，请检查!" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                return;
            }

            installationDegree = 0;
            TextBoxMonitorCommandResults.Text = "";
            Thread thread = new Thread(() => StartTestAndEnableBBR(connectionInfo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        
        //启用BBR的主要进程
        private void StartTestAndEnableBBR(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;

            //******"正在登录远程主机......"******
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    //****** "BBR测试......" ******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestBBR").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"uname -r";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string[] linuxKernelVerStrBBR = currentShellCommandResult.Split('-');

                    bool detectResultBBR = DetectKernelVersionBBR(linuxKernelVerStrBBR[0]);

                    sshShellCommand = @"sysctl net.ipv4.tcp_congestion_control | grep bbr";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    string resultCmdTestBBR = currentShellCommandResult;
                    //如果内核满足大于等于4.9，且还未启用BBR，则启用BBR
                    if (detectResultBBR == true && resultCmdTestBBR.Contains("bbr") == false)
                    {
                        //****** "正在启用BBR......" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableBBR").ToString();
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"bash -c 'echo ""net.core.default_qdisc=fq"" >> /etc/sysctl.conf'";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_congestion_control=bbr"" >> /etc/sysctl.conf'";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"sysctl -p";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }
                    else if (resultCmdTestBBR.Contains("bbr") == true)
                    {
                        //******  "BBR已经启用了！" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_BBRisEnabled").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }
                    else
                    {
                        //****** "系统不满足启用BBR的条件，启用失败！" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_BBRFailed").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }
                    client.Disconnect();//断开服务器ssh连接

                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "主机登录失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }
        
        //检测要启用BBR主要的内核版本
        private static bool DetectKernelVersionBBR(string kernelVer)
        {
            string[] linuxKernelCompared = kernelVer.Split('.');
            if (int.Parse(linuxKernelCompared[0]) > 4)
            {
                return true;
            }
            else if (int.Parse(linuxKernelCompared[0]) < 4)
            {
                return false;
            }
            else if (int.Parse(linuxKernelCompared[0]) == 4)
            {
                if (int.Parse(linuxKernelCompared[1]) >= 9)
                {
                    return true;
                }
                else if (int.Parse(linuxKernelCompared[1]) < 9)
                {
                    return false;
                }

            }
            return false;

        }

        //启动卸载代理
        private void ButtonRemoveAllSoft_Click(object sender, RoutedEventArgs e)
        {
            //******"仅支持卸载由ProxySU安装的代理软件及相关配置，请确保重要配置已备份。不支持卸载使用其他方法或脚本安装的代理。确定要卸载远程主机上的代理软件吗？"******
            string messageShow = Application.Current.FindResource("MessageBoxShow_RemoveAllSoft").ToString();
            MessageBoxResult messageBoxResult = MessageBox.Show(messageShow, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {

                ConnectionInfo connectionInfo = GenerateConnectionInfo();
                if (connectionInfo == null)
                {
                    //****** "远程主机连接信息有误，请检查!" ******
                    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                    return;
                }

                installationDegree = 0;
                TextBoxMonitorCommandResults.Text = "";
                Thread thread = new Thread(() => StartRemoveProxySoft(connectionInfo));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            
        }
        //卸载代理进程
        private void StartRemoveProxySoft(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;
            onlyIpv6 = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(5);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    functionResult = RootAuthorityDetect(client);
                    if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

                    sshShellCommand = @"command -v apt";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    getApt = !String.IsNullOrEmpty(currentShellCommandResult);

                    sshShellCommand = @"command -v dnf";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    getDnf = !String.IsNullOrEmpty(currentShellCommandResult);

                    sshShellCommand = @"command -v yum";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    getYum = !String.IsNullOrEmpty(currentShellCommandResult);

                    //设置安装软件所用的命令格式
                    if (getApt == true)
                    {
                        sshCmdUpdate = @"apt -qq update";
                        //sshCmdInstall = @"apt -y -qq install ";
                    }
                    else if (getDnf == true)
                    {
                        sshCmdUpdate = @"dnf clean all;dnf -q makecache";
                        //sshCmdInstall = @"dnf -y -q install ";
                    }
                    else if (getYum == true)
                    {
                        sshCmdUpdate = @"yum clean all; yum -q makecache";
                        //sshCmdInstall = @"yum -y -q install ";
                    }

                    //检测主机是否为纯ipv6的主机
                    onlyIpv6 = OnlyIpv6HostDetect(client);
                    if (onlyIpv6 == true)
                    {
                        SetUpNat64(client, true);
                        sshShellCommand = $"{sshCmdUpdate}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }

                    
                    //******"开始卸载......"******
                    SetUpProgressBarProcessing(10);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString() + "......";
                    MainWindowsShowInfo(currentStatus);

                    #region 卸载V2Ray

                    //******"检测系统是否已经安装V2ray......"******03
                    SetUpProgressBarProcessing(11);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "V2ray......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"find / -name v2ray";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/local/bin/v2ray") == true)
                    {
                        //******"检测到已安装V2Ray!开始卸载V2Ray......"******
                        SetUpProgressBarProcessing(12);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DiscoverProxySoft").ToString()
                            + "V2Ray!"
                            + Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString()
                            + "V2Ray......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"systemctl stop v2ray";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = $"curl -LROJ https://raw.githubusercontent.com/v2fly/fhs-install-v2ray/master/install-release.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"bash install-release.sh --remove";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl disable v2ray";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -rf /usr/local/etc/v2ray /var/log/v2ray";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -f install-release.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"find / -name v2ray";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (currentShellCommandResult.Contains("/usr/local/bin/v2ray") == true)
                        {
                            //******"V2Ray卸载失败！请向开发者问询！"******
                            currentStatus = "V2Ray" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftFailed").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }
                        else
                        {
                            //******"V2Ray卸载成功！"******
                            SetUpProgressBarProcessing(16);
                            currentStatus ="V2Ray" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }

                    }
                    else
                    {
                        //******"检测结果：未安装V2Ray！"******04
                        SetUpProgressBarProcessing(16);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + "V2Ray!";
                        MainWindowsShowInfo(currentStatus);
                    }

                    #endregion

                    #region 卸载Trojan-go

                    //******"检测系统是否已经安装Trojan-go......"******03
                    SetUpProgressBarProcessing(17);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "Trojan-go......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"find / -name trojan-go";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/local/bin/trojan-go") == true)
                    {
                        //******"检测到已安装Trojan-go,开始卸载Trojan-go......"******
                        SetUpProgressBarProcessing(18);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DiscoverProxySoft").ToString()
                            + "Trojan-go!"
                            + Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString()
                            + "Trojan-go......"; 
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"systemctl stop trojan-go";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = $"curl -LROJ https://raw.githubusercontent.com/proxysu/shellscript/master/trojan-go.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"bash trojan-go.sh --remove";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl disable trojan-go";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -rf /usr/local/etc/trojan-go /var/log/trojan-go";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -f trojan-go.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"find / -name trojan-go";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (currentShellCommandResult.Contains("/usr/local/bin/trojan-go") == true)
                        {
                            //******"Trojan-go卸载失败！请向开发者问询！"******
                            currentStatus = "Trojan-go" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftFailed").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }
                        else
                        {
                            //******"Trojan-go卸载成功！"******
                            SetUpProgressBarProcessing(22);
                            currentStatus = "Trojan-go" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }

                    }
                    else
                    {
                        //******"检测结果：未安装Trojan-go！"******04
                        SetUpProgressBarProcessing(22);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + "Trojan-go!";
                        MainWindowsShowInfo(currentStatus);
                    }

                    #endregion

                    #region 卸载Trojan

                    //******"检测系统是否已经安装Trojan......"******03
                    SetUpProgressBarProcessing(23);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "Trojan......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"find / -name trojan";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/local/bin/trojan") == true)
                    {
                        //******"检测到已安装Trojan,开始卸载Trojan......"******
                        SetUpProgressBarProcessing(24);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DiscoverProxySoft").ToString()
                             + "Trojan!"
                             + Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString()
                             + "Trojan......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"systemctl stop trojan";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl disable trojan";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -rf /usr/local/bin/trojan /etc/systemd/system/trojan.service /usr/local/etc/trojan";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"find / -name trojan";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (currentShellCommandResult.Contains("/usr/local/bin/trojan") == true)
                        {
                            //******"Trojan卸载失败！请向开发者问询！"******
                            currentStatus = "Trojan" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftFailed").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }
                        else
                        {
                            //******"Trojan卸载成功！"******
                            SetUpProgressBarProcessing(30);
                            currentStatus = "Trojan" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }

                    }
                    else
                    {
                        //******"检测结果：未安装Trojan！"******04
                        SetUpProgressBarProcessing(30);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + "Trojan!";
                        MainWindowsShowInfo(currentStatus);
                    }

                    #endregion

                    #region 卸载SSR

                    //******"检测系统是否已经安装SSR......"******03
                    SetUpProgressBarProcessing(31);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "SSR......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"if [ -f /usr/local/shadowsocks/server.py ];then echo '1';else echo '0'; fi";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("1") == true)
                    {
                        //******"检测到已安装SSR,开始卸载SSR......"******
                        SetUpProgressBarProcessing(32);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DiscoverProxySoft").ToString()
                           + "SSR!"
                           + Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString()
                           + "SSR......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"systemctl stop ssr";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = $"curl -LROJ https://raw.githubusercontent.com/proxysu/shellscript/master/ssr/ssr.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"bash ssr.sh uninstall";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl disable ssr";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -f ssr.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"if [ -f /usr/local/shadowsocks/server.py ];then echo '1';else echo '0'; fi";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (currentShellCommandResult.Contains("1") == true)
                        {
                            //******"SSR卸载失败！请向开发者问询！"******
                            currentStatus = "SSR" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftFailed").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }
                        else
                        {
                            //******"SSR卸载成功！"******
                            SetUpProgressBarProcessing(36);
                            currentStatus = "SSR" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }

                    }
                    else
                    {
                        //******"检测结果：未安装SSR！"******04
                        SetUpProgressBarProcessing(36);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + "SSR!";
                        MainWindowsShowInfo(currentStatus);
                    }

                    #endregion

                    #region 卸载SS (Shadowsoks-libev)

                    //******"检测系统是否已经安装SS (Shadowsoks-libev)......"******03
                    SetUpProgressBarProcessing(37);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "SS (Shadowsoks-libev)......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"find / -name ss-server";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/local/bin/ss-server") == true)
                    {
                        //******"检测到SS(Shadowsoks-libev),开始卸载SS(Shadowsoks-libev)......"******
                        SetUpProgressBarProcessing(38);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DiscoverProxySoft").ToString()
                           + "SS (Shadowsoks-libev)!"
                           + Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString()
                           + "SS (Shadowsoks-libev)......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"systemctl stop ss-server";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = $"curl -LROJ https://raw.githubusercontent.com/proxysu/shellscript/master/ss/ss-install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"bash ss-install.sh uninstall";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl disable ss-server";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -f ss-install.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //卸载插件
                        sshShellCommand = @"rm -f /usr/local/bin/obfs-server /usr/local/bin/obfs-local /usr/local/bin/v2ray-plugin /usr/local/bin/kcptun-plugin-server /usr/local/bin/goquiet-plugin-server /usr/local/bin/cloak-plugin-server";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"find / -name ss-server";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (currentShellCommandResult.Contains("/usr/local/bin/ss-server") == true)
                        {
                            //******"SS(Shadowsoks-libev)卸载失败！请向开发者问询！"******
                            currentStatus = "SS (Shadowsoks-libev)" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftFailed").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }
                        else
                        {
                            //******"SS (Shadowsoks-libev)卸载成功！"******
                            SetUpProgressBarProcessing(46);
                            currentStatus = "SS (Shadowsoks-libev)" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }

                    }
                    else
                    {
                        //******"检测结果：未安装SS(Shadowsoks-libev)！"******04
                        SetUpProgressBarProcessing(47);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + "SS (Shadowsoks-libev)!";
                        MainWindowsShowInfo(currentStatus);
                    }


                    #endregion

                    #region 卸载acme.sh

                    //******"检测系统是否已经安装acme.sh......"******03
                    SetUpProgressBarProcessing(48);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "acme.sh......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"if [ -d ~/.acme.sh ];then echo '1';else echo '0'; fi";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("1") == true)
                    {
                        //******"检测到acme.sh,开始卸载acme.sh......"******
                        SetUpProgressBarProcessing(49);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DiscoverProxySoft").ToString()
                           + "acme.sh!"
                           + Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString()
                           + "acme.sh......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"acme.sh --uninstall";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -r  ~/.acme.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"if [ -d ~/.acme.sh ];then echo '1';else echo '0'; fi";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (currentShellCommandResult.Contains("1") == true)
                        {
                            //******"acme.sh卸载失败！请向开发者问询！"******
                            currentStatus = "acme.sh" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftFailed").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }
                        else
                        {
                            //******"acme.sh卸载成功！"******
                            SetUpProgressBarProcessing(46);
                            currentStatus = "acme.sh" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }

                    }
                    else
                    {
                        //******"检测结果：未安装acme.sh！"******04
                        SetUpProgressBarProcessing(46);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + "acme.sh!";
                        MainWindowsShowInfo(currentStatus);
                    }

                    #endregion

                    #region 卸载Caddy/NaiveProxy

                    //******"检测系统是否已经安装Caddy/NaiveProxy......"******03
                    SetUpProgressBarProcessing(48);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "Caddy/NaiveProxy......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"find / -name caddy";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/bin/caddy") == true)
                    {
                        //******"检测到Caddy/NaiveProxy,开始卸载Caddy/NaiveProxy......"******
                        SetUpProgressBarProcessing(49);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DiscoverProxySoft").ToString()
                           + "Caddy/NaiveProxy!"
                           + Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString()
                           + "Caddy/NaiveProxy......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"systemctl stop caddy";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl disable caddy";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        //检测系统是否支持yum 或 apt或zypper
                        //如果不存在组件，则命令结果为空，string.IsNullOrEmpty值为真，

                        sshShellCommand = @"command -v apt";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        bool getApt = ! String.IsNullOrEmpty(currentShellCommandResult);

                        sshShellCommand = @"command -v dnf";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        bool getDnf = ! String.IsNullOrEmpty(currentShellCommandResult);

                        sshShellCommand = @"command -v yum";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        bool getYum = ! String.IsNullOrEmpty(currentShellCommandResult);

                        SetUpProgressBarProcessing(55);

                        sshShellCommand = @"command -v zypper";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        bool getZypper = String.IsNullOrEmpty(currentShellCommandResult);

                        
                        string sshCmdRemove = "";
                        //string sshCmdRemove2;
                        //设置安装软件所用的命令格式
                        if (getApt == true)
                        {
                            //sshCmdUpdate = @"apt -qq update";
                            sshCmdRemove = @"apt -y autoremove --purge ";
                        }
                        else if (getDnf == true)
                        {
                            //sshCmdUpdate = @"dnf -q makecache";
                            sshCmdRemove = @"dnf -y remove ";
                        }
                        else if (getYum == true)
                        {
                            //sshCmdUpdate = @"yum -q makecache";
                            sshCmdRemove = @"yum -y remove ";
                        }
                        else if (getZypper == true)
                        {
                            //sshCmdUpdate = @"zypper ref";
                            sshCmdRemove = @"zypper -y remove ";
                        }

                        sshShellCommand = $"{sshCmdRemove}caddy";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"find / -name caddy";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (currentShellCommandResult.Contains("/usr/local/bin/caddy") == true)
                        {
                            //******"Caddy/NaiveProxy卸载失败！请向开发者问询！"******
                            currentStatus = "Caddy/NaiveProxy" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftFailed").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }
                        else
                        {
                            //******"Caddy/NaiveProxy卸载成功！"******
                            SetUpProgressBarProcessing(60);
                            currentStatus = "Caddy/NaiveProxy" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }

                    }
                    else
                    {
                        //******"检测结果：未安装Caddy/NaiveProxy！"******04
                        SetUpProgressBarProcessing(60);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + "Caddy/NaiveProxy!";
                        MainWindowsShowInfo(currentStatus);
                    }

                    #endregion

                    #region 卸载MtProto
                    //******"检测系统是否已经安装MtProto......"******03
                    SetUpProgressBarProcessing(61);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + "MtProto......";
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"find / -name mtg";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    if (currentShellCommandResult.Contains("/usr/local/bin/mtg") == true)
                    {
                        //******"检测到MtProto,开始卸载MtProto......"******
                        SetUpProgressBarProcessing(63);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DiscoverProxySoft").ToString()
                           + "MtProto!"
                           + Application.Current.FindResource("DisplayInstallInfo_StartRemoveProxy").ToString()
                           + "MtProto......";
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"systemctl stop mtg";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl disable mtg";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -rf /etc/systemd/system/mtg.service";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -rf /usr/local/bin/mtg";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -rf /usr/local/etc/mtg_info.json";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"rm -rf /usr/local/etc/mtg.sh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"find / -name mtg";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        if (currentShellCommandResult.Contains("/usr/local/bin/mtg") == true)
                        {
                            //******"MtProto卸载失败！请向开发者问询！"******
                            currentStatus = "MtProto" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftFailed").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }
                        else
                        {
                            //******"MtProto卸载成功！"******
                            SetUpProgressBarProcessing(65);
                            currentStatus = "MtProto" + Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                            MainWindowsShowInfo(currentStatus);
                        }

                    }
                    else
                    {
                        //******"检测结果：未安装MtProto！"******04
                        SetUpProgressBarProcessing(65);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + "MtProto!";
                        MainWindowsShowInfo(currentStatus);
                    }
                    #endregion

                    //如果是纯ipv6主机，则需要删除前面设置的Nat64网关
                    if (onlyIpv6 == true)
                    {
                        SetUpNat64(client, false);
                        sshShellCommand = $"{sshCmdUpdate}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }

                    
                    //******"卸载成功！"******04
                    SetUpProgressBarProcessing(100);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString();
                    MainWindowsShowInfo(currentStatus);
                    client.Disconnect();

                    MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_RemoveProxySoftSuccess").ToString());

                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "主机登录失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }

        #endregion

        #region 资源工具标签页控制

        private void ButtonWebBrowserHomePage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserResourcesAndTools.Source = new Uri("https://github.com/proxysu/windows/wiki/ResourcesAndTools");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ButtonWebBrowserBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserResourcesAndTools.GoBack();
            }
            catch (Exception ex)
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

        #endregion

        #region 免翻网址资源标签
        private void ButtonWebBrowserHomePageFreeWallURL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserResourcesAndToolsFreeWallURL.Source = new Uri("https://github.com/proxysu/windows/wiki/FreeWallURL");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonWebBrowserBackFreeWallURL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserResourcesAndToolsFreeWallURL.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonWebBrowserForwardFreeWallURL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserResourcesAndToolsFreeWallURL.GoForward();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region 三合一安装过程

        //        //生成三合一的v2ray路径
        //        private void ButtonV2rayPath3in1_Click(object sender, RoutedEventArgs e)
        //        {
        //            string path = RandomUserName();
        //            TextBoxV2rayPath3in1.Text = $"/{path}";
        //        }

        //        //生成三合一Trojan密码
        //        private void ButtonTrojanPassword3in1_Click(object sender, RoutedEventArgs e)
        //        {
        //            TextBoxTrojanPassword3in1.Text = RandomUUID();
        //        }

        //        //生成三合一V2ray的UUID
        //        private void ButtonV2rayUUID3in1_Click(object sender, RoutedEventArgs e)
        //        {
        //            TextBoxV2rayUUID3in1.Text = RandomUUID();
        //        }

        //        //生成三合一中Naive的用户名
        //        private void ButtonNaiveUser3in1_Click(object sender, RoutedEventArgs e)
        //        {
        //            TextBoxNaiveUser3in1.Text = RandomUserName();
        //        }

        //        //生成三合一中Naive的密码
        //        private void ButtonNaivePassword3in1_Click(object sender, RoutedEventArgs e)
        //        {
        //            TextBoxNaivePassword3in1.Text = RandomUUID();
        //        }

        //        //启用三合一安装运行
        //        private void Button_Login3in1_Click(object sender, RoutedEventArgs e)
        //        {
        //            if (string.IsNullOrEmpty(TextBoxDomain3in1.Text) == true)
        //            {
        //                MessageBox.Show("域名不能为空！");
        //                return;
        //            }
        //            //ReceiveConfigurationParameters[0]----模板类型
        //            //ReceiveConfigurationParameters[1]----Trojan的密码
        //            //ReceiveConfigurationParameters[2]----v2ray的uuid
        //            //ReceiveConfigurationParameters[3]----v2ray的path
        //            //ReceiveConfigurationParameters[4]----domain
        //            //ReceiveConfigurationParameters[5]----Naive的用户名
        //            //ReceiveConfigurationParameters[6]----Naive的密码
        //            //ReceiveConfigurationParameters[7]----伪装网站
        //            ConnectionInfo connectionInfo = GenerateConnectionInfo();
        //            if (connectionInfo == null)
        //            {
        //                MessageBox.Show("远程主机连接信息有误，请检查");
        //                return;
        //            }
        //            string serverConfig = "";  //服务端配置文件
        //            string clientConfig = "";   //生成的客户端配置文件
        //            string upLoadPath = ""; //服务端文件位置
        //            //传递参数
        //            ReceiveConfigurationParameters[4] = TextBoxDomain3in1.Text;//传递域名
        //            ReceiveConfigurationParameters[7] = TextBoxSites3in1.Text;//传递伪装网站
        //            ReceiveConfigurationParameters[2] = TextBoxV2rayUUID3in1.Text;//v2ray的uuid
        //            ReceiveConfigurationParameters[3] = TextBoxV2rayPath3in1.Text;//v2ray的path
        //            ReceiveConfigurationParameters[1] = TextBoxTrojanPassword3in1.Text;//Trojan的密码
        //            ReceiveConfigurationParameters[5] = TextBoxNaiveUser3in1.Text;//Naive的用户名
        //            ReceiveConfigurationParameters[6] = TextBoxNaivePassword3in1.Text;//Naive的密码
        //            if (TextBoxSites3in1.Text.ToString().Length >= 7)
        //            {
        //                string testDomain = TextBoxSites3in1.Text.Substring(0, 7);
        //                if (String.Equals(testDomain, "https:/") || String.Equals(testDomain, "http://"))
        //                {
        //                    //MessageBox.Show(testDomain);
        //                    ReceiveConfigurationParameters[7] = TextBoxSites3in1.Text.Replace("/", "\\/");
        //                }
        //                else
        //                {
        //                    ReceiveConfigurationParameters[7] = "http:\\/\\/" + TextBoxSites3in1.Text;
        //                }
        //            }

        //            //Thread thread
        //            Thread thread = new Thread(() => StartSetUp3in1(connectionInfo, TextBlockSetUpProcessing, ProgressBarSetUpProcessing, serverConfig, clientConfig, upLoadPath));
        //            thread.SetApartmentState(ApartmentState.STA);
        //            thread.Start();
        //        }

        //        //登录远程主机布署三合一程序
        //        private void StartSetUp3in1(ConnectionInfo connectionInfo, TextBlock textBlockName, ProgressBar progressBar, string serverConfig, string clientConfig, string upLoadPath)
        //        {
        //            string currentStatus = "正在登录远程主机......";

        //            textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);

        //            try
        //            {
        //                #region 主机指纹，暂未启用
        //                //byte[] expectedFingerPrint = new byte[] {
        //                //                                0x66, 0x31, 0xaf, 0x00, 0x54, 0xb9, 0x87, 0x31,
        //                //                                0xff, 0x58, 0x1c, 0x31, 0xb1, 0xa2, 0x4c, 0x6b
        //                //                            };
        //                #endregion
        //                using (var client = new SshClient(connectionInfo))

        //                {
        //                    #region ssh登录验证主机指纹代码块，暂未启用
        //                    //    client.HostKeyReceived += (sender, e) =>
        //                    //    {
        //                    //        if (expectedFingerPrint.Length == e.FingerPrint.Length)
        //                    //        {
        //                    //            for (var i = 0; i < expectedFingerPrint.Length; i++)
        //                    //            {
        //                    //                if (expectedFingerPrint[i] != e.FingerPrint[i])
        //                    //                {
        //                    //                    e.CanTrust = false;
        //                    //                    break;
        //                    //                }
        //                    //            }
        //                    //        }
        //                    //        else
        //                    //        {
        //                    //            e.CanTrust = false;
        //                    //        }
        //                    //    };
        //                    #endregion

        //                    client.Connect();
        //                    if (client.IsConnected == true)
        //                    {
        //                        currentStatus = "主机登录成功";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                    }
        //                    //检测是否运行在root权限下
        //                    string testRootAuthority = client.RunCommand(@"id -u").Result;
        //                    if (testRootAuthority.Equals("0\n") == false)
        //                    {
        //                        MessageBox.Show("请使用具有root权限的账户登录主机！！");
        //                        client.Disconnect();
        //                        return;
        //                    }
        //                    //检测是否安装有V2ray
        //                    currentStatus = "检测系统是否已经安装 V2ray or Trojan or NaiveProxy......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    string cmdTestV2rayInstalled = @"find / -name v2ray";
        //                    string resultCmdTestV2rayInstalled = client.RunCommand(cmdTestV2rayInstalled).Result;
        //                    string cmdTestTrojanInstalled = @"find / -name trojan";
        //                    string resultCmdTestTrojanInstall = client.RunCommand(cmdTestTrojanInstalled).Result;
        //                    string cmdTestNaiveInstalled = @"find / -name naive";
        //                    string resultcmdTestNaiveInstalled = client.RunCommand(cmdTestNaiveInstalled).Result;
        //                    if (resultCmdTestV2rayInstalled.Contains("/usr/bin/v2ray") == true || resultCmdTestTrojanInstall.Contains("/usr/local/bin/trojan") == true || resultcmdTestNaiveInstalled.Contains("/usr/local/bin/naive") == true)
        //                    {
        //                        MessageBoxResult messageBoxResult = MessageBox.Show("远程主机已安装V2ray or Trojan or NaiveProxy,是否强制重新安装？", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
        //                        if (messageBoxResult == MessageBoxResult.No)
        //                        {
        //                            currentStatus = "安装取消，退出";
        //                            textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                            Thread.Sleep(1000);
        //                            client.Disconnect();
        //                            return;
        //                        }
        //                    }

        //                    //检测远程主机系统环境是否符合要求
        //                    currentStatus = "检测系统是否符合安装要求......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    string result = client.RunCommand("uname -r").Result;

        //                    string[] linuxKernelVerStr = result.Split('-');

        //                    bool detectResult = DetectKernelVersion(linuxKernelVerStr[0]);
        //                    if (detectResult == false)
        //                    {
        //                        MessageBox.Show($"当前系统内核版本为{linuxKernelVerStr[0]}，V2ray要求内核为2.6.23及以上。请升级内核再安装！");
        //                        currentStatus = "系统内核版本不符合要求，安装失败！！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                    }
        //                    result = client.RunCommand("uname -m").Result;

        //                    if (result.Contains("x86_64") == false)
        //                    {
        //                        MessageBox.Show($"请在x86_64系统中安装Trojan/NaivProxy");
        //                        currentStatus = "系统不符合要求，安装失败！！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                    }
        //                    //检测系统是否支持yum 或 apt-get或zypper，且支持Systemd
        //                    //如果不存在组件，则命令结果为空，string.IsNullOrEmpty值为真，
        //                    bool getApt = String.IsNullOrEmpty(client.RunCommand("command -v apt-get").Result);
        //                    bool getYum = String.IsNullOrEmpty(client.RunCommand("command -v yum").Result);
        //                    bool getZypper = String.IsNullOrEmpty(client.RunCommand("command -v zypper").Result);
        //                    bool getSystemd = String.IsNullOrEmpty(client.RunCommand("command -v systemctl").Result);
        //                    bool getGetenforce = String.IsNullOrEmpty(client.RunCommand("command -v getenforce").Result);

        //                    //没有安装apt-get，也没有安装yum，也没有安装zypper,或者没有安装systemd的，不满足安装条件
        //                    //也就是apt-get ，yum, zypper必须安装其中之一，且必须安装Systemd的系统才能安装。
        //                    if ((getApt && getYum && getZypper) || getSystemd)
        //                    {
        //                        MessageBox.Show($"系统缺乏必要的安装组件如:apt-get||yum||zypper||Syetemd，主机系统推荐使用：CentOS 7/8,Debian 8/9/10,Ubuntu 16.04及以上版本");
        //                        currentStatus = "系统环境不满足要求，安装失败！！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                        client.Disconnect();
        //                        return;
        //                    }
        //                    //判断是否启用了SELinux,如果启用了，并且工作在Enforcing模式下，则改为Permissive模式
        //                    if (getGetenforce == false)
        //                    {
        //                        string testSELinux = client.RunCommand("getenforce").Result;
        //                        if (testSELinux.Contains("Enforcing") == true)
        //                        {
        //                            client.RunCommand("setenforce  0");//不重启改为Permissive模式
        //                            client.RunCommand("sed -i 's/SELINUX=enforcing/SELINUX=permissive/' /etc/selinux/config");//重启也工作在Permissive模式下
        //                        }

        //                    }

        //                    //校对时间
        //                    currentStatus = "校对时间......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);
        //                    //获取远程主机的时间戳
        //                    long timeStampVPS = Convert.ToInt64(client.RunCommand("date +%s").Result.ToString());

        //                    //获取本地时间戳
        //                    TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        //                    long timeStampLocal = Convert.ToInt64(ts.TotalSeconds);
        //                    if (Math.Abs(timeStampLocal - timeStampVPS) >= 90)
        //                    {

        //                        MessageBox.Show("本地时间与远程主机时间相差超过限制(90秒)，请先用\"系统工具-->时间校对\"校对时间后再设置");
        //                        currentStatus = "时间较对失败......";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                        client.Disconnect();
        //                        return;
        //                    }

        //                    currentStatus = "正在检测域名是否解析到当前VPS的IP上......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //在相应系统内安装curl(如果没有安装curl)
        //                    if (string.IsNullOrEmpty(client.RunCommand("command -v curl").Result) == true)
        //                    {
        //                        //为假则表示系统有相应的组件。
        //                        if (getApt == false)
        //                        {
        //                            client.RunCommand("apt-get -qq update");
        //                            client.RunCommand("apt-get -y -qq install curl");
        //                        }
        //                        if (getYum == false)
        //                        {
        //                            client.RunCommand("yum -q makecache");
        //                            client.RunCommand("yum -y -q install curl");
        //                        }
        //                        if (getZypper == false)
        //                        {
        //                            client.RunCommand("zypper ref");
        //                            client.RunCommand("zypper -y install curl");
        //                        }
        //                    }

        //                    string vpsIp3in1 = client.RunCommand("curl -4 ip.sb").Result.ToString();
        //                    string testDomainCmd = "ping " + ReceiveConfigurationParameters[4] + " -c 1 | grep -oE -m1 \"([0-9]{1,3}\\.){3}[0-9]{1,3}\"";
        //                    string resulttestDomainCmd = client.RunCommand(testDomainCmd).Result.ToString();

        //                    if (String.Equals(vpsIp3in1, resulttestDomainCmd) == true)
        //                    {
        //                        currentStatus = "解析正确！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        currentStatus = "域名未能正确解析到当前VPS的IP上!安装失败！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                        MessageBox.Show("域名未能正确解析到当前VPS的IP上，请检查！若解析设置正确，请等待生效后再重试安装。如果域名使用了CDN，请先关闭！");
        //                        client.Disconnect();
        //                        return;
        //                    }

        //                    //检测是否安装lsof
        //                    if (string.IsNullOrEmpty(client.RunCommand("command -v lsof").Result) == true)
        //                    {
        //                        //为假则表示系统有相应的组件。
        //                        if (getApt == false)
        //                        {
        //                            client.RunCommand("apt-get -qq update");
        //                            client.RunCommand("apt-get -y -qq install lsof");
        //                        }
        //                        if (getYum == false)
        //                        {
        //                            client.RunCommand("yum -q makecache");
        //                            client.RunCommand("yum -y -q install lsof");
        //                        }
        //                        if (getZypper == false)
        //                        {
        //                            client.RunCommand("zypper ref");
        //                            client.RunCommand("zypper -y install lsof");
        //                        }
        //                    }
        //                    currentStatus = "正在检测端口占用情况......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    if (String.IsNullOrEmpty(client.RunCommand(@"lsof -n -P -i :80 | grep LISTEN").Result) == false || String.IsNullOrEmpty(client.RunCommand(@"lsof -n -P -i :443 | grep LISTEN").Result) == false)
        //                    {
        //                        //MessageBox.Show("80/443端口之一，或全部被占用，请先用系统工具中的“释放80/443端口”工具，释放出，再重新安装");
        //                        MessageBoxResult dialogResult = MessageBox.Show("80/443端口之一，或全部被占用，将强制停止占用80/443端口的程序?", "Stop application", MessageBoxButton.YesNo);
        //                        if (dialogResult == MessageBoxResult.No)
        //                        {
        //                            currentStatus = "端口被占用，安装失败......";
        //                            textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                            Thread.Sleep(1000);
        //                            client.Disconnect();
        //                            return;
        //                        }

        //                        currentStatus = "正在释放80/443端口......";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);

        //                        string cmdTestPort = @"lsof -n -P -i :443 | grep LISTEN";
        //                        string cmdResult = client.RunCommand(cmdTestPort).Result;
        //                        if (String.IsNullOrEmpty(cmdResult) == false)
        //                        {
        //                            string[] cmdResultArry443 = cmdResult.Split(' ');
        //                            client.RunCommand($"systemctl stop {cmdResultArry443[0]}");
        //                            client.RunCommand($"systemctl disable {cmdResultArry443[0]}");
        //                            client.RunCommand($"kill -9 {cmdResultArry443[3]}");
        //                        }

        //                        cmdTestPort = @"lsof -n -P -i :80 | grep LISTEN";
        //                        cmdResult = client.RunCommand(cmdTestPort).Result;
        //                        if (String.IsNullOrEmpty(cmdResult) == false)
        //                        {
        //                            string[] cmdResultArry80 = cmdResult.Split(' ');
        //                            client.RunCommand($"systemctl stop {cmdResultArry80[0]}");
        //                            client.RunCommand($"systemctl disable {cmdResultArry80[0]}");
        //                            client.RunCommand($"kill -9 {cmdResultArry80[3]}");
        //                        }
        //                        currentStatus = "80/443端口释放完毕！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);

        //                    }

        //                    //打开防火墙端口

        //                    if (String.IsNullOrEmpty(client.RunCommand("command -v firewall-cmd").Result) == false)
        //                    {
        //                        client.RunCommand("firewall-cmd --zone=public --add-port=80/tcp --permanent");
        //                        client.RunCommand("firewall-cmd --zone=public --add-port=443/tcp --permanent");
        //                        client.RunCommand("firewall-cmd --reload");

        //                    }
        //                    if (String.IsNullOrEmpty(client.RunCommand("command -v ufw").Result) == false)
        //                    {

        //                        client.RunCommand("ufw allow 80");
        //                        client.RunCommand("ufw allow 443");
        //                        client.RunCommand("yes | ufw reload");
        //                    }

        //                    currentStatus = "符合安装要求,V2ray安装中......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //下载官方安装脚本安装V2ray
        //                    client.RunCommand($"curl -o /tmp/go.sh https://raw.githubusercontent.com/v2fly/fhs-install-v2ray/master/install-release.sh");
        //                    client.RunCommand("yes | bash /tmp/go.sh -f");
        //                    string installResult = client.RunCommand("find / -name v2ray").Result.ToString();

        //                    if (!installResult.Contains("/usr/local/bin/v2ray"))
        //                    {
        //                        MessageBox.Show("安装V2ray失败(官方脚本运行出错！");

        //                        currentStatus = "安装V2ray失败(官方脚本运行出错！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        client.Disconnect();
        //                        return;
        //                    }
        //                    //client.RunCommand("mv /etc/v2ray/config.json /etc/v2ray/config.json.1");

        //                    //上传配置文件
        //                    currentStatus = "V2ray程序安装完毕，配置文件上传中......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //生成服务端配置
        //                    serverConfig = $"{pwdir}" + @"TemplateConfg\WebSocketTLSWeb_server_config.json";
        //                    using (StreamReader reader = File.OpenText(serverConfig))
        //                    {
        //                        JObject serverJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
        //                        //设置uuid
        //                        serverJson["inbounds"][0]["settings"]["clients"][0]["id"] = ReceiveConfigurationParameters[2];
        //                        //设置路径
        //                        serverJson["inbounds"][0]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[3];

        //                        using (StreamWriter sw = new StreamWriter(@"config.json"))
        //                        {
        //                            sw.Write(serverJson.ToString());
        //                        }
        //                    }
        //                    upLoadPath = "usr/local/etc/v2ray/config.json";
        //                    UploadConfig(connectionInfo, @"config.json", upLoadPath);
        //                    File.Delete(@"config.json");

        //                    client.RunCommand("systemctl restart v2ray");
        //                    currentStatus = "启动V2ray，OK！";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //安装Trojan
        //                    currentStatus = "开始安装Trojan......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //下载官方安装脚本安装

        //                    client.RunCommand($"curl -o /tmp/trojan-quickstart.sh https://raw.githubusercontent.com/trojan-gfw/trojan-quickstart/master/trojan-quickstart.sh");
        //                    client.RunCommand("yes | bash /tmp/trojan-quickstart.sh");

        //                    installResult = client.RunCommand("find / -name trojan").Result.ToString();

        //                    if (!installResult.Contains("/usr/local/bin/trojan"))
        //                    {
        //                        MessageBox.Show("安装Trojan失败(官方脚本运行出错！");

        //                        currentStatus = "安装Trojan失败(官方脚本运行出错！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        client.Disconnect();
        //                        return;
        //                    }
        //                    client.RunCommand("mv /usr/local/etc/trojan/config.json /usr/local/etc/trojan/config.json.1");

        //                    //上传配置文件
        //                    currentStatus = "Trojan程序安装完毕，配置文件上传中......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //生成服务端配置
        //                    serverConfig = $"{pwdir}" + @"TemplateConfg\trojan_server_config.json";
        //                    using (StreamReader reader = File.OpenText(serverConfig))
        //                    {
        //                        JObject serverJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
        //                        //设置密码
        //                        serverJson["password"][0] = ReceiveConfigurationParameters[1];

        //                        using (StreamWriter sw = new StreamWriter(@"config.json"))
        //                        {
        //                            sw.Write(serverJson.ToString());
        //                        }
        //                    }
        //                    upLoadPath = "/usr/local/etc/trojan/config.json";
        //                    UploadConfig(connectionInfo, @"config.json", upLoadPath);
        //                    File.Delete(@"config.json");


        //                    //安装NaiveProxy
        //                    currentStatus = "开始安装NaiveProxy......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //下载安装脚本安装

        //                    client.RunCommand($"curl -o /tmp/naive-quickstart.sh https://raw.githubusercontent.com/proxysu/shellscript/master/naive-quickstart.sh");
        //                    client.RunCommand("yes | bash /tmp/naive-quickstart.sh");

        //                    installResult = client.RunCommand("find / -name naive").Result.ToString();

        //                    if (!installResult.Contains("/usr/local/bin/naive"))
        //                    {
        //                        MessageBox.Show("安装NaiveProxy失败(脚本运行出错！");
        //                        client.Disconnect();
        //                        currentStatus = "安装NaiveProxy失败(脚本运行出错！";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        client.Disconnect();
        //                        return;
        //                    }

        //                    currentStatus = "NaiveProxy程序安装完毕......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    client.RunCommand("systemctl restart naive");
        //                    currentStatus = "启动Naive，OK！";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);




        //                    currentStatus = "正在安装acme.sh......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    if (getApt == false)
        //                    {
        //                        //client.RunCommand("apt-get -qq update");
        //                        client.RunCommand("apt-get -y -qq install socat");
        //                    }
        //                    if (getYum == false)
        //                    {
        //                        //client.RunCommand("yum -q makecache");
        //                        client.RunCommand("yum -y -q install socat");
        //                    }
        //                    if (getZypper == false)
        //                    {
        //                        // client.RunCommand("zypper ref");
        //                        client.RunCommand("zypper -y install socat");
        //                    }
        //                    client.RunCommand($"curl https://raw.githubusercontent.com/acmesh-official/acme.sh/master/acme.sh  | INSTALLONLINE=1  sh");
        //                    client.RunCommand("cd ~/.acme.sh/");
        //                    client.RunCommand("alias acme.sh=~/.acme.sh/acme.sh");

        //                    currentStatus = "申请域名证书......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //client.RunCommand("mkdir -p /etc/v2ray/ssl");
        //                    client.RunCommand($"/root/.acme.sh/acme.sh  --issue  --standalone  -d {ReceiveConfigurationParameters[4]}");

        //                    currentStatus = "正在安装证书......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);
        //                    client.RunCommand($"/root/.acme.sh/acme.sh  --installcert  -d {ReceiveConfigurationParameters[4]}  --certpath /usr/local/etc/trojan/trojan_ssl.crt --keypath /usr/local/etc/trojan/trojan_ssl.key  --capath  /usr/local/etc/trojan/trojan_ssl.crt  --reloadcmd  \"systemctl restart trojan\"");

        //                    currentStatus = "证书安装，OK！";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    client.RunCommand("systemctl restart trojan");

        //                    currentStatus = "Trojan重启加载证书，OK！";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    currentStatus = "正在安装Caddy......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    client.RunCommand("curl https://getcaddy.com -o getcaddy");
        //                    client.RunCommand("bash getcaddy personal http.forwardproxy,hook.service");
        //                    client.RunCommand("mkdir -p /etc/caddy");
        //                    client.RunCommand("mkdir -p /var/www");


        //                    currentStatus = "上传Caddy配置文件......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    serverConfig = $"{pwdir}" + @"TemplateConfg\3in1_config.caddyfile";

        //                    upLoadPath = "/etc/caddy/Caddyfile";
        //                    UploadConfig(connectionInfo, serverConfig, upLoadPath);

        //                    //设置邮箱
        //                    string email = $"user@{ReceiveConfigurationParameters[4]}";
        //                    //设置Path
        //                    string sshCmd;
        //                    sshCmd = $"sed -i 's/##path##/\\{ReceiveConfigurationParameters[3]}/' {upLoadPath}";
        //                    client.RunCommand(sshCmd);
        //                    //设置域名
        //                    sshCmd = $"sed -i 's/##domain##/{ReceiveConfigurationParameters[4]}:80/' {upLoadPath}";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = $"sed -i 's/##basicauth##/basicauth {ReceiveConfigurationParameters[5]} {ReceiveConfigurationParameters[6]}/' {upLoadPath}";
        //                    client.RunCommand(sshCmd);
        //                    //设置伪装网站

        //                    if (String.IsNullOrEmpty(ReceiveConfigurationParameters[7]) == false)
        //                    {
        //                        sshCmd = $"sed -i 's/##sites##/proxy \\/ {ReceiveConfigurationParameters[7]}/' {upLoadPath}";
        //                        client.RunCommand(sshCmd);
        //                    }
        //                    Thread.Sleep(2000);

        //                    //安装Caddy服务
        //                    sshCmd = $"caddy -service install -agree -conf /etc/caddy/Caddyfile -email {email}";
        //                    client.RunCommand(sshCmd);

        //                    //启动Caddy服务
        //                    client.RunCommand("caddy -service restart");
        //                    currentStatus = "启动Caddy，OK！";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    currentStatus = "正在启用BBR......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);
        //                    //测试BBR条件，若满足提示是否启用
        //                    result = client.RunCommand("uname -r").Result;
        //                    //var result = client.RunCommand("cat /root/test.ver");
        //                    linuxKernelVerStr = result.Split('-');

        //                    detectResult = DetectKernelVersionBBR(linuxKernelVerStr[0]);
        //                    string resultCmdTestBBR = client.RunCommand(@"sysctl net.ipv4.tcp_congestion_control | grep bbr").Result;
        //                    //如果内核满足大于等于4.9，且还未启用BBR，则启用BBR
        //                    if (detectResult == true && resultCmdTestBBR.Contains("bbr") == false)
        //                    {
        //                        client.RunCommand(@"bash -c 'echo ""net.core.default_qdisc=fq"" >> /etc/sysctl.conf'");
        //                        client.RunCommand(@"bash -c 'echo ""net.ipv4.tcp_congestion_control=bbr"" >> /etc/sysctl.conf'");
        //                        client.RunCommand(@"sysctl -p");
        //                    }
        //                    resultCmdTestBBR = client.RunCommand(@"sysctl net.ipv4.tcp_congestion_control | grep bbr").Result;
        //                    if (resultCmdTestBBR.Contains("bbr") == true)
        //                    {
        //                        currentStatus = "启用BBR,OK!";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        currentStatus = "启用BBR,未成功!";
        //                        textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                        Thread.Sleep(1000);
        //                    }

        //                    currentStatus = "正在优化网络参数......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);
        //                    //优化网络参数
        //                    sshCmd = @"bash -c 'echo ""fs.file-max = 51200"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.core.rmem_max = 67108864"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.core.wmem_max = 67108864"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.core.rmem_default = 65536"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.core.wmem_default = 65536"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.core.netdev_max_backlog = 4096"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.core.somaxconn = 4096"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_syncookies = 1"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_tw_reuse = 1"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_tw_recycle = 0"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_fin_timeout = 30"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_keepalive_time = 1200"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.ip_local_port_range = 10000 65000"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_max_syn_backlog = 4096"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_max_tw_buckets = 5000"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_rmem = 4096 87380 67108864"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_wmem = 4096 65536 67108864"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"bash -c 'echo ""net.ipv4.tcp_mtu_probing = 1"" >> /etc/sysctl.conf'";
        //                    client.RunCommand(sshCmd);
        //                    sshCmd = @"sysctl -p";
        //                    client.RunCommand(sshCmd);

        //                    currentStatus = "优化网络参数,OK!";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //生成客户端配置
        //                    currentStatus = "生成客户端配置......";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);
        //                    //创建3in1文件夹
        //                    if (!Directory.Exists("3in1_config"))//如果不存在就创建file文件夹　　             　　              
        //                    {
        //                        Directory.CreateDirectory("3in1_config");//创建该文件夹　　   
        //                    }
        //                    //生成v2ray官方客户端配置
        //                    clientConfig = $"{pwdir}" + @"TemplateConfg\WebSocketTLSWeb_client_config.json";
        //                    using (StreamReader reader = File.OpenText(clientConfig))
        //                    {
        //                        JObject clientJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

        //                        clientJson["outbounds"][0]["settings"]["vnext"][0]["address"] = ReceiveConfigurationParameters[4];
        //                        clientJson["outbounds"][0]["settings"]["vnext"][0]["port"] = int.Parse("443");
        //                        clientJson["outbounds"][0]["settings"]["vnext"][0]["users"][0]["id"] = ReceiveConfigurationParameters[2];

        //                        clientJson["outbounds"][0]["streamSettings"]["wsSettings"]["path"] = ReceiveConfigurationParameters[3];
        //                        if (!Directory.Exists(@"3in1_config\v2ray_config"))//如果不存在就创建file文件夹　　             　　              
        //                        {
        //                            Directory.CreateDirectory(@"3in1_config\v2ray_config");//创建该文件夹　　   
        //                        }
        //                        using (StreamWriter sw = new StreamWriter(@"3in1_config\v2ray_config\config.json"))
        //                        {
        //                            sw.Write(clientJson.ToString());
        //                        }
        //                    }
        //                    //生成V2rayN的客户端
        //                    string v2rayNjsonFile = @"
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
        //                    JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
        //                    v2rayNjsonObject["v"] = "2";
        //                    v2rayNjsonObject["add"] = ReceiveConfigurationParameters[4]; //设置域名
        //                    v2rayNjsonObject["port"] = "443"; //设置端口
        //                    v2rayNjsonObject["id"] = ReceiveConfigurationParameters[2]; //设置uuid
        //                    v2rayNjsonObject["aid"] = "16"; //设置额外ID
        //                    v2rayNjsonObject["net"] = "ws"; //设置传输模式
        //                    v2rayNjsonObject["type"] = "none"; //设置伪装类型
        //                    v2rayNjsonObject["path"] = ReceiveConfigurationParameters[3];//设置路径
        //                    v2rayNjsonObject["host"] = "";//设置TLS的Host
        //                    v2rayNjsonObject["tls"] = "tls";  //设置是否启用TLS
        //                    v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注

        //                    //如果已存在以前保存目录，则新建后缀数字依次增加
        //                    string saveFileFolderFirst = v2rayNjsonObject["ps"].ToString();
        //                    int num = 1;
        //                    string saveFileFolder = saveFileFolderFirst;
        //                    while (Directory.Exists(@"3in1_config\v2ray_config\" + saveFileFolder))
        //                    {
        //                        saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
        //                        num++;
        //                    }
        //                    Directory.CreateDirectory(@"3in1_config\v2ray_config\" + saveFileFolder);//创建该文件夹


        //                    //生成url和二维码
        //                    byte[] textBytes = Encoding.UTF8.GetBytes(v2rayNjsonObject.ToString());
        //                    string vmessUrl = "vmess://" + Convert.ToBase64String(textBytes);

        //                    using (StreamWriter sw = new StreamWriter($"3in1_config\\v2ray_config\\{saveFileFolder}\\url.txt"))
        //                    {
        //                        sw.WriteLine(vmessUrl);

        //                    }
        //                    //生成二维码
        //                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(vmessUrl, QRCodeGenerator.ECCLevel.Q);
        //                    QRCode qrCode = new QRCode(qrCodeData);
        //                    Bitmap qrCodeImage = qrCode.GetGraphic(20);
        //                    //IntPtr myImagePtr = qrCodeImage.GetHbitmap();
        //                    //BitmapSource imgsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(myImagePtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        //                    //ImageShareQRcode.Source = imgsource;
        //                    ////DeleteObject(myImagePtr);
        //                    qrCodeImage.Save($"3in1_config\\v2ray_config\\{saveFileFolder}\\QR.bmp");

        //                    //生成说明文件
        //                    using (StreamWriter sw = new StreamWriter($"3in1_config\\v2ray_config\\{saveFileFolder}\\说明.txt"))
        //                    {
        //                        sw.WriteLine("config.json");
        //                        sw.WriteLine("此文件为v2ray官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080，http代理地址：127.0.0.1:1081");
        //                        sw.WriteLine("v2ray官方网站：https://www.v2ray.com/");
        //                        sw.WriteLine("v2ray官方程序下载地址：https://github.com/v2ray/v2ray-core/releases");
        //                        sw.WriteLine("下载相应版本，Windows选择v2ray-windows-64.zip或者v2ray-windows-32.zip，解压后提取v2ctl.exe和v2ray.exe。与config.json放在同一目录，运行v2ray.exe即可。");
        //                        sw.WriteLine("-----------------------------------------");
        //                        sw.WriteLine("QR.bmp");
        //                        sw.WriteLine("此文件为v2rayN、v2rayNG(Android)、Shadowrocket(ios)扫码导入节点");
        //                        sw.WriteLine("v2rayN下载网址：https://github.com/2dust/v2rayN/releases");
        //                        sw.WriteLine("v2rayNG(Android)下载网址：https://github.com/2dust/v2rayNG/releases");
        //                        sw.WriteLine("v2rayNG(Android)在Google Play下载网址：https://play.google.com/store/apps/details?id=com.v2ray.ang");
        //                        sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");

        //                        sw.WriteLine("-----------------------------------------");
        //                        sw.WriteLine("url.txt");
        //                        sw.WriteLine("此文件为v2rayN、v2rayNG(Android)、Shadowrocket(ios)复制粘贴导入节点的vmess网址");
        //                        sw.WriteLine("-----------------------------------------\n");
        //                        sw.WriteLine("服务器通用连接配置参数");
        //                        sw.WriteLine($"地址(address)：{ReceiveConfigurationParameters[4]}");
        //                        sw.WriteLine($"端口(Port)：443");
        //                        sw.WriteLine($"用户ID(uuid)：{ReceiveConfigurationParameters[2]}");
        //                        sw.WriteLine($"额外ID：16");
        //                        sw.WriteLine($"加密方式：auto");
        //                        sw.WriteLine($"传输协议：ws");
        //                        sw.WriteLine($"伪装类型：none");
        //                        sw.WriteLine($"是否使用TLS：tls");
        //                        sw.WriteLine($"host：");
        //                        sw.WriteLine($"路径(Path)：{ReceiveConfigurationParameters[3]}");
        //                        sw.WriteLine($"QUIC密钥：");
        //                    }
        //                    //移动V2ray官方配置config.json到与上述文件同一目录
        //                    File.Move(@"3in1_config\v2ray_config\config.json", @"3in1_config\v2ray_config\" + saveFileFolder + @"\config.json");

        //                    //生成Trojan客户端文件
        //                    clientConfig = $"{pwdir}" + @"TemplateConfg\trojan_client_config.json";
        //                    if (!Directory.Exists(@"3in1_config\trojan_config"))//如果不存在就创建file文件夹　　             　　              
        //                    {
        //                        Directory.CreateDirectory(@"3in1_config\trojan_config");//创建该文件夹　　   
        //                    }
        //                    using (StreamReader reader = File.OpenText(clientConfig))
        //                    {
        //                        JObject clientJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

        //                        clientJson["remote_addr"] = ReceiveConfigurationParameters[4];
        //                        //clientJson["remote_port"] = int.Parse(ReceiveConfigurationParameters[1]);
        //                        clientJson["password"][0] = ReceiveConfigurationParameters[1];

        //                        using (StreamWriter sw = new StreamWriter(@"3in1_config\trojan_config\config.json"))
        //                        {
        //                            sw.Write(clientJson.ToString());
        //                        }
        //                    }
        //                    //生成二维码和url
        //                    saveFileFolderFirst = ReceiveConfigurationParameters[4];
        //                    num = 1;
        //                    saveFileFolder = saveFileFolderFirst;
        //                    while (Directory.Exists(@"3in1_config\trojan_config\" + saveFileFolder))
        //                    {
        //                        saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
        //                        num++;
        //                    }
        //                    Directory.CreateDirectory(@"3in1_config\trojan_config\" + saveFileFolder);//创建该文件夹

        //                    string trojanUrl = $"trojan://{ReceiveConfigurationParameters[1]}@{ReceiveConfigurationParameters[4]}:443#{ReceiveConfigurationParameters[4]}";
        //                    using (StreamWriter sw = new StreamWriter($"3in1_config\\trojan_config\\{saveFileFolder}\\url.txt"))
        //                    {
        //                        sw.WriteLine(trojanUrl);

        //                    }
        //                    //生成二维码
        //                    QRCodeGenerator qrGeneratorTrojan = new QRCodeGenerator();
        //                    QRCodeData qrCodeDataTrojan = qrGeneratorTrojan.CreateQrCode(trojanUrl, QRCodeGenerator.ECCLevel.Q);
        //                    QRCode qrCodeTrojan = new QRCode(qrCodeDataTrojan);
        //                    Bitmap qrCodeImageTrojan = qrCodeTrojan.GetGraphic(20);
        //                    qrCodeImageTrojan.Save($"3in1_config\\trojan_config\\{saveFileFolder}\\QR.bmp");

        //                    //生成说明文件
        //                    using (StreamWriter sw = new StreamWriter($"3in1_config\\trojan_config\\{saveFileFolder}\\说明.txt"))
        //                    {
        //                        sw.WriteLine("config.json");
        //                        sw.WriteLine("此文件为Trojan官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080");
        //                        sw.WriteLine("Trojan官方网站：https://trojan-gfw.github.io/trojan/");
        //                        sw.WriteLine("Trojan官方程序下载地址：https://github.com/trojan-gfw/trojan/releases");
        //                        sw.WriteLine("下载相应版本，Windows选择Trojan-x.xx-win.zip,解压后提取trojan.exe。与config.json放在同一目录，运行trojan.exe即可。");
        //                        sw.WriteLine("-----------------------------------------\n");
        //                        sw.WriteLine("QR.bmp");
        //                        sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点");
        //                        sw.WriteLine("Trojan-QT5 (windows)下载网址：https://github.com/TheWanderingCoel/Trojan-Qt5/releases");
        //                        sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
        //                        sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");

        //                        sw.WriteLine("-----------------------------------------\n");
        //                        sw.WriteLine("url.txt");
        //                        sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)复制粘贴导入节点的网址");
        //                        sw.WriteLine("-----------------------------------------\n");
        //                        sw.WriteLine("服务器通用连接配置参数");
        //                        sw.WriteLine($"地址(address)：{ReceiveConfigurationParameters[4]}");
        //                        sw.WriteLine($"端口(Port)：443");
        //                        sw.WriteLine($"密钥：{ReceiveConfigurationParameters[1]}");

        //                    }
        //                    //移动Trojan官方配置config.json到与上述文件同一目录
        //                    File.Move(@"3in1_config\trojan_config\config.json", @"3in1_config\trojan_config\" + saveFileFolder + @"\config.json");

        //                    //生成NaiveProxy的客户端配置
        //                    clientConfig = $"{pwdir}" + @"TemplateConfg\Naiveproxy_client_config.json";
        //                    if (!Directory.Exists(@"3in1_config\naive_config"))//如果不存在就创建file文件夹　　             　　              
        //                    {
        //                        Directory.CreateDirectory(@"3in1_config\naive_config");//创建该文件夹　　   
        //                    }
        //                    using (StreamReader reader = File.OpenText(clientConfig))
        //                    {
        //                        JObject clientJson = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

        //                        clientJson["proxy"] = $"https://{ReceiveConfigurationParameters[5]}:{ReceiveConfigurationParameters[6]}@{ReceiveConfigurationParameters[4]}";

        //                        using (StreamWriter sw = new StreamWriter(@"3in1_config\naive_config\config.json"))
        //                        {
        //                            sw.Write(clientJson.ToString());
        //                        }
        //                    }
        //                    //生成用于NaiveGUI的url
        //                    saveFileFolderFirst = ReceiveConfigurationParameters[4];
        //                    num = 1;
        //                    saveFileFolder = saveFileFolderFirst;
        //                    while (Directory.Exists(@"3in1_config\naive_config\" + saveFileFolder))
        //                    {
        //                        saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
        //                        num++;
        //                    }
        //                    Directory.CreateDirectory(@"3in1_config\naive_config\" + saveFileFolder);//创建该文件夹

        //                    string naiveUrl = $"https://{ReceiveConfigurationParameters[5]}:{ReceiveConfigurationParameters[6]}@{ReceiveConfigurationParameters[4]}:443/?name={ReceiveConfigurationParameters[4]}&padding=true";
        //                    using (StreamWriter sw = new StreamWriter($"3in1_config\\naive_config\\{saveFileFolder}\\url.txt"))
        //                    {
        //                        sw.WriteLine(naiveUrl);
        //                    }
        //                    //生成说明文件
        //                    using (StreamWriter sw = new StreamWriter($"3in1_config\\naive_config\\{saveFileFolder}\\说明.txt"))
        //                    {
        //                        sw.WriteLine("config.json");
        //                        sw.WriteLine("此文件为NaiveProxy官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080");
        //                        sw.WriteLine("NaiveProxy官方网站：https://github.com/klzgrad/naiveproxy");
        //                        sw.WriteLine("NaiveProxy官方程序下载地址：https://github.com/klzgrad/naiveproxy/releases");
        //                        sw.WriteLine("下载相应版本，Windows选择naiveproxy-x.xx-win.zip,解压后提取naive.exe。与config.json放在同一目录，运行naive.exe即可。");
        //                        sw.WriteLine("-----------------------------------------\n");
        //                        //sw.WriteLine("其他平台的客户端，暂未发布");
        //                        //sw.WriteLine("QR.bmp");
        //                        //sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点");
        //                        //sw.WriteLine("Trojan-QT5 (windows)下载网址：https://github.com/TheWanderingCoel/Trojan-Qt5/releases");
        //                        //sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
        //                        //sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");

        //                        //sw.WriteLine("-----------------------------------------\n");
        //                        sw.WriteLine("url.txt");
        //                        sw.WriteLine("此文件为NaiveGUI(windows)复制粘贴导入节点的网址");
        //                        sw.WriteLine("NaiveGUI(windows)下载网址：https://github.com/ExcitedCodes/NaiveGUI/releases");

        //                        sw.WriteLine("-----------------------------------------\n");
        //                        sw.WriteLine("服务器通用连接配置参数");
        //                        sw.WriteLine($"地址(address)：{ReceiveConfigurationParameters[4]}");
        //                        sw.WriteLine($"用户名：{ReceiveConfigurationParameters[5]}");
        //                        sw.WriteLine($"密钥：{ReceiveConfigurationParameters[6]}");
        //                    }
        //                    //移动Naive官方配置config.json到与上述文件同一目录
        //                    File.Move(@"3in1_config\naive_config\config.json", @"3in1_config\naive_config\" + saveFileFolder + @"\config.json");

        //                    client.Disconnect();

        //                    currentStatus = "生成客户端配置，OK！ 安装成功！";
        //                    textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
        //                    Thread.Sleep(1000);

        //                    //显示服务端连接参数
        //                    MessageBox.Show("安装成功，所有相关配置参数与二维码，url都已保存在相应目录下，点击“确定”后打开");
        //                    string openFolderPath = @"3in1_config\";
        //                    System.Diagnostics.Process.Start("explorer.exe", openFolderPath);

        //                    return;
        //                }
        //            }
        //            catch (Exception ex1)//例外处理   
        //            #region 例外处理
        //            {
        //                //MessageBox.Show(ex1.Message);
        //                if (ex1.Message.Contains("连接尝试失败") == true)
        //                {
        //                    MessageBox.Show($"{ex1.Message}\n请检查主机地址及端口是否正确，如果通过代理，请检查代理是否正常工作");
        //                }

        //                else if (ex1.Message.Contains("denied (password)") == true)
        //                {
        //                    MessageBox.Show($"{ex1.Message}\n密码错误或用户名错误");
        //                }
        //                else if (ex1.Message.Contains("Invalid private key file") == true)
        //                {
        //                    MessageBox.Show($"{ex1.Message}\n所选密钥文件错误或者格式不对");
        //                }
        //                else if (ex1.Message.Contains("denied (publickey)") == true)
        //                {
        //                    MessageBox.Show($"{ex1.Message}\n使用密钥登录，密钥文件错误或用户名错误");
        //                }
        //                else if (ex1.Message.Contains("目标计算机积极拒绝") == true)
        //                {
        //                    MessageBox.Show($"{ex1.Message}\n主机地址错误，如果使用了代理，也可能是连接代理的端口错误");
        //                }
        //                else
        //                {
        //                    MessageBox.Show("发生错误");
        //                    MessageBox.Show(ex1.Message);
        //                }
        //                currentStatus = "主机登录失败";
        //                textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);

        //            }
        //            #endregion

        //        }


        #endregion

        #region 测试用代码
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string pwdir = AppDomain.CurrentDomain.BaseDirectory;
            MessageBox.Show(pwdir);
            //ConnectionInfo connectionInfo = GenerateConnectionInfo();
            //if (connectionInfo == null)
            //{
            //    //****** "远程主机连接信息有误，请检查!" ******
            //    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
            //    return;
            //}
            //using (var client = new SshClient(connectionInfo))
            //{
            //    client.Connect();
            //    if (client.IsConnected == true)
            //    {
            //        //******"主机登录成功"******
            //        SetUpProgressBarProcessing(3);
            //        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
            //        MainWindowsShowInfo(currentStatus);

            //    }
            //    //string cmdErr = client.RunCommand(@"aaa ee").Error;
            //    //MessageBox.Show(cmdErr);
            //    SshCommand cmdResult = client.RunCommand(@"pwd");
            //    string result = cmdResult.Result;
            //    MessageBox.Show("result:"+result);
            //    string error = cmdResult.Error;
            //    MessageBox.Show("err:"+error);

            //    SshCommand cmdResultCat = client.RunCommand(@"cat tt.t");
            //    string resultCat = cmdResultCat.Result;
            //    MessageBox.Show("resultCat:" + resultCat);
            //    string errorCat = cmdResultCat.Error;
            //    MessageBox.Show("errCat:" + errorCat);
            //SoftInstalledSuccessOrFail(client, "v2ray", @"/usr/local/bin/v2ray");
            //CaddyInstall(client);
            //if (client.IsConnected == true)
            //{
            //    MessageBox.Show("Connected");
            //}
            //if (client.IsConnected == false)
            //{
            //    MessageBox.Show("disConnected");
            //}
        //}
        }

        private string CaddyInstallTest(SshClient client)
        {

            if (client.IsConnected == true)
            {
                //******"主机登录成功"******
                SetUpProgressBarProcessing(3);
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                MainWindowsShowInfo(currentStatus);
             
                sshShellCommand = @"id -u";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                client.Disconnect();
            }
            return "";
        }
        #endregion

        #region 布署代理所用到的步骤函数

        //检测主机是否为纯ipv6的主机
        private bool OnlyIpv6HostDetect(SshClient client)
        {
            //****** "正在检测是否为纯ipv6主机......." ******11
            //SetUpProgressBarProcessing(34);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_OnlyIpv6HostDetect").ToString();
            MainWindowsShowInfo(currentStatus);

            //再次初始化相关变量
            ipv4 = String.Empty;
            ipv6 = String.Empty;

            //sshShellCommand = @"curl -4 ip.sb";
            sshShellCommand = @"curl -s https://api.ip.sb/ip --ipv4";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            ipv4 = currentShellCommandResult.TrimEnd('\r', '\n');

            sshShellCommand = @"curl -s https://api.ip.sb/ip --ipv6";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            ipv6 = currentShellCommandResult.TrimEnd('\r', '\n');

            if (String.IsNullOrEmpty(ipv4) == false)
            {
                return false;
            }
            else
            {
                if (String.IsNullOrEmpty(ipv6) == false)
                {
                    //apiGithubCom = "api.githubipv6.ga";
                    //scriptGithubUrl = "raw.githubipv6.ga";
                    //githubCom = "github.githubipv6.ga";
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        //设置Nat64与删除设置Nat64
        private bool SetUpNat64(SshClient client,bool set)
        {
            if(set == true)
            {
                //****** "正在设置Nat64网关......" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_SetUpNat64").ToString();
                MainWindowsShowInfo(currentStatus);
                sshShellCommand = @"mv /etc/resolv.conf /etc/resolv.conf.bak";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                sshShellCommand = @"echo ""nameserver   2a09:11c0:f1:bbf0::70"" >>/etc/resolv.conf";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                sshShellCommand = @"echo ""nameserver   2a03:7900:2:0:31:3:104:161"" >>/etc/resolv.conf";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            }
            else
            {
                //****** "正在删除Nat64网关......" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_DeleteSetUpNat64").ToString();
                MainWindowsShowInfo(currentStatus);
                sshShellCommand = @"rm /etc/resolv.conf";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                sshShellCommand = @"mv /etc/resolv.conf.bak /etc/resolv.conf";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            }
           
            return true;
        }
        //纯ipv6主机安装脚本处理
        //private void Ipv6ScriptEdit(SshClient client,string scriptFile)
        //{
        //    sshShellCommand = $"sed -i 's/api.github.com/{apiGithubCom}/g' {scriptFile}";
        //    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

        //    sshShellCommand = $"sed -i 's/raw.githubusercontent.com/raw.githubusercontent.com/g' {scriptFile}";
        //    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

        //    sshShellCommand = $"sed -i 's/https:\\/\\/github.com/https:\\/\\/{githubCom}/g' {scriptFile}";
        //    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
        //}


        #region 安装过程提示信息

        //安装过程提示信息
        //  MainWindowsShowInfo(currentStatus);
        //  currentShellCommandResult = MainWindowsShowCmd(client,sshShellCommand);
        private string MainWindowsShowInfo(string currentStatus)
        {
            TextBlockSetUpProcessing.Dispatcher.BeginInvoke(updateAction, TextBlockSetUpProcessing, ProgressBarSetUpProcessing, currentStatus);
            string currentShellCommandResult = currentStatus;
            TextBoxMonitorCommandResults.Dispatcher.BeginInvoke(updateMonitorAction, TextBoxMonitorCommandResults, currentShellCommandResult);//显示命令执行的结果在监视窗口

            return currentShellCommandResult;
        }
        
        //安装过程所运行的命令与相应结果
        private string MainWindowsShowCmd(SshClient client,string sshShellCommand)
        {
            TextBoxMonitorCommandResults.Dispatcher.BeginInvoke(updateMonitorAction, TextBoxMonitorCommandResults, sshShellCommand);//显示执行的命令
            SshCommand cmdResult = client.RunCommand(sshShellCommand);
            string currentShellCommandResult = cmdResult.Result;
            string currentShellCommandError = cmdResult.Error;
            if (String.IsNullOrEmpty(currentShellCommandResult) == true) { currentShellCommandResult = currentShellCommandError; }
            TextBoxMonitorCommandResults.Dispatcher.BeginInvoke(updateMonitorAction, TextBoxMonitorCommandResults, currentShellCommandResult);//显示命令执行的结果

            return currentShellCommandResult;
        }
        
        //调用的函数返回false后的提示
        private string FunctionResultErr()
        {
            //*****"发生错误，安装中断......"*****
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_FunctionResultErr").ToString();
            MainWindowsShowInfo(currentStatus);
            return "";
        }
        #endregion

        //检测root权限 5--7
        //functionResult = RootAuthorityDetect(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool RootAuthorityDetect(SshClient client)
        {
            //******"检测是否运行在root权限下..."******01
            SetUpProgressBarProcessing(5);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_DetectionRootPermission").ToString();
            MainWindowsShowInfo(currentStatus);

            sshShellCommand = @"id -u";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            //string testRootAuthority = currentShellCommandResult;
            if (currentShellCommandResult.Equals("0\n") == false)
            {
                //******"请使用具有root权限的账户登录主机！！"******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorRootPermission").ToString());
                //client.Disconnect();
                return false;
            }
            else
            {
                //******"检测结果：OK！"******02
                SetUpProgressBarProcessing(7);
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_DetectionRootOK").ToString();
                MainWindowsShowInfo(currentStatus);
                //return true;
            }
            return true;
        }

        //检测是否已安装代理 8--10
        //soft--要检测的程序
        //condition---已安装的条件
        //functionResult = SoftInstalledIsNoYes(client, "v2ray", @"/usr/local/bin/v2ray");
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool SoftInstalledIsNoYes(SshClient client,string soft,string condition)
        {
            //******"检测系统是否已经安装......"******03
            SetUpProgressBarProcessing(8);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestExistSoft").ToString() + $"{soft}......";
            MainWindowsShowInfo(currentStatus);


            sshShellCommand = $"find / -name {soft}";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            //string resultCmdTestV2rayInstalled = currentShellCommandResult;
            if (currentShellCommandResult.Contains($"{condition}") == true)
            {
                //******"远程主机已安装V2ray,是否强制重新安装？"******
                string messageShow = Application.Current.FindResource("MessageBoxShow_ExistedSoft").ToString() +
                                    $"{soft}" +
                                    Application.Current.FindResource("MessageBoxShow_ForceInstallSoft").ToString();
                MessageBoxResult messageBoxResult = MessageBox.Show(messageShow, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    //******"安装取消，退出"******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_InstallationCanceledExit").ToString();
                    MainWindowsShowInfo(currentStatus);
                    //client.Disconnect();
                    return false;
                }
                else
                {
                    //******"已选择强制安装！"******04
                    SetUpProgressBarProcessing(10);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_ForceInstallSoft").ToString() + $"{soft}";
                    MainWindowsShowInfo(currentStatus);

                }
            }
            else
            {
                //******"检测结果：未安装"******04
                SetUpProgressBarProcessing(10);
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_NoInstalledSoft").ToString() + $"{soft}";
                MainWindowsShowInfo(currentStatus);
            }
            return true;
        }

        //检测关闭Selinux及系统组件是否齐全（apt/yum/dnf/systemctl）11--30
        //安装依赖软件，检测端口，防火墙开启端口,检测是否为纯ipv6主机,若是则设置Nat64网关
        //functionResult = ShutDownSelinuxAndSysComponentsDetect(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool ShutDownSelinuxAndSysComponentsDetect(SshClient client)
        {

            //检测系统是否支持yum 或 apt或zypper，且支持Systemd
            //如果不存在组件，则命令结果为空，String.IsNullOrEmpty值为真
            //取反则getApt,getDnf,getYum,getSystem,getGetenforce为假
            //不存在组件，则为假

            //******"检测系统是否符合安装要求......"******
            SetUpProgressBarProcessing(11);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_CheckSystemRequirements").ToString();
            MainWindowsShowInfo(currentStatus);

            sshShellCommand = @"command -v apt";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            getApt = ! String.IsNullOrEmpty(currentShellCommandResult);

            sshShellCommand = @"command -v dnf";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            getDnf = ! String.IsNullOrEmpty(currentShellCommandResult);

            sshShellCommand = @"command -v yum";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            getYum = ! String.IsNullOrEmpty(currentShellCommandResult);

            SetUpProgressBarProcessing(13);

            //sshShellCommand = @"command -v zypper";
            //currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            //bool getZypper = ! String.IsNullOrEmpty(currentShellCommandResult);

            sshShellCommand = @"command -v systemctl";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            bool getSystemd = ! String.IsNullOrEmpty(currentShellCommandResult);

            sshShellCommand = @"command -v getenforce";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            bool getGetenforce = ! String.IsNullOrEmpty(currentShellCommandResult);


            //没有安装apt，也没有安装dnf\yum，也没有安装zypper,或者没有安装systemd的，不满足安装条件
            //也就是apt ，dnf\yum, zypper必须安装其中之一，且必须安装Systemd的系统才能安装。
            if ((getApt == false && getDnf == false && getYum == false) || getSystemd == false)
            {
                //******"系统缺乏必要的安装组件如:apt||dnf||yum||zypper||Syetemd，主机系统推荐使用：CentOS 7/8,Debian 8/9/10,Ubuntu 16.04及以上版本"******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_MissingSystemComponents").ToString());

                //******"系统环境不满足要求，安装失败！！"******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_MissingSystemComponents").ToString();
                MainWindowsShowInfo(currentStatus);

                return false;
            }
            else
            {
                //******"检测结果：OK!"******06
                SetUpProgressBarProcessing(16);
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_SystemRequirementsOK").ToString();
                MainWindowsShowInfo(currentStatus);
            }

            //检测是否安装dig
            if (string.IsNullOrEmpty(client.RunCommand("command -v dig").Result) == true)
            {
                //设置安装软件所用的命令格式
                if (getApt == true)
                {
                    sshCmdUpdate = @"apt -qq update";
                    sshCmdInstall = @"apt -y -qq install dnsutils";
                }
                else if (getDnf == true)
                {
                    sshCmdUpdate = @"dnf -q makecache";
                    sshCmdInstall = @"dnf -y -q install bind-utils";
                }
                else if (getYum == true)
                {
                    sshCmdUpdate = @"yum -q makecache";
                    sshCmdInstall = @"yum -y -q install bind-utils";
                }
                sshShellCommand = sshCmdUpdate;
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = sshCmdInstall;
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }

            //设置安装软件所用的命令格式
            if (getApt == true)
            {
                sshCmdUpdate = @"apt -qq update";
                sshCmdInstall = @"apt -y -qq install ";
            }
            else if (getDnf == true)
            {
                sshCmdUpdate = @"dnf clean all;dnf -q makecache";
                sshCmdInstall = @"dnf -y -q install ";
            }
            else if (getYum == true)
            {
                sshCmdUpdate = @"yum clean all; yum -q makecache";
                sshCmdInstall = @"yum -y -q install ";
            }
            //else if (getZypper == true)
            //{
            //    sshCmdUpdate = @"zypper ref";
            //    sshCmdInstall = @"zypper -y install ";
            //}

            //判断是否启用了SELinux,如果启用了，并且工作在Enforcing模式下，则改为Permissive模式
            if (getGetenforce == true)
            {
                sshShellCommand = @"getenforce";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                //string testSELinux = currentShellCommandResult;

                if (currentShellCommandResult.Contains("Enforcing") == true)
                {
                    //******"检测到系统启用SELinux，且工作在严格模式下，需改为宽松模式！修改中......"******07
                    SetUpProgressBarProcessing(18);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableSELinux").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"setenforce  0";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"sed -i 's/SELINUX=enforcing/SELINUX=permissive/' /etc/selinux/config";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //******"修改完毕！"******08
                    SetUpProgressBarProcessing(20);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_SELinuxModifyOK").ToString();
                    MainWindowsShowInfo(currentStatus);
                }

            }

            
            //在相应系统内安装curl(如果没有安装curl)--此为依赖软件
            if (string.IsNullOrEmpty(client.RunCommand("command -v curl").Result) == true)
            {
                sshShellCommand = $"{sshCmdUpdate}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = $"{sshCmdInstall}curl";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }
            // 在相应系统内安装wget(如果没有安装wget)--此为依赖软件
            if (string.IsNullOrEmpty(client.RunCommand("command -v wget").Result) == true)
            {
                sshShellCommand = $"{sshCmdUpdate}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = $"{sshCmdInstall}wget";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }
            // 在相应系统内安装unzip(如果没有安装unzip)--此为依赖软件
            if (string.IsNullOrEmpty(client.RunCommand("command -v unzip").Result) == true)
            {
                sshShellCommand = $"{sshCmdUpdate}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = $"{sshCmdInstall}unzip";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }
            //处理极其少见的xz-utils未安装的情况
            if (string.IsNullOrEmpty(client.RunCommand("command -v xz-utils").Result) == true)
            {
                sshShellCommand = $"{sshCmdUpdate}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = $"{sshCmdInstall}xz-utils";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }
            //检测是否安装lsof
            if (string.IsNullOrEmpty(client.RunCommand("command -v lsof").Result) == true)
            {
                sshShellCommand = $"{sshCmdUpdate}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = $"{sshCmdInstall}lsof";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }

            //检测主机是否为纯ipv6的主机
            onlyIpv6 = OnlyIpv6HostDetect(client);
            if(onlyIpv6 == true)
            {
                SetUpNat64(client, true);
                sshShellCommand = $"{sshCmdUpdate}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            }

            //****** "检测端口占用情况......" ******
            SetUpProgressBarProcessing(22);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestPortUsed").ToString();
            MainWindowsShowInfo(currentStatus);

            if (String.Equals(ReceiveConfigurationParameters[1], "80") == true || String.Equals(ReceiveConfigurationParameters[1], "443") == true)
            {
                string testPort80 = string.Empty;
                string testPort443 = string.Empty;

                sshShellCommand = @"lsof -n -P -i :80 | grep LISTEN";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                testPort80 = currentShellCommandResult;

                sshShellCommand = @"lsof -n -P -i :443 | grep LISTEN";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                testPort443 = currentShellCommandResult;


                if (String.IsNullOrEmpty(testPort80) == false || String.IsNullOrEmpty(testPort443) == false)
                {
                    //****** "80/443端口之一，或全部被占用，将强制停止占用80/443端口的程序?" ******
                    MessageBoxResult dialogResult = MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorPortUsed").ToString(), "Stop application", MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.No)
                    {
                        //****** "端口被占用，安装失败......" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_ErrorPortUsedFail").ToString();
                        MainWindowsShowInfo(currentStatus);
                        //client.Disconnect();
                        return false;
                    }

                    //****** "正在释放80/443端口......" ******
                    SetUpProgressBarProcessing(24);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_ReleasePort").ToString();
                    MainWindowsShowInfo(currentStatus);

                    if (String.IsNullOrEmpty(testPort443) == false)
                    {
                        string[] cmdResultArry443 = testPort443.Split(' ');

                        sshShellCommand = $"systemctl stop {cmdResultArry443[0]}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = $"systemctl disable {cmdResultArry443[0]}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = $"kill -9 {cmdResultArry443[3]}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }

                    if (String.IsNullOrEmpty(testPort80) == false)
                    {
                        string[] cmdResultArry80 = testPort80.Split(' ');

                        sshShellCommand = $"systemctl stop {cmdResultArry80[0]}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = $"systemctl disable {cmdResultArry80[0]}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = $"kill -9 {cmdResultArry80[3]}";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }
                    //****** "80/443端口释放完毕！" ******
                    SetUpProgressBarProcessing(26);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_ReleasePortOK").ToString();
                    MainWindowsShowInfo(currentStatus);
                }
                else
                {
                    //****** "检测结果：未被占用！" ******
                    SetUpProgressBarProcessing(26);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_PortNotUsed").ToString();
                    MainWindowsShowInfo(currentStatus);
                }
            }
            else
            {
                string testPort = string.Empty;

                sshShellCommand = $"lsof -n -P -i :{ReceiveConfigurationParameters[1]} | grep LISTEN";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                testPort = currentShellCommandResult;

                if (String.IsNullOrEmpty(testPort) == false)
                {
                    //****** "端口被占用，将强制停止占用此端口的程序?" ******
                    MessageBoxResult dialogResult = MessageBox.Show(ReceiveConfigurationParameters[1] + Application.Current.FindResource("MessageBoxShow_ErrorPortUsedOther").ToString(), "Stop application", MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.No)
                    {
                        //****** "端口被占用，安装失败......" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_ErrorPortUsedFail").ToString();
                        MainWindowsShowInfo(currentStatus);
                        //client.Disconnect();
                        return false;
                    }

                    //****** "正在释放端口......" ******
                    SetUpProgressBarProcessing(24);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_ReleasePortOther").ToString();
                    MainWindowsShowInfo(currentStatus);

                    string[] cmdResultArry = testPort.Split(' ');

                    sshShellCommand = $"systemctl stop {cmdResultArry[0]}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = $"systemctl disable {cmdResultArry[0]}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = $"kill -9 {cmdResultArry[3]}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //****** "端口释放完毕！" ******
                    SetUpProgressBarProcessing(26);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_ReleasePortOKOther").ToString();
                    MainWindowsShowInfo(currentStatus);

                }
                else
                {
                    //****** "检测结果：未被占用！" ******
                    SetUpProgressBarProcessing(26);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_PortNotUsed").ToString();
                    MainWindowsShowInfo(currentStatus);
                }
            }
            //****** "开启防火墙相应端口......" ******
            SetUpProgressBarProcessing(27);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_OpenFireWallPort").ToString();
            MainWindowsShowInfo(currentStatus);
            string openFireWallPort = ReceiveConfigurationParameters[1];
            if (String.IsNullOrEmpty(client.RunCommand("command -v firewall-cmd").Result) == false)
            {
                if (String.Equals(openFireWallPort, "443"))
                {
                    sshShellCommand = @"firewall-cmd --zone=public --add-port=80/tcp --permanent";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"firewall-cmd --zone=public --add-port=80/udp --permanent";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"firewall-cmd --zone=public --add-port=443/tcp --permanent";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"firewall-cmd --zone=public --add-port=443/udp --permanent";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"yes | firewall-cmd --reload";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                }
                else
                {
                    sshShellCommand = $"firewall-cmd --zone=public --add-port={openFireWallPort}/tcp --permanent";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = $"firewall-cmd --zone=public --add-port={openFireWallPort}/udp --permanent";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"yes | firewall-cmd --reload";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                }
            }
            else if (String.IsNullOrEmpty(client.RunCommand("command -v ufw").Result) == false)
            {
                if (String.Equals(openFireWallPort, "443"))
                {
                    sshShellCommand = @"ufw allow 80/tcp";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"ufw allow 80/udp";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"ufw allow 443/tcp";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"ufw allow 443/udp";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"yes | ufw reload";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                }
                else
                {
                    sshShellCommand = $"ufw allow {openFireWallPort}/tcp";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = $"ufw allow {openFireWallPort}/udp";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = @"yes | ufw reload";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                }
            }

           

            SetUpProgressBarProcessing(30);
            return true;
        }

        //检测校对时间 31--33
        //functionResult = CheckProofreadingTime(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool CheckProofreadingTime(SshClient client)
        {
            //******"校对时间......"******09
            SetUpProgressBarProcessing(31);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_ProofreadingTime").ToString();
            MainWindowsShowInfo(currentStatus);

            //获取远程主机的时间戳
            long timeStampVPS = Convert.ToInt64(client.RunCommand("date +%s").Result.ToString());

            //获取本地时间戳
            TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long timeStampLocal = Convert.ToInt64(ts.TotalSeconds);
            if (Math.Abs(timeStampLocal - timeStampVPS) >= 90)
            {
                //******"本地时间与远程主机时间相差超过限制(90秒)，请先用 '系统工具-->时间校对' 校对时间后再设置"******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_TimeError").ToString());
                //"时间较对失败......"
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_TimeError").ToString();
                MainWindowsShowInfo(currentStatus);

                //client.Disconnect();
                return false;
            }
            //******"时间差符合要求，OK!"******10
            SetUpProgressBarProcessing(33);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_TimeOK").ToString();
            MainWindowsShowInfo(currentStatus);
            return true;
        }

       
        //检测域名是否解析到当前IP上 34---36
        //functionResult = DomainResolutionCurrentIPDetect(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool DomainResolutionCurrentIPDetect(SshClient client)
        {
            //****** "正在检测域名是否解析到当前VPS的IP上......" ******11
            SetUpProgressBarProcessing(34);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestDomainResolve").ToString();
            MainWindowsShowInfo(currentStatus);

            //再次初始化相关变量
            ipv4 = String.Empty;
            ipv6 = String.Empty;
            onlyIpv6 = false;

            //sshShellCommand = @"curl -4 ip.sb";
            sshShellCommand = @"curl -s https://api.ip.sb/ip --ipv4";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            ipv4 = currentShellCommandResult.TrimEnd('\r', '\n');

            sshShellCommand = @"curl -s https://api.ip.sb/ip --ipv6";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            ipv6 = currentShellCommandResult.TrimEnd('\r', '\n');

            if (String.IsNullOrEmpty(ipv4) == false)
            {
                string nativeIp = ipv4;

                sshShellCommand = "ping " + ReceiveConfigurationParameters[4] + " -c1 | grep -oE -m1 \"([0-9]{1,3}\\.){3}[0-9]{1,3}\"";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                string resultTestDomainCmd = currentShellCommandResult.TrimEnd('\r', '\n');
                if (String.Equals(nativeIp, resultTestDomainCmd) == true)
                {
                    //****** "解析正确！OK!" ******12
                    SetUpProgressBarProcessing(36);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_DomainResolveOK").ToString();
                    MainWindowsShowInfo(currentStatus);
                    onlyIpv6 = false;
                    return true;
                }
                else
                {
                    //****** "域名未能正确解析到当前VPS的IP上!安装失败！" ******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_ErrorDomainResolve").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //****** "域名未能正确解析到当前VPS的IP上，请检查！若解析设置正确，请等待生效后再重试安装。如果域名使用了CDN，请先关闭！" ******
                    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorDomainResolve").ToString());
                    //client.Disconnect();
                    return false;
                }
            }
            else
            {
                if (String.IsNullOrEmpty(ipv6) == false)
                {
                    string nativeIp = ipv6;

                    //sshShellCommand = "ping6 " + ReceiveConfigurationParameters[4] + " -c1 | grep -oE -m1 \"([0-9]{1,3}\\.){3}[0-9]{1,3}\"";
                    sshShellCommand = $"dig @resolver1.opendns.com AAAA {ReceiveConfigurationParameters[4]} +short -6";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    string resultTestDomainCmd = currentShellCommandResult.TrimEnd('\r', '\n');
                    if (String.Equals(nativeIp, resultTestDomainCmd) == true)
                    {
                        //****** "解析正确！OK!" ******12
                        SetUpProgressBarProcessing(36);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_DomainResolveOK").ToString();
                        MainWindowsShowInfo(currentStatus);
                        onlyIpv6 = true;
                        //apiGithubCom = "api.githubipv6.ga";
                        //scriptGithubUrl = "raw.githubipv6.ga";
                        //githubCom = "github.githubipv6.ga";
                        //纯ipv6主机，目前暂不支持
                        //currentStatus = Application.Current.FindResource("DisplayInstallInfo_OnlyIpv6").ToString();
                        //MainWindowsShowInfo(currentStatus);
                        //MessageBox.Show(currentStatus);
                        return true;
                    }
                    else
                    {
                        //****** "域名未能正确解析到当前VPS的IP上!安装失败！" ******
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_ErrorDomainResolve").ToString();
                        MainWindowsShowInfo(currentStatus);

                        //****** "域名未能正确解析到当前VPS的IP上，请检查！若解析设置正确，请等待生效后再重试安装。如果域名使用了CDN，请先关闭！" ******
                        MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorDomainResolve").ToString());
                        //client.Disconnect();
                        return false;
                    }
                    
                }
                else
                {
                    //****** "域名未能正确解析到当前VPS的IP上!安装失败！" ******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_ErrorDomainResolve").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //****** "域名未能正确解析到当前VPS的IP上，请检查！若解析设置正确，请等待生效后再重试安装。如果域名使用了CDN，请先关闭！" ******
                    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorDomainResolve").ToString());
                    //client.Disconnect();
                    return false;
                }
            }
            
            return true;
        }


        //安装代理程序 37--40
        private bool SoftInstall()
        {
            return true;
        }
      
        //程序是否安装成功检测并设置开机启动 41--43
        //soft--要检测的程序
        //condition---成功安装的条件
        //functionResult = SoftInstalledSuccessOrFail(client,"v2ray",@"/usr/local/bin/v2ray");
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool SoftInstalledSuccessOrFail(SshClient client,string soft,string condition)
        {
            SetUpProgressBarProcessing(41);
            sshShellCommand = $"find / -name {soft}";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            if (!currentShellCommandResult.Contains($"{condition}"))
            {
                //****** "安装失败,官方脚本运行出错！" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorInstallSoftFail").ToString());
                //****** "安装失败,官方脚本运行出错！" ******
                currentStatus = Application.Current.FindResource("MessageBoxShow_ErrorInstallSoftFail").ToString();
                MainWindowsShowInfo(currentStatus);

                //client.Disconnect();
                return false;
            }
            else
            {
                //****** "安装成功！" ******20
                SetUpProgressBarProcessing(43);
                currentStatus = $"{soft}" + Application.Current.FindResource("DisplayInstallInfo_SoftInstallSuccess").ToString();
                MainWindowsShowInfo(currentStatus);

                //开机启动
                sshShellCommand = $"systemctl enable {soft}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            }
            return true;
        }


        //生成服务端配置 44--46
        //functionResult = GenerateServerConfiguration(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }

       
        //上传服务配置 47--50


        //acme.sh安装与申请证书 51--57
        //functionResult = AcmeShInstall(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool AcmeShInstall(SshClient client)
        {

            //****** "正在安装acme.sh......" ******22
            SetUpProgressBarProcessing(51);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallAcmeSh").ToString();
            MainWindowsShowInfo(currentStatus);

            //安装所依赖的软件
            sshShellCommand = $"{sshCmdUpdate}";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = $"{sshCmdInstall}socat";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            //解决搬瓦工CentOS缺少问题
            sshShellCommand = $"{sshCmdInstall}automake autoconf libtool";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = $"curl https://raw.githubusercontent.com/acmesh-official/acme.sh/master/acme.sh  | INSTALLONLINE=1  sh";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            if (currentShellCommandResult.Contains("Install success") == true)
            {
                //****** "acme.sh安装成功！" ******23
                SetUpProgressBarProcessing(54);
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_AcmeShInstallSuccess").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            else
            {
                //****** "acme.sh安装失败！原因未知，请向开发者提问！" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_ErrorAcmeShInstallFail").ToString();
                MainWindowsShowInfo(currentStatus);
                return false;
            }

            sshShellCommand = @"cd ~/.acme.sh/";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            sshShellCommand = @"alias acme.sh=~/.acme.sh/acme.sh";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            //****** "申请域名证书......" ******24
            SetUpProgressBarProcessing(55);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartApplyCert").ToString();
            MainWindowsShowInfo(currentStatus);

            if (onlyIpv6 == true)
            {
                sshShellCommand = $"/root/.acme.sh/acme.sh --force --issue  --standalone  -d {ReceiveConfigurationParameters[4]} --listen-v6";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            }
            else
            {
                sshShellCommand = $"/root/.acme.sh/acme.sh --force --issue  --standalone  -d {ReceiveConfigurationParameters[4]}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            }

            if (currentShellCommandResult.Contains("Cert success") == true)
            {
                //****** "证书申请成功！" ******25
                SetUpProgressBarProcessing(57);
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_ApplyCertSuccess").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            else
            {
                //****** "证书申请失败！原因未知，请向开发者提问！" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_ApplyCertFail").ToString();
                MainWindowsShowInfo(currentStatus);
                return false;
            }
            return true;

        }



        //安装证书到代理程序 58--60
        private bool CertInstallProxy(SshClient client)
        {
            return true;
        }
        

        //Caddy安装与检测安装是否成功 61--66
        //functionResult = CaddyInstall(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool CaddyInstall(SshClient client)
        {
            //****** "安装Caddy......" ******28
            SetUpProgressBarProcessing(61);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartInstallCaddy").ToString();
            MainWindowsShowInfo(currentStatus);

            //为真则表示系统有相应的组件。
            if (getApt == true)
            {

                sshShellCommand = @"echo ""deb [trusted=yes] https://apt.fury.io/caddy/ /"" | tee -a /etc/apt/sources.list.d/caddy-fury.list";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"apt install -y apt-transport-https";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"apt -qq update";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"apt -y -qq install caddy";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }
            else if (getDnf == true)
            {

                sshShellCommand = @"dnf install 'dnf-command(copr)' -y";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"dnf copr enable @caddy/caddy -y";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                //sshShellCommand = @"dnf -q makecache";
                //currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"dnf -y -q install caddy";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }
            else if (getYum == true)
            {

                sshShellCommand = @"yum install yum-plugin-copr -y";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"yum copr enable @caddy/caddy -y";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                //sshShellCommand = @"yum -q makecache";
                //currentShellCommandResult = MainWindowsShowCmd(client,sshShellCommand);

                sshShellCommand = @"yum -y -q install caddy";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"sed -i 's/AmbientCapabilities/#AmbientCapabilities/g' /usr/lib/systemd/system/caddy.service";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"sed -i 's/=caddy/=root/g' /usr/lib/systemd/system/caddy.service";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"systemctl daemon-reload";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            }

            sshShellCommand = @"find / -name caddy";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            if (!currentShellCommandResult.Contains("/usr/bin/caddy"))
            {
                //****** "安装Caddy失败！" ******
                MessageBox.Show(Application.Current.FindResource("DisplayInstallInfo_ErrorInstallCaddyFail").ToString());
                //****** "安装Caddy失败！" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_ErrorInstallCaddyFail").ToString();
                MainWindowsShowInfo(currentStatus);

                //client.Disconnect();
                return false;
            }
            //****** "Caddy安装成功！" ******29
            SetUpProgressBarProcessing(66);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_InstalledCaddyOK").ToString();
            MainWindowsShowInfo(currentStatus);


            sshShellCommand = @"systemctl enable caddy";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            return true;
        }


        //上传Caddy配置文件67--70
        private bool UpConfigCaddy(SshClient client)
        {
            return true;
        }

        //程序启动检测
        //soft--要检测的程序
        //condition---成功启动的条件
        //functionResult = SoftStartDetect(client, "v2ray", @"/usr/local/bin/v2ray");
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool SoftStartDetect(SshClient client,string soft,string condition)
        {
            //****** "正在启动......" ******33
            //SetUpProgressBarProcessing(87);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartSoft").ToString() + $"{soft}......";
            MainWindowsShowInfo(currentStatus);

            //启动服务
            sshShellCommand = $"systemctl restart {soft}";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            Thread.Sleep(3000);

            sshShellCommand = $"ps aux | grep {soft}";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            if (currentShellCommandResult.Contains($"{condition}") == true)
            {
                //****** "启动成功！" ******34
                //SetUpProgressBarProcessing(88);
                currentStatus = $"{soft}" + Application.Current.FindResource("DisplayInstallInfo_StartSoftOK").ToString();
                MainWindowsShowInfo(currentStatus);

            }
            else
            {
                //****** "启动失败！" ******
                currentStatus = $"{soft}" + Application.Current.FindResource("DisplayInstallInfo_StartSoftFail").ToString();
                MainWindowsShowInfo(currentStatus);

                Thread.Sleep(1000);

                //****** "正在启动（第二次尝试）！" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_StartSoftSecond").ToString() + $"{soft}";
                MainWindowsShowInfo(currentStatus);

                Thread.Sleep(3000);
                sshShellCommand = $"systemctl restart {soft}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                Thread.Sleep(3000);

                sshShellCommand = $"ps aux | grep {soft}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                if (currentShellCommandResult.Contains($"{condition}") == true)
                {
                    //****** "启动成功！" ******
                    currentStatus = $"{soft}" + Application.Current.FindResource("DisplayInstallInfo_StartSoftOK").ToString();
                    MainWindowsShowInfo(currentStatus);

                }
                else
                {
                    //****** "启动失败(第二次)！退出安装！" ******
                    currentStatus = $"{soft}" + Application.Current.FindResource("DisplayInstallInfo_StartSoftSecondFail").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //显示启动失败原因
                    sshShellCommand = $"journalctl -n50 --no-pager --boot -u  {soft}";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //****** "启动失败，原因如上！请排查原因！" ******
                    currentStatus = $"{soft}" + Application.Current.FindResource("DisplayInstallInfo_StartSoftFailedExit").ToString();
                    MainWindowsShowInfo(currentStatus);

                    //****** "启动失败，原因如上！请排查原因！" ******
                    MessageBox.Show($"{soft}" + Application.Current.FindResource("DisplayInstallInfo_StartSoftFailedExit").ToString());
                    return false;
                }
            }
            return true;
        }

        //检测BBR，满足条件并启动 90--95
        //functionResult = DetectBBRandEnable(client);
        //if (functionResult == false) { FunctionResultErr(); client.Disconnect(); return; }
        private bool DetectBBRandEnable(SshClient client)
        {
            //测试BBR条件，若满足提示是否启用
            //****** "BBR测试......" ******37
            SetUpProgressBarProcessing(90);
            currentStatus = Application.Current.FindResource("DisplayInstallInfo_TestBBR").ToString();
            MainWindowsShowInfo(currentStatus);

            sshShellCommand = @"uname -r";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            string[] linuxKernelVerStrBBR = currentShellCommandResult.Split('-');

            bool detectResultBBR = DetectKernelVersionBBR(linuxKernelVerStrBBR[0]);

            sshShellCommand = @"sysctl net.ipv4.tcp_congestion_control | grep bbr";
            currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

            string resultCmdTestBBR = currentShellCommandResult;
            //如果内核满足大于等于4.9，且还未启用BBR，则启用BBR
            if (detectResultBBR == true && resultCmdTestBBR.Contains("bbr") == false)
            {
                //****** "正在启用BBR......" ******38
                SetUpProgressBarProcessing(93);
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableBBR").ToString();
                MainWindowsShowInfo(currentStatus);

                sshShellCommand = @"bash -c 'echo ""net.core.default_qdisc=fq"" >> /etc/sysctl.conf'";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"bash -c 'echo ""net.ipv4.tcp_congestion_control=bbr"" >> /etc/sysctl.conf'";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"sysctl -p";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                sshShellCommand = @"sysctl net.ipv4.tcp_congestion_control | grep bbr";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                if (currentShellCommandResult.Contains("bbr") == true)
                {
                    //******  "BBR启用成功！" ******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_BBREnabledSuccess").ToString();
                    MainWindowsShowInfo(currentStatus);
                }
                else
                {
                    //****** "系统不满足启用BBR的条件，启用失败！" ******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_BBRFailed").ToString();
                    MainWindowsShowInfo(currentStatus);
                    //return false;
                }
            }
            else if (resultCmdTestBBR.Contains("bbr") == true)
            {
                //******  "BBR已经启用了！" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_BBRisEnabled").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            else
            {
                //****** "系统不满足启用BBR的条件，启用失败！" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_BBRFailed").ToString();
                MainWindowsShowInfo(currentStatus);
                //return false;
            }

            //如果是纯ipv6主机，则需要删除前面设置的Nat64网关
            if (onlyIpv6 == true)
            {
                SetUpNat64(client, false);
                sshShellCommand = $"{sshCmdUpdate}";
                currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
            }

            SetUpProgressBarProcessing(95);
            return true;
        }



        //生成客户端配置 96--98


        #endregion

        #region 启用Root密码登录
        private void ButtonEnableRootPassWord_Click(object sender, RoutedEventArgs e)
        {
            //******"本功能需要当前登录的账户具有sudo权限，是否为远程主机启用root账户并设置密码？"******
            string messageShow = Application.Current.FindResource("MessageBoxShow_EnableRootPassword").ToString();
            MessageBoxResult messageBoxResult = MessageBox.Show(messageShow, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ConnectionInfo connectionInfo = GenerateConnectionInfo();
                if (connectionInfo == null)
                {
                    //****** "远程主机连接信息有误，请检查!" ******
                    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                    return;
                }

                ReceiveConfigurationParameters[4] = TextBoxHost.Text;//传递主机地址
                ReceiveConfigurationParameters[2] = PasswordBoxHostPassword.Password;//传递当前账户密码

                installationDegree = 0;
                TextBoxMonitorCommandResults.Text = "";
                Thread thread = new Thread(() => EnableRootPassWord(connectionInfo));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }
        //启用Root密码登录进程
        private void EnableRootPassWord(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;
            onlyIpv6 = false;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(5);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    //******"检测是否运行在root权限下..."******01
                    SetUpProgressBarProcessing(5);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_DetectionRootPermission").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"id -u";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    if (currentShellCommandResult.TrimEnd('\r','\n').Equals("0") == true)
                    {
                        //******"当前账户已经具有root权限，无需再设置！"******
                        currentStatus = Application.Current.FindResource("MessageBoxShow_AlreadyRoot").ToString();
                        MainWindowsShowInfo(currentStatus);
                        MessageBox.Show(currentStatus);
                        client.Disconnect();
                        return;
                    }

                    SetUpProgressBarProcessing(10);
                    string hostPassword = "'" + ReceiveConfigurationParameters[2] + "'";
                    //MessageBox.Show(hostPassword);
                    sshShellCommand = $"echo {hostPassword} | sudo -S id -u";
                    //MessageBox.Show(sshShellCommand);
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    if (currentShellCommandResult.TrimEnd('\r', '\n').Equals("0") == false)
                    {
                        //******"当前账户无法获取sudo权限，设置失败！"******
                        currentStatus = Application.Current.FindResource("MessageBoxShow_NoSudoToAccount").ToString();
                        MainWindowsShowInfo(currentStatus);
                        MessageBox.Show(currentStatus);
                        client.Disconnect();
                        return;
                    }

                    SetUpProgressBarProcessing(20);
                    string cmdPre = $"echo {hostPassword} | sudo -S id -u" + ';';
                    sshShellCommand = cmdPre + @"sudo sed -i 's/PermitRootLogin /#PermitRootLogin /g' /etc/ssh/sshd_config";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = cmdPre + @"sudo sed -i 's/PasswordAuthentication /#PasswordAuthentication /g' /etc/ssh/sshd_config";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    SetUpProgressBarProcessing(30);
                    sshShellCommand = cmdPre + @"sudo sed -i 's/PermitEmptyPasswords /#PermitEmptyPasswords /g' /etc/ssh/sshd_config";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = cmdPre + @"echo 'PermitRootLogin yes' | sudo tee -a /etc/ssh/sshd_config";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    SetUpProgressBarProcessing(40);
                    sshShellCommand = cmdPre + @"echo 'PasswordAuthentication yes' | sudo tee -a /etc/ssh/sshd_config";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    sshShellCommand = cmdPre + @"echo 'PermitEmptyPasswords no' | sudo tee -a /etc/ssh/sshd_config";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    SetUpProgressBarProcessing(60);
                    sshShellCommand = cmdPre + @"sudo systemctl restart sshd";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    //******"生成20位随机密码！"******
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_GenerateRandomPassword").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"cat /dev/urandom | tr -dc '_A-Z#\-+=a-z(0-9%^>)]{<|' | head -c 20 ; echo ''";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    SetUpProgressBarProcessing(80);
                    string setPassword = currentShellCommandResult.TrimEnd('\r', '\n') + '\n';

                    sshShellCommand = cmdPre + $"echo -e \"{setPassword}{setPassword}\" | sudo passwd root";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    client.Disconnect();

                    //***保存密码信息***
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableRootPasswordSavePasswordInfo").ToString();
                    MainWindowsShowInfo(currentStatus);

                    string filePath = ReceiveConfigurationParameters[4].Replace(':', '_');
                    CheckDir(filePath);
                    using (StreamWriter sw = new StreamWriter($"{filePath}\\host_password_info.txt"))
                    {
                        sw.WriteLine(ReceiveConfigurationParameters[4]);
                        sw.WriteLine("root");
                        sw.WriteLine(setPassword);
                    }

                    SetUpProgressBarProcessing(100);
                    //***远程主机Root账户密码登录已启用，密码保存在随后打开的文件夹中！***
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableRootPasswordSuccess").ToString();
                    MainWindowsShowInfo(currentStatus);
                    MessageBox.Show(currentStatus);
                    System.Diagnostics.Process.Start("explorer.exe", filePath);
                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "主机登录失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }

        #endregion

        #region 启用Root证书密钥登录
        private void ButtonEnableRootCert_Click(object sender, RoutedEventArgs e)
        {
            //******"本功能需要当前登录的账户具有root或者sudo权限，是否为远程主机启用root证书密钥登录？"******
            string messageShow = Application.Current.FindResource("MessageBoxShow_ButtonEnableRootCert").ToString();
            MessageBoxResult messageBoxResult = MessageBox.Show(messageShow, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ConnectionInfo connectionInfo = GenerateConnectionInfo();
                if (connectionInfo == null)
                {
                    //****** "远程主机连接信息有误，请检查!" ******
                    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                    return;
                }

                ReceiveConfigurationParameters[4] = TextBoxHost.Text;//传递主机地址
                ReceiveConfigurationParameters[2] = PasswordBoxHostPassword.Password;

                installationDegree = 0;
                TextBoxMonitorCommandResults.Text = "";
                Thread thread = new Thread(() => EnableRootCert(connectionInfo));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        //启用Root证书密钥登录进程
        private void EnableRootCert(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;
            onlyIpv6 = false;

            string filePath = String.Empty;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(5);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    //******"检测是否运行在root权限下..."******01
                    SetUpProgressBarProcessing(5);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_DetectionRootPermission").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"id -u";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    if (currentShellCommandResult.TrimEnd('\r', '\n').Equals("0") == true)
                    {
                        SetUpProgressBarProcessing(20);

                        //***正在生成密钥......***
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableRootCertGenerateCert").ToString();
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"rm -rf /tmp/rootuser.key /tmp/rootuser.key.pub";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"yes | ssh-keygen -b 2048 -t rsa -f /tmp/rootuser.key -q -N ''";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"yes | ssh-keygen -p -P '' -N '' -m PEM -f /tmp/rootuser.key";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"mkdir -p /root/.ssh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"cat /tmp/rootuser.key.pub | tee -a /root/.ssh/authorized_keys";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"chmod 777 /tmp/rootuser.key";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(30);
                        //***正在下载密钥......***
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableRootCertDownloadCert").ToString();
                        MainWindowsShowInfo(currentStatus);
                       
                        filePath = CreateConfigSaveDir(@"root_cert", ReceiveConfigurationParameters[4].Replace(':', '_'));
                        string localConfigSavePathAndFileName = $"{filePath}\\rootuser.key";
                        string remoteConfigPathAndFileName = @"/tmp/rootuser.key";
                        DownloadConfig(connectionInfo, localConfigSavePathAndFileName, remoteConfigPathAndFileName);

                        localConfigSavePathAndFileName = $"{filePath}\\rootuser.key.pub";
                        remoteConfigPathAndFileName = @"/tmp/rootuser.key.pub";
                        DownloadConfig(connectionInfo, localConfigSavePathAndFileName, remoteConfigPathAndFileName);

                        sshShellCommand = @"rm -rf /tmp/rootuser.key /tmp/rootuser.key.pub";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(50);
                        //***远程主机启用密钥登录......***
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableRootCertSetCertEnable").ToString();
                        MainWindowsShowInfo(currentStatus);
                        
                        sshShellCommand = @"sed -i 's/PermitRootLogin /#PermitRootLogin /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"sed -i 's/StrictModes /#StrictModes /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(70);
                        sshShellCommand = @"sed -i 's/PubkeyAuthentication /#PubkeyAuthentication /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"sed -i 's/#AuthorizedKeysFile /AuthorizedKeysFile /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(80);
                        sshShellCommand = @"sed -i 's/#RSAAuthentication /RSAAuthentication /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"echo 'PermitRootLogin yes' | tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(90);
                        sshShellCommand = @"echo 'StrictModes no' | tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"echo 'PubkeyAuthentication yes' | tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl restart sshd";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    }
                    else
                    {
                        SetUpProgressBarProcessing(10);
                        string hostPassword = "'" + ReceiveConfigurationParameters[2] + "'";
                        //MessageBox.Show(hostPassword);
                        sshShellCommand = $"echo {hostPassword} | sudo -S id -u";
                        //MessageBox.Show(sshShellCommand);
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        //MessageBox.Show(currentShellCommandResult);
                        if (currentShellCommandResult.TrimEnd('\r', '\n').Equals("0") == false)
                        {
                            //******"当前账户无法获取sudo权限，设置失败！"******
                            currentStatus = Application.Current.FindResource("MessageBoxShow_NoSudoToAccount").ToString();
                            MainWindowsShowInfo(currentStatus);
                            MessageBox.Show(currentStatus);
                            client.Disconnect();
                            return;
                        }

                        SetUpProgressBarProcessing(20);
                        string cmdPre = $"echo {hostPassword} | sudo -S id -u" + ';';
                        //***正在生成密钥......***
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableRootCertGenerateCert").ToString();
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = cmdPre + @"sudo rm -rf /tmp/rootuser.key /tmp/rootuser.key.pub";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"yes | sudo ssh-keygen -b 2048 -t rsa -f /tmp/rootuser.key -q -N ''";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"yes | sudo ssh-keygen -p -P '' -N '' -m PEM -f /tmp/rootuser.key";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"sudo mkdir -p /root/.ssh";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"cat /tmp/rootuser.key.pub | sudo tee -a /root/.ssh/authorized_keys";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"sudo chmod 777 /tmp/rootuser.key";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(30);
                        //***正在下载密钥......***
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableRootCertDownloadCert").ToString();
                        MainWindowsShowInfo(currentStatus);

                        filePath = CreateConfigSaveDir(@"root_cert", ReceiveConfigurationParameters[4].Replace(':', '_'));
                        string localConfigSavePathAndFileName = $"{filePath}\\rootuser.key";
                        string remoteConfigPathAndFileName = @"/tmp/rootuser.key";
                        DownloadConfig(connectionInfo, localConfigSavePathAndFileName, remoteConfigPathAndFileName);

                        localConfigSavePathAndFileName = $"{filePath}\\rootuser.key.pub";
                        remoteConfigPathAndFileName = @"/tmp/rootuser.key.pub";
                        DownloadConfig(connectionInfo, localConfigSavePathAndFileName, remoteConfigPathAndFileName);

                        sshShellCommand = cmdPre + @"sudo rm -rf /tmp/rootuser.key /tmp/rootuser.key.pub";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(50);
                        //***远程主机启用密钥登录......***
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_EnableRootCertSetCertEnable").ToString();
                        MainWindowsShowInfo(currentStatus);

                        //string cmdPre = $"echo {hostPassword} | sudo -S id -u" + ';';
                        sshShellCommand = cmdPre + @"sudo sed -i 's/PermitRootLogin /#PermitRootLogin /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"sudo sed -i 's/StrictModes /#StrictModes /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(70);
                        sshShellCommand = cmdPre + @"sudo sed -i 's/PubkeyAuthentication /#PubkeyAuthentication /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"sudo sed -i 's/#AuthorizedKeysFile /AuthorizedKeysFile /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(80);
                        sshShellCommand = cmdPre + @"sudo sed -i 's/#RSAAuthentication /RSAAuthentication /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"echo 'PermitRootLogin yes' | sudo tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        SetUpProgressBarProcessing(90);
                        sshShellCommand = cmdPre + @"echo 'StrictModes no' | sudo tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"echo 'PubkeyAuthentication yes' | sudo tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"sudo systemctl restart sshd";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    }
                    client.Disconnect();

                    SetUpProgressBarProcessing(100);
                    //******"远程主机root账户证书密钥登录已启用，密钥文件rootuser.key保存在随后打开的文件夹中！"******
                    currentStatus = Application.Current.FindResource("MessageBoxShow_ButtonEnableRootCertSuccess").ToString();
                    MainWindowsShowInfo(currentStatus);
                    MessageBox.Show(currentStatus);
                    System.Diagnostics.Process.Start("explorer.exe", filePath);

                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "主机登录失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion

        }


        #endregion

        //root禁止密码登录
        private void ButtonRootProhibitsPasswordLogin_Click(object sender, RoutedEventArgs e)
        {
            //******"本功能需要远程主机已经开启了其他登录方式，如密钥方式等，否则将可能造成远程主机无法连接，是否禁止远程主机的root账户密码登录方式？"******
            string messageShow = Application.Current.FindResource("MessageBoxShow_ButtonRootProhibitsPasswordLogin").ToString();
            MessageBoxResult messageBoxResult = MessageBox.Show(messageShow, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ConnectionInfo connectionInfo = GenerateConnectionInfo();
                if (connectionInfo == null)
                {
                    //****** "远程主机连接信息有误，请检查!" ******
                    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_ErrorHostConnection").ToString());
                    return;
                }

                ReceiveConfigurationParameters[4] = TextBoxHost.Text;//传递主机地址
                ReceiveConfigurationParameters[2] = PasswordBoxHostPassword.Password;//传递主机密码

                installationDegree = 0;
                TextBoxMonitorCommandResults.Text = "";
                Thread thread = new Thread(() => RootProhibitsPasswordLogin(connectionInfo));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        //禁止root密码登录进程
        private void RootProhibitsPasswordLogin(ConnectionInfo connectionInfo)
        {
            functionResult = true;
            getApt = false;
            getDnf = false;
            getYum = false;
            onlyIpv6 = false;

            string filePath = String.Empty;

            //******"正在登录远程主机......"******
            SetUpProgressBarProcessing(1);
            string currentStatus = Application.Current.FindResource("DisplayInstallInfo_Login").ToString();
            MainWindowsShowInfo(currentStatus);

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
                        //******"主机登录成功"******
                        SetUpProgressBarProcessing(5);
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginSuccessful").ToString();
                        MainWindowsShowInfo(currentStatus);
                    }

                    //检测root权限 5--7
                    //******"检测是否运行在root权限下..."******01
                    SetUpProgressBarProcessing(5);
                    currentStatus = Application.Current.FindResource("DisplayInstallInfo_DetectionRootPermission").ToString();
                    MainWindowsShowInfo(currentStatus);

                    sshShellCommand = @"id -u";
                    currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    if (currentShellCommandResult.TrimEnd('\r', '\n').Equals("0") == true)
                    {
                        SetUpProgressBarProcessing(20);

                        //***正在关闭root账户密码登录方式......***
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_SetRootProhibitsPasswordLogin").ToString();
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = @"sed -i 's/PasswordAuthentication /#PasswordAuthentication /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"sed -i 's/PermitEmptyPasswords /#PermitEmptyPasswords /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"echo 'PasswordAuthentication no' | tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"echo 'PermitEmptyPasswords no' | tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = @"systemctl restart sshd";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                    }
                    else
                    {
                        SetUpProgressBarProcessing(10);
                        string hostPassword = "'" + ReceiveConfigurationParameters[2] + "'";
                        //MessageBox.Show(hostPassword);
                        sshShellCommand = $"echo {hostPassword} | sudo -S id -u";
                        //MessageBox.Show(sshShellCommand);
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);
                        //MessageBox.Show(currentShellCommandResult);
                        if (currentShellCommandResult.TrimEnd('\r', '\n').Equals("0") == false)
                        {
                            //******"当前账户无法获取sudo权限，设置失败！"******
                            currentStatus = Application.Current.FindResource("MessageBoxShow_NoSudoToAccount").ToString();
                            MainWindowsShowInfo(currentStatus);
                            MessageBox.Show(currentStatus);
                            client.Disconnect();
                            return;
                        }

                        SetUpProgressBarProcessing(20);
                        string cmdPre = $"echo {hostPassword} | sudo -S id -u" + ';';

                        //***正在关闭root账户密码登录方式......***
                        currentStatus = Application.Current.FindResource("DisplayInstallInfo_SetRootProhibitsPasswordLogin").ToString();
                        MainWindowsShowInfo(currentStatus);

                        sshShellCommand = cmdPre + @"sudo sed -i 's/PasswordAuthentication /#PasswordAuthentication /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"sudo sed -i 's/PermitEmptyPasswords /#PermitEmptyPasswords /g' /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"echo 'PasswordAuthentication no' | sudo tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"echo 'PermitEmptyPasswords no' | sudo tee -a /etc/ssh/sshd_config";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                        sshShellCommand = cmdPre + @"sudo systemctl restart sshd";
                        currentShellCommandResult = MainWindowsShowCmd(client, sshShellCommand);

                    }
                    client.Disconnect();

                    SetUpProgressBarProcessing(100);
                    //******"远程主机root账户密码登录方式已关闭！"******
                    currentStatus = Application.Current.FindResource("MessageBoxShow_RootProhibitsPasswordLoginOK").ToString();
                    MainWindowsShowInfo(currentStatus);
                    MessageBox.Show(currentStatus);

                    return;
                }
            }
            catch (Exception ex1)//例外处理   
            #region 例外处理
            {
                ProcessException(ex1.Message);

                //****** "主机登录失败!" ******
                currentStatus = Application.Current.FindResource("DisplayInstallInfo_LoginFailed").ToString();
                MainWindowsShowInfo(currentStatus);
            }
            #endregion
        }
    }

}
