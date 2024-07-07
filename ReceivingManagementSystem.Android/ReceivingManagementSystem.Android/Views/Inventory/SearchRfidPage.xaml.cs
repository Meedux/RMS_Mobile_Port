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
    public partial class SearchRfidPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;

        public SearchRfidPage(InventoryCustodyItemViewModel item)
        {
            InitializeComponent();

            this.BindingContext = new SearchRfidModel(item, this);
        }

        protected override void OnAppearing()
        {
            SearchRfidModel viewModel = (SearchRfidModel)this.BindingContext;
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
            SearchRfidModel viewModel = (SearchRfidModel)this.BindingContext;

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
            SearchRfidModel viewModel = (SearchRfidModel)this.BindingContext;

            viewModel.SearchRFIDCommand.Execute(null);
        }

        private void btnRfidShelfNumber_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            SearchRfidModel viewModel = (SearchRfidModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }

        private void txtRfidShelfNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                char key = e.NewTextValue?.Last() ?? ' ';

                Log.Information(e.NewTextValue);

                if (e.NewTextValue.EndsWith("\r\n"))
                {
                    SearchRfidModel viewModel = (SearchRfidModel)this.BindingContext;
                    viewModel.Rfid = viewModel.Rfid.Replace("\r\n", "");
                    viewModel.SelectShelfCommand.Execute(null);
                }
            }
        }

        private void txtRfidShelfNumber_Completed(object sender, EventArgs e)
        {
            SearchRfidModel viewModel = (SearchRfidModel)this.BindingContext;
            viewModel.SelectShelfCommand.Execute(null);
        }
    }
}