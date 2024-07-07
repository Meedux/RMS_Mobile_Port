using ReceivingManagementSystem.Android.ViewModels.Warehousing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Warehousing
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WarehousingPage : ContentPage
    {
        public WarehousingPage()
        {
            InitializeComponent();

            this.BindingContext = new WarehousingViewModel(this);
        }

        protected override void OnAppearing()
        {
            WarehousingViewModel viewModel = (WarehousingViewModel)this.BindingContext;

            viewModel.RfidInit();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            WarehousingViewModel viewModel = (WarehousingViewModel)this.BindingContext;

            viewModel.RfidStop();

            base.OnDisappearing();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            WarehousingViewModel viewModel = (WarehousingViewModel)this.BindingContext;

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
                    WarehousingViewModel viewModel = (WarehousingViewModel)this.BindingContext;

                    viewModel.OkCommand.Execute(null);
                }
            }
        }

        private void txtRfid_Completed(object sender, EventArgs e)
        {
            WarehousingViewModel viewModel = (WarehousingViewModel)this.BindingContext;

            viewModel.OkCommand.Execute(null);
        }
    }
}