using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.ReturnReception;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.Warehousing;
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

namespace ReceivingManagementSystem.Android.ViewModels.Custodies
{
    public class ReturnReceptionViewModel : BaseViewModel
    {
        #region Properties

        private CustodyItemViewModel _returnReceptionInfo;
        public CustodyItemViewModel ReturnReceptionInfo
        {
            get { return _returnReceptionInfo; }
            set { this.SetProperty(ref this._returnReceptionInfo, value); }
        }

        private bool _isPickerOpen;
        public bool IsPickerOpen
        {
            get { return _isPickerOpen; }
            set { this.SetProperty(ref this._isPickerOpen, value); }
        }
        #endregion

        #region Command

        public ICommand OkCommand { get; }
        public ICommand SetToDayCommand { get; }
        public ICommand OpenPickerCommand { get; }
        public ICommand OkSelectDateCommand { get; }

        #endregion


        public ReturnReceptionViewModel(CustodyItemViewModel itemView, ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            SetToDayCommand = new Command(SetToDay);
            OpenPickerCommand = new Command(OpenPicker);
            OkSelectDateCommand = new Command(OkSelectDate);

            ReturnReceptionInfo = itemView;
            this.Owner.Appearing += Owner_Appearing;

            ReturnReceptionInfo.SelectDate = DateTime.Now;
        }

        private void Owner_Appearing(object sender, EventArgs e)
        {
            if (IsClose)
            {
                Close(true);
            }
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

            ReturnReceptionConfirmPage returnReceptionSearchResultPage = new ReturnReceptionConfirmPage(_returnReceptionInfo);
            returnReceptionSearchResultPage.OnCloseOk += ReturnReceptionSearchResultPage_Disappearing; ;
            await this.Owner.Navigation.PushAsync(returnReceptionSearchResultPage, true);
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(_returnReceptionInfo.InputDate))
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.ReturnDate));
            }

            if (errors.Count > 0)
            {
                string action = await Owner.DisplayActionSheet(ResourceProvider.GetResourceByName(TextResourceKey.NotificationTitle),
                   ResourceProvider.GetResourceByName(TextResourceKey.Cancel), null, errors.ToArray());
                return false;
            }

            return true;
        }

        private void ReturnReceptionSearchResultPage_Disappearing(object sender, EventArgs e)
        {
            ReturnReceptionConfirmPage returnReceptionSearchResultPage = (ReturnReceptionConfirmPage)sender;

            var viewModel = (ReturnReceptionConfirmViewModel)returnReceptionSearchResultPage.BindingContext;

            IsClose = viewModel.IsOK;
        }

        /// <summary>
        /// Set 預り予定日 = ToDay
        /// </summary>
        private void SetToDay()
        {
            ReturnReceptionInfo.SelectDate = DateTime.Now;
            ReturnReceptionInfo.InputDate = ReturnReceptionInfo.SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }

        private void OpenPicker()
        {
            IsPickerOpen = true;
        }

        private void OkSelectDate()
        {
            ReturnReceptionInfo.InputDate = ReturnReceptionInfo.SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }
    }
}
