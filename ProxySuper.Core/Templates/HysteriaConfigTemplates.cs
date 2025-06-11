using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Templates
{
    public static class HysteriaConfigTemplates
    {

        #region ServerHysteria2Config
        public static string ServerHysteria2Config_base = """
            listen: :443

            auth:
              type: password
              password: your_password 

            sniff:
              enable: true 
              timeout: 2s 
              rewriteDomain: false 
              tcpPorts: 80,443,8000-9000 
              udpPorts: all
            """;

        public static string ServerHysteria2Config_acme = """
            acme:
              domains:
                - domain1.com
              email: your@email.net
              ca: letsencrypt 
            """;

        public static string ServerHysteria2Config_tls = """
            tls: 
              cert: /etc/hysteria/ssl/hysteria_ssl.crt
              key: /etc/hysteria/ssl/hysteria_ssl.key
              sniGuard: dns-san
            """;

        public static string ServerHysteria2Config_masquerade_proxy = """
            masquerade:
              type: proxy
              proxy:
                url: https://some.site.net 
                rewriteHost: true 
                insecure: false 
              listenHTTP: :80 
              listenHTTPS: :443 
              forceHTTPS: true
            """;

        public static string ServerHysteria2Config_masquerade_file = """
            masquerade:
              type: file
              file:
                dir: /var/www/hymasq 
              listenHTTP: :80 
              listenHTTPS: :443 
              forceHTTPS: true
            """;


        public static string ServerHysteria2Config_bbr = """
            ignoreClientBandwidth: true
            """;

        public static string ServerHysteria1ConfigTemplateOld = """
            {
              "listen": ":36712",
              "acme": {
                "domains": [
                  "your.domain.com"
                ],
                "email": "your@email.com"
              },
              "obfs": "8ZuA2Zpqhuk8yakXvMjDqEXBwY"
            }
            """;
        #endregion

        #region ServerAndClientHysteria2Config
        public static string ServerAndClientHysteria2Config_obfs = """
            obfs:
              type: salamander 
              salamander:
                password: cry_me_a_r1ver
            """;

        public static string ServerAndClientHysteria2Config_bandwidth = """
            bandwidth:
              up: 0 gbps
              down: 0 gbps
            """;
        #endregion

        #region ClientHysteria2Config
        public static string ClientHysteria2Config_base = """
            server: example.com
            auth: some_password
            bandwidth:
              up: 20 mbps
              down: 100 mbps
            socks5:
              listen: 127.0.0.1:1080 
            http:
              listen: 127.0.0.1:8080 
            """;

        public static string ClientHysteria2Config_url = """
            server: hysteria2://user:pass@example.com/
            """;

        #endregion
    }
}
