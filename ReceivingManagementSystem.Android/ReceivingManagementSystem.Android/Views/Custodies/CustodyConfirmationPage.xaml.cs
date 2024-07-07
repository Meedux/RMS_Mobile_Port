﻿using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Warehousing;
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
    public partial class CustodyConfirmationPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;

        public CustodyConfirmationPage(CustodyItemViewModel itemViewModel)
        {
            InitializeComponent();

            this.BindingContext = new CustodyConfirmationViewModel(itemViewModel, this);
        }

        protected override void OnDisappearing()
        {
            OnCloseOk.Raise(this, new EventArgs());

            base.OnDisappearing();
        }
    }
}