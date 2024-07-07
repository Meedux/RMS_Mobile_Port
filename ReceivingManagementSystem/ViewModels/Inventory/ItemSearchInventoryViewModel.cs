using CsvHelper;
using DensoScannerSDK.RFID;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Inventory;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.ViewModels.Delivery;
using ReceivingManagementSystem.Views;
using ReceivingManagementSystem.Wrapper;
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

namespace ReceivingManagementSystem.ViewModels.Inventory
{
    public class ItemSearchInventoryViewModel : BaseViewModel
    {
        #region Properties

        private ItemInventoryItemViewModel _item;
        public ItemInventoryItemViewModel Item
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

        public ICommand OkCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand SearchRFIDCommand { get; }
        public ICommand ReadRFIDHIDCommand { get; }

        #endregion

        CommonBase m_hCommonBase = CommonBase.GetInstance();
        Assembly m_pAssembly = null;
        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private RFIDScannerSettings _rfidSettings = null;
        private string _device;

        public ItemSearchInventoryViewModel(ItemInventoryItemViewModel item, ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            SearchRFIDCommand = new Command(SearchRFID);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            Item = item;

            RadioStrength = "0";

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");
            if (m_hCommonBase.IsCommScanner())
            {
                // RFID関連設定値を取得
                _rfidSettings = m_hCommonBase.GetCommScanner().GetRFIDScanner().GetSettings();
            }

            m_pAssembly = this.GetType().Assembly;
            BackgroundImageSource = ImageSource.FromResource("ReceivingManagementSystem.Resource.locate_tag_radar_background.png", m_pAssembly);
            PowerImageSource = ImageSource.FromResource("ReceivingManagementSystem.Resource.locate_tag_circle_under_75.png", m_pAssembly);
        }

        public void RfidInit()
        {
            _readRfidService.OnReadPowerLevel += OnReadPowerLevel;
            _readRfidService.OnInit();
            if (_rfidSettings != null)
            {
                // 二度読み防止解除 SP1設定値送信
                _rfidSettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.Free;
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(_rfidSettings);
            }
        }

        public void RfidStop()
        {
            _readRfidService.OnReadPowerLevel -= OnReadPowerLevel;
            _readRfidService.Stop();
            if (_rfidSettings != null)
            {
                // 二度読み防止 SP1設定値送信
                _rfidSettings.scan.doubleReading = RFIDScannerSettings.Scan.DoubleReading.PREVENT1;
                m_hCommonBase.GetCommScanner().GetRFIDScanner().SetSettings(_rfidSettings);
            }
        }

        /// <summary>
        /// Delivery
        /// </summary>
        private void Ok()
        {
            Close(true);
        }

        private void SearchRFID()
        {
            RadioStrength = "0.0";
            if ("SP1".Equals(_device))
            {
                PowerImageVisible = !PowerImageVisible;
                if (PowerImageVisible)
                {
                    _readRfidService.SearchRfid(_item.Rfid);
                }
                else
                {
                    _readRfidService.StopRead();
                }
            }
            else
            {
                PowerImageVisible = false;
                SearchRfid = string.Empty;
            }
        }

        private void OnReadPowerLevel(object sender, RfidResultEventArgs args)
        {
            RadioStrength = args.PowerLevel.ToString();

            if (args.PowerImage != null)
            {
                PowerImageSource = args.PowerImage;

                PowerImageVisible = true;
            }

            SearchRfid = args.Rfid;
        }
    }
}
