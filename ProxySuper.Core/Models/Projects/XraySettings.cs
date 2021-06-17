using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public partial class XraySettings : IProjectSettings
    {
        public XraySettings()
        {
            var guid = Guid.NewGuid().ToString();
            Port = 443;
            VLESS_KCP_Port = 2001;
            VLESS_gRPC_Port = 2002;
            VMESS_KCP_Port = 3001;
            ShadowSocksPort = 4001;

            UUID = guid;
            Types = new List<XrayType>();

            VLESS_WS_Path = "/vlessws";
            VLESS_KCP_Type = "none";
            VLESS_KCP_Seed = guid;
            VLESS_gRPC_ServiceName = "xray_gRPC";

            VMESS_WS_Path = "/vmessws";
            VMESS_TCP_Path = "/vmesstcp";
            VMESS_KCP_Seed = guid;
            VMESS_KCP_Type = "none";

            TrojanPassword = guid;

            ShadowSocksPassword = guid;
            ShadowSocksMethod = "aes-128-gcm";
        }

        public List<int> FreePorts
        {
            get
            {
                return new List<int>
                {
                    VLESS_KCP_Port,
                    VMESS_KCP_Port,
                    ShadowSocksPort,
                };
            }
        }

        public ProjectType Type { get; set; } = ProjectType.Xray;

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// UUID
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// 伪装域名
        /// </summary>
        public string MaskDomain { get; set; }

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

                return $"{UUID.Substring(2, 6)}@gmail.com";
            }
        }

        /// <summary>
        /// 安装类型
        /// </summary>
        public List<XrayType> Types { get; set; } = new List<XrayType>();

        /// <summary>
        /// 根据xray类型获取路径
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetPath(XrayType type)
        {
            switch (type)
            {
                case XrayType.VLESS_WS:
                    return VLESS_WS_Path;
                case XrayType.VMESS_TCP:
                    return VMESS_TCP_Path;
                case XrayType.VMESS_WS:
                    return VMESS_WS_Path;
                default:
                    return string.Empty;
            }
        }
    }
}
