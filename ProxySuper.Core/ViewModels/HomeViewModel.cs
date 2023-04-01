using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
            List<Record> records = new List<Record>();
            if (File.Exists("Data/Record.json"))
            {
                var json = File.ReadAllText("Data/Record.json");
                records = JsonConvert.DeserializeObject<List<Record>>(json);
            }

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

        public void SaveToJson()
        {
            var json = JsonConvert.SerializeObject(Records, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
            File.WriteAllText("Data/Record.json", json);
        }

        public void SortDone(string id)
        {
            var item = Records.Where(x => x.Id == id).FirstOrDefault();
            if (item == null) return;

            var index = Records.IndexOf(item);
            if (index >= Records.Count - 1) return;

            Records.Remove(item);
            Records.Insert(index + 1, item);

            RaisePropertyChanged("Records");
            SaveToJson();
        }

        public void SortUp(string id)
        {
            var item = Records.Where(x => x.Id == id).FirstOrDefault();
            if (item == null) return;

            var index = Records.IndexOf(item);
            if (index <= 0) return;

            Records.Remove(item);
            Records.Insert(index - 1, item);

            RaisePropertyChanged("Records");
            SaveToJson();
        }

        public MvxObservableCollection<Record> Records { get; set; }

        public IMvxCommand SortUpCommand => new MvxCommand<string>(SortUp);

        public IMvxCommand SortDoneCommand => new MvxCommand<string>(SortDone);

        public IMvxCommand AddV2rayCommand => new MvxAsyncCommand(AddV2rayRecord);

        public IMvxCommand AddXrayCommand => new MvxAsyncCommand(AddXrayRecord);

        public IMvxCommand AddTrojanGoCommand => new MvxAsyncCommand(AddTrojanGoRecord);

        public IMvxCommand AddNaiveProxyCommand => new MvxAsyncCommand(AddNaiveProxyRecord);

        public IMvxCommand AddMTProtoGoCommand => new MvxAsyncCommand(AddMTProtoGoRecord);

        public IMvxCommand AddBrookCommand => new MvxAsyncCommand(AddBrookRecord);

        public IMvxCommand AddHysteriaCommand => new MvxAsyncCommand(AddHysteriaRecord);

        public IMvxCommand RemoveCommand => new MvxAsyncCommand<string>(DeleteRecord);

        public IMvxCommand EditCommand => new MvxAsyncCommand<string>(EditRecord);

        public IMvxCommand ViewConfigCommand => new MvxAsyncCommand<string>(ViewConfig);

        public IMvxCommand InstallCommand => new MvxAsyncCommand<string>(GoToInstall);

        public async Task AddV2rayRecord()
        {
            Record record = new Record();
            record.Id = Utils.GetTickID();
            record.Host = new Host();
            record.V2raySettings = new V2raySettings();

            var result = await _navigationService.Navigate<V2rayEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);
            SaveToJson();
        }

        public async Task AddXrayRecord()
        {
            Record record = new Record();
            record.Id = Utils.GetTickID();
            record.Host = new Host();
            record.XraySettings = new XraySettings();

            var result = await _navigationService.Navigate<XrayEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);
            SaveToJson();
        }

        public async Task AddTrojanGoRecord()
        {
            Record record = new Record();
            record.Id = Utils.GetTickID();
            record.Host = new Host();
            record.TrojanGoSettings = new TrojanGoSettings();

            var result = await _navigationService.Navigate<TrojanGoEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);

            SaveToJson();
        }

        public async Task AddMTProtoGoRecord()
        {
            Record record = new Record();
            record.Id = Utils.GetTickID();
            record.Host = new Host();
            record.MTProtoGoSettings = new MTProtoGoSettings();

            var result = await _navigationService.Navigate<MTProtoGoEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);

            SaveToJson();
        }

        public async Task AddNaiveProxyRecord()
        {
            Record record = new Record();
            record.Id = Utils.GetTickID();
            record.Host = new Host();
            record.NaiveProxySettings = new NaiveProxySettings();

            var result = await _navigationService.Navigate<NaiveProxyEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);

            SaveToJson();
        }

        public async Task AddBrookRecord()
        {
            Record record = new Record();
            record.Id = Utils.GetTickID();
            record.Host = new Host();
            record.BrookSettings = new BrookSettings();

            var result = await _navigationService.Navigate<BrookEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);

            SaveToJson();
        }

        public async Task AddHysteriaRecord()
        {
            Record record = new Record();
            record.Id = Utils.GetTickID();
            record.Host = new Host();
            record.HysteriaSettings = new HysteriaSettings();

            var result = await _navigationService.Navigate<HysteriaEditorViewModel, Record, Record>(record);
            if (result == null) return;

            Records.Add(result);

            SaveToJson();
        }


        public async Task EditRecord(string id)
        {
            var record = Records.FirstOrDefault(x => x.Id == id);
            if (record == null) return;

            Record result = null;
            if (record.Type == ProjectType.V2ray)
            {
                result = await _navigationService.Navigate<V2rayEditorViewModel, Record, Record>(record);
                if (result == null) return;

                record.Host = result.Host;
                record.V2raySettings = result.V2raySettings;
            }
            if (record.Type == ProjectType.Xray)
            {
                result = await _navigationService.Navigate<XrayEditorViewModel, Record, Record>(record);
                if (result == null) return;

                record.Host = result.Host;
                record.XraySettings = result.XraySettings;
            }
            if (record.Type == ProjectType.TrojanGo)
            {
                result = await _navigationService.Navigate<TrojanGoEditorViewModel, Record, Record>(record);
                if (result == null) return;

                record.Host = result.Host;
                record.TrojanGoSettings = result.TrojanGoSettings;
            }
            if (record.Type == ProjectType.NaiveProxy)
            {
                result = await _navigationService.Navigate<NaiveProxyEditorViewModel, Record, Record>(record);
                if (result == null) return;

                record.Host = result.Host;
                record.NaiveProxySettings = result.NaiveProxySettings;
            }
            if (record.Type == ProjectType.Brook)
            {
                result = await _navigationService.Navigate<BrookEditorViewModel, Record, Record>(record);
                if (result == null) return;

                record.Host = result.Host;
                record.BrookSettings = result.BrookSettings;
            }
            if (record.Type == ProjectType.MTProtoGo)
            {
                result = await _navigationService.Navigate<MTProtoGoEditorViewModel, Record, Record>(record);
                if (result == null) return;

                record.Host = result.Host;
                record.MTProtoGoSettings = result.MTProtoGoSettings;
            }
            if (record.Type == ProjectType.Hysteria)
            {
                result = await _navigationService.Navigate<HysteriaEditorViewModel, Record, Record>(record);
                if (result == null) return;

                record.Host = result.Host;
                record.HysteriaSettings = result.HysteriaSettings;
            }

            SaveToJson();
        }

        public async Task DeleteRecord(string id)
        {
            var result = MessageBox.Show($"您确认删除主机吗？", "提示", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                var record = Records.FirstOrDefault(x => x.Id == id);
                if (record != null)
                {
                    Records.Remove(record);
                    SaveToJson();
                }
            }
            await Task.CompletedTask;
        }

        public async Task ViewConfig(string id)
        {
            var record = Records.FirstOrDefault(x => x.Id == id);
            if (record == null) return;

            if (record.Type == ProjectType.V2ray)
            {
                await _navigationService.Navigate<V2rayConfigViewModel, V2raySettings>(record.V2raySettings);
            }
            if (record.Type == ProjectType.Xray)
            {
                await _navigationService.Navigate<XrayConfigViewModel, XraySettings>(record.XraySettings);
            }
            if (record.Type == ProjectType.TrojanGo)
            {
                await _navigationService.Navigate<TrojanGoConfigViewModel, TrojanGoSettings>(record.TrojanGoSettings);
            }
            if (record.Type == ProjectType.NaiveProxy)
            {
                await _navigationService.Navigate<NaiveProxyConfigViewModel, NaiveProxySettings>(record.NaiveProxySettings);
            }
            if (record.Type == ProjectType.Brook)
            {
                await _navigationService.Navigate<BrookConfigViewModel, BrookSettings>(record.BrookSettings);
            }
            if (record.Type == ProjectType.MTProtoGo)
            {
                await _navigationService.Navigate<MTProtoGoConfigViewModel, MTProtoGoSettings>(record.MTProtoGoSettings);
            }
            if (record.Type == ProjectType.Hysteria)
            {
                await _navigationService.Navigate<HysteriaConfigViewModel, HysteriaSettings>(record.HysteriaSettings);
            }
        }

        public async Task GoToInstall(string id)
        {
            var record = Records.FirstOrDefault(x => x.Id == id);
            if (record == null) return;
            record.OnSave = SaveToJson;

            if (record.Type == ProjectType.V2ray)
            {
                await _navigationService.Navigate<V2rayInstallViewModel, Record>(record);
            }
            if (record.Type == ProjectType.Xray)
            {
                await _navigationService.Navigate<XrayInstallViewModel, Record>(record);
            }
            if (record.Type == ProjectType.TrojanGo)
            {
                await _navigationService.Navigate<TrojanGoInstallViewModel, Record>(record);
            }
            if (record.Type == ProjectType.NaiveProxy)
            {
                await _navigationService.Navigate<NaiveProxyInstallViewModel, Record>(record);
            }
            if (record.Type == ProjectType.Brook)
            {
                await _navigationService.Navigate<BrookInstallViewModel, Record>(record);
            }
            if (record.Type == ProjectType.MTProtoGo)
            {
                await _navigationService.Navigate<MTProtoGoInstallViewModel, Record>(record);
            }
            if (record.Type == ProjectType.Hysteria)
            {
                await _navigationService.Navigate<HysteriaInstallViewModel, Record>(record);
            }

            SaveToJson();
        }

    }
}
