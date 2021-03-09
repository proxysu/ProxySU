using ProxySU_Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ProxySU_Core.ViewModels
{
    public class Terminal : BaseViewModel
    {
        private string outputText;
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

        public string OutputText
        {
            get => outputText;
        }

        public void ClearOutput()
        {
            outputText = "";
            Notify("OutputText");
        }

        public void AddOutput(string text)
        {
            outputText += text;

            if (!text.EndsWith("\n"))
            {
                outputText += "\n";
            }
            Notify("OutputText");
        }
    }
}
