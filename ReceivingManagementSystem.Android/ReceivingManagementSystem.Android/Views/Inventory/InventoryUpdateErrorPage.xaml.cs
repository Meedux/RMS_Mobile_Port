using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Inventory;
using ReceivingManagementSystem.Android.ViewModels.Warehousing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Inventory
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryUpdateErrorPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;
        public InventoryUpdateErrorPage(InventoryCustodyItemViewModel itemViewModel, string shelfNumber)
        {
            InitializeComponent();

            this.BindingContext = new InventoryUpdateErrorViewModel(itemViewModel, shelfNumber, this);
        }

        protected override void OnDisappearing()
        {
            InventoryUpdateErrorViewModel viewModel = (InventoryUpdateErrorViewModel)this.BindingContext;

            OnCloseOk.Raise(this, new EventArgs());

            base.OnDisappearing();
        }
    }
}