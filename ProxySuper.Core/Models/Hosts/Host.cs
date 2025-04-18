using Microsoft.Win32;
using MvvmCross.Commands;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Models.Hosts
{
    public class Host
    {

        public Host()
        {
            Proxy = new LocalProxy();
        }


        public string Tag { get; set; }

        public string Address { get; set; }

        public int Port { get; set; } = 22;

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PrivateKeyPath { get; set; }

        public string PrivateKeyPassPhrase { get; set; }

        public LocalProxy Proxy { get; set; }

        public LoginSecretType SecretType { get; set; }

        //public IMvxCommand UploadPrivateKeyCommand => new MvxCommand(UploadPrivateKey);
        
        private readonly IMvxCommand _uploadPrivateKeyCommand;
        public IMvxCommand UploadPrivateKeyCommand => _uploadPrivateKeyCommand ?? new MvxCommand(UploadPrivateKey);

        private void UploadPrivateKey()
        {
            var fileDialog = new OpenFileDialog() 
            {
                Filter = "Private Key (*.pem;*.key)|*.pem;*.key|All File (*.*)|*.*",
                Title = "Select the private key file"
            };
            fileDialog.FileOk += OnFileOk;
            fileDialog.ShowDialog();
        }

        private async void OnFileOk(object sender, CancelEventArgs e)
        {
            var file = sender as OpenFileDialog;
            if (file != null)
            {
                PrivateKeyPath = file.FileName;

                //Task.Delay(300).ContinueWith((t) =>
                //{
                //    MessageBox.Show("OK:" + PrivateKeyPath, "Tips");
                //});

                await Task.Delay(300);
                MessageBox.Show("OK:" + PrivateKeyPath, "Tips");
            }
            else
            {
                //Task.Delay(300).ContinueWith((t) =>
                //{
                //    MessageBox.Show("Error:Unable to get file!", "Tips");
                //});

                await Task.Delay(300);
                MessageBox.Show("Error:Unable to get file!", "Tips");
            }
        }
    }
}
