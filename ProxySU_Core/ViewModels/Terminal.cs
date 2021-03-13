using ProxySU_Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ProxySU_Core.ViewModels
{
    public class Terminal : BaseViewModel
    {
        private bool hasConnected;

        public Terminal(Host host)
        {
            Host = host;
            HasConnected = false;
        }

        public bool HasConnected
        {
            get
            {
                return hasConnected;
            }
            set
            {
                hasConnected = value;
                Notify("HasConnected");
            }
        }

        public Host Host { get; set; }

        public string CommandText { get; set; }

        public string OutputText { get; set; }
    }
}
