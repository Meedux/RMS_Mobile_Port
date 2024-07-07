using ReceivingManagementSystem.Android.ViewModels.Custodies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Custodies
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustodySearchResultPage : ContentPage
    {
        public CustodySearchResultPage(List<CustodyItemViewModel> custodies)
        {
            InitializeComponent();

            CustodySearchResultViewModel viewModel = new CustodySearchResultViewModel(custodies, this);
            this.BindingContext = viewModel;
        }

        private void dataGrid_GridDoubleTapped(object sender, Syncfusion.SfDataGrid.XForms.GridDoubleTappedEventArgs e)
        {   
            CustodySearchResultViewModel viewModel = (CustodySearchResultViewModel)this.BindingContext;

            if (e.RowData != null)
            {
                viewModel.DoubleClickCommand.Execute(null);
            }
        }
    }
}