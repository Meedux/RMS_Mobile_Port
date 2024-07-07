using ReceivingManagementSystem.ViewModels.PalletRegistration;
using ReceivingManagementSystem.ViewModels.Receipt;
using ReceivingManagementSystem.ViewModels.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Views.Shipping
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ShippingPage : ContentPage
	{
		public ShippingPage()
		{
			InitializeComponent ();

            var viewModel = new ShippingViewModel(this);
            viewModel.OnRfidChange += OnRfidChange;
            this.BindingContext = viewModel;
        }

        private void OnRfidChange(object sender, EventArgs e)
        {
            txtShippedNumber.Focus();
        }

        protected override void OnAppearing()
        {
            ShippingViewModel viewModel = (ShippingViewModel)this.BindingContext;

            viewModel.RfidInit();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            ShippingViewModel viewModel = (ShippingViewModel)this.BindingContext;

            viewModel.RfidStop();

            base.OnDisappearing();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            ShippingViewModel viewModel = (ShippingViewModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }

        private void txtRfid_Completed(object sender, EventArgs e)
        {
            ShippingViewModel viewModel = (ShippingViewModel)this.BindingContext;

            viewModel.ReadRFIDHIDCommand.Execute(null);

            txtShippedNumber.Focus();
        }
    }
}