using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Custodies;
using ReceivingManagementSystem.Orders;
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

namespace ReceivingManagementSystem.ViewModels.Custodies
{
    public class CustodySearchResultViewModel : BaseViewModel
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

        public CustodySearchResultViewModel(List<CustodyItemViewModel> custodies, ContentPage owner) : base(owner)
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

            CustodyPage custodyPage = new CustodyPage(_custodySelected);
            custodyPage.OnCloseOk += CustodyPage_Disappearing;
            await this.Owner.Navigation.PushAsync(custodyPage, true);
        }

        private void CustodyPage_Disappearing(object sender, EventArgs e)
        {
            CustodyPage custodyPage = (CustodyPage)sender;

            var viewModel = (CustodyViewModel)custodyPage.BindingContext;

            if (viewModel.IsOK)
            {
                Custodies.Remove(_custodySelected);
                CustodySelected = null;
            }
        }
    }
}
