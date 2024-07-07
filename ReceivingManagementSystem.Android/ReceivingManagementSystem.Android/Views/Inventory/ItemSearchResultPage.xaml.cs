using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Inventory
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemSearchResultPage : ContentPage
    {
        public ItemSearchResultPage()
        {

            InitializeComponent();
            this.BindingContext = new ItemSearchResultViewModel(this);
        }
    }
}