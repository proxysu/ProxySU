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
using System.Windows.Navigation;
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
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Globalization;
using Microsoft.Win32;
using System.Security;

namespace ProxySU
{
    class ClassModel
    {
        //检测域名是否为空
        public static bool TestDomainIsEmpty(string domainStr)
        {
            domainStr = domainStr.Trim();
            if (string.IsNullOrEmpty(domainStr) == true)
            {
                //****** "域名不能为空，请检查相关参数设置！" ******
                MessageBox.Show(Application.Current.FindResource("MessageBoxShow_DomainNotEmpty").ToString());
                return false;
            }
            else
            {
                return true;
            }
        }
        //伪装网站预处理
        public static bool PreDomainMask(string uri)
        {
            uri = uri.Trim();
            if (String.IsNullOrEmpty(uri) == false)
            {
                if (uri.Contains("/") == true)
                {
                    //MessageBox.Show("伪装网址输入格式错误！请重新输入！");
                    MessageBox.Show(Application.Current.FindResource("MessageBoxShow_MaskSitesToolTip").ToString());
                    return false;
                }
            }
            return true;
        }
        //伪装网址的处理
        public static string DisguiseURLprocessing(string fakeUrl)
        {
            fakeUrl = fakeUrl.Trim();
            return fakeUrl;
            //处理伪装网站域名中的前缀
            //if (fakeUrl.Length >= 7)
            //{
            //    string testDomainMask = fakeUrl.Substring(0, 7);
            //    if (String.Equals(testDomainMask, "https:/") || String.Equals(testDomainMask, "http://"))
            //    {
            //        var uri = new Uri(fakeUrl);
            //        return uri.Host;
            //    }

            //}

        }
    }
}
