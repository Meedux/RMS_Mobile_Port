using ReceivingManagementSystem.Android.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Orders
{
    public partial class CustomerSearchResultPopup
    {
        public CustomerSearchResultPopup(List<CustomerSearchParamViewModel> clients)
        {
            InitializeComponent();

            CustomerSearchResultViewModel viewModel = new CustomerSearchResultViewModel(clients);
            BindingContext = viewModel;
        }

        private void btnOK_Clicked(object sender, EventArgs e)
        {
            Ok();
        }

        private void btnCancel_Clicked(object sender, EventArgs e)
        {
            Dismiss(null);
        }

        private void dataGrid_GridDoubleTapped(object sender, Syncfusion.SfDataGrid.XForms.GridDoubleTappedEventArgs e)
        {
            CustomerSearchResultViewModel viewModel = (CustomerSearchResultViewModel)this.BindingContext;

            if (e.RowData != null)
            {
                Ok();
            }
        }

        private void Ok()
        {
            CustomerSearchResultViewModel viewModel = (CustomerSearchResultViewModel)this.BindingContext;

            Dismiss(viewModel.ClientSelected);
        }
    }
}