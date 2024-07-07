using ReceivingManagementSystem.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.UI.Views;

namespace ReceivingManagementSystem.Orders
{
    public partial class CustomerSearchPopup
    {
        public CustomerSearchPopup()
        {
            InitializeComponent();

            CustomerSearchViewModel viewModel = new CustomerSearchViewModel();
            this.BindingContext = viewModel;
        }

        private void btnSearch_Clicked(object sender, EventArgs e)
        {
            CustomerSearchViewModel viewModel = (CustomerSearchViewModel)this.BindingContext;

            Dismiss(viewModel.CustomerSearch);
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            Dismiss(null);
        }
    }
}