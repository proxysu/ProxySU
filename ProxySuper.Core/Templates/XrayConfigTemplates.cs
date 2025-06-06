using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Templates
{
    public static class XrayConfigTemplates
    {
        #region VLESS_XTLS_RAW_REALITY Configs
        //参考自https://github.com/XTLS/Xray-examples/tree/main/VLESS-TCP-REALITY%20(without%20being%20stolen)
        //添加 xtls-rprx-vision
        public static string VLESS_XTLS_RAW_REALITY_ServerConfig = """
            {
                "inbounds": [
                    {
                        "tag": "dokodemo-in",
                        "port": 443,
                        "protocol": "dokodemo-door",
                        "settings": {
                            "address": "127.0.0.1",
                            "port": 4431,
                            "network": "tcp"
                        },
                        "sniffing": {
                            "enabled": true,
                            "destOverride": [
                                "tls"
                            ],
                            "routeOnly": true
                        }
                    },
                    {
                        "listen": "127.0.0.1",
                        "port": 4431,
                        "protocol": "vless",
                        "settings": {
                            "clients": [
                                {
                                    "id": "",
                                    "flow": "xtls-rprx-vision"
                                }
                            ],
                            "decryption": "none"
                        },
                        "streamSettings": {
                            "network": "raw",
                            "security": "reality",
                            "realitySettings": {
                                "target": "speed.cloudflare.com:443",
                                "serverNames": [
                                    "speed.cloudflare.com"
                                ],
                                "privateKey": "",
                                "shortIds": [ "" ]
                            }
                        },
                        "sniffing": {
                            "enabled": true,
                            "destOverride": [
                                "http",
                                "tls",
                                "quic"
                            ],
                            "routeOnly": true
                        }
                    }
                ],
                "routing": {
                    "rules": [
                        {
                            "inboundTag": [
                                "dokodemo-in"
                            ],
                            "domain": [
                                "speed.cloudflare.com"
                            ],
                            "outboundTag": "direct"
                        },
                        {
                            "inboundTag": [
                                "dokodemo-in"
                            ],
                            "outboundTag": "block"
                        }
                    ]
                }
            }
            """;

        public static string VLESS_XTLS_REALITY_ClientConfig = """
            {
                "outbounds": [
                    {
                        "protocol": "vless",
                        "settings": {
                            "vnext": [
                                {
                                    "address": "127.0.0.1", 
                                    "port": 443, 
                                    "users": [
                                        {
                                            "id": "", 
                                            "flow": "xtls-rprx-vision",
                                            "encryption": "none"
                                        }
                                    ]
                                }
                            ]
                        },
                        "streamSettings": {
                            "network": "raw",
                            "security": "reality",
                            "realitySettings": {
                                "fingerprint": "chrome", 
                                "serverName": "speed.cloudflare.com",
                                "publicKey": "",
                                "spiderX": "/",
                                "shortId": ""
                            }
                        },
                        "tag": "proxy"
                    }
                ]
            }
            """;
        #endregion

        #region VLESS XHTTP Configs

        public static string VLESS_XHTTP_ServerConfig = """
            {
                "port": 443,
                "protocol": "vless",
                "settings": {
                    "clients": [
                        {
                            "id": ""
                        }
                    ],
                    "decryption": "none",
                    "fallbacks": []
                },
                "streamSettings": {
                    "network": "xhttp",
                    "xhttpSettings": {
                        "path": ""
                    },
                    "security": "tls",
                    "tlsSettings": {
                        "rejectUnknownSni": true,
                        "minVersion": "1.2",
                        "certificates": [
                            {
                                "ocspStapling": 3600,
                                "certificateFile": "/usr/local/etc/xray/ssl/xray_ssl.crt",
                                "keyFile": "/usr/local/etc/xray/ssl/xray_ssl.key"
                            }
                        ]
                    }

                }
            }
            """;

        public static string VLESS_XHTTP_ClientConfig = """
            {
              "outbounds": [
                {
                  "tag": "proxy",
                  "protocol": "vless",
                  "settings": {
                    "vnext": [
                      {
                        "address": "",
                        "port": 443,
                        "users": [
                          {
                            "id": "",
                            "encryption": "none"
                          }
                        ]
                      }
                    ]
                  },
                  "streamSettings": {
                    "network": "xhttp",
                    "security": "tls",
                    "tlsSettings": {
                      "allowInsecure": false,
                      "alpn": [
                        "h3",
                        "h2",
                        "http/1.1"
                      ]
                    },
                    "xhttpSettings": {
                      "path": "",
                      "mode": "packet-up"
                    }
                  }
                }
              ]
            }

            """;
        #endregion

        #region VLESS XTLS Configs
        public static string VLESS_XTLS_ServerConfig = """
            {
              "port": 443,
              "protocol": "vless",
              "settings": {
                "clients": [
                  {
                    "id": "",
                    "flow": "xtls-rprx-vision"
                  }
                ],
                "decryption": "none",
                "fallbacks": []
              },
              "streamSettings": {
                "network": "raw",
                "security": "tls",
                "tlsSettings": {
                  "rejectUnknownSni": true,
                  "minVersion": "1.2",
                  "certificates": [
                    {
                      "ocspStapling": 3600,
                      "certificateFile": "/usr/local/etc/xray/ssl/xray_ssl.crt",
                      "keyFile": "/usr/local/etc/xray/ssl/xray_ssl.key"
                    }
                  ]
                }
              }
            }
            """;
        public static string VLESS_XTLS_ClientConfig = """

            """;

        #endregion

        #region VLESS WS Configs
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
                        {
              "outbounds": [
                {
                  "protocol": "vless",
                  "settings": {
                    "vnext": [
                      {
                        "address": "",
                        "port": 443,
                        "users": [
                          {
                            "id": "",
                            "encryption": "none",
                            "level": 0
                          }
                        ]
                      }
                    ]
                  },
                  "streamSettings": {
                    "network": "ws",
                    "security": "tls",
                    "tlsSettings": {
                      "serverName": ""
                    },
                    "wsSettings": {
                      "path": ""
                    }
                  }
                }
              ]
            }
            
            """;
        #endregion

        #region VLESS gRPC Configs
        public static string VLESS_gRPC_ServerConfig = """
            {
              "port": 2002,
              "listen": "0.0.0.0",
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
                "network": "grpc",
                "security": "tls",
                "tlsSettings": {
                  "serverName": "domain",
                  "alpn": [
                    "h2"
                  ],
                  "certificates": [
                    {
                      "certificateFile": "/usr/local/etc/xray/ssl/xray_ssl.crt",
                      "keyFile": "/usr/local/etc/xray/ssl/xray_ssl.key"
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
                "network": "raw",
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
                    "domainMatcher": "mph",
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
