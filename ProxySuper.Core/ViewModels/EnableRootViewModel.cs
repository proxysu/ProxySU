using MvvmCross.Commands;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models.Hosts;
using Renci.SshNet;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.ViewModels
{
    public class EnableRootViewModel : MvxViewModel
    {
        private SshClient _sshClient;

        public EnableRootViewModel()
        {
            Host = new Host();
        }

        public Host Host { get; set; }

        public string RootUserName { get; set; }

        public string RootPassword { get; set; }

        public string OutputText { get; set; }

        public IMvxCommand ExecuteCommand => new MvxCommand(Execute);

        public override void ViewDisappearing()
        {
            base.ViewDisappearing();
            if (_sshClient != null)
            {
                _sshClient.Disconnect();
                _sshClient.Dispose();
            }
        }

        public void Execute()
        {
            Task.Factory.StartNew(() =>
            {
                OpenConnect();
                if (!_sshClient.IsConnected)
                {
                    MessageBox.Show("连接失败，请重试！");
                    return;
                }

                string result = string.Empty;
                result = RunCmd("id -u");

                if (result.TrimEnd('\r', '\n') == "0")
                {
                    MessageBox.Show("当前账户已经具有root权限，无需再设置！");
                    return;
                }

                result = RunCmd($"echo {Host.Password} | sudo -S id -u");
                if (result.TrimEnd('\r', '\n') != "0")
                {
                    MessageBox.Show("当前账户无法获取sudo权限，设置失败！");
                    return;
                }

                string cmdPre = $"echo {Host.Password} | sudo -S id -u" + ';';
                RunCmd(cmdPre + "sudo sed -i 's/PermitRootLogin /#PermitRootLogin /g' /etc/ssh/sshd_config");
                RunCmd(cmdPre + "sudo sed -i 's/PasswordAuthentication /#PasswordAuthentication /g' /etc/ssh/sshd_config");
                RunCmd(cmdPre + "sudo sed -i 's/PermitEmptyPasswords /#PermitEmptyPasswords /g' /etc/ssh/sshd_config");
                RunCmd(cmdPre + "echo 'PermitRootLogin yes' | sudo tee -a /etc/ssh/sshd_config");
                RunCmd(cmdPre + "echo 'PasswordAuthentication yes' | sudo tee -a /etc/ssh/sshd_config");
                RunCmd(cmdPre + "echo 'PermitEmptyPasswords no' | sudo tee -a /etc/ssh/sshd_config");
                RunCmd(cmdPre + "sudo systemctl restart sshd");

                result = RunCmd(@"cat /dev/urandom | tr -dc '_A-Z#\-+=a-z(0-9%^>)]{<|' | head -c 20 ; echo ''");
                string setPassword = result.TrimEnd('\r', '\n') + '\n';
                RunCmd(cmdPre + $"echo -e \"{setPassword}{setPassword}\" | sudo passwd root");
                RunCmd("sudo systemctl restart sshd ");

                RootUserName = "root";
                RootPassword = setPassword.Trim('\n');
                RaisePropertyChanged("RootUserName");
                RaisePropertyChanged("RootPassword");


                var filePath = Host.Address.Replace(':', '_');
                using (StreamWriter sw = new StreamWriter("Logs\\host_password_info.txt"))
                {
                    sw.WriteLine(Host.Address);
                    sw.WriteLine("root");
                    sw.WriteLine(setPassword);
                }
                WriteOutput("设置成功，账号信息保存在Logs/host_password_info.txt");
                WriteOutput("账号:\nroot");
                WriteOutput($"密码:\n{setPassword}");
            });
        }

        protected string RunCmd(string cmdStr)
        {
            var cmd = _sshClient.CreateCommand(cmdStr);
            WriteOutput(cmdStr);

            var result = cmd.Execute();
            WriteOutput(result);
            return result;
        }

        private void WriteOutput(string text)
        {
            OutputText += text + '\n';
            RaisePropertyChanged("OutputText");
        }


        private void OpenConnect()
        {
            WriteOutput("正在建立连接...");
            var conneInfo = CreateConnectionInfo(Host);
            _sshClient = new SshClient(conneInfo);
            try
            {
                _sshClient.Connect();
                WriteOutput("Connected...");
            }
            catch (Exception ex)
            {
                WriteOutput(ex.Message);
                MessageBox.Show(ex.Message);
            }
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
    }
}
