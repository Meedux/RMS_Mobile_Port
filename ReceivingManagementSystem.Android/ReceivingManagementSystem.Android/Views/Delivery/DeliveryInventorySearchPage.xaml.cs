using ReceivingManagementSystem.Android.ViewModels.Delivery;
using ReceivingManagementSystem.Android.ViewModels.Warehousing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Delivery
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeliveryInventorySearchPage : ContentPage
    {
        public DeliveryInventorySearchPage()
        {
            InitializeComponent();

            this.BindingContext = new DeliveryInventorySearchViewModel(this);
        }

        protected override void OnAppearing()
        {
            DeliveryInventorySearchViewModel viewModel = (DeliveryInventorySearchViewModel)this.BindingContext;

            viewModel.RfidInit();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            DeliveryInventorySearchViewModel viewModel = (DeliveryInventorySearchViewModel)this.BindingContext;

            viewModel.RfidStop();

            base.OnDisappearing();
        }

        private void RfidButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            DeliveryInventorySearchViewModel viewModel = (DeliveryInventorySearchViewModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }

        private void ShelfRfidButton_Clicked(object sender, EventArgs e)
        {
            txtShelfRfid.Focus();
            DeliveryInventorySearchViewModel viewModel = (DeliveryInventorySearchViewModel)this.BindingContext;
            viewModel.ReadShelfRFIDCommand.Execute(null);
        }

        private void txtRfid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                char key = e.NewTextValue?.Last() ?? ' ';

                Log.Information(e.NewTextValue);

                if (e.NewTextValue.EndsWith("\r\n"))
                {
                    DeliveryInventorySearchViewModel viewModel = (DeliveryInventorySearchViewModel)this.BindingContext;
                    viewModel.SearchParams.Rfid = viewModel.SearchParams.Rfid.Replace("\r\n", "");
                }
            }
        }

        private void txtShelfRfid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                char key = e.NewTextValue?.Last() ?? ' ';

                Log.Information(e.NewTextValue);

                if (e.NewTextValue.EndsWith("\r\n"))
                {
                    DeliveryInventorySearchViewModel viewModel = (DeliveryInventorySearchViewModel)this.BindingContext;
                    viewModel.ShelfRfid = viewModel.ShelfRfid.Replace("\r\n", "");
                    viewModel.SelectShelfCommand.Execute(null);
                }
            }
        }

        private void txtShelfRfid_Completed(object sender, EventArgs e)
        {
            DeliveryInventorySearchViewModel viewModel = (DeliveryInventorySearchViewModel)this.BindingContext;
            viewModel.SelectShelfCommand.Execute(null);
        }
    }
}