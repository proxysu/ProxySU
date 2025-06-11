using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Templates
{
    public static class V2rayConfigTemplates
    {
        #region VLESS_TCP_TLS_ServerConfig
        public static string VLESS_TCP_TLS_ServerConfig = """
                        {
              "port": 443,
              "protocol": "vless",
              "settings": {
                "clients": [
                  {
                    "id": "",
                    "level": 0
                  }
                ],
                "decryption": "none",
                "fallbacks": []
              },
              "streamSettings": {
                "network": "tcp",
                "security": "tls",
                "tlsSettings": {
                  "alpn": [
                    "http/1.1"
                  ],
                  "certificates": [
                    {
                      "certificateFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.crt",
                      "keyFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.key"
                    }
                  ]
                }
              }
            }
            """;
        public static string VLESS_TCP_TLS_ClientConfig = """

            """;
        #endregion

        #region VLESS_WS_ServerConfig
        public static string VLESS_WS_ServerConfig = """
                        {
              "port": 1234,
              "listen": "127.0.0.1",
              "protocol": "vless",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ],
                "decryption": "none"
              },
              "streamSettings": {
                "network": "ws",
                "security": "none",
                "wsSettings": {
                  "acceptProxyProtocol": true,
                  "path": "/websocket"
                }
              }
            }
            
            """;
        public static string VLESS_WS_ClientConfig = """

            """;
        #endregion

        #region VLESS_QUIC_ServerConfig
        public static string VLESS_QUIC_ServerConfig = """
                        {
              "port": 2000,
              "protocol": "vless",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ],
                "decryption": "none"
              },
              "streamSettings": {
                "network": "quic",
                "quicSettings": {
                  "security": "none",
                  "key": "",
                  "header": {
                    "type": "none"
                  }
                },
                "security": "tls",
                "tlsSettings": {
                  "certificates": [
                    {
                      "certificateFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.crt",
                      "keyFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.key"
                    }
                  ]
                }
              }
            }
            """;
        public static string VLESS_QUIC_ClientConfig = """

            """;
        #endregion

        #region VLESS_KCP_ServerConfig
        public static string VLESS_KCP_ServerConfig = """
                        {
              "port": 3456,
              "protocol": "vless",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ],
                "decryption": "none"
              },
              "streamSettings": {
                "network": "mkcp",
                "kcpSettings": {
                  "uplinkCapacity": 100,
                  "downlinkCapacity": 100,
                  "congestion": true,
                  "header": {
                    "type": "none"
                  },
                  "seed": null
                }
              }
            }
            
            """;
        public static string VLESS_KCP_ClientConfig = """

            """;
        #endregion

        #region VLESS_HTTP2_ServerConfig
        public static string VLESS_HTTP2_ServerConfig = """
                        {
              "port": 1234,
              "listen": "127.0.0.1",
              "protocol": "vmess",
              "settings": {
                "decryption": "none",
                "clients": [
                  {
                    "id": ""
                  }
                ]
              },
              "streamSettings": {
                "network": "h2",
                "httpSettings": {
                  "path": ""
                }
              }
            }
            """;
        public static string VLESS_HTTP2_ClientConfig = """

            """;
        #endregion

        #region VLESS_gRPC_ServerConfig
        public static string VLESS_gRPC_ServerConfig = """
                        {
              "port": 2002,
              "listen": "0.0.0.0",
              "protocol": "vless",
              "settings": {
                "decryption": "none",
                "clients": [
                  {
                    "id": ""
                  }
                ]
              },
              "streamSettings": {
                "network": "grpc",
                "security": "tls",
                "tlsSettings": {
                  "serverName": "domain",
                  "alpn": [
                    "h2"
                  ],
                  "certificates": [
                    {
                      "certificateFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.crt",
                      "keyFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.key"
                    }
                  ]
                },
                "grpcSettings": {
                  "serviceName": "service_name"
                }
              }
            }
            
            """;
        public static string VLESS_gRPC_ClientConfig = """

            """;
        #endregion

        #region VMESS_WS_ServerConfig
        public static string VMESS_WS_ServerConfig = """
                        {
              "port": 3456,
              "listen": "127.0.0.1",
              "protocol": "vmess",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ]
              },
              "streamSettings": {
                "network": "ws",
                "security": "none",
                "wsSettings": {
                  "acceptProxyProtocol": true,
                  "path": "/vmessws"
                }
              }
            }
            
            """;
        public static string VMESS_WS_ClientConfig = """

            """;
        #endregion

        #region VMESS_TCP_ServerConfig
        public static string VMESS_TCP_ServerConfig = """
                        {
              "port": 443,
              "listen": "127.0.0.1",
              "protocol": "vmess",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ]
              },
              "streamSettings": {
                "network": "tcp",
                "security": "none",
                "tcpSettings": {
                  "acceptProxyProtocol": true,
                  "header": {
                    "type": "http",
                    "request": {
                      "path": [
                        "/vmesstcp"
                      ]
                    }
                  }
                }
              }
            }
            
            """;
        public static string VMESS_TCP_ClientConfig = """

            """;
        #endregion

        #region VMESS_QUIC_ServerConfig
        public static string VMESS_QUIC_ServerConfig = """
                        {
              "port": 3000,
              "protocol": "vmess",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ],
                "decryption": "none"
              },
              "streamSettings": {
                "network": "quic",
                "quicSettings": {
                  "security": "none",
                  "key": "",
                  "header": {
                    "type": "none"
                  }
                },
                "security": "tls",
                "tlsSettings": {
                  "certificates": [
                    {
                      "certificateFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.crt",
                      "keyFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.key"
                    }
                  ]
                }
              }
            }
            """;
        public static string VMESS_QUIC_ClientConfig = """

            """;
        #endregion


        #region VMESS KCP Configs
        public static string VMESS_KCP_ServerConfig = """
            {
              "port": 3456,
              "protocol": "vmess",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ]
              },
              "streamSettings": {
                "network": "mkcp",
                "kcpSettings": {
                  "uplinkCapacity": 100,
                  "downlinkCapacity": 100,
                  "congestion": true,
                  "header": {
                    "type": "none"
                  },
                  "seed": null
                }
              }
            }
            
            """;
        public static string VMESS_KCP_ClientConfig = """

            """;
        #endregion


        #region VMESS_HTTP2_ServerConfig
        public static string VMESS_HTTP2_ServerConfig = """
                        {
              "port": 1234,
              "listen": "127.0.0.1",
              "protocol": "vmess",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ]
              },
              "streamSettings": {
                "network": "h2",
                "httpSettings": {
                  "path": ""
                }
              }
            }
            """;
        public static string VMESS_HTTP2_ClientConfig = """

            """;
        #endregion

        #region VMESS_gRPC_ServerConfig
        public static string VMESS_gRPC_ServerConfig = """
            {
              "port": 2002,
              "listen": "0.0.0.0",
              "protocol": "vmess",
              "settings": {
                "clients": [
                  {
                    "id": ""
                  }
                ]
              },
              "streamSettings": {
                "network": "grpc",
                "security": "tls",
                "tlsSettings": {
                  "serverName": "domain",
                  "alpn": [
                    "h2"
                  ],
                  "certificates": [
                    {
                      "certificateFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.crt",
                      "keyFile": "/usr/local/etc/v2ray/ssl/v2ray_ssl.key"
                    }
                  ]
                },
                "grpcSettings": {
                  "serviceName": "service_name"
                }
              }
            }
            
            """;
        public static string VMESS_gRPC_ClientConfig = """

            """;
        #endregion

        #region Trojan Configs
        public static string Trojan_ServerConfig = """
            {
              "port": 1310,
              "listen": "127.0.0.1",
              "protocol": "trojan",
              "settings": {
                "clients": [
                  {
                    "password": ""
                  }
                ],
                "fallbacks": [
                  {
                    "dest": 8080
                  }
                ]
              },
              "streamSettings": {
                "network": "tcp",
                "security": "none",
                "tcpSettings": {
                  "acceptProxyProtocol": true
                }
              }
            }
            
            """;
        public static string Trojan_ClientConfig = """

            """;
        #endregion

        #region Shadowsocks Configs
        public static string Shadowsocks_ServerConfig = """
            {
              "port": 12345,
              "protocol": "shadowsocks",
              "settings": {
                "password": "",
                "method": "aes-128-gcm",
                "network": "tcp,udp"
              }
            }
            """;
        public static string Shadowsocks_ClientConfig = """

            """;
        #endregion


        #region ServerGeneralConfig
        public static string ServerGeneralConfig_log = """
            {
                "log": {
                    "access": "none",
                    "loglevel": "none"
                }
            }
            """;

        public static string ServerGeneralConfig_outbounds = """
            {
                "outbounds": [
                    {
                        "protocol": "freedom",
                        "tag": "direct"
                    },
                    {
                        "protocol": "blackhole",
                        "tag": "block"
                    }
                ]
            }
            """;

        public static string ServerGeneralConfig_inbounds = """
            {
              "inbounds": []
            }
            
            """;

        public static string ServerGeneralConfig_dns = """
            {
                "dns": {}
            }
            """;

        public static string ServerGeneralConfig_routing_BlockPrivateIPAndCN = """
            {
                "routing": {
                    "rules": [
                        {
                            "ip": [
                                "geoip:private",
                                "geoip:cn"
                            ],
                            "outboundTag": "block"
                        }
                    ]
                }
            }
            """;

        public static string ServerGeneralConfig_routing_BlockPrivateIP = """
            {
                "routing": {
                    "rules": [
                        {
                            "ip": [
                                "geoip:private"
                            ],
                            "outboundTag": "block"
                        }
                    ]
                }
            }
            """;

        #endregion


        #region ClientGeneralConfig
        public static string ClientGeneralConfig_log = """
            {
                "log": {
                    "loglevel": "warning"
                }
            }
            """;

        public static string ClientGeneralConfig_inbounds = """
            {
              "inbounds": [
                {
                  "port": 10808,
                  "protocol": "socks",
                  "settings": {
                    "udp": true,
                    "auth": "noauth"
                  }
                }
              ]
            }
            """;


        public static string ClientGeneralConfig_outbounds = """
            {
                "outbounds": []
            }
            """;

        public static string ClientGeneralConfig_dns = """
            {
                "dns": {}
            }
            """;

        public static string ClientGeneralConfig_routing = """
            {
                "routing": {}
            }
            """;

        public static string ClientGeneralConfig_WhiteList = """
            {
                "outbounds": [
                    {
                        "tag": "direct",
                        "protocol": "freedom"
                    },
                    {
                        "tag": "block",
                        "protocol": "blackhole"
                    }
                ],
                "dns": {
                    "servers": [
                        "1.1.1.1",
                        "8.8.8.8",
                        "8.8.4.4",
                        "2001:4860:4860::8888",
                        "2001:4860:4860::8844",
                        "https://1.1.1.1/dns-query",
                        "https://dns.google/dns-query",
                        {
                            "address": "223.5.5.5",
                            "domains": [
                                "geosite:cn"
                            ],
                            "expectIPs": [
                                "geoip:cn"
                            ]
                        }
                        {
                            "address": "2400:3200::1",
                            "domains": [
                                "geosite:cn"
                            ],
                            "expectIPs": [
                                "geoip:cn"
                            ]
                        }
                    ]
                },
                "routing": {
                    "domainStrategy": "IPIfNonMatch",
                    "rules": [
                        {
                            "network": "udp",
                            "port": "443",
                            "outboundTag": "blocked"
                        },
                        {
                            "ip": [
                                "223.5.5.5",
                                "2400:3200::1",
                                "geoip:private", 
                                "geoip:cn"
                            ], 
                            "outboundTag": "direct"
                        },
                        {
                            "domain": ["geosite:cn"],
                            "outboundTag": "direct"
                        }
                    ]
                }
            }
            """;

        #endregion

        public static string BaseConfig = """
            {
                "log": {},
                "dns": {},
                "routing": {},
                "inbounds": [],
                "outbounds": []
            }
            """;
    }
}
