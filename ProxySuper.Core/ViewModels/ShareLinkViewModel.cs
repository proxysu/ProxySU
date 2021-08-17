using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using System.Collections.Generic;
using System.Text;

namespace ProxySuper.Core.ViewModels
{
    public class ShareLinkViewModel : MvxViewModel<List<Record>>
    {
        public List<Record> Records { get; set; }

        public override void Prepare(List<Record> parameter)
        {
            Records = parameter;
        }

        public string ShareLinks
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                Records.ForEach(record =>
                {
                    var link = record.GetShareLink();
                    sb.AppendLine(link);
                });
                return sb.ToString();
            }
        }
    }
}
