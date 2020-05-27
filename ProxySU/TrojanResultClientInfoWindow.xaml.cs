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
    /// TrojanResultClientInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TrojanResultClientInfoWindow : Window
    {
        private string saveFileFolder = "";
        public TrojanResultClientInfoWindow()
        {
            InitializeComponent();
            //主机地址
            TextBoxTrojanServerHost.Text = MainWindow.ReceiveConfigurationParameters[4];
            //主机端口
            TextBoxTrojanServerPort.Text = MainWindow.ReceiveConfigurationParameters[1];
            //密钥（uuid）
            TextBoxTrojanServerPassword.Text = MainWindow.ReceiveConfigurationParameters[2];

            GenerateV2rayShareQRcodeAndBase64Url();

        }

        //生成Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)客户端导入文件
        private void GenerateV2rayShareQRcodeAndBase64Url()
        {
           
            string saveFileFolderFirst = TextBoxTrojanServerHost.Text;
            int num = 1;
            saveFileFolder = saveFileFolderFirst;
            CheckDir("trojan_config");
            while (Directory.Exists(@"trojan_config\" + saveFileFolder))
            {
                saveFileFolder = saveFileFolderFirst + "_copy_" + num.ToString();
                num++;
            }
            CheckDir(@"trojan_config\" + saveFileFolder);
            string trojanUrl = $"trojan://{TextBoxTrojanServerPassword.Text}@{TextBoxTrojanServerHost.Text}:{TextBoxTrojanServerPort.Text}#{TextBoxTrojanServerHost.Text}";
            //MessageBox.Show(v2rayNjsonObject.ToString());
            //string trojanUrl = "trojan://" + ToBase64Encode(v2rayNjsonObject.ToString());
            TextBoxTrojanUrl.Text = trojanUrl;
            using (StreamWriter sw = new StreamWriter($"trojan_config\\{saveFileFolder}\\url.txt"))
            {
                sw.WriteLine(trojanUrl);

            }
            CreateQRCode(trojanUrl);

            //移动Trojan官方程序配置文件到相应目录
            if (File.Exists(@"trojan_config\config.json"))
            {
                File.Move(@"trojan_config\config.json", @"trojan_config\" + saveFileFolder + @"\config.json");
                //File.Delete(@"config\config.json");//删除该文件
            }

            using (StreamWriter sw = new StreamWriter($"trojan_config\\{saveFileFolder}\\说明.txt"))
            {
                sw.WriteLine("config.json");
                sw.WriteLine("此文件为Trojan官方程序所使用的客户端配置文件，配置为全局模式，socks5地址：127.0.0.1:1080");
                sw.WriteLine("Trojan官方网站：https://trojan-gfw.github.io/trojan/");
                sw.WriteLine("Trojan官方程序下载地址：https://github.com/trojan-gfw/trojan/releases");
                sw.WriteLine("下载相应版本，Windows选择Trojan-x.xx-win.zip,解压后提取trojan.exe。与config.json放在同一目录，运行trojan.exe即可。");
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("QR.bmp");
                sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)扫码导入节点");
                sw.WriteLine("Trojan-QT5 (windows)下载网址：https://github.com/TheWanderingCoel/Trojan-Qt5/releases");
                sw.WriteLine("igniter（Android）下载网址：https://github.com/trojan-gfw/igniter/releases");
                sw.WriteLine("Shadowrocket(ios)下载,需要使用国外区的AppleID。请自行谷歌方法。");

                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("url.txt");
                sw.WriteLine("此文件为Trojan-QT5 (windows)、igniter（Android）、Shadowrocket(ios)复制粘贴导入节点的网址");
                sw.WriteLine("-----------------------------------------\n");
                sw.WriteLine("服务器通用连接配置参数");
                sw.WriteLine($"地址(address)：{TextBoxTrojanServerHost.Text}");
                sw.WriteLine($"端口(Port)：{TextBoxTrojanServerPort.Text}");
                sw.WriteLine($"密钥：{TextBoxTrojanServerPassword.Text}");

            }



        }
        //生成base64
        //private string ToBase64Encode(string text)
        //{
        //    if (String.IsNullOrEmpty(text))
        //    {
        //        return text;
        //    }

        //    byte[] textBytes = Encoding.UTF8.GetBytes(text);
        //    return Convert.ToBase64String(textBytes);
        //}

        //生成QRcoder图片
        private void CreateQRCode(string varBase64)
        {
            //string varBase64 = varBase64;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(varBase64, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            IntPtr myImagePtr = qrCodeImage.GetHbitmap();
            BitmapSource imgsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(myImagePtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            ImageTrojanShareQRurl.Source = imgsource;
            //DeleteObject(myImagePtr);
            qrCodeImage.Save($"trojan_config\\{saveFileFolder}\\QR.bmp");
       
        }
        //判断目录是否存在，不存在则创建
        private static bool CheckDir(string folder)
        {
            try
            {
                if (!Directory.Exists(folder))//如果不存在就创建file文件夹
                    Directory.CreateDirectory(folder);//创建该文件夹　　            
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void ButtonTrojanResultOpen_Click(object sender, RoutedEventArgs e)
        {
            string openFolderPath = @"trojan_config\" + saveFileFolder;
            System.Diagnostics.Process.Start("explorer.exe", openFolderPath);
            this.Close();
        }
    }
}
