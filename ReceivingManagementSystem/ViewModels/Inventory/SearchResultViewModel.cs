using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.Services.Rfid;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using static PleasanterOperation.OperationData;
using static RMS_Pleasanter.Client;
using static RMS_Pleasanter.Contents;
using System.Collections.ObjectModel;
using static RMS_Pleasanter.CustodyDetail;
using ReceivingManagementSystem.Services.Pleasanter;
using static RMS_Pleasanter.Custody;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Inventory;
using ReceivingManagementSystem.Wrapper;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Data;
using CsvHelper.Configuration;
using ReceivingManagementSystem.Views;

namespace ReceivingManagementSystem.ViewModels.Inventory
{
    public class SearchResultViewModel : BaseViewModel
    {
        #region Properties

        private ObservableCollection<InventoryCustodyItemViewModel> _custodies;

        public ObservableCollection<InventoryCustodyItemViewModel> Custodies
        {
            get { return _custodies; }
            set { this.SetProperty(ref this._custodies, value); }
        }

        private InventoryCustodyItemViewModel _custodySelected;

        public InventoryCustodyItemViewModel CustodySelected
        {
            get { return _custodySelected; }
            set
            {
                this.SetProperty(ref this._custodySelected, value);
            }
        }

        #endregion

        #region Command

        public ICommand DoubleClickCommand { get; set; }
        #endregion

        private IPleasanterService _pleasanterService;

        public SearchResultViewModel(List<InventoryCustodyItemViewModel> custodies, ContentPage owner) : base(owner)
        {
            Custodies = new ObservableCollection<InventoryCustodyItemViewModel>(custodies);

            DoubleClickCommand = new Command(Update);

            _pleasanterService = DependencyService.Get<IPleasanterService>();
        }

        /// <summary>
        /// Double click row
        /// </summary>
        private async void Update()
        {
            if (_custodySelected != null && !_custodySelected.Id.HasValue)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.Cancel, MessageResourceKey.E0006);

                return;
            }

            SearchRfidPage custodyPage = new SearchRfidPage(_custodySelected);
            custodyPage.OnCloseOk += Update_Disappearing;
            await this.Owner.Navigation.PushAsync(custodyPage, true);
        }

        private void Update_Disappearing(object sender, EventArgs e)
        {
            SearchRfidPage custodyPage = (SearchRfidPage)sender;

            var viewModel = (SearchRfidModel)custodyPage.BindingContext;

            if (viewModel.IsOK)
            {
                // 更新する
                CustodySelected.ShelfNumber = viewModel.Item.ShelfNumber;
                CustodySelected.Status = viewModel.Item.Status;
                CustodySelected = null;
            }
        }
    }
}
