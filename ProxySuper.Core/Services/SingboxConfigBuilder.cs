using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProxySuper.Core.Services
{
    public class SingboxConfigBuilder
    {
        public static string BuildSingboxConfig(SingboxSettings settings)
        {
            var uuidList = new List<string> { settings.UUID };
            uuidList.AddRange(settings.MulitUUID);

            var config = new JObject();

            // Log
            config["log"] = new JObject
            {
                ["disabled"] = false,
                ["level"] = "info",
                ["timestamp"] = true
            };

            // Experimental / clash API
            config["experimental"] = new JObject
            {
                ["clash_api"] = new JObject
                {
                    ["external_controller"] = "127.0.0.1:9090",
                    ["external_ui"] = "metacubexd",
                    ["external_ui_download_url"] = "https://gh-proxy.com/https://github.com/MetaCubeX/metacubexd/archive/refs/heads/gh-pages.zip",
                    ["external_ui_download_detour"] = "direct",
                    ["default_mode"] = "rule"
                },
                ["cache_file"] = new JObject
                {
                    ["enabled"] = true
                }
            };

            // DNS - complete DNS servers with domestic/foreign split
            config["dns"] = new JObject
            {
                ["servers"] = new JArray
                {
                    new JObject // dns_proxy - for foreign DNS over HTTPS
                    {
                        ["tag"] = "dns_proxy",
                        ["type"] = "https",
                        ["server"] = "1.1.1.1",
                        ["server_port"] = 443,
                        ["path"] = "/dns-query",
                        ["domain_resolver"] = "dns_resolver"
                    },
                    new JObject // dns_direct - for domestic DNS over H3
                    {
                        ["tag"] = "dns_direct",
                        ["type"] = "h3",
                        ["server"] = "dns.alidns.com",
                        ["server_port"] = 443,
                        ["path"] = "/dns-query",
                        ["domain_resolver"] = "dns_resolver"
                    },
                    new JObject // google DNS over TLS
                    {
                        ["tag"] = "google",
                        ["type"] = "tls",
                        ["server"] = "8.8.4.4",
                        ["domain_resolver"] = "dns_resolver"
                    },
                    new JObject // dns_resolver - local UDP resolver
                    {
                        ["tag"] = "dns_resolver",
                        ["type"] = "udp",
                        ["server"] = "114.114.114.114"
                    },
                    new JObject // local - block
                    {
                        ["type"] = "local",
                        ["tag"] = "local"
                    }
                },
                ["rules"] = new JArray
                {
                    new JObject { ["action"] = "route", ["clash_mode"] = "direct", ["server"] = "dns_direct" },
                    new JObject { ["action"] = "route", ["clash_mode"] = "global", ["server"] = "dns_proxy" },
                    new JObject { ["action"] = "route", ["rule_set"] = "geosite-cn", ["server"] = "dns_direct" },
                    new JObject { ["action"] = "route", ["rule_set"] = "geosite-geolocation-!cn", ["server"] = "dns_proxy" }
                },
                ["independent_cache"] = true,
                ["strategy"] = "prefer_ipv4",
                ["final"] = "dns_direct"
            };

            // Inbounds
            var inbounds = new JArray();

            // TUN inbound
            inbounds.Add(new JObject
            {
                ["type"] = "tun",
                ["tag"] = "tun-in",
                ["stack"] = "system",
                ["address"] = new JArray { "172.18.0.1/30", "fdfe:dcba:9876::1/126" },
                ["route_address"] = new JArray { "0.0.0.0/1", "128.0.0.0/1", "::/1", "8000::/1" },
                ["route_exclude_address"] = new JArray { "192.168.0.0/16", "10.0.0.0/8", "172.16.0.0/12", "fc00::/7" },
                ["mtu"] = 9000,
                ["auto_route"] = true,
                ["strict_route"] = true
            });

            // Mixed inbound (for HTTP/SOCKS proxy)
            inbounds.Add(new JObject
            {
                ["type"] = "mixed",
                ["tag"] = "mixed-in",
                ["listen"] = "127.0.0.1",
                ["listen_port"] = 1082,
                ["users"] = new JArray()
            });

            // Inbounds - build based on selected types
            var outbounds = new JArray();

            // Outbounds - urltest (自动选择) and selector (手动切换) for proxy groups
            outbounds.Add(new JObject
            {
                ["type"] = "urltest",
                ["tag"] = "自动选择",
                ["outbounds"] = new JArray(), // Will be populated with proxy tags
                ["url"] = "https://www.gstatic.com/generate_204",
                ["interval"] = "3m",
                ["tolerance"] = 50,
                ["interrupt_exist_connections"] = false
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "手动切换",
                ["outbounds"] = new JArray() // Will be populated with proxy tags and groups
            });

            // Category selectors
            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "Telegram",
                ["outbounds"] = new JArray { "手动切换", "自动选择" }
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "YouTube",
                ["outbounds"] = new JArray { "手动切换", "自动选择" }
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "netflix",
                ["outbounds"] = new JArray { "手动切换", "自动选择" }
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "OpenAI",
                ["outbounds"] = new JArray { "手动切换", "自动选择" }
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "Apple",
                ["outbounds"] = new JArray { "手动切换", "自动选择", "direct" }
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "Google",
                ["outbounds"] = new JArray { "手动切换", "自动选择" }
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "Microsoft",
                ["outbounds"] = new JArray { "手动切换", "自动选择", "direct" }
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "Github",
                ["outbounds"] = new JArray { "手动切换", "自动选择", "direct" }
            });

            outbounds.Add(new JObject
            {
                ["type"] = "selector",
                ["tag"] = "本地直连",
                ["outbounds"] = new JArray { "direct", "手动切换", "自动选择" },
                ["default"] = "direct"
            });

            // Outbound - direct
            outbounds.Add(new JObject
            {
                ["type"] = "direct",
                ["tag"] = "direct"
            });

            // Outbound - block
            outbounds.Add(new JObject
            {
                ["type"] = "block",
                ["tag"] = "block"
            });

            // Outbound - DNS
            outbounds.Add(new JObject
            {
                ["type"] = "dns",
                ["tag"] = "dns-out"
            });

            // Main inbound (main port) - only if no types selected
            if (settings.Types.Count == 0)
            {
                var mainInbound = BuildVlessTcp(settings);
                if (mainInbound != null)
                {
                    inbounds.Add(mainInbound);
                }
            }

            // Additional protocol inbounds
            foreach (var type in settings.Types)
            {
                var inbound = BuildProtocolInbound(settings, type);
                if (inbound != null)
                {
                    inbounds.Add(inbound);
                }
            }

            config["inbounds"] = inbounds;
            config["outbounds"] = outbounds;

            // Route - complete route configuration with all rule_sets
            config["route"] = new JObject
            {
                ["default_domain_resolver"] = new JObject
                {
                    ["server"] = "dns_resolver",
                    ["strategy"] = "prefer_ipv4"
                },
                ["rule_set"] = new JArray
                {
                    new JObject { ["tag"] = "geosite-category-ads-all", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/category-ads-all.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-telegram", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/telegram.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geoip-telegram", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geoip/telegram.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-youtube", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/youtube.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-netflix", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/netflix.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geoip-netflix", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geoip/netflix.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-openai@ads", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/openai@ads.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-openai", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/openai.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-apple", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/apple.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-google", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/google.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geoip-google", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geoip/google.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-microsoft", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/microsoft.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-geolocation-!cn", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/geolocation-!cn.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-github", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/github.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-private", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/private.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geosite-cn", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geosite/cn.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geoip-private", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geoip/private.srs", ["update_interval"] = "1d" },
                    new JObject { ["tag"] = "geoip-cn", ["type"] = "remote", ["format"] = "binary", ["url"] = "https://gh-proxy.com/https://raw.githubusercontent.com/MetaCubeX/meta-rules-dat/sing/geo/geoip/cn.srs", ["update_interval"] = "1d" }
                },
                ["rules"] = new JArray
                {
                    new JObject { ["inbound"] = "mixed-in", ["action"] = "sniff", ["timeout"] = "1s" },
                    new JObject { ["inbound"] = "tun-in", ["action"] = "sniff", ["timeout"] = "1s" },
                    new JObject { ["protocol"] = "dns", ["action"] = "hijack-dns" },
                    new JObject { ["ip_is_private"] = true, ["outbound"] = "direct" },
                    new JObject { ["clash_mode"] = "global", ["outbound"] = "手动切换" },
                    new JObject { ["clash_mode"] = "direct", ["outbound"] = "本地直连" },
                    new JObject
                    {
                        ["type"] = "logical",
                        ["mode"] = "or",
                        ["rules"] = new JArray
                        {
                            new JObject { ["rule_set"] = "geosite-category-ads-all" },
                            new JObject { ["domain_regex"] = "^stun\\..+" },
                            new JObject { ["domain_keyword"] = new JArray { "stun", "httpdns" } },
                            new JObject { ["protocol"] = "stun" }
                        },
                        ["action"] = "reject"
                    },
                    new JObject { ["rule_set"] = new JArray { "geosite-telegram", "geoip-telegram" }, ["outbound"] = "Telegram" },
                    new JObject { ["rule_set"] = "geosite-youtube", ["outbound"] = "YouTube" },
                    new JObject { ["rule_set"] = new JArray { "geosite-netflix", "geoip-netflix" }, ["outbound"] = "netflix" },
                    new JObject { ["rule_set"] = "geosite-openai@ads", ["action"] = "reject" },
                    new JObject { ["rule_set"] = "geosite-openai", ["outbound"] = "OpenAI" },
                    new JObject { ["rule_set"] = "geosite-apple", ["outbound"] = "Apple" },
                    new JObject { ["rule_set"] = new JArray { "geosite-google", "geoip-google" }, ["outbound"] = "Google" },
                    new JObject { ["rule_set"] = "geosite-microsoft", ["outbound"] = "Microsoft" },
                    new JObject { ["rule_set"] = "geosite-github", ["outbound"] = "Github" },
                    new JObject { ["rule_set"] = "geosite-geolocation-!cn", ["outbound"] = "手动切换" },
                    new JObject { ["rule_set"] = new JArray { "geosite-private", "geosite-cn", "geoip-private", "geoip-cn" }, ["outbound"] = "本地直连" }
                },
                ["final"] = "本地直连",
                ["auto_detect_interface"] = true
            };

            return JsonConvert.SerializeObject(config, Formatting.Indented);
        }

        private static JArray BuildUsersByUUID(SingboxSettings settings)
        {
            var uuidList = new List<string> { settings.UUID };
            uuidList.AddRange(settings.MulitUUID);
            var users = new JArray();
            foreach (var uuid in uuidList)
            {
                users.Add(new JObject { ["uuid"] = uuid, ["flow"] = "" });
            }
            return users;
        }

        private static JArray BuildVmessUsers(SingboxSettings settings)
        {
            var uuidList = new List<string> { settings.UUID };
            uuidList.AddRange(settings.MulitUUID);
            var users = new JArray();
            foreach (var uuid in uuidList)
            {
                users.Add(new JObject { ["uuid"] = uuid, ["alterId"] = 0 });
            }
            return users;
        }

        private static JObject BuildTlsConfig(SingboxSettings settings)
        {
            if (!settings.WithTLS) return null;
            return JObject.FromObject(new
            {
                enabled = true,
                server_name = settings.Domain,
                server_certificate_path = "/etc/sing-box/cert.pem",
                server_key_path = "/etc/sing-box/key.pem"
            });
        }

        private static JObject BuildTransport(string type, string path = null, string serviceName = null)
        {
            var transport = new JObject { ["type"] = type };
            if (!string.IsNullOrEmpty(path))
                transport["path"] = path;
            if (!string.IsNullOrEmpty(serviceName))
                transport["service_name"] = serviceName;
            return transport;
        }

        private static JObject BuildBaseInbound(string type, string tag, int port)
        {
            var inbound = new JObject
            {
                ["type"] = type,
                ["tag"] = tag,
                ["listen"] = "::",
                ["listen_port"] = port,
                ["sniff"] = true,
                ["sniff_override_destination"] = true,
                ["domain_strategy"] = "prefer_ipv4"
            };
            return inbound;
        }

        private static JObject BuildProtocolInbound(SingboxSettings settings, SingBoxType type)
        {
            switch (type)
            {
                case SingBoxType.SingBox_VLESS_TCP:
                    return BuildVlessTcp(settings);
                case SingBoxType.SingBox_VLESS_WS:
                    return BuildVlessWs(settings);
                case SingBoxType.SingBox_VLESS_gRPC:
                    return BuildVlessGrpc(settings);
                case SingBoxType.SingBox_VLESS_XHTTP_REALITY:
                    return BuildVlessXhttpReality(settings);
                case SingBoxType.SingBox_VMESS_TCP:
                    return BuildVmessTcp(settings);
                case SingBoxType.SingBox_VMESS_WS:
                    return BuildVmessWs(settings);
                case SingBoxType.SingBox_Trojan:
                    return BuildTrojan(settings);
                case SingBoxType.SingBox_Shadowsocks:
                    return BuildShadowsocks(settings);
                case SingBoxType.SingBox_Shadowsocks_2022:
                    return BuildShadowsocks2022(settings);
                case SingBoxType.SingBox_WireGuard:
                    return BuildWireGuard(settings);
                case SingBoxType.SingBox_Hysteria:
                    return BuildHysteria(settings);
                case SingBoxType.SingBox_Hysteria2:
                    return BuildHysteria2(settings);
                case SingBoxType.SingBox_TUIC:
                    return BuildTuic(settings);
                case SingBoxType.SingBox_NaiveProxy:
                    return BuildNaiveProxy(settings);
                case SingBoxType.SingBox_ShadowTLS:
                    return BuildShadowTLS(settings);
                case SingBoxType.SingBox_SOCKS:
                    return BuildSocks(settings);
                case SingBoxType.SingBox_HTTP:
                    return BuildHttp(settings);
                default:
                    return null;
            }
        }

        private static JObject BuildVlessTcp(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("vless", "vless-tcp-in", settings.Port);
            inbound["users"] = BuildUsersByUUID(settings);
            var tls = BuildTlsConfig(settings);
            if (tls != null) inbound["tls"] = tls;
            return inbound;
        }

        private static JObject BuildVlessWs(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("vless", "vless-ws-in", settings.VLESS_WS_Port);
            inbound["users"] = BuildUsersByUUID(settings);
            inbound["transport"] = BuildTransport("ws", path: settings.VLESS_WS_Path);
            var tls = BuildTlsConfig(settings);
            if (tls != null) inbound["tls"] = tls;
            return inbound;
        }

        private static JObject BuildVlessGrpc(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("vless", "vless-grpc-in", settings.VLESS_gRPC_Port);
            inbound["users"] = BuildUsersByUUID(settings);
            inbound["transport"] = BuildTransport("grpc", serviceName: settings.VLESS_gRPC_ServiceName);
            var tls = BuildTlsConfig(settings);
            if (tls != null) inbound["tls"] = tls;
            return inbound;
        }

        private static JObject BuildVlessXhttpReality(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("vless", "vless-reality-in", settings.VLESS_XHTTP_REALITY_Port);

            var users = new JArray();
            var uuidList = new List<string> { settings.UUID };
            uuidList.AddRange(settings.MulitUUID);
            foreach (var uuid in uuidList)
            {
                users.Add(new JObject
                {
                    ["uuid"] = uuid,
                    ["flow"] = "xtls-rprx-vision"
                });
            }
            inbound["users"] = users;

            inbound["tls"] = JObject.FromObject(new
            {
                enabled = true,
                server_name = settings.XHTTP_Reality_ServerNames?.Split(',')[0] ?? settings.Domain,
                reality = new
                {
                    enabled = true,
                    private_key = settings.XHTTP_Reality_PrivateKey,
                    short_id = new string[] { settings.XHTTP_Reality_ShortId ?? "0" },
                    max_time_difference = settings.XHTTP_Reality_MaxTimediff,
                    handshake = new
                    {
                        server = settings.XHTTP_Reality_Dest ?? "www.microsoft.com",
                        server_port = 443
                    }
                }
            });

            return inbound;
        }

        private static JObject BuildVmessTcp(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("vmess", "vmess-tcp-in", settings.Port);
            inbound["users"] = BuildVmessUsers(settings);
            var tls = BuildTlsConfig(settings);
            if (tls != null) inbound["tls"] = tls;
            return inbound;
        }

        private static JObject BuildVmessWs(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("vmess", "vmess-ws-in", settings.VMESS_WS_Port);
            inbound["users"] = BuildVmessUsers(settings);
            inbound["transport"] = BuildTransport("ws", path: settings.VMESS_WS_Path);
            var tls = BuildTlsConfig(settings);
            if (tls != null) inbound["tls"] = tls;
            return inbound;
        }

        private static JObject BuildTrojan(SingboxSettings settings)
        {
            var password = string.IsNullOrEmpty(settings.Trojan_Password)
                ? settings.UUID
                : settings.Trojan_Password;

            var inbound = BuildBaseInbound("trojan", "trojan-in", settings.Trojan_Port);
            inbound["users"] = new JArray { new JObject { ["password"] = password } };
            var tls = BuildTlsConfig(settings);
            if (tls != null) inbound["tls"] = tls;
            return inbound;
        }

        private static JObject BuildShadowsocks(SingboxSettings settings)
        {
            var password = string.IsNullOrEmpty(settings.Shadowsocks_Password)
                ? settings.UUID
                : settings.Shadowsocks_Password;

            var inbound = BuildBaseInbound("shadowsocks", "shadowsocks-in", settings.Shadowsocks_Port);
            inbound["method"] = settings.Shadowsocks_Method;
            inbound["password"] = password;
            return inbound;
        }

        private static JObject BuildShadowsocks2022(SingboxSettings settings)
        {
            var password = string.IsNullOrEmpty(settings.Shadowsocks_2022_Password)
                ? settings.UUID
                : settings.Shadowsocks_2022_Password;

            var inbound = BuildBaseInbound("shadowsocks", "shadowsocks-2022-in", settings.Shadowsocks_2022_Port);
            inbound["method"] = settings.Shadowsocks_2022_Method;
            inbound["password"] = password;
            return inbound;
        }

        private static JObject BuildWireGuard(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("wireguard", "wireguard-in", settings.WireGuard_Port);
            inbound["private_key"] = settings.WireGuard_PrivateKey;
            inbound["peers"] = new JArray
            {
                new JObject
                {
                    ["public_key"] = settings.WireGuard_Peer_PublicKey,
                    ["allowed_ips"] = settings.WireGuard_Peer_IP
                }
            };
            return inbound;
        }

        private static JObject BuildHysteria(SingboxSettings settings)
        {
            var auth = string.IsNullOrEmpty(settings.Hysteria_Auth)
                ? settings.UUID
                : settings.Hysteria_Auth;

            var inbound = BuildBaseInbound("hysteria", "hysteria-in", settings.Hysteria_Port);
            inbound["up_speed"] = settings.Hysteria_Up * 1000000;
            inbound["down_speed"] = settings.Hysteria_Down * 1000000;
            inbound["auth_str"] = auth;
            // Hysteria requires TLS
            inbound["tls"] = JObject.FromObject(new
            {
                enabled = true,
                server_name = settings.Domain,
                server_certificate_path = "/etc/sing-box/cert.pem",
                server_key_path = "/etc/sing-box/key.pem"
            });
            return inbound;
        }

        private static JObject BuildHysteria2(SingboxSettings settings)
        {
            var auth = string.IsNullOrEmpty(settings.Hysteria2_Auth)
                ? settings.UUID
                : settings.Hysteria2_Auth;

            var inbound = BuildBaseInbound("hysteria2", "hysteria2-in", settings.Hysteria2_Port);
            inbound["up_speed"] = settings.Hysteria2_Up * 1000000;
            inbound["down_speed"] = settings.Hysteria2_Down * 1000000;
            inbound["password"] = auth;
            // Hysteria2 requires TLS
            inbound["tls"] = JObject.FromObject(new
            {
                enabled = true,
                server_name = settings.Domain,
                server_certificate_path = "/etc/sing-box/cert.pem",
                server_key_path = "/etc/sing-box/key.pem"
            });
            return inbound;
        }

        private static JObject BuildTuic(SingboxSettings settings)
        {
            var uuid = string.IsNullOrEmpty(settings.TUIC_UUID)
                ? settings.UUID
                : settings.TUIC_UUID;
            var password = string.IsNullOrEmpty(settings.TUIC_Password)
                ? settings.UUID
                : settings.TUIC_Password;

            var inbound = BuildBaseInbound("tuic", "tuic-in", settings.TUIC_Port);
            inbound["users"] = new JArray
            {
                new JObject { ["uuid"] = uuid, ["password"] = password }
            };
            inbound["congestion_control"] = "bbr";
            // TUIC requires TLS
            inbound["tls"] = JObject.FromObject(new
            {
                enabled = true,
                server_name = settings.Domain,
                server_certificate_path = "/etc/sing-box/cert.pem",
                server_key_path = "/etc/sing-box/key.pem"
            });
            return inbound;
        }

        private static JObject BuildNaiveProxy(SingboxSettings settings)
        {
            var user = string.IsNullOrEmpty(settings.NaiveProxy_User)
                ? settings.UUID
                : settings.NaiveProxy_User;
            var password = string.IsNullOrEmpty(settings.NaiveProxy_Password)
                ? settings.UUID
                : settings.NaiveProxy_Password;

            var inbound = BuildBaseInbound("naive", "naive-in", settings.NaiveProxy_Port);
            inbound["users"] = new JArray
            {
                new JObject { ["username"] = user, ["password"] = password }
            };
            // Naive requires TLS
            inbound["tls"] = JObject.FromObject(new
            {
                enabled = true,
                server_name = settings.Domain,
                server_certificate_path = "/etc/sing-box/cert.pem",
                server_key_path = "/etc/sing-box/key.pem"
            });
            return inbound;
        }

        private static JObject BuildShadowTLS(SingboxSettings settings)
        {
            var password = string.IsNullOrEmpty(settings.ShadowTLS_Password)
                ? settings.UUID
                : settings.ShadowTLS_Password;

            var inbound = BuildBaseInbound("shadowtls", "shadowtls-in", settings.ShadowTLS_Port);
            inbound["version"] = 3;
            inbound["password"] = password;
            inbound["server"] = $"tcp+https://{settings.ShadowTLS_Dest}:443";
            return inbound;
        }

        private static JObject BuildSocks(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("socks", "socks-in", settings.SOCKS_Port);

            if (!string.IsNullOrEmpty(settings.SOCKS_User))
            {
                inbound["users"] = new JArray
                {
                    new JObject
                    {
                        ["username"] = settings.SOCKS_User,
                        ["password"] = settings.SOCKS_Password
                    }
                };
            }
            return inbound;
        }

        private static JObject BuildHttp(SingboxSettings settings)
        {
            var inbound = BuildBaseInbound("http", "http-in", settings.HTTP_Port);
            return inbound;
        }
    }
}
