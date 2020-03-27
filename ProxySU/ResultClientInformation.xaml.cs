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
            //额外ID
            TextBoxUUIDextra.Text = "16";
            //路径Path
            TextBoxPath.Text = MainWindow.ReceiveConfigurationParameters[3];
            //加密方式，一般都为auto
            TextBoxEncryption.Text = "auto";
            //伪装类型
            TextBoxCamouflageType.Text = MainWindow.ReceiveConfigurationParameters[5];

            if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "WebSocketTLS2Web"))
            {
                TextBoxTransmission.Text = "ws";
                TextBoxCamouflageType.Text = "none";
                ShowPathAndTLS();
                TextBoxTLS.Text = "tls";
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TCPhttp"))
            {
                TextBoxTransmission.Text = "tcp";
                TextBoxCamouflageType.Text = "http";
                TextBoxTLS.Text = "none";
                HidePathAndTLS();
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "MkcpNone"))
            {
                TextBoxTransmission.Text = "kcp";
                TextBoxCamouflageType.Text = "none";
                TextBoxTLS.Text = "none";
                HidePathAndTLS();
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2SRTP"))
            {
                TextBoxTransmission.Text = "kcp";
                TextBoxCamouflageType.Text = "srtp";
                TextBoxTLS.Text = "none";
                HidePathAndTLS();
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCPuTP"))
            {
                TextBoxTransmission.Text = "kcp";
                TextBoxCamouflageType.Text = "utp";
                TextBoxTLS.Text = "none";
                HidePathAndTLS();
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WechatVideo"))
            {
                TextBoxTransmission.Text = "kcp";
                TextBoxCamouflageType.Text = "wechat-video";
                TextBoxTLS.Text = "none";
                HidePathAndTLS();
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2DTLS"))
            {
                TextBoxTransmission.Text = "kcp";
                TextBoxCamouflageType.Text = "dtls";
                TextBoxTLS.Text = "none";
                HidePathAndTLS();
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "mKCP2WireGuard"))
            {
                TextBoxTransmission.Text = "kcp";
                TextBoxCamouflageType.Text = "wireguard";
                TextBoxTLS.Text = "none";
                HidePathAndTLS();
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "HTTP2"))
            {
                TextBoxTransmission.Text = "h2";
                TextBoxCamouflageType.Text = "none";
                ShowPathAndTLS();
                TextBoxTLS.Text = "tls";
            }
            else if (String.Equals(MainWindow.ReceiveConfigurationParameters[0], "TLS"))
            {
                TextBoxTransmission.Text = "tcp";
                TextBoxCamouflageType.Text = "none";
                TextBoxTLS.Text = "tls";
                HidePathAndTLS();
            }
            else
            {
                TextBoxTransmission.Text = "tcp";
                TextBoxCamouflageType.Text = "none";
                TextBoxTLS.Text = "none";
                HidePathAndTLS();
            }

            GenerateV2rayNshareQRcodeAndBase64Url();

        }
        private void HidePathAndTLS()
        {
            TextBlockPath.Visibility = Visibility.Collapsed;
            TextBoxPath.Visibility = Visibility.Collapsed;
            TextBlockPathExplain.Visibility = Visibility.Collapsed;
            //TextBlocTLSonOrNo.Visibility = Visibility.Collapsed;
            //TextBoxTLS.Visibility = Visibility.Collapsed;
            //TextBlocTLSonOrNoExplain.Visibility = Visibility.Collapsed;
        }
        private void ShowPathAndTLS()
        {
            TextBlockPath.Visibility = Visibility.Visible;
            TextBoxPath.Visibility = Visibility.Visible;
            TextBlockPathExplain.Visibility = Visibility.Visible;
            //TextBlocTLSonOrNo.Visibility = Visibility.Visible;
            //TextBoxTLS.Visibility = Visibility.Visible;
            //TextBlocTLSonOrNoExplain.Visibility = Visibility.Visible;
        }
        //生成v2rayN客户端导入文件
        private void GenerateV2rayNshareQRcodeAndBase64Url()
        {
            //生成v2rayN的json文件
            string v2rayNjsonFile = @"
{
  ""v"": """",
  ""ps"": """",
  ""add"": """",
  ""port"": """",
  ""id"": """",
  ""aid"": """",
  ""net"": """",
  ""type"": """",
  ""host"": """",
  ""path"": """",
  ""tls"": """"
}";
            //MessageBox.Show(v2rayNjsonFile);
            JObject v2rayNjsonObject = JObject.Parse(v2rayNjsonFile);
            v2rayNjsonObject["v"] = "2";
            v2rayNjsonObject["add"] = TextBoxHostAddress.Text.ToString(); //设置域名
            v2rayNjsonObject["port"] = TextBoxPort.Text.ToString(); //设置端口
            v2rayNjsonObject["id"] = TextBoxUUID.Text.ToString(); //设置uuid
            v2rayNjsonObject["aid"] = TextBoxUUIDextra.Text.ToString(); //设置额外ID
            v2rayNjsonObject["net"] = TextBoxTransmission.Text.ToString(); //设置传输模式
            v2rayNjsonObject["type"] = TextBoxCamouflageType.Text.ToString(); //设置伪装类型
            v2rayNjsonObject["host"] = "";
            v2rayNjsonObject["path"] = TextBoxPath.Text.ToString(); //设置路径
            v2rayNjsonObject["tls"] = TextBoxTLS.Text.ToString();  //设置是否启用TLS
            v2rayNjsonObject["ps"] = v2rayNjsonObject["add"];  //设置备注
            //MessageBox.Show(v2rayNjsonObject["v"].ToString());

            
            MessageBox.Show(v2rayNjsonObject.ToString());
            string vmessUrl = "vmess://" + ToBase64Encode(v2rayNjsonObject.ToString());
            using (StreamWriter sw = new StreamWriter(@"config\v2rayNvmessUrl.txt"))
            {
                  sw.WriteLine(vmessUrl);

            }
            CreateQRCode(vmessUrl);


        }
        //生成base64
        private string ToBase64Encode(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(textBytes);
        }
        //生成QRcoder图片
        private void CreateQRCode(string varBase64)
        {
            //string varBase64 = varBase64;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(varBase64, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            qrCodeImage.Save(@"config\v2rayN.bmp");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string openFolderPath = @"config";
            System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
            this.Close();
        }
    }
}
