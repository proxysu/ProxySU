using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Services
{
    public class TrojanGoConfigBuilder
    {
        public static readonly int WebPort = 8088;

        public static readonly string TrojanGoSettingPath = @"Templates\trojan-go\trojan-go.json";

        public static readonly string CaddyFilePath = @"Templates\trojan-go\base.caddyfile";

        public static string BuildTrojanGoConfig(TrojanGoSettings parameters)
        {
            var jsonStr = File.ReadAllText(TrojanGoSettingPath);
            var settings = JToken.FromObject(JsonConvert.DeserializeObject(jsonStr));

            settings["remote_port"] = WebPort;
            settings["password"][0] = parameters.Password;
            settings["ssl"]["sni"] = parameters.Domain;

            if (parameters.EnableWebSocket)
            {
                settings["websocket"]["enabled"] = true;
                settings["websocket"]["path"] = parameters.WebSocketPath;
                settings["websocket"]["host"] = parameters.Domain;
            }

            return JsonConvert.SerializeObject(settings, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public static string BuildCaddyConfig(TrojanGoSettings parameters, bool useCustomWeb = false)
        {
            var caddyStr = File.ReadAllText(CaddyFilePath);
            caddyStr = caddyStr.Replace("##domain##", parameters.Domain);
            caddyStr = caddyStr.Replace("##port##", WebPort.ToString());

            if (!useCustomWeb && !string.IsNullOrEmpty(parameters.MaskDomain))
            {
                var prefix = "http://";
                if (parameters.MaskDomain.StartsWith("https://"))
                {
                    prefix = "https://";
                }
                var domain = parameters.MaskDomain
                    .TrimStart("http://".ToCharArray())
                    .TrimStart("https://".ToCharArray());

                caddyStr = caddyStr.Replace("##reverse_proxy##", $"reverse_proxy {prefix}{domain} {{ \n        header_up Host {domain} \n    }}");
            }
            else
            {
                caddyStr = caddyStr.Replace("##reverse_proxy##", "");
            }

            return caddyStr;
        }
    }

}
