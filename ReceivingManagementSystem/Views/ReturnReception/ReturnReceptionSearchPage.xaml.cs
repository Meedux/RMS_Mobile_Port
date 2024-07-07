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
    public partial class ReturnReceptionSearchPage : ContentPage
    {
        public ReturnReceptionSearchPage()
        {
            InitializeComponent();
            this.BindingContext = new ReturnReceptionSearchViewModel(this);
        }
    }
}