using CsvHelper;
using PleasanterOperation;
using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
using ReceivingManagementSystem.ViewModels.PalletRegistration;
using ReceivingManagementSystem.Views;
using ReceivingManagementSystem.Wrapper;
using RMS_Pleasanter;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.ViewModels.Shipping
{
    public class ShippingViewModel : BaseViewModel
    {
        public event EventHandler<EventArgs> OnRfidChange;

        #region Properties

        private ShippingInfoViewModel _shippingInfo;
        public ShippingInfoViewModel ShippingInfo
        {
            get { return _shippingInfo; }
            set { this.SetProperty(ref this._shippingInfo, value); }
        }

        private string _device;
        #endregion

        #region Command

        public ICommand OkCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand ReadRFIDHIDCommand { get; }
        #endregion

        private IPleasanterService _pleasanterService;
        private IReadRfidService _readRfidService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public ShippingViewModel(ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            ReadRFIDCommand = new Command(ReadRFID);
            ReadRFIDHIDCommand = new Command(OnReadRfidHID);

            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            ShippingInfo = new ShippingInfoViewModel();

            GetItems();
            GetServices();

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");
        }

        public void RfidInit()
        {
            _readRfidService.OnReadRfid += OnReadRfid;
            _readRfidService.OnInit();
        }

        public void RfidStop()
        {
            _readRfidService.OnReadRfid -= OnReadRfid;
            _readRfidService.Stop();
        }

        /// <summary>
        /// ShowConfirm Order
        /// </summary>
        private async void Ok()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.Item, PleasanterObjectTypeEnum.ItemInventory,
                PleasanterObjectTypeEnum.ItemInventoryCount, PleasanterObjectTypeEnum.PalletMaster))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0008);
                return;
            }

            bool isValidate = await Validate();

            if (!isValidate)
            {
                return;
            }

            bool result = await Register();

            if (result)
            {
                Close();
            }
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            if (_shippingInfo.ItemSelected == null)
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Item));
            }

            if (string.IsNullOrEmpty(_shippingInfo.Rfid))
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Rfid));
            }

            if (!_shippingInfo.ShippedNumber.HasValue || _shippingInfo.ShippedNumber.Value <= 0)
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.ShippedNumber));
            }

            if (errors.Count > 0)
            {
                string action = await Owner.DisplayActionSheet(ResourceProvider.GetResourceByName(TextResourceKey.NotificationTitle),
                   ResourceProvider.GetResourceByName(TextResourceKey.OK), null, errors.ToArray());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Register pallet
        /// </summary>
        private async Task<bool> Register()
        {
            var itemInventory = await _pleasanterService.GetItemInventoryByItemIdAndRfid(_shippingInfo.ItemSelected.Id.ToString(), _shippingInfo.Rfid);

            if (itemInventory.Count == 0)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0002, TextResourceKey.ItemInventory);
                return false;
            }

            decimal? custodyId = await _pleasanterService.CreateReceivingAndShipping(_shippingInfo.GetReceivingAndShipping());

            if (!custodyId.HasValue)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.Receiving);

                return false;
            }

            itemInventory[0].inventory -= _shippingInfo.ShippedNumber;
            bool result = await _pleasanterService.UpdateItemInventory(itemInventory[0]);

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.ItemInventory);

                return false;
            }

            return true;
        }

        private async void GetItems()
        {
            var items = await _pleasanterService.GetItems();

            ShippingInfo.Items = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = $"{s.itemNumber} / {s.itemName} / {s.itemType}",
                Value = s.itemId,
                Id = s.id.Value
            }).ToList();
        }

        private async void GetServices()
        {
            var items = await _pleasanterService.GetServices();

            ShippingInfo.Services = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = s.serviceName,
                Value = s.serviceName,
                Id = s.id.Value
            }).ToList();
        }

        /// <summary>
        /// Read RFID
        /// </summary>
        private void ReadRFID()
        {
            if ("SP1".Equals(_device))
            {
                _readRfidService.ReadRfid();
            }
            else
            {
                ShippingInfo.Rfid = string.Empty;
            }
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            ShippingInfo.Rfid = args.Rfid;
            SetItemByRFID();
            OnRfidChange.Raise(this, new EventArgs());
        }

        private void OnReadRfidHID()
        {
            SetItemByRFID();
        }

        private async void SetItemByRFID()
        {
            var itemInventory = await _pleasanterService.GetItemInventoryByRfid(_shippingInfo.Rfid);

            if (itemInventory.Count == 0)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0002, TextResourceKey.ItemInventory);
                return;
            }

            _shippingInfo.ItemSelected = _shippingInfo.Items.FirstOrDefault(r => r.Id.ToString().Equals(itemInventory[0].itemId));
            await _shippingInfo.GetPallet();
            _shippingInfo.PalletSelected = _shippingInfo.Pallets.FirstOrDefault(r => r.Id.ToString().Equals(itemInventory[0].palletNumber));
        }
    }
}
