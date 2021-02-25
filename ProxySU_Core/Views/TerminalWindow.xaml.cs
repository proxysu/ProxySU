using MahApps.Metro.Controls.Dialogs;
using ProxySU_Core.ViewModels;
using ProxySU_Core.ViewModels.Developers;
using Renci.SshNet;
using System;
using System.Collections.Generic;
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
        private readonly Terminal _vm;
        private SshClient _sshClient;

        public TerminalWindow(Record project)
        {
            InitializeComponent();

            _vm = new Terminal(project.Host);
            DataContext = _vm;

            _vm.AddOutput("Connect ...");
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
            var conneInfo = CreateConnectionInfo(host);
            _sshClient = new SshClient(conneInfo);
            _sshClient.Connect();
            _vm.AddOutput("Connected");
        }

        private void WriteShell(string outShell)
        {
            _vm.AddOutput(outShell);
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            var project = new XrayProject(
                _sshClient,
                new XrayParameters { Port = 443 },
                WriteShell
            );
            Task.Run(() =>
            {
                try
                {
                    project.Execute();
                }
                catch (Exception ex)
                {
                    _vm.AddOutput(ex.Message);
                }
            });
        }


    }
}
