using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public void ReadRecords()
        {
            var json = File.ReadAllText("Data/Record.json");
            var records = JsonConvert.DeserializeObject<List<Record>>(json);
            this.Records = new MvxObservableCollection<Record>();

            records.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    item.Id = Guid.NewGuid().ToString();
                }
                this.Records.Add(item);
            });
        }

        public void SaveRecords()
        {
            var json = JsonConvert.SerializeObject(Records);
            File.WriteAllText("Data/Record.json", json);
        }

        public MvxObservableCollection<Record> Records { get; set; }

        public IMvxCommand AddXrayCommand => new MvxAsyncCommand(AddXrayRecord);

        public IMvxCommand AddTrojanGoCommand => new MvxAsyncCommand(AddTrojanGoRecord);

        public async Task AddXrayRecord()
        {
            Record record = new Record();
            record.Id = Guid.NewGuid().ToString();
            record.Host = new Host();
            record.XraySettings = new XraySettings();

            var result = await _navigationService.Navigate<XrayEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);
        }

        public async Task AddTrojanGoRecord()
        {
            Record record = new Record();
            record.Id = Guid.NewGuid().ToString();
            record.Host = new Host();
            record.TrojanGoSettings = new TrojanGoSettings();

            var result = await _navigationService.Navigate<TrojanGoEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);
        }
    }
}
