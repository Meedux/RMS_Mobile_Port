using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Inventory;
using ReceivingManagementSystem.Android.ViewModels.Warehousing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Inventory
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventorySearchResultPage : ContentPage
    {
        //Timer scanTimer = null;

        public InventorySearchResultPage(List<InventoryCustodyItemViewModel> custodies)
        {

            InitializeComponent();
            InventorySearchResultViewModel viewModel = new InventorySearchResultViewModel(custodies, this);
            this.BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            InventorySearchResultViewModel viewModel = (InventorySearchResultViewModel)this.BindingContext;

            viewModel.RfidInit();;

            Refresh();

            base.OnAppearing();
        }

        private void Refresh()
        {
            Thread.Sleep(2000);

            dataGrid.GridColumnSizer.Refresh(true);
        }

        protected override void OnDisappearing()
        {
            InventorySearchResultViewModel viewModel = (InventorySearchResultViewModel)this.BindingContext;

            viewModel.RfidStop();

            base.OnDisappearing();
        }

        private void btnUpdate_Clicked(object sender, EventArgs e)
        {
            InventorySearchResultViewModel viewModel = (InventorySearchResultViewModel)this.BindingContext;

            viewModel.UpdateCommand.Execute(null);
        }

        private void btnReadMultiRfid_Clicked(object sender, EventArgs e)
        {
            InventorySearchResultViewModel viewModel = (InventorySearchResultViewModel)this.BindingContext;

            viewModel.StartScanCommand.Execute(null);

            txtRfid.Text = "";
        }
    }
}