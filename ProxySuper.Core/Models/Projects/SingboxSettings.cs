using Newtonsoft.Json;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ProxySuper.Core.Models.Projects
{
    public class SingboxSettings : IProjectSettings
    {
        public SingboxSettings()
        {
            var guid = Guid.NewGuid().ToString();
            Port = 443;
            UUID = guid;
            Types = new List<SingBoxType>();

            // Default ports for protocols
            VLESS_WS_Port = 1111;
            VLESS_gRPC_Port = 1112;
            VMESS_WS_Port = 1211;
            VMESS_HTTP_Port = 1212;
            Trojan_Port = 1310;
            Shadowsocks_Port = 1410;
            Shadowsocks_2022_Port = 1411;
            WireGuard_Port = 51820;
            Hysteria_Port = 9120;
            Hysteria2_Port = 9121;
            TUIC_Port = 9180;
            NaiveProxy_Port = 9220;
            ShadowTLS_Port = 9330;
            VLESS_XHTTP_REALITY_Port = 1150;
            SOCKS_Port = 1080;
            HTTP_Port = 1081;

            // Default paths
            VLESS_WS_Path = "/" + Utils.RandomString(6);
            VMESS_WS_Path = "/" + Utils.RandomString(8);
            VLESS_gRPC_ServiceName = "/" + Utils.RandomString(7);

            // Default methods
            Shadowsocks_Method = "aes-128-gcm";
            Shadowsocks_2022_Method = "2022-blake3-aes-128-gcm";

            // Default TLS
            WithTLS = true;
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
                var list = new List<int> { 80, Port };

                if (Types.Contains(SingBoxType.SingBox_VLESS_WS))
                    list.Add(VLESS_WS_Port);
                if (Types.Contains(SingBoxType.SingBox_VLESS_gRPC))
                    list.Add(VLESS_gRPC_Port);
                if (Types.Contains(SingBoxType.SingBox_VMESS_WS))
                    list.Add(VMESS_WS_Port);
                if (Types.Contains(SingBoxType.SingBox_VMESS_HTTP))
                    list.Add(VMESS_HTTP_Port);
                if (Types.Contains(SingBoxType.SingBox_Shadowsocks))
                    list.Add(Shadowsocks_Port);
                if (Types.Contains(SingBoxType.SingBox_Shadowsocks_2022))
                    list.Add(Shadowsocks_2022_Port);
                if (Types.Contains(SingBoxType.SingBox_WireGuard))
                    list.Add(WireGuard_Port);
                if (Types.Contains(SingBoxType.SingBox_Hysteria))
                    list.Add(Hysteria_Port);
                if (Types.Contains(SingBoxType.SingBox_Hysteria2))
                    list.Add(Hysteria2_Port);
                if (Types.Contains(SingBoxType.SingBox_TUIC))
                    list.Add(TUIC_Port);
                if (Types.Contains(SingBoxType.SingBox_NaiveProxy))
                    list.Add(NaiveProxy_Port);
                if (Types.Contains(SingBoxType.SingBox_ShadowTLS))
                    list.Add(ShadowTLS_Port);
                if (Types.Contains(SingBoxType.SingBox_VLESS_XHTTP_REALITY))
                    list.Add(VLESS_XHTTP_REALITY_Port);
                if (Types.Contains(SingBoxType.SingBox_SOCKS))
                    list.Add(SOCKS_Port);
                if (Types.Contains(SingBoxType.SingBox_HTTP))
                    list.Add(HTTP_Port);

                return list.Distinct().ToList();
            }
        }

        [JsonIgnore]
        public string Email
        {
            get
            {
                if (!string.IsNullOrEmpty(Domain))
                {
                    var arr = Domain.Split('.');
                    if (arr.Length >= 2)
                    {
                        return $"{arr[0]}@{arr[1]}";
                    }
                }
                return $"{UUID.Substring(2, 6)}@gmail.com";
            }
        }

        #region Basic Settings

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 是否启用 TLS
        /// </summary>
        public bool WithTLS { get; set; } = true;

        /// <summary>
        /// 是否使用 Docker 部署
        /// </summary>
        public bool UseDocker { get; set; } = false;

        /// <summary>
        /// 是否启用订阅服务
        /// </summary>
        public bool EnableSubscribe { get; set; } = false;

        /// <summary>
        /// 订阅服务端口
        /// </summary>
        public int SubscribePort { get; set; } = 8080;

        /// <summary>
        /// UUID
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// 多用户UUID列表
        /// </summary>
        public List<string> MulitUUID { get; set; } = new List<string>();

        /// <summary>
        /// 协议类型列表
        /// </summary>
        public List<SingBoxType> Types { get; set; } = new List<SingBoxType>();

        #endregion

        #region VLESS Settings

        /// <summary>
        /// VLESS WS 端口
        /// </summary>
        public int VLESS_WS_Port { get; set; }

        /// <summary>
        /// VLESS WS 路径
        /// </summary>
        public string VLESS_WS_Path { get; set; }

        /// <summary>
        /// VLESS gRPC 端口
        /// </summary>
        public int VLESS_gRPC_Port { get; set; }

        /// <summary>
        /// VLESS gRPC 服务名
        /// </summary>
        public string VLESS_gRPC_ServiceName { get; set; }

        #endregion

        #region VMess Settings

        /// <summary>
        /// VMess WS 端口
        /// </summary>
        public int VMESS_WS_Port { get; set; }

        /// <summary>
        /// VMess WS 路径
        /// </summary>
        public string VMESS_WS_Path { get; set; }

        /// <summary>
        /// VMess HTTP 端口
        /// </summary>
        public int VMESS_HTTP_Port { get; set; }

        #endregion

        #region Trojan Settings

        /// <summary>
        /// Trojan 密码
        /// </summary>
        public string Trojan_Password { get; set; }

        /// <summary>
        /// Trojan 端口
        /// </summary>
        public int Trojan_Port { get; set; }

        #endregion

        #region Shadowsocks Settings

        /// <summary>
        /// Shadowsocks 密码
        /// </summary>
        public string Shadowsocks_Password { get; set; }

        /// <summary>
        /// Shadowsocks 加密方法
        /// </summary>
        public string Shadowsocks_Method { get; set; }

        /// <summary>
        /// Shadowsocks 端口
        /// </summary>
        public int Shadowsocks_Port { get; set; }

        /// <summary>
        /// Shadowsocks 2022 密码
        /// </summary>
        public string Shadowsocks_2022_Password { get; set; }

        /// <summary>
        /// Shadowsocks 2022 加密方法
        /// </summary>
        public string Shadowsocks_2022_Method { get; set; }

        /// <summary>
        /// Shadowsocks 2022 端口
        /// </summary>
        public int Shadowsocks_2022_Port { get; set; }

        #endregion

        #region WireGuard Settings

        /// <summary>
        /// WireGuard 端口
        /// </summary>
        public int WireGuard_Port { get; set; }

        /// <summary>
        /// WireGuard 私钥
        /// </summary>
        public string WireGuard_PrivateKey { get; set; }

        /// <summary>
        /// WireGuard 公钥
        /// </summary>
        public string WireGuard_PublicKey { get; set; }

        /// <summary>
        /// WireGuard Peer 公钥
        /// </summary>
        public string WireGuard_Peer_PublicKey { get; set; }

        /// <summary>
        /// WireGuard Peer IP
        /// </summary>
        public string WireGuard_Peer_IP { get; set; } = "10.0.0.2/32";

        /// <summary>
        /// WireGuard MTU
        /// </summary>
        public int WireGuard_MTU { get; set; } = 1280;

        #endregion

        #region Hysteria Settings

        /// <summary>
        /// Hysteria 端口
        /// </summary>
        public int Hysteria_Port { get; set; }

        /// <summary>
        /// Hysteria 认证密码
        /// </summary>
        public string Hysteria_Auth { get; set; }

        /// <summary>
        /// Hysteria 上行速度 (Mbps)
        /// </summary>
        public int Hysteria_Up { get; set; } = 100;

        /// <summary>
        /// Hysteria 下行速度 (Mbps)
        /// </summary>
        public int Hysteria_Down { get; set; } = 500;

        #endregion

        #region Hysteria2 Settings

        /// <summary>
        /// Hysteria2 端口
        /// </summary>
        public int Hysteria2_Port { get; set; }

        /// <summary>
        /// Hysteria2 认证密码
        /// </summary>
        public string Hysteria2_Auth { get; set; }

        /// <summary>
        /// Hysteria2 上行速度 (Mbps)
        /// </summary>
        public int Hysteria2_Up { get; set; } = 100;

        /// <summary>
        /// Hysteria2 下行速度 (Mbps)
        /// </summary>
        public int Hysteria2_Down { get; set; } = 500;

        #endregion

        #region TUIC Settings

        /// <summary>
        /// TUIC 端口
        /// </summary>
        public int TUIC_Port { get; set; }

        /// <summary>
        /// TUIC UUID
        /// </summary>
        public string TUIC_UUID { get; set; }

        /// <summary>
        /// TUIC 密码
        /// </summary>
        public string TUIC_Password { get; set; }

        #endregion

        #region NaiveProxy Settings

        /// <summary>
        /// NaiveProxy 端口
        /// </summary>
        public int NaiveProxy_Port { get; set; }

        /// <summary>
        /// NaiveProxy 用户名
        /// </summary>
        public string NaiveProxy_User { get; set; }

        /// <summary>
        /// NaiveProxy 密码
        /// </summary>
        public string NaiveProxy_Password { get; set; }

        #endregion

        #region ShadowTLS Settings

        /// <summary>
        /// ShadowTLS 端口
        /// </summary>
        public int ShadowTLS_Port { get; set; }

        /// <summary>
        /// ShadowTLS 密码
        /// </summary>
        public string ShadowTLS_Password { get; set; }

        /// <summary>
        /// ShadowTLS 目标网站
        /// </summary>
        public string ShadowTLS_Dest { get; set; } = "www.mi.com";

        #endregion

        #region SOCKS/HTTP Settings

        /// <summary>
        /// SOCKS 端口
        /// </summary>
        public int SOCKS_Port { get; set; }

        /// <summary>
        /// SOCKS 用户名
        /// </summary>
        public string SOCKS_User { get; set; }

        /// <summary>
        /// SOCKS 密码
        /// </summary>
        public string SOCKS_Password { get; set; }

        /// <summary>
        /// HTTP 端口
        /// </summary>
        public int HTTP_Port { get; set; }

        #endregion

        #region REALITY Settings

        /// <summary>
        /// VLESS XHTTP REALITY 端口
        /// </summary>
        public int VLESS_XHTTP_REALITY_Port { get; set; }

        /// <summary>
        /// REALITY 私钥
        /// </summary>
        public string XHTTP_Reality_PrivateKey { get; set; }

        /// <summary>
        /// REALITY 公钥
        /// </summary>
        public string XHTTP_Reality_PublicKey { get; set; }

        /// <summary>
        /// REALITY 目标网站（回落）
        /// </summary>
        public string XHTTP_Reality_Dest { get; set; } = "www.microsoft.com";

        /// <summary>
        /// REALITY serverNames（逗号分隔）
        /// </summary>
        public string XHTTP_Reality_ServerNames { get; set; } = "www.microsoft.com";

        /// <summary>
        /// REALITY shortId
        /// </summary>
        public string XHTTP_Reality_ShortId { get; set; }

        /// <summary>
        /// REALITY 最大客户端
        /// </summary>
        public int XHTTP_Reality_MaxClient { get; set; }

        /// <summary>
        /// REALITY 最大时间差 (ms)
        /// </summary>
        public int XHTTP_Reality_MaxTimediff { get; set; } = 500;

        #endregion
    }
}