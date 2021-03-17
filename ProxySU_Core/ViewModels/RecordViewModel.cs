using ProxySU_Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySU_Core.ViewModels
{
    public class RecordViewModel : BaseViewModel
    {
        public Record record;
        private bool _isChecked;

        public RecordViewModel(Record record)
        {
            this.record = record;
            this._isChecked = false;
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                Notify("IsChecked");
            }
        }

        public Host Host
        {
            get => record.Host;
            set
            {
                record.Host = value;
                Notify("Host");
            }
        }

        public XraySettings Settings
        {
            get => record.Settings;
            set
            {
                record.Settings = value;
                Notify("Settings");
            }
        }

        public void Notify()
        {
            Notify("Host");
            Notify("Settings");
        }
    }
}
