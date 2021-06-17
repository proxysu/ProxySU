using Newtonsoft.Json;
using ProxySuper.Core.Services;
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
            WebSocketPath = "/ws";
            Password = Guid.NewGuid().ToString();
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
                return !string.IsNullOrEmpty(WebSocketPath);
            }
        }

        /// <summary>
        /// websocket路径
        /// </summary>
        public string WebSocketPath { get; set; }


        [JsonIgnore]
        public string Email
        {
            get
            {
                if (!string.IsNullOrEmpty(Domain))
                {
                    var arr = Domain.Split('.');
                    if (arr.Length == 3)
                    {
                        return $"{arr[0]}@{arr[1]}.{arr[2]}";
                    }
                }

                var prefix = Password.Length > 7 ? Password.Substring(0, 7) : Password;
                return $"{prefix}@gmail.com";
            }
        }

    }
}
