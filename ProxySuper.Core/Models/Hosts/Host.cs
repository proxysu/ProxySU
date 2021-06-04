using Microsoft.Win32;
using MvvmCross.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Port { get; set; } = 22;

        public string PrivateKeyPath { get; set; }

        public LocalProxy Proxy { get; set; }

        public LoginSecretType SecretType { get; set; }

        public IMvxCommand UploadPrivateKeyCommand => new MvxCommand(UploadPrivateKey);

        private void UploadPrivateKey()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.FileOk += OnFileOk;
            fileDialog.ShowDialog();
        }

        private void OnFileOk(object sender, CancelEventArgs e)
        {
            var file = sender as OpenFileDialog;
            PrivateKeyPath = file.FileName;

            Task.Delay(300).ContinueWith((t) =>
            {
                MessageBox.Show("上传成功", "提示");
            });
        }
    }
}
