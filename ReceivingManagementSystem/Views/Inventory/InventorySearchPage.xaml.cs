using ReceivingManagementSystem.ViewModels.Inventory;
using ReceivingManagementSystem.ViewModels.Warehousing;
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
    public partial class InventorySearchPage : ContentPage
    {
        public InventorySearchPage()
        {
            InitializeComponent();

            this.BindingContext = new InventorySearchViewModel(this);
        }

        protected override void OnAppearing()
        {
            InventorySearchViewModel viewModel = (InventorySearchViewModel)this.BindingContext;

            viewModel.RfidInit();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            InventorySearchViewModel viewModel = (InventorySearchViewModel)this.BindingContext;

            viewModel.RfidStop();

            base.OnDisappearing();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();

            InventorySearchViewModel viewModel = (InventorySearchViewModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }

        private void txtRfid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                char key = e.NewTextValue?.Last() ?? ' ';

                Log.Information(e.NewTextValue);

                if (e.NewTextValue.EndsWith("\r\n"))
                {
                    InventorySearchViewModel viewModel = (InventorySearchViewModel)this.BindingContext;
                    viewModel.SelectShelfCommand.Execute(null);
                }
            }
        }
    }
}