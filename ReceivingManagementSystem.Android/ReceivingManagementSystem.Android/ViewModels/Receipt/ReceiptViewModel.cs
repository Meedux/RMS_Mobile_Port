using CsvHelper;
using PleasanterOperation;
using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.ViewModels.PalletRegistration;
using ReceivingManagementSystem.Android.Views;
using ReceivingManagementSystem.Android.Interfaces;
using RMS_Pleasanter;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;
using static RMS_Pleasanter.ItemInventory;

namespace ReceivingManagementSystem.Android.ViewModels.Receipt
{
    public class ReceiptViewModel : BaseViewModel
    {
        public event EventHandler<EventArgs> OnRfidChange;

        #region Properties

        private ReceiptInfoViewModel _receiptInfo;
        public ReceiptInfoViewModel ReceiptInfo
        {
            get { return _receiptInfo; }
            set { this.SetProperty(ref this._receiptInfo, value); }
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

        public ReceiptViewModel(ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            ReadRFIDCommand = new Command(ReadRFID);
            ReadRFIDHIDCommand = new Command(OnReadRfidHID);

            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            ReceiptInfo = new ReceiptInfoViewModel();

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

            if (_receiptInfo.ItemSelected == null)
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Item));
            }

            if (string.IsNullOrEmpty(_receiptInfo.Rfid))
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Rfid));
            }

            if (!_receiptInfo.ReceivedNumber.HasValue || _receiptInfo.ReceivedNumber.Value <= 0)
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.ReceivedNumber));
            }

            if (errors.Count > 0)
            {
                string action = await Owner.DisplayActionSheet(ResourceProvider.GetResourceByName(TextResourceKey.NotificationTitle),
                   ResourceProvider.GetResourceByName(TextResourceKey.Cancel), null, errors.ToArray());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Register pallet
        /// </summary>
        private async Task<bool> Register()
        {
            decimal? receivingAndShippingId = await _pleasanterService.CreateReceivingAndShipping(_receiptInfo.GetReceivingAndShipping());

            if (!receivingAndShippingId.HasValue)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.Receiving);

                return false;
            }

            var itemInventory = await _pleasanterService.GetItemInventoryByItemIdAndRfid(_receiptInfo.ItemSelected.Id.ToString(), _receiptInfo.Rfid);

            if (itemInventory.Count > 0)
            {
                itemInventory[0].inventory += _receiptInfo.ReceivedNumber;
                bool result = await _pleasanterService.UpdateItemInventory(itemInventory[0]);

                if (!result)
                {
                    await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.ItemInventory);

                    return false;
                }
            }
            else
            {
                ItemInventoryBody itemInventoryBody = new ItemInventoryBody()
                {
                    inventory = _receiptInfo.ReceivedNumber,
                    itemId = _receiptInfo.ItemSelected.Id.ToString(),
                    palletNumber = _receiptInfo.PalletSelected != null && _receiptInfo.PalletSelected.Id.ToString() != "-1" ? _receiptInfo.PalletSelected.Id.ToString() : string.Empty,
                    RFID = _receiptInfo.Rfid
                };

                decimal? itemInventoryId = await _pleasanterService.CreateItemInventory(itemInventoryBody);

                if (!itemInventoryId.HasValue)
                {
                    await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.ItemInventory);

                    return false;
                }
            }

            return true;
        }

        private async void GetItems()
        {
            var items = await _pleasanterService.GetItems();

            ReceiptInfo.Items = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = $"{s.itemNumber} / {s.itemName} / {s.itemType}",
                Value = s.itemId,
                Id = s.id.Value
            }).ToList();
        }

        private async void GetServices()
        {
            var services = await _pleasanterService.GetServices();

            ReceiptInfo.Services = services.Select(s => new ComboBoxItemViewModel()
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
                ReceiptInfo.Rfid = string.Empty;
            }
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            ReceiptInfo.Rfid = args.Rfid;
            OnRfidChange.Raise(this, new EventArgs());
        }

        private void OnReadRfidHID()
        {
        }
    }
}
