using Microsoft.Win32;
using Newtonsoft.Json;
using ProxySU_Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ProxySU_Core.ViewModels
{
    public class HostViewModel : BaseViewModel
    {
        public Host host;

        private readonly ICommand _selectKeyCommand;


        public HostViewModel(Host host)
        {
            _selectKeyCommand = new BaseCommand(obj => OpenFileDialog(obj));
            this.host = host;
        }


        public string Tag
        {
            get => host.Tag;
            set
            {
                host.Tag = value;
                Notify("Tag");
            }
        }

        public string Address
        {
            get => host.Address;
            set
            {
                host.Address = value;
                Notify("Address");
            }
        }

        public string UserName
        {
            get => host.UserName;
            set => host.UserName = value;
        }

        public string Password
        {
            get => host.Password;
            set => host.Password = value;
        }

        public int Port
        {
            get => host.Port;
            set => host.Port = value;
        }

        public string PrivateKeyPath
        {
            get => host.PrivateKeyPath;
            set => host.PrivateKeyPath = value;
        }

        public LocalProxy Proxy
        {
            get => host.Proxy;
            set
            {
                host.Proxy = value;
                Notify("Proxy");
            }
        }

        public LoginSecretType SecretType
        {
            get => host.SecretType;
            set
            {
                host.SecretType = value;
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
