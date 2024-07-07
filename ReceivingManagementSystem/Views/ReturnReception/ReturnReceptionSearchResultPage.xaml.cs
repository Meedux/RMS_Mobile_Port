using ReceivingManagementSystem.ViewModels.Custodies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.ReturnReception
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnReceptionSearchResultPage : ContentPage
    {
        public ReturnReceptionSearchResultPage(List<CustodyItemViewModel> items)
        {
            InitializeComponent();

            this.BindingContext = new ReturnReceptionSearchResultViewModel(items, this);
        }

        private void dataGrid_GridDoubleTapped(object sender, Syncfusion.SfDataGrid.XForms.GridDoubleTappedEventArgs e)
        {
            ReturnReceptionSearchResultViewModel viewModel = (ReturnReceptionSearchResultViewModel)this.BindingContext;

            if (e.RowData != null)
            {
                viewModel.DoubleClickCommand.Execute(null);
            }
        }
    }
}