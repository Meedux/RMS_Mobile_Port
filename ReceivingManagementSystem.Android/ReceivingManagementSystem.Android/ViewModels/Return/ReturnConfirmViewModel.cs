using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Warehousing;
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

namespace ReceivingManagementSystem.Android.ViewModels.Warehousing
{
    public class ReturnConfirmViewModel : BaseViewModel
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

        public ICommand ReturnCommand { get; }
        public ICommand ReadRFIDCommand { get; }

        #endregion

        #region Dependency
        
        private IPleasanterService _pleasanterService;

        #endregion

        public ReturnConfirmViewModel(CustodyItemViewModel itemViewModel, ContentPage owner) : base(owner)
        {
            ReturnCommand = new Command(Return);

            _pleasanterService = DependencyService.Get<IPleasanterService>();

            CustodyItem = itemViewModel;
        }

        private async void Return()
        {
            var custodyDetail = _custodyItem.GetCustodyDetail();

            custodyDetail.status = CustodyStatusEnum.Return.Value;

            var result = await _pleasanterService.UpdateCustodyDetail(custodyDetail);

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Return);
            }
            else
            {
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.Return);
                Close(true);
            }
        }
    }
}
