using ReceivingManagementSystem.ViewModels.PalletRegistration;
using ReceivingManagementSystem.ViewModels.Receipt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Views.Receipt
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ReceiptPage : ContentPage
	{
		public ReceiptPage()
		{
			InitializeComponent ();

            var viewModel = new ReceiptViewModel(this);
            viewModel.OnRfidChange += OnRfidChange;
            this.BindingContext = viewModel;
        }

        private void OnRfidChange(object sender, EventArgs e)
        {
            txtReceivedNumber.Focus();
        }

        protected override void OnAppearing()
        {
            ReceiptViewModel viewModel = (ReceiptViewModel)this.BindingContext;

            viewModel.RfidInit();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            ReceiptViewModel viewModel = (ReceiptViewModel)this.BindingContext;

            viewModel.RfidStop();

            base.OnDisappearing();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            ReceiptViewModel viewModel = (ReceiptViewModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }

        private void txtRfid_Completed(object sender, EventArgs e)
        {
            txtReceivedNumber.Focus();
        }
    }
}