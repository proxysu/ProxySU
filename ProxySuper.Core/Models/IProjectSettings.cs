using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models
{
    public interface IProjectSettings
    {
        /// <summary>
        /// 端口
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        string Domain { get; set; }

        /// <summary>
        /// 额外需要开放的端口
        /// </summary>
        List<int> FreePorts { get; }
    }
}
