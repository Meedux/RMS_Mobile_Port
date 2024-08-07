﻿using ReceivingManagementSystem.Common;
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
    public partial class ReturnReceptionConfirmPage : ContentPage
    {
        public event EventHandler<EventArgs> OnCloseOk;
        public ReturnReceptionConfirmPage(CustodyItemViewModel item)
        {
            InitializeComponent();
            this.BindingContext = new ReturnReceptionConfirmViewModel(item, this);
        }

        protected override void OnDisappearing()
        {
            OnCloseOk.Raise(this, new EventArgs());

            base.OnDisappearing();
        }
    }
}