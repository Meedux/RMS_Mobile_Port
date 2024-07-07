using ReceivingManagementSystem.ViewModels.Orders;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPage : ContentPage
    {
        public SettingPage()
        {
            InitializeComponent();
            this.BindingContext = new SettingViewModel(this);
        }


        protected override void OnAppearing()
        {
            SettingViewModel viewModel = (SettingViewModel)this.BindingContext;

            viewModel.RfidInit();
        }

        protected override void OnDisappearing()
        {
            SettingViewModel viewModel = (SettingViewModel)this.BindingContext;

            viewModel.RfidStop();
        }

        private void txtRfid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                char key = e.NewTextValue?.Last() ?? ' ';

                Log.Information(e.NewTextValue);

                if (e.NewTextValue.EndsWith("\r\n"))
                {
                    //WarehousingViewModel viewModel = (WarehousingViewModel)this.BindingContext;

                    //viewModel.OkCommand.Execute(null);
                }
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            SettingViewModel viewModel = (SettingViewModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }
    }
}