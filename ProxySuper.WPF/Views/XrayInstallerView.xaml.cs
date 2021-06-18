using Microsoft.Win32;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Services;
using ProxySuper.Core.ViewModels;
using Renci.SshNet;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Documents;
using System.Windows.Threading;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// XrayInstallerView.xaml 的交互逻辑
    /// </summary>
    [MvxWindowPresentation(Identifier = nameof(XrayInstallerView), Modal = false)]
    public partial class XrayInstallerView : MvxWindow
    {
        public XrayInstallerView()
        {
            InitializeComponent();
        }

        public new XrayInstallerViewModel ViewModel
        {
            get
            {
                var t = base.ViewModel;
                return (XrayInstallerViewModel)base.ViewModel;
            }
        }

        public XrayProject Project { get; set; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            base.Loaded += (sender, arg) =>
            {
                Task.Factory.StartNew(OpenConnect);
            };

            base.Closed += SaveInstallLog;
        }

        private void SaveInstallLog(object sender, EventArgs e)
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            var fileName = Path.Combine("Logs", DateTime.Now.ToString("yyyy-MM-dd hh-mm") + ".xary.txt");
            File.WriteAllText(fileName, ViewModel.CommandText);
        }

        private SshClient _sshClient;
        private void OpenConnect()
        {
            WriteOutput("正在登陆服务器 ...");
            var conneInfo = CreateConnectionInfo(ViewModel.Host);
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

            ViewModel.Connected = true;
            Project = new XrayProject(_sshClient, ViewModel.Settings, WriteOutput);
        }

        private void WriteOutput(string outShell)
        {
            if (!outShell.EndsWith("\n"))
            {
                outShell += "\n";
            }
            ViewModel.CommandText += outShell;

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText(outShell);
                OutputTextBox.ScrollToEnd();
            });
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

        #region 功能

        private void OpenLink(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Project.Install);
        }

        private void UpdateXrayCore(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Project.UpdateXrayCore);
        }

        private void UpdateXraySettings(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Project.UpdateXraySettings);
        }

        private void InstallCert(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Project.InstallCertToXray);
        }

        private void UninstallXray(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Project.UninstallProxy);
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

        private void DoUploadWeb(object sender, CancelEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                var file = sender as OpenFileDialog;
                using (var stream = file.OpenFile())
                {
                    Project.UploadWeb(stream);
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
                    Project.UploadCert(stream);
                }
            });
        }
        #endregion

    }
}
