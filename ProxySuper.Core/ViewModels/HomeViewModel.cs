using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using ProxySuper.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class HomeViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService _navigationService;

        public HomeViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
            ReadRecords();
        }

        private void ReadRecords()
        {
            var json = File.ReadAllText("Data/Record.json");
            var records = JsonConvert.DeserializeObject<List<Record>>(json);
            this.Records = new MvxObservableCollection<Record>();
            records.ForEach(item =>
            {
                this.Records.Add(item);
            });


        }

        public MvxObservableCollection<Record> Records { get; set; }
    }
}
