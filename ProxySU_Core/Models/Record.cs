using ProxySU_Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProxySU_Core.Models
{
    public class Record
    {
        public Record()
        {

        }

        public Record(Host host)
        {
            this.Host = host;
        }

        public Host Host { get; set; } = new Host();

        public XraySettings Settings { get; set; } = new XraySettings();
    }
}
