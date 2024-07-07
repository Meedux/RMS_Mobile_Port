using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
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

namespace ReceivingManagementSystem.ViewModels.Orders
{
    public class CustomerSearchResultViewModel : BasePopupViewModel
    {
        #region Properties


        private List<CustomerSearchParamViewModel> _clients;

        public List<CustomerSearchParamViewModel> Clients
        {
            get { return _clients; }
            set { this.SetProperty(ref this._clients, value); }
        }

        private CustomerSearchParamViewModel _clientSelected;

        public CustomerSearchParamViewModel ClientSelected
        {
            get { return _clientSelected; }
            set { this.SetProperty(ref this._clientSelected, value); }
        }


        private bool _isRefreshing;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { this.SetProperty(ref this._isRefreshing, value); }
        }

        #endregion

        #region Command


        #endregion


        public CustomerSearchResultViewModel(List<CustomerSearchParamViewModel> clients) : base()
        {
            Clients = clients;
        }
    }
}
