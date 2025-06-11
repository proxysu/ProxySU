using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Templates
{
    public static class NaiveServerCaddyfile
    {
        public static string BaseCaddyFile = """
            :##port##, ##domain##
            route {
              forward_proxy {
                ##basicauth##
                hide_ip
                hide_via
                probe_resistance
              }
              #file_server { root /usr/share/caddy }
            }
            ##reverse_proxy##
            """;
    }
}
