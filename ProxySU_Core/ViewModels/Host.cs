using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ProxySU_Core.ViewModels
{
    public class Host : BaseModel
    {
        private LoginSecretType _secretType;
        private string tag = string.Empty;
        private string _address;
        private LocalProxy proxy;
        private readonly ICommand _selectKeyCommand;

        public Host()
        {
            _selectKeyCommand = new BaseCommand(obj => OpenFileDialog(obj));
            Proxy = new LocalProxy();
        }


        public string Tag
        {
            get => tag; set
            {
                tag = value;
                Notify("Tag");
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                Notify("Address");
            }
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Port { get; set; } = 22;

        public string PrivateKeyPath { get; set; }

        public LocalProxy Proxy
        {
            get => proxy; set
            {
                proxy = value;
                Notify("Proxy");
            }
        }

        public LoginSecretType SecretType
        {
            get
            {
                return _secretType;
            }
            set
            {
                _secretType = value;
                Notify("SecretType");
                Notify("KeyUploaderVisiblity");
                Notify("PasswordVisiblity");
            }
        }

        [JsonIgnore]
        public Visibility PasswordVisiblity
        {
            get
            {
                if (SecretType == LoginSecretType.Password)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        [JsonIgnore]
        public Visibility KeyUploaderVisiblity
        {
            get
            {
                if (SecretType == LoginSecretType.PrivateKey)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }


        [JsonIgnore]
        public ICommand SelectKeyCommand
        {
            get
            {
                return _selectKeyCommand;
            }
        }


        private void OpenFileDialog(object obj)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.FileOk += OnFileOk;
            fileDialog.ShowDialog();
        }

        private void OnFileOk(object sender, CancelEventArgs e)
        {
            var file = sender as OpenFileDialog;
            PrivateKeyPath = file.FileName;
        }
    }


}
