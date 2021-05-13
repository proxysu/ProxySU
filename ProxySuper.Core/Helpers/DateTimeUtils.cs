
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Helpers
{
    public static class DateTimeUtils
    {
        /// <summary>  
        /// 从国家授时中心获取标准GMT时间，读取https://www.tsa.cn  
        /// GMT时间与UTC时间没有差别，可以UTC=GMT
        /// </summary>  
        /// <returns>返回网络时间</returns>  
        public static DateTime GetUTCTime()
        {
            DateTime time;
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
            catch
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0);
            }
        }

        public static bool GMTStrParse(string gmtStr, out DateTime gmtTime)  //抓取的date是GMT格式的字符串，这里转成datetime
        {
            CultureInfo enUS = new CultureInfo("en-US");
            bool s = DateTime.TryParseExact(gmtStr, "r", enUS, DateTimeStyles.None, out gmtTime);
            return s;
        }

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
