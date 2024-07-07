using ReceivingManagementSystem.Android.ViewModels.Orders;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;

namespace ReceivingManagementSystem.Android.Orders
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderReceiptPage : ContentPage
    {
        public OrderReceiptPage()
        {
            InitializeComponent();

            this.BindingContext = new OrderReceiptViewModel(this);
        }

        protected override void OnAppearing()
        {
            OrderReceiptViewModel viewModel = (OrderReceiptViewModel)this.BindingContext;

            viewModel.RfidInit();
        }

        protected override void OnDisappearing()
        {
            OrderReceiptViewModel viewModel = (OrderReceiptViewModel)this.BindingContext;

            viewModel.RfidStop();
        }

        private void TelephoneNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                bool numberChecked = true;
                switch(e.NewTextValue.Length)
                {
                    case 1:
                        if (e.NewTextValue[0] != '0')
                        {
                            numberChecked = false;
                        }
                        break;
                    case 2:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 3:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{2}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 4:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{3}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 5:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{4}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{3}-") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-\d") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d{2}"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 6:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{5}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{3}-\d") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-\d{2}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d{3}"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 7:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{6}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{3}-\d{2}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-\d{3}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d{4}"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 8:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{7}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{3}-\d{2}-") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-\d{3}-") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d{4}-") &&
                            !Regex.IsMatch(e.NewTextValue, @"(090|080|070)-\d{4}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0120-\d{3}"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 9:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{8}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{3}-\d{2}-\d") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-\d{3}-\d") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d{4}-\d") &&
                            !Regex.IsMatch(e.NewTextValue, @"(090|080|070)-\d{4}-") &&
                            !Regex.IsMatch(e.NewTextValue, @"0120-\d{3}-"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 10:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{9}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{3}-\d{2}-\d{2}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-\d{3}-\d{2}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d{4}-\d{2}") &&
                            !Regex.IsMatch(e.NewTextValue, @"(090|080|070)-\d{4}-\d") &&
                            !Regex.IsMatch(e.NewTextValue, @"0120-\d{3}-\d"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 11:
                        if (!Regex.IsMatch(e.NewTextValue, @"(090|080|070)\d{8}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{3}-\d{2}-\d{3}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-\d{3}-\d{3}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d{4}-\d{3}") &&
                            !Regex.IsMatch(e.NewTextValue, @"(090|080|070)-\d{4}-\d{2}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0120-\d{3}-\d{2}"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 12:
                        if (!Regex.IsMatch(e.NewTextValue, @"0\d{3}-\d{2}-\d{4}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d{2}-\d{3}-\d{4}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0\d-\d{4}-\d{4}") &&
                            !Regex.IsMatch(e.NewTextValue, @"(090|080|070)-\d{4}-\d{3}") &&
                            !Regex.IsMatch(e.NewTextValue, @"0120-\d{3}-\d{3}"))
                        {
                            numberChecked = false;
                        }
                        break;
                    case 13:
                        if (!Regex.IsMatch(e.NewTextValue, @"(090|080|070)-\d{4}-\d{4}"))
                        {
                            numberChecked = false;
                        }
                        break;
                    default:
                        numberChecked = false;
                        break;
                }
                if (!numberChecked)
                {
                    ((OrderReceiptViewModel)this.BindingContext).OrderInfo.TelephoneNumber = e.OldTextValue;
                }
            }
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                char key = e.NewTextValue?.Last() ?? ' ';

                Log.Information(e.NewTextValue);

                if (e.NewTextValue.EndsWith("\r\n"))
                {
                    Log.Information("RP902");
                }
            }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            txtRfid.Focus();
            OrderReceiptViewModel viewModel = (OrderReceiptViewModel)this.BindingContext;

            viewModel.ReadRFIDCommand.Execute(null);
        }
    }
}