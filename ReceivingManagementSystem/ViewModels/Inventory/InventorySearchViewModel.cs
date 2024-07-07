using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Inventory;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Return;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.Views;
using ReceivingManagementSystem.Warehousing;
using ReceivingManagementSystem.Wrapper;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using static PleasanterOperation.OperationData;
using static RMS_Pleasanter.Contents;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.ViewModels.Inventory
{
    public class InventorySearchViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// RFID
        /// </summary>
        private string _rfid;

        /// <summary>
        /// RFID
        /// </summary>
        public string Rfid
        {
            get { return _rfid; }

            set { this.SetProperty(ref this._rfid, value); }
        }

        /// <summary>
        /// 棚卸開始日
        /// </summary>
        private DateTime? _stocktakingStartDate;

        /// <summary>
        /// 棚卸開始日
        /// </summary>
        public DateTime? StocktakingStartDate
        {
            get { return _stocktakingStartDate; }

            set { this.SetProperty(ref this._stocktakingStartDate, value); }
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
            set { this.SetProperty(ref this._shelfNumbers, value); }
        }

        /// <summary>
        /// 棚番号 selected
        /// </summary>
        private ComboBoxItemViewModel _shelfNumberSelected;

        /// <summary>
        /// 棚番号 selected
        /// </summary>
        public ComboBoxItemViewModel ShelfNumberSelected
        {
            get { return _shelfNumberSelected; }
            set
            {
                this.SetProperty(ref this._shelfNumberSelected, value);
            }
        }

        private bool _isPickerOpen;
        public bool IsPickerOpen
        {
            get { return _isPickerOpen; }
            set { this.SetProperty(ref this._isPickerOpen, value); }
        }

        /// <summary>
        /// Input Date
        /// </summary>
        private string _inputDate;

        /// <summary>
        /// Input Date
        /// </summary>
        public string InputDate
        {
            get { return _inputDate; }
            set { this.SetProperty(ref this._inputDate, value); }
        }

        /// <summary>
        /// Select date
        /// </summary>
        private DateTime _selectDate;

        /// <summary>
        /// Select date
        /// </summary>
        public DateTime SelectDate
        {
            get { return _selectDate; }
            set
            {
                this.SetProperty(ref this._selectDate, value);
            }
        }

        private string _device;
        private List<InventoryCustodyItemViewModel> _custodys;
        #endregion

        #region Command

        public ICommand StartCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand SelectShelfCommand { get; }
        public ICommand SetToDayCommand { get; }
        public ICommand OpenPickerCommand { get; }
        public ICommand OkSelectDateCommand { get; }

        #endregion

        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private bool _transition = false;

        public InventorySearchViewModel(ContentPage owner) : base(owner)
        {
            StartCommand = new Command(Start);
            ReadRFIDCommand = new Command(ReadRFID);
            SelectShelfCommand = new Command(GetShelfNumberByRfid);
            SetToDayCommand = new Command(SetToDay);
            OpenPickerCommand = new Command(OpenPicker);
            OkSelectDateCommand = new Command(OkSelectDate);


            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");

            this.Owner.Appearing += Owner_Appearing;
            SetToDay();
            GetShelfNumbers();
        }

        private void Owner_Appearing(object sender, EventArgs e)
        {
            Rfid = null;
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

        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.CustodyDetail, PleasanterObjectTypeEnum.Custody))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0008);
                return;
            }

            CustodyDetailBody custodyDetailBodyParams = new CustodyDetailBody()
            {
                status = CustodyStatusEnum.Storage.Value
            };

            if (ShelfNumberSelected != null)
            {
                custodyDetailBodyParams.shelfNumber = ShelfNumberSelected.Value.ToString();
            }

            var items = await _pleasanterService.GetCustody(new CustodyBody(), custodyDetailBodyParams);

            if (items == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.Cancel,
                    MessageResourceKey.E0002, TextResourceKey.Custody);

                return;
            }

            DateTime? selectDate = DateHelper.GetDateByInputDate(_inputDate);
            if (selectDate.HasValue)
            {
                items = items.Where(r => !r.CustodyDetail.InventoryDate.HasValue || r.CustodyDetail.InventoryDate.Value.Date < selectDate.Value.Date).ToList();
            }
            foreach(var item in items)
            {
                if(item.CustodyDetail.InventoryDate.ToString() == "1899/12/30 0:00:00")
                    item.CustodyDetail.InventoryDate = null;
                if (item.CustodyDetail.returnDate.ToString() == "1899/12/30 0:00:00")
                    item.CustodyDetail.returnDate = null;
            }

            RfidStop();
            _transition = true;

            _custodys = items.Select(s => new InventoryCustodyItemViewModel(s)).ToList();

            InventorySearchResultPage inventorySearchResultPage = new InventorySearchResultPage(_custodys);
            await this.Owner.Navigation.PushAsync(inventorySearchResultPage, true);
        }

        private async void GetShelfNumbers()
        {
            var items = await _pleasanterService.GetShelfNumbers();

            ShelfNumbers = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = s.shelfNumber,
                Value = s.shelfNumber,
                Id = s.shelfRfid
            }).ToList();
        }

        /// <summary>
        /// Read RFID
        /// </summary>
        private void ReadRFID()
        {
            Rfid = string.Empty;
            if ("SP1".Equals(_device))
            {
                _readRfidService.ReadRfid();
            }
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            Rfid = args.Rfid + "\r\n";
        }

        private async void GetShelfNumberByRfid()
        {
            var shelfNumber = _shelfNumbers.FirstOrDefault(r => r.Id.ToString().Equals(_rfid));

            if (shelfNumber == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0002, TextResourceKey.ShelfNumber);
            }
            else
            {
                ShelfNumberSelected = shelfNumber;
            }
        }

        /// <summary>
        /// Set ToDay
        /// </summary>
        private void SetToDay()
        {
            SelectDate = DateTime.Now;
            InputDate = SelectDate.ToString("yyyy/MM/dd");
        }

        private void OpenPicker()
        {
            IsPickerOpen = true;
        }

        private void OkSelectDate()
        {
            InputDate = SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }
    }
}
