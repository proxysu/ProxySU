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

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            //byte[] expectedFingerPrint = new byte[] {
            //                                0x66, 0x31, 0xaf, 0x00, 0x54, 0xb9, 0x87, 0x31,
            //                                0xff, 0x58, 0x1c, 0x31, 0xb1, 0xa2, 0x4c, 0x6b
            //                            };
            string sshHostName = TextBoxHost.Text.ToString();
            int sshPort = int.Parse(TextBoxPort.Text);
            string sshUser = TextBoxUserName.Text.ToString();
            string sshPassword = PasswordBoxHostPassword.Password.ToString();
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
            string sshProxyHost = TextBoxProxyHost.Text.ToString();
            int sshProxyPort = int.Parse(TextBoxProxyPort.Text.ToString());
            string sshProxyUser = TextBoxProxyUserName.Text.ToString();
            string sshProxyPassword = PasswordBoxProxyPassword.Password.ToString();


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
                                        new PasswordAuthenticationMethod(sshUser, sshPassword),
                                        new PrivateKeyAuthenticationMethod(sshUser, new PrivateKeyFile(sshPrivateKey))
                                        );
           
            using (var client = new SshClient(connectionInfo))
            //using (var client = new SshClient(sshHostName, sshPort, sshUser, sshPassword))
            {
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
                client.Connect();
                client.RunCommand("echo 1111 >> test.json");
                MessageBox.Show(client.ConnectionInfo.ServerVersion.ToString());
                client.Disconnect();
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

        //private void RadioButtonHttp_Checked(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("PrxoyHttp");
        //}
             

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
    }
}
