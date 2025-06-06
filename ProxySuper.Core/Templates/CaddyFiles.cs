using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Templates
{
    public static class CaddyFiles
    {
        public static string BaseCaddyFile = """
            :##port## {
                root * /usr/share/caddy
                file_server
                ##reverse_proxy##
            }

            ##domain##:80 {
                redir https://##domain##{uri}
            }
            """;
    }
}
