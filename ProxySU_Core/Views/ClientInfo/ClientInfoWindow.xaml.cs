using Microsoft.Win32;
using ProxySU_Core.Models;
using ProxySU_Core.ViewModels;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

namespace ProxySuper.WPF.Controls
{
    /// <summary>
    /// ClientInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientInfoWindow
    {
        public XraySettingsViewModel Settings { get; set; }

        public ClientInfoWindow(Record record)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Settings = new XraySettingsViewModel(record.Settings);
            DataContext = this;
        }
    }
}
