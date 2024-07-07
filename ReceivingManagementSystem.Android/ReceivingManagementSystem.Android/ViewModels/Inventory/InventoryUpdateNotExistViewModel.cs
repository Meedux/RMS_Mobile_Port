using CsvHelper;
using DensoScannerSDK.RFID;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Inventory;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Delivery;
using ReceivingManagementSystem.Android.Views;
using ReceivingManagementSystem.Android.Interfaces;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace ReceivingManagementSystem.Android.ViewModels.Inventory
{
    public class InventoryUpdateNotExistViewModel : BaseViewModel
    {
        #region Properties

        private InventoryCustodyItemViewModel _item;
        public InventoryCustodyItemViewModel Item
        {
            get { return _item; }
            set { this.SetProperty(ref this._item, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _radioStrength;

        /// <summary>
        /// 
        /// </summary>
        public string RadioStrength
        {
            get { return _radioStrength; }

            set { this.SetProperty(ref this._radioStrength, value); }
        }

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

        /// RFID
        /// </summary>
        private string _searchRfid;

        /// <summary>
        /// RFID
        /// </summary>
        public string SearchRfid
        {
            get { return _searchRfid; }

            set { this.SetProperty(ref this._searchRfid, value); }
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

        /// <summary>
        /// 
        /// </summary>
        private ImageSource _backgroundImageSource;

        /// <summary>
        /// 
        /// </summary>
        public ImageSource BackgroundImageSource
        {
            get { return _backgroundImageSource; }

            set { this.SetProperty(ref this._backgroundImageSource, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        private ImageSource _powerImageSource;

        /// <summary>
        /// 
        /// </summary>
        public ImageSource PowerImageSource
        {
            get { return _powerImageSource; }

            set { this.SetProperty(ref this._powerImageSource, value); }
        }


        /// <summary>
        /// 
        /// </summary>
        private bool _powerImageVisible;

        /// <summary>
        /// 
        /// </summary>
        public bool PowerImageVisible
        {
            get { return _powerImageVisible; }

            set { this.SetProperty(ref this._powerImageVisible, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool _isVisibleSearch;

        /// <summary>
        /// 
        /// </summary>
        public bool IsVisibleSearch
        {
            get { return _isVisibleSearch; }

            set { this.SetProperty(ref this._isVisibleSearch, value); }
        }

        public int LengthWithoutSearch
        {
            get
            {
                if ("SP1".Equals(_device))
                {
                    return 1;
                }
                return 2;
            }
        }

        public bool IsVisibleTxtRfid
        {
            get
            {
                if ("SP1".Equals(_device))
                {
                    return false;
                }
                return true;
            }
        }

        public bool IsRfidSearch
        {
            get
            {
                if ("SP1".Equals(_device))
                {
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Command

        public ICommand UpdateCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand SearchRFIDCommand { get; }
        public ICommand SelectShelfCommand { get; }
        public new ICommand CloseCommand { get; }

        #endregion

        CommonBase m_hCommonBase = CommonBase.GetInstance();
        private Assembly m_pAssembly = null;
        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private RFIDScannerSettings _rfidSettings = null;
        private string _device;

        public InventoryUpdateNotExistViewModel(InventoryCustodyItemViewModel item, ContentPage owner) : base(owner)
        {
            UpdateCommand = new Command(Update);
            ReadRFIDCommand = new Command(ReadRFID);
            SearchRFIDCommand = new Command(SearchRFID);
            SelectShelfCommand = new Command(GetShelfNumberByRfid);
            CloseCommand = new Command(Close);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");
            if (m_hCommonBase.IsCommScanner())
            {
                // RFID関連設定値を取得
                _rfidSettings = m_hCommonBase.GetCommScanner().GetRFIDScanner().GetSettings();
            }

            Item = item;

            RadioStrength = "0.0";

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");
            GetShelfNumbers();

            m_pAssembly = this.GetType().Assembly;
            BackgroundImageSource = ImageSource.FromResource("ReceivingManagementSystem.Resource.locate_tag_radar_background.png", m_pAssembly);
            PowerImageSource = ImageSource.FromResource("ReceivingManagementSystem.Resource.locate_tag_circle_under_75.png", m_pAssembly);
        }

        public void RfidInit()
        {
            _readRfidService.OnReadRfid += OnReadRfid;
            _readRfidService.OnReadPowerLevel += OnReadPowerLevel;
            _readRfidService.OnInit();
        }

        public void RfidStop()
        {
            if (_rfidSettings != null)
            {
                // 二度読み防止 SP1設定値送信
                _rfidSettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.PREVENT1;
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(_rfidSettings);
            }

            _readRfidService.OnReadRfid -= OnReadRfid;
            _readRfidService.OnReadPowerLevel -= OnReadPowerLevel;
            _readRfidService.Stop();
        }

        /// <summary>
        /// Delivery
        /// </summary>
        private async void Update()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.CustodyDetail))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0008);
                return;
            }

            bool isValidate = await Validate();

            if (!isValidate)
            {
                return;
            }

            if (_shelfNumberSelected == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0013, TextResourceKey.ShelfNumber);
                InventoryUpdateNormalPage custodyPage = new InventoryUpdateNormalPage(_item);
                custodyPage.OnCloseOk += UpdateNormalPage_Disappearing;
                await this.Owner.Navigation.PushAsync(custodyPage, true);

                return;
            }

            var custodyDetailBody = _item.GetCustodyDetail();
            custodyDetailBody.InventoryDate = DateTime.Now;
            custodyDetailBody.shelfNumber = _shelfNumberSelected.Value.ToString();

            bool result = await _pleasanterService.UpdateCustodyDetail(custodyDetailBody);

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.Inventory);
            }
            else
            {
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.Inventory);
                Close(true);
            }
        }

        private void UpdateNormalPage_Disappearing(object sender, EventArgs e)
        {
            InventoryUpdateNormalPage custodyPage = (InventoryUpdateNormalPage)sender;

            var viewModel = (InventoryUpdateNormalViewModel)custodyPage.BindingContext;

            if (viewModel.IsOK)
            {
                IsClose = true;
            }
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            //必須なし

            if (errors.Count > 0)
            {
                await ShowActionSheet(TextResourceKey.NotificationTitle, TextResourceKey.OK, errors);
                //return false;
            }

            return true;
        }

        /// <summary>
        /// Read RFID
        /// </summary>
        private void ReadRFID()
        {
            if ("SP1".Equals(_device))
            {
                _readRfidService.Stop();
                _readRfidService.OnInit();
                if (_rfidSettings != null)
                {
                    // 二度読み防止 SP1設定値送信
                    _rfidSettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.PREVENT1;
                    m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(_rfidSettings);
                }
                IsVisibleSearch = false;
                _readRfidService.ReadRfid();
            }
            else
            {
                Rfid = string.Empty;
            }
        }

        private void SearchRFID()
        {
            RadioStrength = "0.0";
            if ("SP1".Equals(_device))
            {
                PowerImageVisible = !PowerImageVisible;
                if (PowerImageVisible)
                {
                    _readRfidService.Stop();
                    _readRfidService.OnInit();
                    if (_rfidSettings != null)
                    {
                        // 二度読み防止解除 SP1設定値送信
                        _rfidSettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.Free;
                        m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(_rfidSettings);
                    }
                    _readRfidService.SearchRfid(_item.Rfid);
                }
                else
                {
                    _readRfidService.Stop();
                    if (_rfidSettings != null)
                    {
                        // 二度読み防止 SP1設定値送信
                        _rfidSettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.PREVENT1;
                        m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(_rfidSettings);
                    }
                }
            }
            else
            {
                PowerImageVisible = false;
                SearchRfid = string.Empty;
            }
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            Rfid = args.Rfid + "\r\n";
        }

        private void OnReadPowerLevel(object sender, RfidResultEventArgs args)
        {
            RadioStrength = args.PowerLevel.ToString();

            if (args.PowerImage != null)
            {
                PowerImageSource = args.PowerImage;

                PowerImageVisible = true;
            }
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

        private void GetShelfNumberByRfid()
        {
            var shelfNumber = _shelfNumbers.FirstOrDefault(r => r.Id.ToString().Equals(_rfid));

            if (shelfNumber != null)
            {
                ShelfNumberSelected = shelfNumber;
            }
        }

        private new void Close()
        {
            ShowMessage(MessageResourceKey.E0012, TextResourceKey.Inventory);
            Close(false);
        }
    }
}
