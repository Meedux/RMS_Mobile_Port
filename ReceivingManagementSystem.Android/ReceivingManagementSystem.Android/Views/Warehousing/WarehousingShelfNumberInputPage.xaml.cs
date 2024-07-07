using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
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
    public partial class WarehousingShelfNumberInputPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;

        public WarehousingShelfNumberInputPage(CustodyItemViewModel itemViewModel)
        {
            InitializeComponent();

            this.BindingContext = new WarehousingShelfNumberInputViewModel(itemViewModel, this);
        }

        protected override void OnAppearing()
        {
            WarehousingShelfNumberInputViewModel viewModel = (WarehousingShelfNumberInputViewModel)this.BindingContext;

            viewModel.RfidInit();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            WarehousingShelfNumberInputViewModel viewModel = (WarehousingShelfNumberInputViewModel)this.BindingContext;

            viewModel.RfidStop();

            base.OnDisappearing();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            WarehousingShelfNumberInputViewModel viewModel = (WarehousingShelfNumberInputViewModel)this.BindingContext;

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
                    WarehousingShelfNumberInputViewModel viewModel = (WarehousingShelfNumberInputViewModel)this.BindingContext;

                    viewModel.Rfid = viewModel.Rfid.Replace("\r\n", "");
                    viewModel.SelectShelfCommand.Execute(null);
                    if(viewModel.ShelfNumberSelected != null)
                        viewModel.OkCommand.Execute(null);
                }
            }
        }

        private void txtRfid_Completed(object sender, EventArgs e)
        {
            WarehousingShelfNumberInputViewModel viewModel = (WarehousingShelfNumberInputViewModel)this.BindingContext;

            viewModel.SelectShelfCommand.Execute(null);
            if (viewModel.ShelfNumberSelected != null)
                viewModel.OkCommand.Execute(null);
        }

        public void OnClose()
        {
            OnCloseOk.Raise(this, new EventArgs());
        }
    }
}