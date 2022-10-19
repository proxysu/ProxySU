using Newtonsoft.Json;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ProxySuper.Core.Models.Projects
{
    public partial class V2raySettings : IProjectSettings
    {
        public V2raySettings()
        {
            WithTLS = true;
            UTLSOption = string.Empty;

            var guid = Guid.NewGuid().ToString();
            Port = 443;
            VLESS_KCP_Port = 2001;
            VLESS_gRPC_Port = 2002;
            VMESS_KCP_Port = 3001;
            ShadowSocksPort = 4001;

            UUID = guid;
            Types = new List<RayType>();

            VLESS_WS_Path = "/" + Utils.RandomString(6);
            VLESS_KCP_Type = "none";
            VLESS_KCP_Seed = guid;
            VLESS_gRPC_ServiceName = "/" + Utils.RandomString(7);

            VMESS_WS_Path = "/" + Utils.RandomString(8);
            VMESS_TCP_Path = "/" + Utils.RandomString(9);
            VMESS_KCP_Seed = guid;
            VMESS_KCP_Type = "none";

            TrojanPassword = guid;

            ShadowSocksPassword = guid;
            ShadowSocksMethod = "aes-128-gcm";
        }

        [JsonIgnore]
        public bool IsIPAddress
        {
            get
            {
                return IPAddress.TryParse(Domain, out _);
            }
        }

        [JsonIgnore]
        public List<int> FreePorts
        {
            get
            {
                var list = new List<int>();
                list.Add(80);
                list.Add(Port);

                if (Types.Contains(RayType.VLESS_KCP))
                {
                    list.Add(VLESS_KCP_Port);
                }

                if (Types.Contains(RayType.VMESS_KCP))
                {
                    list.Add(VMESS_KCP_Port);
                }

                if (Types.Contains(RayType.ShadowsocksAEAD))
                {
                    list.Add(ShadowSocksPort);
                }

                if (Types.Contains(RayType.VLESS_gRPC))
                {
                    list.Add(VLESS_gRPC_Port);
                }

                return list.Distinct().ToList();
            }
        }

        //public ProjectType Type { get; set; } = ProjectType.Xray;

        /// <summary>
        /// 是否安装证书，
        /// 上传自有证书时选False，则不会自动安装证书。
        /// </summary>
        public bool WithTLS { get; set; }

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
        /// 多用户
        /// </summary>
        public List<string> MulitUUID { get; set; } = new List<string>();

        /// <summary>
        /// 伪装域名
        /// </summary>
        public string MaskDomain { get; set; }

        /// <summary>
        /// uTLS选项.
        /// </summary>
        public string UTLSOption { get; set; }

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
        public List<RayType> Types { get; set; } = new List<RayType>();

        /// <summary>
        /// 根据xray类型获取路径
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetPath(RayType type)
        {
            switch (type)
            {
                case RayType.VLESS_WS:
                    return VLESS_WS_Path;
                case RayType.VMESS_TCP:
                    return VMESS_TCP_Path;
                case RayType.VMESS_WS:
                    return VMESS_WS_Path;
                default:
                    return string.Empty;
            }
        }
    }
}
