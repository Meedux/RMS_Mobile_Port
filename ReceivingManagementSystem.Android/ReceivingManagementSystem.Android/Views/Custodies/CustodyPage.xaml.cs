using ReceivingManagementSystem.Android.ViewModels.Custodies;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ReceivingManagementSystem.Common;

namespace ReceivingManagementSystem.Android.Custodies
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustodyPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;

        public CustodyPage(CustodyItemViewModel itemModel)
        {
            InitializeComponent();

            CustodyViewModel viewModel = new CustodyViewModel(itemModel, this);
            this.BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            CustodyViewModel viewModel = (CustodyViewModel)this.BindingContext;

                viewModel.RfidInit();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            OnCloseOk.Raise(this, new EventArgs());

            base.OnDisappearing();
            CustodyViewModel viewModel = (CustodyViewModel)this.BindingContext;

            viewModel.RfidStop();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            CustodyViewModel viewModel = (CustodyViewModel)this.BindingContext;

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
                    Log.Information("RP902");
                }
            }
        }

        private void txtRfid_Completed(object sender, EventArgs e)
        {
            CustodyViewModel viewModel = (CustodyViewModel)this.BindingContext;

            viewModel.OkCommand.Execute(null);
        }
    }
}