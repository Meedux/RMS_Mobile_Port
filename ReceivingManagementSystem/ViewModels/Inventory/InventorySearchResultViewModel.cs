using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.Services.Rfid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using static RMS_Pleasanter.CustodyDetail;
using ReceivingManagementSystem.Services.Pleasanter;
using static RMS_Pleasanter.Custody;
using ReceivingManagementSystem.Inventory;
using ReceivingManagementSystem.Wrapper;
using System.IO;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using Serilog;
using static RMS_Pleasanter.ShelfNumber;
using RMS_Pleasanter;

namespace ReceivingManagementSystem.ViewModels.Inventory
{
    public class InventorySearchResultViewModel : BaseViewModel
    {
        #region Properties
        public enum ScanState
        {
            None,
            Start,
            Finish
        }

        private ScanState _scanState;

        private ObservableCollection<InventoryCustodyItemViewModel> _custodieAll;
        private ObservableCollection<InventoryCustodyItemViewModel> _custodies;

        public ObservableCollection<InventoryCustodyItemViewModel> Custodies
        {
            get { return _custodies; }
            set { this.SetProperty(ref this._custodies, value); }
        }

        private InventoryCustodyItemViewModel _custodySelected;

        public InventoryCustodyItemViewModel CustodySelected
        {
            get { return _custodySelected; }
            set
            {
                this.SetProperty(ref this._custodySelected, value);
            }
        }


        private bool _isRefreshing;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { this.SetProperty(ref this._isRefreshing, value); }
        }

        private bool _isScan;

        public bool IsScan
        {
            get => _isScan;
            set
            {
                this.SetProperty(ref this._isScan, value);
            }
        }

        private bool _isEnableShelf;

        public bool IsEnableShelf
        {
            get => _isEnableShelf;
            set { this.SetProperty(ref this._isEnableShelf, value); }
        }

        private bool _isScanFinish;

        public bool IsScanFinish
        {
            get => _isScanFinish;
            set { this.SetProperty(ref this._isScanFinish, value); }
        }

        private bool _isVisible;

        public bool IsVisible
        {
            get => _isVisible;
            set { this.SetProperty(ref this._isVisible, value); }
        }

        private string _rfid;

        public string Rfid
        {
            get => _rfid;
            set { this.SetProperty(ref this._rfid, value); }
        }

        /// <summary>
        /// 棚番号
        /// </summary>
        private List<ComboBoxItemViewModel> _shelfNumbers;

        /// <summary>
        /// 棚番号
        /// </summary>
        public List<ComboBoxItemViewModel> ShelfNumbers
        {
            get { return _shelfNumbers; }
            set
            {
                this.SetProperty(ref this._shelfNumbers, value);
                IsScan = _custodySelected != null;
            }
        }

        /// <summary>
        /// 棚番号 selected
        /// </summary>
        private ComboBoxItemViewModel _shelfSelected;

        /// <summary>
        /// 棚番号 selected
        /// </summary>
        public ComboBoxItemViewModel ShelfSelected
        {
            get { return _shelfSelected; }
            set
            {
                this.SetProperty(ref this._shelfSelected, value);
                IsScan = _shelfSelected != null;
            }
        }


        private string _rfidText;

        public string RfidText
        {
            get => _rfidText;
            set { this.SetProperty(ref this._rfidText, value); }
        }

        #endregion

        #region Command
        public ICommand StartScanCommand { get; set; }
        public ICommand DoubleClickCommand { get; set; }
        public ICommand AppearingCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand ExportCsvCommand { get; set; }
        public ICommand ReadRFIDCommand { get; }
        #endregion

        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private IFileWrapper _fileWrapper;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private string _device;
        private bool _transition = false;

        public InventorySearchResultViewModel(List<InventoryCustodyItemViewModel> custodies, ContentPage owner) : base(owner)
        {
            _custodieAll = new ObservableCollection<InventoryCustodyItemViewModel>(custodies);
            Custodies = new ObservableCollection<InventoryCustodyItemViewModel>(custodies);

            StartScanCommand = new Command(StartScan);
            DoubleClickCommand = new Command(Update);
            UpdateCommand = new Command(Update);
            ExportCsvCommand = new Command(ExportCsv);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _fileWrapper = DependencyService.Get<IFileWrapper>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");

            GetShelfNumbers();

            _scanState = ScanState.None;
            IsEnableShelf = true;
            IsScan = false;
            SetRfidText();
        }

        private void SetRfidText()
        {
            RfidText = _scanState == ScanState.Start ? "　スキャン終了" : "スキャン開始";
        }

        public void RfidInit()
        {
            _readRfidService.OnReadRfid += OnReadRfid;
            _readRfidService.OnInit();
            _transition = false;
        }

        public void RfidStop()
        {
            if (!_transition)
            {
                _readRfidService.OnReadRfid -= OnReadRfid;
                _readRfidService.Stop();
            }
        }

