﻿using ReceivingManagementSystem.Android.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Views.Android.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendSettingPage : ContentPage
    {
        public SendSettingPage()
        {
            InitializeComponent();

            this.BindingContext = new SendSettingViewModel(this);
        }
    }
}