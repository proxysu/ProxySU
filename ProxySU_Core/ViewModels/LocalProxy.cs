using System;
using System.Collections.Generic;
using System.Text;

namespace ProxySU_Core.ViewModels
{
    public class LocalProxy : BaseModel
    {
        private LocalProxyType type = LocalProxyType.None;

        public string Address { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 1080;

        public LocalProxyType Type
        {
            get => type; set
            {
                type = value;
                Notify("Type");
            }

        }

        public string UserName { get; set; }

        public string Password { get; set; }

    }


}