        private async void GetShelfNumbers()
        {
            var items = await _pleasanterService.GetShelfNumbers();
            items.Insert(0, new ShelfNumberBody());

            ShelfNumbers = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = s.shelfNumber,
                Value = s.shelfNumber,
                Id = s.shelfRfid
            }).ToList();

            ShelfSelected = ShelfNumbers[0];
        }

        private void StartScan()
        {
            if (_shelfSelected == null)
            {
                return;
            }

            if (_scanState == ScanState.Start)
            {
                IsEnableShelf = true;
                IsScanFinish = true;
                _scanState = ScanState.Finish;
                _readRfidService.StopRead();
                GetCustodyByRfid();
            }
            else
            {
                IsEnableShelf = false;
                IsScanFinish = false;
                _scanState = ScanState.Start;

                _readRfidService.ReadMultiRfid();
            }

            IsVisible = !IsVisible;
            SetRfidText();
        }

        private async void GetCustodyByRfid()
        {
            CustodyDetailBody custodyDetailBodyParams = new CustodyDetailBody();
            InventoryCustodyItemViewModel inventoryItem;
            List<InventoryCustodyItemViewModel> listError = new List<InventoryCustodyItemViewModel>();
            CustodyDetailBody custodyDetailBody;

            string[] rfids = new string[0];
            if (Rfid != null)
            {
                rfids = Rfid.Split('\r');
            }

            foreach (var rfid in rfids)
            {
                if (rfid == string.Empty)
                    continue;

                custodyDetailBodyParams.rfid = rfid;

                var items = await _pleasanterService.GetCustody(new CustodyBody(), custodyDetailBodyParams);

                if (items.Count > 0)
                {
                    inventoryItem = new InventoryCustodyItemViewModel(items[0]);
                    inventoryItem.IsExist = true;

                    InventoryCustodyItemViewModel itemExist;

                    itemExist = Custodies.FirstOrDefault(r => r.Id == inventoryItem.Id);
                    if (itemExist == null)
                    {

                        inventoryItem.IsError = true;
                        listError.Add(inventoryItem);
                    }
                    else
                    {
                        itemExist.IsExist = true;
                        custodyDetailBody = itemExist.GetCustodyDetail();
                        custodyDetailBody.InventoryDate = DateTime.Now;
                        if(_shelfSelected.DisplayValue != String.Empty)
                        {
                            custodyDetailBody.shelfNumber = _shelfSelected.DisplayValue;
                        }

                        await _pleasanterService.UpdateCustodyDetail(custodyDetailBody);

                        Custodies.Remove(itemExist);
                        _custodieAll.Remove(itemExist);
                    }
                }
                else
                {
                    inventoryItem = new InventoryCustodyItemViewModel();
                    inventoryItem.IsError = true;
                    inventoryItem.Rfid = rfid;
                    listError.Add(inventoryItem);
                }
            }

            foreach (var item in listError)
            {
                Custodies.Add(item);
            }
        }

        /// <summary>
        /// Double click row
        /// </summary>
        private async void Update()
        {
            if (ScanState.Finish != _scanState || _custodySelected == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0014);

                return;
            }

            if (!_custodySelected.Id.HasValue)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0006);

                return;
            }

            if (_custodySelected.IsError)
            {
                RfidStop();
                _transition = true;

                InventoryUpdateErrorPage custodyPage = new InventoryUpdateErrorPage(_custodySelected, _shelfSelected.Value?.ToString() ?? "");
                custodyPage.OnCloseOk += UpdateErrorPage_Disappearing;
                await this.Owner.Navigation.PushAsync(custodyPage, true);
            }
            else if (_custodySelected.IsExist)
            {
                RfidStop();
                _transition = true;

                InventoryUpdateNotExistPage custodyPage = new InventoryUpdateNotExistPage(_custodySelected);
                custodyPage.OnCloseOk += UpdateNotExistPage_Disappearing;
                await this.Owner.Navigation.PushAsync(custodyPage, true);
            }

        }

        private void UpdateNotExistPage_Disappearing(object sender, EventArgs e)
        {
            InventoryUpdateNotExistPage custodyPage = (InventoryUpdateNotExistPage)sender;

            var viewModel = (InventoryUpdateNotExistViewModel)custodyPage.BindingContext;

            if (viewModel.IsOK)
            {
                Custodies.Remove(_custodySelected);
                CustodySelected = null;
            }
        }

        private void UpdateErrorPage_Disappearing(object sender, EventArgs e)
        {
            InventoryUpdateErrorPage custodyPage = (InventoryUpdateErrorPage)sender;

            var viewModel = (InventoryUpdateErrorViewModel)custodyPage.BindingContext;

            if (viewModel.IsOK)
            {
                Custodies.Remove(_custodySelected);
                CustodySelected = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            if (Rfid != string.Empty)
                Rfid += "\r\n";
            Rfid += args.Rfid;
        }

        private async void ExportCsv()
        {
            try
            {
                List<InventoryCsvModel> inventory = Custodies.Where(r => r.IsExist).Select(s => new InventoryCsvModel
                {
                    CompanyName = s.CompanyName,
                    Contents = s.Contents,
                    CustodyDate = s.CustodyDate.HasValue ? s.CustodyDate.Value.ToString("yyyy/MM/dd") : string.Empty,
                    CustomerName = s.CustomerName,
                    InventoryDate = s.InventoryDate.HasValue ? s.InventoryDate.Value.ToString("yyyy/MM/dd") : string.Empty,
                    ReturnDate = s.ReturnDate.HasValue ? s.ReturnDate.Value.ToString("yyyy/MM/dd") : string.Empty,
                    Rfid = s.Rfid,
                    ShelfNumber = s.ShelfNumber,
                    Status = s.Status,
                    TelephoneNumber = s.TelephoneNumber
                }).ToList();

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                    Encoding = Encoding.GetEncoding(932)
                };
                List<string> header = new List<string>() { "最終棚卸日", "状態", "RFID", "棚番号", "預り日", "返却日", "内容", "会社名", "氏名", "電話番号" };
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(memoryStream, Encoding.GetEncoding(932)))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteField(header);
                        csv.NextRecord();
                        csv.WriteRecords(inventory);
                    }

                    await _fileWrapper.ExportFile(memoryStream);
                }

                ShowMessage(MessageResourceKey.E0007);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }
    }
}
