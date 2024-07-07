using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Delivery
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeliveryInventorySearchResultPage : ContentPage
    {
        public DeliveryInventorySearchResultPage(List<CustodyItemViewModel> items)
        {
            InitializeComponent();

            this.BindingContext = new DeliveryInventorySearchResultViewModel(items, this);
        }

        private void dataGrid_GridDoubleTapped(object sender, Syncfusion.SfDataGrid.XForms.GridDoubleTappedEventArgs e)
        {
            DeliveryInventorySearchResultViewModel viewModel = (DeliveryInventorySearchResultViewModel)this.BindingContext;

            if (e.RowData != null)
            {
                viewModel.DoubleClickCommand.Execute(null);
            }
        }
    }
}