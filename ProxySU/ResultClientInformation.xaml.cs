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

namespace ProxySU
{
    /// <summary>
    /// ResultClientInformation.xaml 的交互逻辑
    /// </summary>
    public partial class ResultClientInformation : Window
    {
        public ResultClientInformation()
        {
            InitializeComponent();
            //主机地址
            TextBoxHostAddress.Text = MainWindow.ReceiveConfigurationParameters[4];
            //主机端口
            TextBoxPort.Text = MainWindow.ReceiveConfigurationParameters[1];
            //用户ID(uuid)
            TextBoxUUID.Text = MainWindow.ReceiveConfigurationParameters[2];
            //路径Path
            TextBoxPath.Text = MainWindow.ReceiveConfigurationParameters[3];
            //加密方式，一般都为auto
            TextBoxEncryption.Text = "auto";
            //伪装类型
            TextBoxCamouflageType.Text = MainWindow.ReceiveConfigurationParameters[5];

            if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCP"))
            {
                TextBoxTransmission.Text = "tcp";
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLS2Web"))
            {
                TextBoxTransmission.Text = "WebSocket(ws)";
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCPhttp"))
            {
                TextBoxTransmission.Text = "tcp";

            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "MkcpNone") || String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2SRTP") || String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPuTP") || String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WechatVideo") || String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2DTLS") || String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WireGuard"))
            {
                TextBoxTransmission.Text = "mKCP(kcp)";
            }

            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "HTTP2"))
            {
                TextBoxTransmission.Text = "h2";
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TLS"))
            {
                TextBoxTransmission.Text = "";
            }
            

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string openFolderPath = @"config";
            System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
            this.Close();
        }
    }
}
