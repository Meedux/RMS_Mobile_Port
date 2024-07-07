using ReceivingManagementSystem.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundSettingPage : ContentPage
    {
        public SoundSettingPage()
        {
            InitializeComponent();

            this.BindingContext = new SoundSettingViewModel(this);
        }
    }
}