using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Email;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
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

namespace ReceivingManagementSystem.ViewModels.Custodies
{
    public class ReturnReceptionConfirmViewModel : BaseViewModel
    {
        #region Properties

        private CustodyItemViewModel _returnReceptionInfo;
        public CustodyItemViewModel ReturnReceptionInfo
        {
            get { return _returnReceptionInfo; }
            set { this.SetProperty(ref this._returnReceptionInfo, value); }
        }

        private string _mailTo;
        #endregion

        #region Command

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        private IPleasanterService _pleasanterService;
        private IEmailService _emailService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public ReturnReceptionConfirmViewModel(CustodyItemViewModel itemView, ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Cancel);

            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _emailService = DependencyService.Get<IEmailService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            _mailTo = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Send_Mail_To, string.Empty);

            ReturnReceptionInfo = itemView;
        }

        /// <summary>
        /// ShowConfirm Order
        /// </summary>
        private async void Ok()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.Custody, PleasanterObjectTypeEnum.CustodyDetail))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0008);
                return;
            }

            var custodyDetailBody = _returnReceptionInfo.GetCustodyDetail();
            custodyDetailBody.status = CustodyStatusEnum.Return_Reception.Value;
            custodyDetailBody.returnDate = _returnReceptionInfo.GetInputDate();

            bool result = await _pleasanterService.UpdateCustodyDetail(custodyDetailBody);

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.ReturnReception);
            }
            else
            {
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.ReturnReception);

                await SendMail(custodyDetailBody.rfid, _returnReceptionInfo.InputDate);

                Close(true);
            }
        }

        private async Task<bool> SendMail(string rfid, string returnDate)
        {
            if (string.IsNullOrEmpty(_mailTo))
            {
                return false;
            }

            StringBuilder emailContent = new StringBuilder();
            emailContent.AppendLine($"<div>以下のRFIDの出荷作業を行ってください。</div>");

            emailContent.AppendLine($"<br>");

            emailContent.AppendLine($"<div>RFID：{rfid}</div>");
            emailContent.AppendLine($"<div>出荷予定日: {returnDate}</div>");

            bool result = await _emailService.SendEmail(_mailTo, "", "返却依頼があります", emailContent.ToString());

            return result;
        }

        /// <summary>
        /// ShowConfirm Order
        /// </summary>
        private void Cancel()
        {
            ShowMessage(MessageResourceKey.E0012, TextResourceKey.ReturnReception);
            Close(false);
        }
    }
}
