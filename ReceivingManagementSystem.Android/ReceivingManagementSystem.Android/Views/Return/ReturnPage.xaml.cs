using ReceivingManagementSystem.Android.ViewModels.Warehousing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Return
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnPage : ContentPage
    {
        public ReturnPage()
        {
            InitializeComponent();

            this.BindingContext = new ReturnViewModel(this);
        }

        protected override void OnAppearing()
        {
            ReturnViewModel viewModel = (ReturnViewModel)this.BindingContext;

            viewModel.RfidInit();
        }

        protected override void OnDisappearing()
        {
            ReturnViewModel viewModel = (ReturnViewModel)this.BindingContext;

            viewModel.RfidStop();
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                char key = e.NewTextValue?.Last() ?? ' ';

                Log.Information(e.NewTextValue);

                if (e.NewTextValue.EndsWith("\r\n"))
                {
                    ReturnViewModel viewModel = (ReturnViewModel)this.BindingContext;

                    viewModel.OkCommand.Execute(null);
                }
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            ReturnViewModel viewModel = (ReturnViewModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }

        private void txtRfid_Completed(object sender, EventArgs e)
        {

        }
    }
}