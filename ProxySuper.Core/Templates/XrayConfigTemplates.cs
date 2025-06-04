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
                    "network": "xhttp",
                    "security": "none",
                    "xhttpSettings": {
                        "acceptProxyProtocol": true,
                        "path": ""
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


        #region ServerGeneralConfig
        public static string ServerGeneralConfig_log = """
            {
                "log": {
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










    }
}
