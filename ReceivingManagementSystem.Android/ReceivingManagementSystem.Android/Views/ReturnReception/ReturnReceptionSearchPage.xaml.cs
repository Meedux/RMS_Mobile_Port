using ReceivingManagementSystem.Android.ViewModels.Custodies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Android.ReturnReception
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReturnReceptionSearchPage : ContentPage
    {
        public ReturnReceptionSearchPage()
        {
            InitializeComponent();

            try{
                this.BindingContext = new ReturnReceptionSearchViewModel(this); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}