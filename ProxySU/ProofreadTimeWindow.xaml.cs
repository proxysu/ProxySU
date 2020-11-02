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
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Globalization;

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
                client.RunCommand("rm -f /etc/localtime");
                client.RunCommand("ln -s /usr/share/zoneinfo/UTC /etc/localtime");
                //获取远程主机的时间戳
                long timeStampVPS = Convert.ToInt64(client.RunCommand("date +%s").Result.ToString());

                //获取本地时间戳
                TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                long timeStampLocal = Convert.ToInt64(ts.TotalSeconds);
                client.Disconnect();
                if (Math.Abs(timeStampLocal - timeStampVPS) >= 90)
                {

                    MessageBox.Show("本地时间与远程主机时间相差超过限制(90秒)，V2ray无法建立连接");

                    return;
                }
                else
                {
                    MessageBox.Show("误差为：" + Math.Abs(timeStampLocal - timeStampVPS).ToString()+" 可以连接");
                }
            }
        }

        private void ButtonProofreading_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonUpDateLocalTime.IsChecked == true)
            {
                //将本机电脑与网络时间同步
                DateTime netUTCtime = NetTime.GetUTCTime();
                if (!DateTime.Equals(netUTCtime, new DateTime(1970, 1, 1, 0, 0, 0, 0)))
                {
                    DateTime localTime = netUTCtime.ToLocalTime();
                    bool setD = UpdateTime.SetDate(localTime);
                    if (setD == true)
                    {
                        MessageBox.Show("本机时间已经更新为网络时间(国家授时中心获取)");
                    }
                    else
                    {
                        MessageBox.Show("更新失败，请重试。");
                    }
                }
                return;
            }
            using (var client = new SshClient(ProfreadTimeReceiveConnectionInfo))
            {
                client.Connect();
                //设置vps为UTC时区
                client.RunCommand("rm -f /etc/localtime");
                client.RunCommand("ln -s /usr/share/zoneinfo/UTC /etc/localtime");
               
                if (RadioButtonLocalTime.IsChecked == true)
                {
                    //以本地时间为准，校正远程主机时间
                    //获取本地时间戳
                    TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    long timeStampLocal = Convert.ToInt64(ts.TotalSeconds);

                    string sshCmd = $"date --set=\"$(date \"+%Y-%m-%d %H:%M:%S\" -d @{timeStampLocal.ToString()})\"";

                    client.RunCommand(sshCmd);
                    MessageBox.Show("同步本地时间校时完毕");


                }
                else
                {
                    //以网络时间为准，校正远程主机时间
                    TimeSpan utcTS = NetTime.GetUTCTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    long timeStampVPS = Convert.ToInt64(utcTS.TotalSeconds);
                    if (timeStampVPS!=0)
                    {

                        string sshCmd = $"date --set=\"$(date \"+%Y-%m-%d %H:%M:%S\" -d @{timeStampVPS.ToString()})\"";

                        client.RunCommand(sshCmd);
                        MessageBox.Show("同步网络时间校时完毕");
                    }
                
                }
                client.Disconnect();
            }

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RadioButtonNetworkTime.IsChecked = true;
        }

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            RadioButtonLocalTime.IsChecked = true;
        }

        private void TextBlock_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            RadioButtonUpDateLocalTime.IsChecked = true;
        }

        //private void ButtonTEST_Click(object sender, RoutedEventArgs e)
        //{
        //    //NetTime netTime = new NetTime();
        //    string netDatetime = NetTime.GetUTCTime().ToString();
        //    MessageBox.Show(netDatetime);
        //    //NetTime netTime = new NetTime();
        //    //UpdateTime updateTime = new UpdateTime();
        //    //DateTime netDateTime = netTime.GetBeijingTime();
        //    //MessageBox.Show(netDateTime.ToString());
        //}

    }

    /// <summary>  
    /// 网络时间  代码从网上复制，原网址：https://www.codeleading.com/article/23791981303/
    /// </summary>  
    public class NetTime
    {
        /// <summary>  
        /// 从国家授时中心获取标准GMT时间，读取https://www.tsa.cn  
        /// GMT时间与UTC时间没有差别，可以UTC=GMT
        /// </summary>  
        /// <returns>返回网络时间</returns>  
        public static DateTime GetUTCTime()
        {
            DateTime time;
            ////Thread.Sleep(5000);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.tsa.cn");
                request.Method = "HEAD";
                request.AllowAutoRedirect = false;
                HttpWebResponse reponse = (HttpWebResponse)request.GetResponse();
                string cc = reponse.GetResponseHeader("date");
                reponse.Close();

                bool s = GMTStrParse(cc, out time);
                return time;
            }
            catch (Exception ex1)
            {
                if (ex1.ToString().Contains("403"))
                {
                    MessageBox.Show("校时操作太频繁，请稍等片刻再操作！");
                }
                else
                {
                    MessageBox.Show(ex1.Message);
                }
                return time = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            }

            //return time.AddHours(8); //GMT要加8个小时才是北京时间
        }
        public static bool GMTStrParse(string gmtStr, out DateTime gmtTime)  //抓取的date是GMT格式的字符串，这里转成datetime
        {
            CultureInfo enUS = new CultureInfo("en-US");
            bool s = DateTime.TryParseExact(gmtStr, "r", enUS, DateTimeStyles.None, out gmtTime);
            return s;
        }

    }

    /// <summary>
    /// 更新系统时间，代码从网上复制，原网址：https://www.open-open.com/code/view/1430552965599
    /// </summary>
    public class UpdateTime
    {
        //设置系统时间的API函数
        [DllImport("kernel32.dll")]
        private static extern bool SetLocalTime(ref SYSTEMTIME time);

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public short year;
            public short month;
            public short dayOfWeek;
            public short day;
            public short hour;
            public short minute;
            public short second;
            public short milliseconds;
        }

        /// <summary>
        /// 设置系统时间
        /// </summary>
        /// <param name="dt">需要设置的时间</param>
        /// <returns>返回系统时间设置状态，true为成功，false为失败</returns>
        public static bool SetDate(DateTime dt)
        {
            SYSTEMTIME st;

            st.year = (short)dt.Year;
            st.month = (short)dt.Month;
            st.dayOfWeek = (short)dt.DayOfWeek;
            st.day = (short)dt.Day;
            st.hour = (short)dt.Hour;
            st.minute = (short)dt.Minute;
            st.second = (short)dt.Second;
            st.milliseconds = (short)dt.Millisecond;
            bool rt = SetLocalTime(ref st);
            return rt;
        }
    }

}
