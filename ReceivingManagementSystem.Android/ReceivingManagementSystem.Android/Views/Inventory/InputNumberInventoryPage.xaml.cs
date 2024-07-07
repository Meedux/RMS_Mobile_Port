using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Delivery;
using ReceivingManagementSystem.Android.ViewModels.Inventory;
using ReceivingManagementSystem.Android.ViewModels.Orders;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Inventory
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InputNumberInventoryPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;

        public InputNumberInventoryPage(ItemInventoryItemViewModel item)
        {
            InitializeComponent();

            this.BindingContext = new InputNumberInventoryViewModel(item, this);
        }

        protected override void OnAppearing()
        {
            InputNumberInventoryViewModel viewModel = (InputNumberInventoryViewModel)this.BindingContext;
            if (viewModel.IsClose)
            {
                OnCloseOk.Raise(this, new EventArgs());
                viewModel.Close(true);
            }
            else
            {
                viewModel.RfidInit();
            }

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            InputNumberInventoryViewModel viewModel = (InputNumberInventoryViewModel)this.BindingContext;

            viewModel.RfidStop();

            OnCloseOk.Raise(this, new EventArgs());

            base.OnDisappearing();
        }

        private void txtRfid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                char key = e.NewTextValue?.Last() ?? ' ';

                Log.Information(e.NewTextValue);

                if (e.NewTextValue.EndsWith("\r\n"))
                {
                   
                }
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            InputNumberInventoryViewModel viewModel = (InputNumberInventoryViewModel)this.BindingContext;

            viewModel.SearchRFIDCommand.Execute(null);
        }
    }
}