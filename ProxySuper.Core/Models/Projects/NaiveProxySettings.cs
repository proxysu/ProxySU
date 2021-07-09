using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProxySuper.Core.Models.Projects
{
    public class NaiveProxySettings : IProjectSettings
    {
        public NaiveProxySettings()
        {
            Port = 443;
        }

        public List<int> FreePorts => new List<int>();

        public ProjectType Type { get; set; } = ProjectType.NaiveProxy;

        public int Port { get; set; }

        public string Domain { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string MaskDomain { get; set; }

        [JsonIgnore]
        public string Email
        {
            get
            {
                if (!string.IsNullOrEmpty(Domain))
                {
                    var arr = Domain.Split('.');
                    if (arr.Length == 3)
                    {
                        return $"{arr[0]}@{arr[1]}.{arr[2]}";
                    }
                }


                return $"{UserName + Port.ToString()}@gmail.com";
            }
        }
    }
}
