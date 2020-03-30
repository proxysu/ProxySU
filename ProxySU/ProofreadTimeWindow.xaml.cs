using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Renci.SshNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Drawing;
using QRCoder;

namespace ProxySU
{
    /// <summary>
    /// ProofreadTimeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProofreadTimeWindow : Window
    {
        public static ConnectionInfo ProfreadTimeReceiveConnectionInfo { get; set; }
        //ProfreadTimeReceiveParameters
        public ProofreadTimeWindow()
        {
            InitializeComponent();
            
        }

        private void ButtonTestTime_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new SshClient(ProfreadTimeReceiveConnectionInfo))
            {
                client.Connect();
                long timeStampVPS = Convert.ToInt64(client.RunCommand("date +%s").Result.ToString());
                //MessageBox.Show(timesStampVPS.ToString());
                //获取本地时间戳
                TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                long timeStampLocal = Convert.ToInt64(ts.TotalSeconds);
                client.Disconnect();
                if (Math.Abs(timeStampLocal - timeStampVPS) >= 90)
                {

                    MessageBox.Show("本地时间与远程主机时间相差超过限制(90秒)，请先用\"系统工具-->时间校对\"校对时间后再设置");
                    //currentStatus = "时间较对失败......";
                    //textBlockName.Dispatcher.BeginInvoke(updateAction, textBlockName, progressBar, currentStatus);
                    //Thread.Sleep(1000);
                    return;
                }
                else
                {
                    MessageBox.Show("误差为："+Math.Abs(timeStampLocal - timeStampVPS).ToString());
                }
            }
        }


        //MainWindow.ConnectionInfo
        //using (var client = new SshClient(MainWindow.ConnectionInfo))

    }
}
