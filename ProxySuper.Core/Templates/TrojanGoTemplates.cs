using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Templates
{
    public static class TrojanGoTemplates
    {
        public static string BaseCaddyfile = """
            :##port## {
                root * /usr/share/caddy
                file_server
                ##reverse_proxy##
            }

            ##domain##:80 {
                redir https://##domain##{uri}
            }
            """;

        public static string TrojanGoJson = """
            {
              "log_level": 5,
              "run_type": "server",
              "local_addr": "0.0.0.0",
              "local_port": 443,
              "remote_addr": "127.0.0.1",
              "remote_port": 80,
              "password": [
                ""
              ],
              "ssl": {
                "cert": "/usr/local/etc/trojan-go/ssl/trojan-go.crt",
                "key": "/usr/local/etc/trojan-go/ssl/trojan-go.key",
                "sni": "example.com"
              },
              "websocket": {
                "enabled": false,
                "path": "/ws",
                "host": "example.com"
              }
            }
            
            """;
    }
}
