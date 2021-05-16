using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class IProjectSettings
    {
        /// <summary>
        /// 端口
        /// </summary>
        public virtual int Port { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        public virtual string Domain { get; set; }

        /// <summary>
        /// 额外需要开放的端口
        /// </summary>
        public virtual List<int> FreePorts { get; }

        /// <summary>
        /// 类型
        /// </summary>
        public virtual ProjectType Type { get; set; }
    }
}
