using ReceivingManagementSystem.ViewModels.Orders;

namespace ReceivingManagementSystem.Orders
{
    public partial class OrderReceiptPopup
    {
        public OrderReceiptPopup(OrderReceiptConfirmViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        } 

		protected override string GetLightDismissResult() => "Light Dismiss";

        private void btnMultiRegistraton_Clicked(object sender, System.EventArgs e)
        {
            Dismiss("1");
        }

        private void btnRegister_Clicked(object sender, System.EventArgs e)
        {
            Dismiss("0");

        }
    }
}