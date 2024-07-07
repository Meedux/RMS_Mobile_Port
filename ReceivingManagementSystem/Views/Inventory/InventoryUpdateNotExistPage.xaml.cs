using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.ViewModels.Delivery;
using ReceivingManagementSystem.ViewModels.Inventory;
using ReceivingManagementSystem.ViewModels.Orders;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Inventory
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryUpdateNotExistPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;

        public InventoryUpdateNotExistPage(InventoryCustodyItemViewModel item)
        {
            InitializeComponent();

            this.BindingContext = new InventoryUpdateNotExistViewModel(item, this);
        }

        protected override void OnAppearing()
        {
            InventoryUpdateNotExistViewModel viewModel = (InventoryUpdateNotExistViewModel)this.BindingContext;
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
            InventoryUpdateNotExistViewModel viewModel = (InventoryUpdateNotExistViewModel)this.BindingContext;

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
            InventoryUpdateNotExistViewModel viewModel = (InventoryUpdateNotExistViewModel)this.BindingContext;

            viewModel.SearchRFIDCommand.Execute(null);
        }

        private void btnRfidShelfNumber_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            InventoryUpdateNotExistViewModel viewModel = (InventoryUpdateNotExistViewModel)this.BindingContext;

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
                    InventoryUpdateNotExistViewModel viewModel = (InventoryUpdateNotExistViewModel)this.BindingContext;
                    viewModel.Rfid = viewModel.Rfid.Replace("\r\n", "");
                    viewModel.SelectShelfCommand.Execute(null);
                }
            }
        }

        private void txtRfidShelfNumber_Completed(object sender, EventArgs e)
        {
            InventoryUpdateNotExistViewModel viewModel = (InventoryUpdateNotExistViewModel)this.BindingContext;
            viewModel.SelectShelfCommand.Execute(null);
        }
    }
}