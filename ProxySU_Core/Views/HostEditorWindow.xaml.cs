using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ProxySU_Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
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
    /// HostEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HostEditorWindow
    {
        private readonly Host vm;

        public event EventHandler<Host> OnSaveEvent;

        public HostEditorWindow(Host host = null)
        {
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            vm = host ?? new Host();
            DataContext = vm;
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            var isValid = ValidateForm();
            if (isValid == false) return;

            OnSaveEvent?.Invoke(sender, vm);
            Close();
        }

        private bool ValidateForm()
        {
            var res = Application.Current.Resources.MergedDictionaries[0];
            var warning = Application.Current.FindResource("Warning").ToString();
            if (string.IsNullOrWhiteSpace(vm.Address))
            {
                DialogManager.ShowMessageAsync(this,
                    warning,
                    res["HostAddressNotNull"].ToString());
                return false;
            }

            if (vm.Port <= 0)
            {
                DialogManager.ShowMessageAsync(this,
                    res["Warning"].ToString(),
                    res["HostPortNotNull"].ToString());
                return false;
            }

            if (vm.Proxy.Type > LocalProxyType.None)
            {
                if (string.IsNullOrEmpty(vm.Proxy.Address))
                {
                    DialogManager.ShowMessageAsync(this,
                        res["Warning"].ToString(),
                        res["HostAddressNotNull"].ToString());
                    return false;
                }

                if (vm.Proxy.Port <= 0)
                {
                    DialogManager.ShowMessageAsync(this,
                        res["Warning"].ToString(),
                        res["ProxyPortNotNull"].ToString());
                    return false;
                }
            }

            return true;
        }
    }
}
