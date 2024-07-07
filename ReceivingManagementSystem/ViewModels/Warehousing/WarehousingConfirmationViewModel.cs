using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.ViewModels.Warehousing;
using ReceivingManagementSystem.Warehousing;
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

namespace ReceivingManagementSystem.ViewModels.Warehousing
{
    public class WarehousingConfirmationViewModel : BaseViewModel
    {
        #region Properties

        private CustodyItemViewModel _custodyItem;
        public CustodyItemViewModel CustodyItem
        {
            get { return _custodyItem; }
            set { this.SetProperty(ref this._custodyItem, value); }
        }

        #endregion

        #region Command

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Dependency

        private IPleasanterService _pleasanterService;

        #endregion

        public WarehousingConfirmationViewModel(CustodyItemViewModel itemViewModel, ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Cancel);

            _pleasanterService = DependencyService.Get<IPleasanterService>();

            CustodyItem = itemViewModel;
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

            var custodyDetail = _custodyItem.GetCustodyDetail();
            custodyDetail.status = CustodyStatusEnum.Storage.Value;
            custodyDetail.shelfNumber = _custodyItem.ShelfNumber;

            bool result = await _pleasanterService.UpdateCustodyDetail(custodyDetail);

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.Warehousing);
            }
            else
            {
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.Warehousing);
                Close(true);
            }
        }

        /// <summary>
        /// ShowConfirm Order
        /// </summary>
        private void Cancel()
        {
            ShowMessage(MessageResourceKey.E0012, TextResourceKey.Warehousing);
            Close(false);
        }
    }
}
