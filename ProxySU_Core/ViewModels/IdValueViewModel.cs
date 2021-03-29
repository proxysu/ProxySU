using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySU_Core.ViewModels
{
    public class IdValueViewModel
    {
        public IdValueViewModel(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; set; }

        public string Value { get; set; }
    }
}
