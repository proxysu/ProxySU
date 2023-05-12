using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class XraySettings : V2raySettings
    {
        public static List<string> UTLSList = new List<string> { "", "chrome", "firefox", "safari", "ios", "android", "edge", "360", "qq", "random", "randomized" };

        //流控参数在服务端只有两种 "none", "xtls-rprx-vision"，客户端可以选择三种："none", "xtls-rprx-vision", "xtls-rprx-vision-udp443",但是选择了XTLS模式就是默认flow不为空或者"none",所以这里不再填加"none"这一项。
        public static List<string> FlowList = new List<string> { "xtls-rprx-vision", "xtls-rprx-vision-udp443" };  //{ "xtls-rprx-origin", "xtls-rprx-origin-udp443", "xtls-rprx-direct", "xtls-rprx-direct-udp443", "xtls-rprx-splice", "xtls-rprx-splice-udp443" };

        public string UTLS { get; set; } = UTLSList[1];

        public string Flow { get; set; } = FlowList[0];

        /// <summary>
        /// vless xtls shareLink
        /// </summary>
        public string VLESS_TCP_XTLS_ShareLink
        {
            get
            {
                return ShareLink.Build(RayType.VLESS_TCP_XTLS, this);
            }
        }
    }
}
