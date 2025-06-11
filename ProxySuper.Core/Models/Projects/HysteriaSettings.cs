using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator;

namespace ProxySuper.Core.Models.Projects
{
    public class HysteriaSettings : IProjectSettings
    {
        public HysteriaSettings()
        {
            Password = Guid.NewGuid().ToString();

            Email = GetEmailRandom();

            ObfsPassword = Guid.NewGuid().ToString();
        }
        public string Domain { get; set; } = "";

        public int Port { get; set; } = 443;

        public string Password { get; set; } = "";

        public bool AutoACME { get; set; } = true;

        public string Email { get; set; } = "";

        public bool EnableObfs { get; set; } = false;
        public string ObfsPassword { get; set; } = "";

        public int UpMbps { get; set; } = 0;

        public int DownMbps { get; set; } = 0;

        public bool BBRenable { get; set; } = false;

        public string MaskDomain { get; set; } = "";

        public bool EnableUpDisguisedWeb { get; set; } = false;

        public string HysteriaShareLink
        {
            get
            {
                return ShareLink.BuildHysteria(this);
            }
        }
        public List<int> FreePorts
        {
            get
            {
                return new List<int> { Port, 80 };
            }
        }

        public string GetEmailRandom()
        {
            if (!string.IsNullOrEmpty(Domain))
            {
                var arr = Domain.Split('.');
                if (arr.Length == 3)
                {
                    return $"{arr[0]}@{arr[1]}.{arr[2]}";
                }
            }

            return $"{Guid.NewGuid().ToString().Substring(2, 6)}@gmail.com";
        }

        public string ClientHysteria2Config { get; set; }
    }
}
