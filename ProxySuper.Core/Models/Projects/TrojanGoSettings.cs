using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class TrojanGoSettings : IProjectSettings
    {
        public TrojanGoSettings()
        {
            Port = 443;
        }

        public List<int> FreePorts
        {
            get
            {
                return new List<int>();
            }
        }

        public ProjectType Type { get; set; } = ProjectType.TrojanGo;

        /// <summary>
        /// 域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 伪装域名
        /// </summary>
        public string MaskDomain { get; set; }

        /// <summary>
        /// 是否开启WebSocket
        /// </summary>
        [JsonIgnore]
        public bool EnableWebSocket
        {
            get
            {
                return !string.IsNullOrEmpty(WebSocketPath) && !string.IsNullOrEmpty(WebSocketDomain);
            }
        }

        /// <summary>
        /// websocket路径
        /// </summary>
        public string WebSocketPath { get; set; }

        /// <summary>
        /// websocket域名
        /// </summary>
        public string WebSocketDomain { get; set; }
    }
}
