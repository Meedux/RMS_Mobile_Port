using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Custodies;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.ReturnReception;
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

namespace ReceivingManagementSystem.Android.ViewModels.Custodies
{
    public class ReturnReceptionSearchResultViewModel : BaseViewModel
    {
        #region Properties

        private ObservableCollection<CustodyItemViewModel> _custodies;

        public ObservableCollection<CustodyItemViewModel> Custodies
        {
            get { return _custodies; }
            set { this.SetProperty(ref this._custodies, value); }
        }

        private CustodyItemViewModel _custodySelected;

        public CustodyItemViewModel CustodySelected
        {
            get { return _custodySelected; }
            set { this.SetProperty(ref this._custodySelected, value); }
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


        public ReturnReceptionSearchResultViewModel(List<CustodyItemViewModel> custodies, ContentPage owner) : base(owner)
        {
            Custodies = new ObservableCollection<CustodyItemViewModel>(custodies);

            OkCommand = new Command(Ok);
            DoubleClickCommand = new Command(Ok);
        }

        private async void Ok()
        {
            if(_custodySelected ==null)
            {
                return;
            }

            ReturnReceptionPage returnReceptionPage = new ReturnReceptionPage(_custodySelected);
            returnReceptionPage.OnCloseOk += ReturnReceptionPage_Disappearing;
            await this.Owner.Navigation.PushAsync(returnReceptionPage, true);
        }

        private void ReturnReceptionPage_Disappearing(object sender, EventArgs e)
        {
            ReturnReceptionPage returnReceptionPage = (ReturnReceptionPage)sender;

            var viewModel = (ReturnReceptionViewModel)returnReceptionPage.BindingContext;

            if (viewModel.IsOK)
            {
                _custodies.Remove(_custodySelected);
                CustodySelected = null;
            }
        }
    }
}
