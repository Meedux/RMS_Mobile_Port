using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Warehousing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.Return
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnConfirmPage : ContentPage
    {
        public ReturnConfirmPage(CustodyItemViewModel itemViewModel)
        {
            InitializeComponent();

            this.BindingContext = new ReturnConfirmViewModel(itemViewModel, this);
        }
    }
}