using ProxySU_Core.Models;
using ProxySU_Core.ViewModels;
using System;
using System.Collections.Generic;
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

namespace ProxySU_Core.Views
{
    /// <summary>
    /// RecordEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RecordEditorWindow
    {
        public Record Record { get; set; }

        public HostViewModel Host { get; set; }

        public XraySettingsViewModel Settings { get; set; }

        public EventHandler OnSaved;

        public RecordEditorWindow(Record record)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.Record = record;
            Host = new HostViewModel(record.Host.DeepClone());
            Settings = new XraySettingsViewModel(record.Settings.DeepClone());
            this.DataContext = this;
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            Record.Host = Host.host;
            Record.Settings = Settings.settings;
            DialogResult = true;
            Close();
        }

        public void RandomUuid(object sender, RoutedEventArgs e)
        {
            Settings.UUID = Guid.NewGuid().ToString();
        }

    }
}
