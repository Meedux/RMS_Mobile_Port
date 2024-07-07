using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using static PleasanterOperation.OperationData;
using static RMS_Pleasanter.Client;
using static RMS_Pleasanter.Contents;

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class CustomerSearchViewModel : BasePopupViewModel
    {
        #region Properties

        private CustomerSearchParamViewModel _customerSearch;
        public CustomerSearchParamViewModel CustomerSearch
        {
            get { return _customerSearch; }
            set { this.SetProperty(ref this._customerSearch, value); }
        }

        #endregion

        #region Command

        public ICommand SearchCommand { get; }

        #endregion

        public CustomerSearchViewModel() : base()
        {
            SearchCommand = new Command(Search);

            CustomerSearch = new CustomerSearchParamViewModel();
        }

        /// <summary>
        /// Search customer
        /// </summary>
        private void Search()
        {
            this.Owner.Dismiss(_customerSearch);
        }
    }
}
