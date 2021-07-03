﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Models.Projects
{
    public class BrookSettings : IProjectSettings
    {
        public string Domain { get; set; }

        public string Password { get; set; }

        public BrookType BrookType { get; set; }

        public int Port { get; set; } = 443;

        public List<int> FreePorts
        {
            get
            {
                return new List<int>()
                {
                    Port
                };
            }
        }

        public string Email => "server@brook.com";

        public ProjectType Type { get; set; } = ProjectType.Brook;

    }
}
