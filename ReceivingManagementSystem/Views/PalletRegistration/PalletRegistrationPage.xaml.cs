﻿using ReceivingManagementSystem.ViewModels;
using ReceivingManagementSystem.ViewModels.PalletRegistration;
using ReceivingManagementSystem.ViewModels.Warehousing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ReceivingManagementSystem.Views.PalletRegistration
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PalletRegistrationPage : ContentPage
	{
		public PalletRegistrationPage ()
		{
			InitializeComponent ();

			this.BindingContext = new PalletRegistrationViewModel(this);
        }

        private void Item_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker pickerItem = (Picker)sender;

            PalletRegistrationViewModel viewModel = (PalletRegistrationViewModel)this.BindingContext;
            viewModel.ItemSelectedChanged(pickerItem.SelectedItem);
        }
    }
}