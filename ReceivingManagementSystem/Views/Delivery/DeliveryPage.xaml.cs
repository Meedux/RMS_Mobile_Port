using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.ViewModels.Delivery;
using ReceivingManagementSystem.ViewModels.Orders;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Delivery
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeliveryPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;

        public DeliveryPage(CustodyItemViewModel item)
        {
            InitializeComponent();

            this.BindingContext = new DeliveryViewModel(item, this);
        }

        protected override void OnAppearing()
        {
            DeliveryViewModel viewModel = (DeliveryViewModel)this.BindingContext;

            viewModel.RfidInit();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            DeliveryViewModel viewModel = (DeliveryViewModel)this.BindingContext;

            viewModel.RfidStop();

            OnCloseOk.Raise(this, new EventArgs());

            base.OnDisappearing();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            DeliveryViewModel viewModel = (DeliveryViewModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }
    }
}