using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Custodies;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.Views;
using ReceivingManagementSystem.Android.Warehousing;
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
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.Android.ViewModels.Warehousing
{
    public class WarehousingViewModel : BaseViewModel
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
        #endregion

        #region Command

        public ICommand OkCommand { get; }
        public ICommand ReadRFIDCommand { get; }

        #endregion

        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private string _device;
        private bool _transition = false;

        public WarehousingViewModel(ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            ReadRFIDCommand = new Command(ReadRFID);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");
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

        private void WarehousingShelfNumberInputPage_Disappearing(object sender, EventArgs e)
        {
            WarehousingShelfNumberInputPage warehousingShelfNumberInputPage = (WarehousingShelfNumberInputPage)sender;

            var viewModel = (WarehousingShelfNumberInputViewModel)warehousingShelfNumberInputPage.BindingContext;

            if (viewModel.IsOK)
            {
                Rfid = "";
            }
        }

        /// <summary>
        /// ShowConfirm Order
        /// </summary>
        private async void Ok()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.Custody, PleasanterObjectTypeEnum.CustodyDetail))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0008);
                return;
            }

            bool isValidate = await Validate();

            if (!isValidate)
            {
                return;
            }

            CustodyDetailBody custodyDetailBodyParams = new CustodyDetailBody()
            {
                rfid = _rfid,
                status = CustodyStatusEnum.Custody.Value
            };

            CustodyItemModel custodyItemModel = await _pleasanterService.GetCustodyItemByDetail(custodyDetailBodyParams);
            if (custodyItemModel == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0002, TextResourceKey.Custody);

                return;
            }

            RfidStop();
            _transition = true;
            WarehousingShelfNumberInputPage warehousingShelfNumberInputPage = new WarehousingShelfNumberInputPage(new CustodyItemViewModel(custodyItemModel));
            warehousingShelfNumberInputPage.OnCloseOk += WarehousingShelfNumberInputPage_Disappearing;
            await this.Owner.Navigation.PushAsync(warehousingShelfNumberInputPage, true);
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(_rfid))
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Rfid));
            }

            if (errors.Count > 0)
            {
                await Owner.DisplayActionSheet(ResourceProvider.GetResourceByName(TextResourceKey.NotificationTitle),
                   ResourceProvider.GetResourceByName(TextResourceKey.OK), null, errors.ToArray());
                return false;
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
                _readRfidService.ReadRfid();
            }
            else
            {
                Rfid = string.Empty;
            }
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            Rfid = args.Rfid + "\r\n";
        }
    }
}
