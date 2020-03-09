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
using Renci.SshNet;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProxySU
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RadioButtonPasswordLogin.IsChecked = true;
            RadioButtonNoProxy.IsChecked = true;
            RadioButtonProxyNoLogin.IsChecked = true;
        }
        //System.Diagnostics.Process exitProgram = System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id);
        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            //byte[] expectedFingerPrint = new byte[] {
            //                                0x66, 0x31, 0xaf, 0x00, 0x54, 0xb9, 0x87, 0x31,
            //                                0xff, 0x58, 0x1c, 0x31, 0xb1, 0xa2, 0x4c, 0x6b
            //                            };
            if (string.IsNullOrEmpty(TextBoxHost.Text) == true || string.IsNullOrEmpty(TextBoxPort.Text) == true || string.IsNullOrEmpty(TextBoxUserName.Text) == true)
            {
                MessageBox.Show("主机地址、主机端口、用户名为必填项，不能为空");
                //exitProgram.Kill();
                return;
            }
            string sshHostName = TextBoxHost.Text.ToString();
            int sshPort = int.Parse(TextBoxPort.Text);
            string sshUser = TextBoxUserName.Text.ToString();
            if (RadioButtonPasswordLogin.IsChecked == true && string.IsNullOrEmpty(PasswordBoxHostPassword.Password) == true)
            {
                MessageBox.Show("登录密码为必填项，不能为空");
                //exitProgram.Kill();
                return;
            }
            string sshPassword = PasswordBoxHostPassword.Password.ToString();
            if (RadioButtonCertLogin.IsChecked == true && string.IsNullOrEmpty(TextBoxCertFilePath.Text) == true)
            {
                MessageBox.Show("密钥文件为必填项，不能为空");
                //exitProgram.Kill();
                return;
            }
            string sshPrivateKey = TextBoxCertFilePath.Text.ToString();
            ProxyTypes proxyTypes = new ProxyTypes();//默认为None
            //MessageBox.Show(proxyTypes.ToString());
            //proxyTypes = ProxyTypes.Socks5;
            if (RadioButtonHttp.IsChecked==true)
            {
                proxyTypes = ProxyTypes.Http;
            }
            else if (RadioButtonSocks4.IsChecked==true)
            {
                proxyTypes = ProxyTypes.Socks4;
            }
            else if (RadioButtonSocks5.IsChecked==true)
            {
                proxyTypes = ProxyTypes.Socks5;
            }
            else
            {
                proxyTypes = ProxyTypes.None;
            }

            //MessageBox.Show(proxyTypes.ToString());
            if (RadioButtonNoProxy.IsChecked==false&&(string.IsNullOrEmpty(TextBoxProxyHost.Text)==true||string.IsNullOrEmpty(TextBoxProxyPort.Text)==true))
            {
                MessageBox.Show("如果选择了代理，则代理地址与端口不能为空");
                //exitProgram.Kill();
                return;
            }
            string sshProxyHost = TextBoxProxyHost.Text.ToString();
            int sshProxyPort = int.Parse(TextBoxProxyPort.Text.ToString());
            if (RadiobuttonProxyYesLogin.IsChecked == true && (string.IsNullOrEmpty(TextBoxProxyUserName.Text) == true || string.IsNullOrEmpty(PasswordBoxProxyPassword.Password) == true))
            {
                MessageBox.Show("如果代理需要登录，则代理登录的用户名与密码不能为空");
                //exitProgram.Kill();
                return;
            }
            string sshProxyUser = TextBoxProxyUserName.Text.ToString();
            string sshProxyPassword = PasswordBoxProxyPassword.Password.ToString();

            //TextBlockSetUpProcessing.Text = "登录中";
            //ProgressBarSetUpProcessing.IsIndeterminate = true;

           
            try
            {

                //var connectionInfo = new PasswordConnectionInfo(sshHostName, sshPort, sshUser, sshPassword);

                var connectionInfo = new ConnectionInfo(
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

                //using (var client = new SshClient(sshHostName, sshPort, sshUser, sshPassword))
                using (var client = new SshClient(connectionInfo))

                {
                    #region
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
                    //if (client.IsConnected == true)
                    //{
                    //    TextBlockSetUpProcessing.Text = "主机已登录";
                    //    ProgressBarSetUpProcessing.IsIndeterminate = false;
                    //    ProgressBarSetUpProcessing.Value = 100;
                    //}
                    //else
                    //{
                    //    TextBlockSetUpProcessing.Text = "主机登录失败";
                    //    ProgressBarSetUpProcessing.IsIndeterminate = false;
                    //    ProgressBarSetUpProcessing.Value = 0;
                    //}
                    client.RunCommand("echo 1111 >> test.json");
                    MessageBox.Show(client.ConnectionInfo.ServerVersion.ToString());
                    //MessageBox.Show(client);
                    client.Disconnect();


                }
            }
            catch (Exception ex1)
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
                    MessageBox.Show("未知错误");
                }
                //TextBlockSetUpProcessing.Text = "主机登录失败";
                //ProgressBarSetUpProcessing.IsIndeterminate = false;
                //ProgressBarSetUpProcessing.Value = 0;
            }
        }

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
        #region 
        private void RadioButtonNoProxy_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockProxyHost.IsEnabled = false;
            TextBoxProxyHost.IsEnabled = false;
            TextBlockProxyPort.IsEnabled = false;
            TextBoxProxyPort.IsEnabled = false;
            RadioButtonProxyNoLogin.IsEnabled = false;
            RadiobuttonProxyYesLogin.IsEnabled = false;
            TextBlockProxyUser.IsEnabled = false;
            TextBoxProxyUserName.IsEnabled = false;
            TextBlockProxyPassword.IsEnabled = false;
            PasswordBoxProxyPassword.IsEnabled = false;
        }

        private void RadioButtonNoProxy_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBlockProxyHost.IsEnabled = true;
            TextBoxProxyHost.IsEnabled = true;
            TextBlockProxyPort.IsEnabled = true;
            TextBoxProxyPort.IsEnabled = true;
            RadioButtonProxyNoLogin.IsEnabled = true;
            RadiobuttonProxyYesLogin.IsEnabled = true;
            if (RadioButtonProxyNoLogin.IsChecked == true)
            {
                TextBlockProxyUser.IsEnabled = false;
                TextBlockProxyPassword.IsEnabled = false;
                TextBoxProxyUserName.IsEnabled = false;
                PasswordBoxProxyPassword.IsEnabled = false;
            }
            else
            {
                TextBlockProxyUser.IsEnabled = true;
                TextBoxProxyUserName.IsEnabled = true;
                TextBlockProxyPassword.IsEnabled = true;
                PasswordBoxProxyPassword.IsEnabled = true;
            }
        }

        private void RadioButtonPasswordLogin_Checked(object sender, RoutedEventArgs e)
        {
            ButtonOpenFileDialog.IsEnabled = false;
            TextBoxCertFilePath.IsEnabled = false;
            PasswordBoxHostPassword.IsEnabled = true;
        }

        private void RadioButtonCertLogin_Checked(object sender, RoutedEventArgs e)
        {
            PasswordBoxHostPassword.IsEnabled = false;
            ButtonOpenFileDialog.IsEnabled = true;
            TextBoxCertFilePath.IsEnabled = true;
        }

        private void RadioButtonProxyNoLogin_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockProxyUser.IsEnabled = false;
            TextBlockProxyPassword.IsEnabled = false;
            TextBoxProxyUserName.IsEnabled = false;
            PasswordBoxProxyPassword.IsEnabled = false;
        }

        private void RadiobuttonProxyYesLogin_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockProxyUser.IsEnabled = true;
            TextBlockProxyPassword.IsEnabled = true;
            TextBoxProxyUserName.IsEnabled = true;
            PasswordBoxProxyPassword.IsEnabled = true;
        }
        #endregion
        private void Begin(TextBlock textBlock)
        {
            //int i = 100000000;
            //while (i > 0)
            //{
            //    i--;
            //}
            //Random random = new Random();
            //String Num = random.Next(0, 100).ToString();
            Action<TextBlock, String> updateAction = new Action<TextBlock, string>(UpdateTextBlockSetUpProcessing);
            TextBlockSetUpProcessing.Dispatcher.BeginInvoke(updateAction, textBlock, Num);
        }
        //更新UI代码
        private void UpdateTextBlockSetUpProcessing(TextBlock textBlock, string text)
        {
            textBlock.Text = text;
        }
    }
}
