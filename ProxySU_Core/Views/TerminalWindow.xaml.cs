using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using ProxySU_Core.Models;
using ProxySU_Core.Models.Developers;
using ProxySU_Core.ViewModels;
using ProxySU_Core.Views;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProxySU_Core
{
    /// <summary>
    /// TerminalWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TerminalWindow
    {
        private Record Record { get; set; }
        private readonly Terminal _vm;
        private SshClient _sshClient;

        XrayProject project;

        public TerminalWindow(Record record)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.Record = record;
            _vm = new Terminal(record.Host);
            DataContext = _vm;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    OpenConnect(_vm.Host);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _vm.HasConnected = false;

            if (_sshClient != null)
                _sshClient.Disconnect();

            if (_sshClient != null)
                _sshClient.Dispose();
        }

        private ConnectionInfo CreateConnectionInfo(Host host)
        {
            AuthenticationMethod auth = null;

            if (host.SecretType == LoginSecretType.Password)
            {
                auth = new PasswordAuthenticationMethod(host.UserName, host.Password);
            }
            else if (host.SecretType == LoginSecretType.PrivateKey)
            {
                auth = new PrivateKeyAuthenticationMethod(host.UserName, new PrivateKeyFile(host.PrivateKeyPath));
            }

            if (host.Proxy.Type == LocalProxyType.None)
            {
                return new ConnectionInfo(host.Address, host.Port, host.UserName, auth);
            }
            else
            {
                return new ConnectionInfo(
                    host: host.Address,
                    port: host.Port,
                    username: host.UserName,
                    proxyType: (ProxyTypes)(int)host.Proxy.Type,
                    proxyHost: host.Proxy.Address,
                    proxyPort: host.Proxy.Port,
                    proxyUsername: host.Proxy.UserName,
                    proxyPassword: host.Proxy.Password,
                    authenticationMethods: auth);
            }

        }

        private void OpenConnect(Host host)
        {

            WriteOutput("正在登陆服务器 ...");
            var conneInfo = CreateConnectionInfo(host);
            _sshClient = new SshClient(conneInfo);
            try
            {
                _sshClient.Connect();
            }
            catch (Exception ex)
            {
                WriteOutput("登陆失败！");
                WriteOutput(ex.Message);
                return;
            }
            WriteOutput("登陆服务器成功！");

            _vm.HasConnected = true;
            project = new XrayProject(_sshClient, Record.Settings, WriteOutput);
        }

        private void WriteOutput(string outShell)
        {
            if (!outShell.EndsWith("\n"))
            {
                outShell += "\n";
            }
            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText(outShell);
                OutputTextBox.ScrollToEnd();
            });
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                project.Install();
            });
        }

        private void UpdateXrayCore(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                project.UpdateXrayCore();
            });
        }

        private void UpdateXraySettings(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                project.UpdateXraySettings();
            });
        }

        private void InstallCert(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                project.InstallCert();
            });
        }

        private void UninstallXray(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                project.Uninstall();
            });
        }

        private void UploadCert(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "压缩文件|*.zip";
            fileDialog.FileOk += DoUploadCert;
            fileDialog.ShowDialog();
        }

        private void UploadWeb(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "压缩文件|*.zip";
            fileDialog.FileOk += DoUploadWeb;
            fileDialog.ShowDialog();
        }

        private void ReinstallCaddy(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                project.ReinstallCaddy();
            });
        }

        private void DoUploadWeb(object sender, CancelEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                var file = sender as OpenFileDialog;
                using (var stream = file.OpenFile())
                {
                    project.UploadWeb(stream);
                }
            });
        }

        private void DoUploadCert(object sender, CancelEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                var file = sender as OpenFileDialog;
                using (var stream = file.OpenFile())
                {
                    project.UploadCert(stream);
                }
            });
        }

        private void OpenLink(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }

    }
}
