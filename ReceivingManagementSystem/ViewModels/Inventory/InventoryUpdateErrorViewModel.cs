using CsvHelper;
using DensoScannerSDK.RFID;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.ViewModels.Delivery;
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

namespace ReceivingManagementSystem.ViewModels.Inventory
{
    public class InventoryUpdateErrorViewModel : BaseViewModel
    {
        #region Properties

        private InventoryCustodyItemViewModel _item;
        public InventoryCustodyItemViewModel Item
        {
            get { return _item; }
            set { this.SetProperty(ref this._item, value); }
        }

        private string _shelfNumber;
        public string ShelfNumber
        {
            get { return _shelfNumber; }
            set { this.SetProperty(ref this._shelfNumber, value); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { this.SetProperty(ref this._status, value); }
        }

        private bool _isVisibleShelfNumber;
        public bool IsVisibleShelfNumber
        {
            get { return _isVisibleShelfNumber; }
            set { this.SetProperty(ref this._isVisibleShelfNumber, value); }
        }

        private bool _isVisibleShelfNumberNormal;
        public bool IsVisibleShelfNumberNormal
        {
            get { return _isVisibleShelfNumberNormal; }
            set { this.SetProperty(ref this._isVisibleShelfNumberNormal, value); }
        }

        private bool _isVisibleStatus;
        public bool IsVisibleStatus
        {
            get { return _isVisibleStatus; }
            set { this.SetProperty(ref this._isVisibleStatus, value); }
        }

        private bool _isVisibleStatusNormal;
        public bool IsVisibleStatusNormal
        {
            get { return _isVisibleStatusNormal; }
            set { this.SetProperty(ref this._isVisibleStatusNormal, value); }
        }

        #endregion

        #region Command

        public ICommand RenewCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand SearchRFIDCommand { get; }
        public ICommand ReadRFIDHIDCommand { get; }

        #endregion

        private IPleasanterService _pleasanterService;

        public InventoryUpdateErrorViewModel(InventoryCustodyItemViewModel item, string shelfNumber, ContentPage owner) : base(owner)
        {
            RenewCommand = new Command(Renew);
            _pleasanterService = DependencyService.Get<IPleasanterService>();

            Item = item;

            if (shelfNumber == string.Empty)
            {
                ShelfNumber = Item.ShelfNumber;
            }
            else
            {
                ShelfNumber = shelfNumber;
            }

            IsVisibleShelfNumberNormal = true;

            if (!shelfNumber.Equals(item.ShelfNumber))
            {
                IsVisibleShelfNumber = true;
                IsVisibleShelfNumberNormal = false;
            }

            Status = CustodyStatusEnum.Storage.Value;
            IsVisibleStatusNormal = true;
            if (!item.Status.Equals(CustodyStatusEnum.Storage.Value))
            {
                IsVisibleStatus = true;
                IsVisibleStatusNormal = false;
            }
        }

        /// <summary>
        /// Delivery
        /// </summary>
        private async void Renew()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.CustodyDetail))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0008);
                return;
            }

            var custodyDetailBody = _item.GetCustodyDetail();
            custodyDetailBody.InventoryDate = DateTime.Now;
            custodyDetailBody.shelfNumber = _shelfNumber;
            custodyDetailBody.status = CustodyStatusEnum.Storage.Value;
            custodyDetailBody.returnDate = null;


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
    }
}
