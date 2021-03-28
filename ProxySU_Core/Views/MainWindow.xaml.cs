using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProxySU_Core.Common;
using ProxySU_Core.Models;
using ProxySU_Core.ViewModels;
using ProxySU_Core.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace ProxySU_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string RecordPath = @"Data\Record.json";

        public ObservableCollection<RecordViewModel> Records { get; set; }

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            Records = new ObservableCollection<RecordViewModel>();

            if (File.Exists(RecordPath))
            {
                var recordsJson = File.ReadAllText(RecordPath, Encoding.UTF8);
                if (!string.IsNullOrEmpty(recordsJson))
                {
                    var records = JsonConvert.DeserializeObject<List<Record>>(recordsJson);
                    records.ForEach(item =>
                    {
                        Records.Add(new RecordViewModel(item));
                    });
                }
            }


            DataContext = this;
        }

        private void SaveRecord()
        {
            var recordList = Records.Select(x => x.record);
            var json = JsonConvert.SerializeObject(recordList,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
            File.WriteAllText("Data\\Record.json", json, Encoding.UTF8);
        }

        private void LaunchGitHubSite(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU");
        }

        private void LaunchCoffeeSite(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/proxysu/ProxySU");
        }

        private void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            var selection = cmbLanguage.SelectedValue as ComboBoxItem;

            if (selection.Name == "zh_cn")
            {
                ChangeLanguage("zh_cn");
            }
            else if (selection.Name == "en")
            {
                ChangeLanguage("en");
            }
        }

        private void ChangeLanguage(string culture)
        {
            ResourceDictionary resource = new ResourceDictionary();

            if (string.Equals(culture, "zh_cn", StringComparison.OrdinalIgnoreCase))
            {
                resource.Source = new Uri(@"Resources\Languages\zh_cn.xaml", UriKind.Relative);
            }

            else if (string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase))
            {
                resource.Source = new Uri(@"Resources\Languages\en.xaml", UriKind.Relative);
            }

            Application.Current.Resources.MergedDictionaries[0] = resource;
        }

        private void ExportXraySettings(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var record in Records.Where(x => x.IsChecked))
            {
                record.Settings.Types.ForEach(type =>
                {
                    var link = ShareLink.Build(type, record.Settings);
                    sb.AppendLine(link);
                });
            }
            var tbx = new TextBoxWindow("分享链接", sb.ToString());
            tbx.ShowDialog();
        }

        private void ExportXraySub(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var record in Records.Where(x => x.IsChecked))
            {
                record.Settings.Types.ForEach(type =>
                {
                    var link = ShareLink.Build(type, record.Settings);
                    sb.AppendLine(link);
                });
            }
            var result = Base64.Encode(sb.ToString());
            var tbx = new TextBoxWindow("订阅内容", result);
            tbx.ShowDialog();
        }

        private void AddHost(object sender, RoutedEventArgs e)
        {
            var newRecord = new Record();
            var hostWindow = new RecordEditorWindow(newRecord);
            var result = hostWindow.ShowDialog();
            if (result == true)
            {
                Records.Add(new RecordViewModel(newRecord));
                SaveRecord();
            }
        }

        private void EditHost(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem is RecordViewModel project)
            {
                var hostWindow = new RecordEditorWindow(project.record);
                var result = hostWindow.ShowDialog();
                if (result == true)
                {
                    project.Notify();
                    SaveRecord();
                }
            }
        }

        private void ShowClientInfo(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem is RecordViewModel project)
            {
                var dialog = new ClientInfoWindow(project.record);
                dialog.ShowDialog();
            }
        }


        private void DeleteHost(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem is RecordViewModel project)
            {
                var result = MessageBox.Show($"您确认删除主机{project.Host.Tag}吗？", "提示", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    Records.Remove(project);
                }
            }

        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            var project = DataGrid.SelectedItem as RecordViewModel;
            if (project == null)
            {
                DialogManager.ShowMessageAsync(this, "提示", "请选择一个服务器");
            }

            TerminalWindow terminalWindow = new TerminalWindow(project.record);
            terminalWindow.Show();
        }
    }
}
