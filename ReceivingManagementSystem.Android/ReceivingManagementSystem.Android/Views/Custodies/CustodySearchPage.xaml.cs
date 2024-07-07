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
    public partial class CustodySearchPage : ContentPage
    {
        public CustodySearchPage()
        {
            InitializeComponent();
            this.BindingContext = new CustodySearchViewModel(this);
        }
    }
}