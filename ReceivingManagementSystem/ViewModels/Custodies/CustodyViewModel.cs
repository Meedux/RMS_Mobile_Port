using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Custodies;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
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

namespace ReceivingManagementSystem.ViewModels.Custodies
{
    public class CustodyViewModel : BaseViewModel
    {
        #region Properties

        private CustodyItemViewModel _custodyInfo;
        public CustodyItemViewModel CustodyInfo
        {
            get { return _custodyInfo; }
            set { this.SetProperty(ref this._custodyInfo, value); }
        }

        private string _device;
        #endregion

        #region Command

        public ICommand OkCommand { get; }
        public ICommand ReadRFIDCommand { get; }

        #endregion

        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public CustodyViewModel(CustodyItemViewModel itemView, ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            ReadRFIDCommand = new Command(ReadRFID);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            CustodyInfo = itemView;

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");

            this.Owner.Appearing += Owner_Appearing;
        }

        private void Owner_Appearing(object sender, EventArgs e)
        {
            if (IsClose)
            {
                Close(true);
            }
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
            bool isValidate = await Validate();

            if (!isValidate)
            {
                return;
            }

            Update();
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(_custodyInfo.Rfid))
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Rfid));
            }
            else
            {
                if (_custodyInfo.Rfid.Any(char.IsLower))
                {
                    errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0010));
                }

                var items = await _pleasanterService.GetCustodyDetail(new CustodyDetailBody()
                {
                    rfid = _custodyInfo.Rfid
                });

                if (items.Count > 0 && items.Count(r => r.id != _custodyInfo.Id) > 0)
                {
                    errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0011, TextResourceKey.Rfid));
                }
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
        /// Register order
        /// </summary>
        private async void Update()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.CustodyDetail))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0008);
                return;
            }

            //var custodyDetail = _custodyInfo.GetCustodyDetail();
            //custodyDetail.status = CustodyStatusEnum.Custody.Value;
            //custodyDetail.rfid = _custodyInfo.Rfid;

            //bool result = await _pleasanterService.UpdateCustodyDetail(custodyDetail);

            //if (!result)
            //{
            //    await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
            //         MessageResourceKey.E0003, TextResourceKey.Custody);
            //}
            //else
            //{
            //    ShowMessage(MessageResourceKey.E0004, TextResourceKey.Custody);
            //    Close(true);
            //}

            CustodyConfirmationPage warehousingConfirmationPage = new CustodyConfirmationPage(_custodyInfo);
            warehousingConfirmationPage.OnCloseOk += CustodyPage_Disappearing;
            await this.Owner.Navigation.PushAsync(warehousingConfirmationPage, true);
        }

        private void CustodyPage_Disappearing(object sender, EventArgs e)
        {
            CustodyConfirmationPage custodyPage = (CustodyConfirmationPage)sender;

            var viewModel = (CustodyConfirmationViewModel)custodyPage.BindingContext;

            if (viewModel.IsOK)
            {
                IsClose = true;
            }
        }

        /// <summary>
        /// Read RFID
        /// </summary>
        private void ReadRFID()
        {
            _custodyInfo.Rfid = string.Empty;
            if ("SP1".Equals(_device))
            {
                _readRfidService.ReadRfid();
            }
            else
            {
                CustodyInfo.Rfid = string.Empty;
            }
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            CustodyInfo.Rfid = args.Rfid;
            Ok();
        }
    }
}
