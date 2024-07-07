using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.ReturnReception
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnReceptionPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;
        public ReturnReceptionPage(CustodyItemViewModel itemViewModel)
        {
            InitializeComponent();
            this.BindingContext = new ReturnReceptionViewModel(itemViewModel, this);
        }

        protected override void OnDisappearing()
        {
            OnCloseOk.Raise(this, new EventArgs());

            base.OnDisappearing();
        }
    }
}