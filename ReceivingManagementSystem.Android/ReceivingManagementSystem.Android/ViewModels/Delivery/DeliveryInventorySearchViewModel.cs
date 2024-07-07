using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Delivery;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.Views;
using ReceivingManagementSystem.Android.Interfaces;
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

namespace ReceivingManagementSystem.Android.ViewModels.Delivery
{
    public class DeliveryInventorySearchViewModel : BaseViewModel
    {
        #region Properties

        private DeliveryInventorySearchParamViewModel _searchParams;
        public DeliveryInventorySearchParamViewModel SearchParams
        {
            get { return _searchParams; }
            set { this.SetProperty(ref this._searchParams, value); }
        }

        /// <summary>
        /// 内容
        /// </summary>
        private List<ComboBoxItemViewModel> _contentItems;

        /// <summary>
        /// 内容
        /// </summary>
        public List<ComboBoxItemViewModel> ContentItems
        {
            get { return _contentItems; }
            set { this.SetProperty(ref this._contentItems, value); }
        }

        /// <summary>
        /// 内容 selected
        /// </summary>
        private ComboBoxItemViewModel _contentSelected;

        /// <summary>
        /// 内容 selected
        /// </summary>
        public ComboBoxItemViewModel ContentSelected
        {
            get { return _contentSelected; }
            set
            {
                _searchParams.Contents = value != null ? value.Value.ToString() : null;
                this.SetProperty(ref this._contentSelected, value);
            }
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
                _searchParams.ShelfNumber = value != null ? value.Value.ToString() : null;
                this.SetProperty(ref this._shelfNumberSelected, value);
            }
        }

        /// <summary>
        /// Rfid
        /// </summary>
        private string _shelfRfid;

        /// <summary>
        /// Rfid
        /// </summary>
        public string ShelfRfid
        {
            get { return _shelfRfid; }

            set { this.SetProperty(ref this._shelfRfid, value); }
        }

        private bool _isPickerOpen;
        public bool IsPickerOpen
        {
            get { return _isPickerOpen; }
            set { this.SetProperty(ref this._isPickerOpen, value); }
        }
        #endregion

        #region Command

        public ICommand SearchCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand ReadShelfRFIDCommand { get; }
        public ICommand SelectShelfCommand { get; }
        public ICommand SetToDayCommand { get; }
        public ICommand OpenPickerCommand { get; }
        public ICommand OkSelectDateCommand { get; }

        #endregion

        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private enum RFIDTYPE {
            RFID,
            ShelfRFID
        }
        private RFIDTYPE _RFID = RFIDTYPE.RFID;
        private string _device;
        private List<CustodyItemViewModel> _items;
        private bool _transition = false;

        public DeliveryInventorySearchViewModel(ContentPage owner) : base(owner)
        {
            SearchCommand = new Command(DeliverySearch);
            ReadRFIDCommand = new Command(ReadRFID);
            ReadShelfRFIDCommand = new Command(ReadShelfRFID);
            SelectShelfCommand = new Command(GetShelfNumberByRfid);
            SetToDayCommand = new Command(SetToDay); 
            OpenPickerCommand = new Command(OpenPicker);
            OkSelectDateCommand = new Command(OkSelectDate);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            SearchParams = new DeliveryInventorySearchParamViewModel();

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");

            GetContents();
            GetShelfNumbers();
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

        private async void GetContents()
        {
            var items = await _pleasanterService.GetContents();

            ContentItems = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = s.contents,
                Value = s.contents
            }).ToList();
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
        /// Search 
        /// </summary>
        private async void DeliverySearch()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.CustodyDetail, PleasanterObjectTypeEnum.Custody))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0008);
                return;
            }

            await Search();

            RfidStop();
            _transition = true;

            if (_items.Count == 1)
            {
                DeliveryPage deliveryPage = new DeliveryPage(_items[0]);
                await this.Owner.Navigation.PushAsync(deliveryPage, true);
                return;
            }

            DeliveryInventorySearchResultPage deliveryInventorySearchResultPage = new DeliveryInventorySearchResultPage(_items);
            await this.Owner.Navigation.PushAsync(deliveryInventorySearchResultPage, true);

        }

        private async Task Search()
        {
            var items = await _pleasanterService.GetCustody(_searchParams.GetCustodyBodySearch(), _searchParams.GetCustodyDetailBodySearch());

            _items = items.Select(s => new CustodyItemViewModel(s)).ToList();
        }

        /// <summary>
        /// Read RFID
        /// </summary>
        private void ReadRFID()
        {
            SearchParams.Rfid = string.Empty;
            if ("SP1".Equals(_device))
            {
                _RFID = RFIDTYPE.RFID;
                _readRfidService.ReadRfid();
            }
        }

        private void ReadShelfRFID()
        {
            ShelfRfid = string.Empty;
            if ("SP1".Equals(_device))
            {
                _RFID = RFIDTYPE.ShelfRFID;
                _readRfidService.ReadRfid();
            }
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            switch(_RFID)
            {
            case RFIDTYPE.RFID:
                    SearchParams.Rfid = args.Rfid + "\r\n";
                    break;
            case RFIDTYPE.ShelfRFID:
                    ShelfRfid = args.Rfid + "\r\n";
                    break;
            default:
                break;
            }
        }

        private async void GetShelfNumberByRfid()
        {
            var shelfNumber = _shelfNumbers.FirstOrDefault(r => r.Id.ToString().Equals(ShelfRfid));

            if(shelfNumber == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0002, TextResourceKey.ShelfNumber);
            }
            else
            {
                ShelfNumberSelected = shelfNumber;
                SearchParams.ShelfNumber = shelfNumber.DisplayValue;
            }
        }

        /// <summary>
        /// Set 預り予定日 = ToDay
        /// </summary>
        private void SetToDay()
        {
            SearchParams.SelectDate = DateTime.Now;
            SearchParams.InputDate = SearchParams.SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }

        private void OpenPicker()
        {
            IsPickerOpen = true;
        }

        private void OkSelectDate()
        {
            SearchParams.InputDate = SearchParams.SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }
    }
}
