using Newtonsoft.Json;
using ProxySuper.Core.Services;
using ProxySuper.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ProxySuper.Core.Models.Projects
{
    public partial class XraySettings : IProjectSettings
    {

        public XraySettings()
        {
            WithTLS = true;

            var guid = Guid.NewGuid().ToString();
            Port = 443;
            VLESS_gRPC_Port = 8443;
            VMESS_KCP_Port = 3001;
            ShadowSocksPort = 4001;

            UUID = guid;
            Types = new List<XrayType>();

            VLESS_XHTTP_Path = "/" + Utils.RandomString(6);
            VLESS_WS_Path = "/" + Utils.RandomString(6);
            VLESS_gRPC_ServiceName = Utils.RandomString(7);

            VMESS_KCP_Seed = guid;
            VMESS_KCP_Type = "none";

            TrojanPassword = guid;

            ShadowSocksPassword = guid;
            ShadowSocksMethod = "aes-128-gcm";

            var x25519Key = new X25519KeyPair();
            REALITY_privateKey = x25519Key.PrivateKey;
            REALITY_publicKey = x25519Key.PublicKey;
        }

        /// <summary>
        /// Xray 使用浏览器的 TLS Client Hello 指纹可选列表
        /// </summary>
        [JsonIgnore]
        public static List<string> UTLSList = new List<string> { "", "chrome", "firefox", "safari", "ios", "android", "edge", "360", "qq", "random", "randomized" };

        /// <summary>
        /// Xray 使用浏览器的 TLS Client Hello 指纹参数
        /// </summary>
        public string UTLS { get; set; } = UTLSList[1];

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


                if (Types.Contains(XrayType.VMESS_KCP))
                {
                    list.Add(VMESS_KCP_Port);
                }


                if (Types.Contains(XrayType.ShadowsocksAEAD))
                {
                    list.Add(ShadowSocksPort);
                }

                if (Types.Contains(XrayType.VLESS_gRPC))
                {
                    list.Add(VLESS_gRPC_Port);
                }

                return list.Distinct().ToList();
            }
        }

        /// <summary>
        /// 使用REALITY模式时，客户端爬虫初始路径与参数
        /// </summary>
        public string REALITY_spiderX { get; set; } = "/";

        /// <summary>
        /// 使用REALITY模式时，私钥
        /// </summary>
        public string REALITY_privateKey { get; set; } = string.Empty;

        /// <summary>
        /// 使用REALITY模式时，公钥
        /// </summary>
        public string REALITY_publicKey {  get; set; } = string.Empty;

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
                default:
                    return string.Empty;
            }
        }
    }
}
