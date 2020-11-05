using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySU
{
    class ClassModel
    {
        //伪装网址的处理
        public static string DisguiseURLprocessing(string fakeUrl)
        {
            var uri = new Uri(fakeUrl);
            return uri.Host;
            //Console.WriteLine(uri.Host);

            ////处理伪装网站域名中的前缀
            //if (fakeUrl.Length >= 7)
            //{
            //    string testDomainMask = fakeUrl.Substring(0, 7);
            //    if (String.Equals(testDomainMask, "https:/") || String.Equals(testDomainMask, "http://"))
            //    {
            //        string[] tmpUrl = fakeUrl.Split('/');
            //        fakeUrl = tmpUrl[2];
            //    }

            //}

        }
    }
}
