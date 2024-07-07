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

namespace ReceivingManagementSystem.ViewModels.Custodies
{
    public class CustodyConfirmationViewModel : BaseViewModel
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

        #endregion

        #region Dependency
        
        private IPleasanterService _pleasanterService;

        #endregion

        public CustodyConfirmationViewModel(CustodyItemViewModel itemViewModel, ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);

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
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0008);
                return;
            }

            var custodyDetail = _custodyItem.GetCustodyDetail();
            custodyDetail.status = CustodyStatusEnum.Custody.Value;
            custodyDetail.rfid = _custodyItem.Rfid;

            bool result = await _pleasanterService.UpdateCustodyDetail(custodyDetail);

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Custody);
            }
            else
            {
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.Custody);
                Close(true);
            }
        }
    }
}
