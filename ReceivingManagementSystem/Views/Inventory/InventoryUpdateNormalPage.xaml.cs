using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.ViewModels.Delivery;
using ReceivingManagementSystem.ViewModels.Inventory;
using ReceivingManagementSystem.ViewModels.Orders;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Inventory
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventoryUpdateNormalPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;

        public InventoryUpdateNormalPage(InventoryCustodyItemViewModel item, bool Inventory = true)
        {
            InitializeComponent();

            this.BindingContext = new InventoryUpdateNormalViewModel(item, Inventory, this);
        }

        protected override void OnDisappearing()
        {
            OnCloseOk.Raise(this, new EventArgs());

            base.OnDisappearing();
        }
    }
}