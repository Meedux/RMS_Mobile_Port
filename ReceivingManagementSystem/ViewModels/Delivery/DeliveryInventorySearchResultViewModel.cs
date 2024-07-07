using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Custodies;
using ReceivingManagementSystem.Delivery;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.ViewModels.Delivery;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ReceivingManagementSystem.ViewModels.Delivery
{
    public class DeliveryInventorySearchResultViewModel : BaseViewModel
    {
        #region Properties

        private ObservableCollection<CustodyItemViewModel> _items;

        public ObservableCollection<CustodyItemViewModel> Items
        {
            get { return _items; }
            set { this.SetProperty(ref this._items, value); }
        }

        private CustodyItemViewModel _itemSelected;

        public CustodyItemViewModel ItemSelected
        {
            get { return _itemSelected; }
            set { this.SetProperty(ref this._itemSelected, value); }
        }

        private bool _isRefreshing;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { this.SetProperty(ref this._isRefreshing, value); }
        }

        #endregion

        #region Command

        public ICommand OkCommand { get; set; }
        public ICommand DoubleClickCommand { get; set; }
        #endregion

        public DeliveryInventorySearchResultViewModel(List<CustodyItemViewModel> items, ContentPage owner) : base(owner)
        {
            Items = new ObservableCollection<CustodyItemViewModel>(items);

            OkCommand = new Command(Ok);
            DoubleClickCommand = new Command(Ok);
        }

        private async void Ok()
        {
            if (_itemSelected == null)
            {
                return;
            }

            DeliveryPage deliveryPage = new DeliveryPage(_itemSelected);
            deliveryPage.OnCloseOk += DeliveryPage_Disappearing;
            await this.Owner.Navigation.PushAsync(deliveryPage, true);
        }

        private void DeliveryPage_Disappearing(object sender, EventArgs e)
        {
            DeliveryPage deliveryPage = (DeliveryPage)sender;

            var viewModel = (DeliveryViewModel)deliveryPage.BindingContext;

            if (viewModel.IsOK)
            {
                Items.Remove(_itemSelected);
                ItemSelected = null;
            }
        }
    }
}
