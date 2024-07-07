using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Custodies;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Pleasanter;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using static PleasanterOperation.OperationData;
using static RMS_Pleasanter.Contents;
using static RMS_Pleasanter.Custody;

namespace ReceivingManagementSystem.ViewModels.Custodies
{
    public class CustodySearchViewModel : BaseViewModel
    {
        #region Properties

        private CustodySearchParamViewModel _searchParams;
        public CustodySearchParamViewModel SearchParams
        {
            get { return _searchParams; }
            set { this.SetProperty(ref this._searchParams, value); }
        }

        /// <summary>
        /// 棚番号
        /// </summary>
        private List<ComboBoxItemViewModel> _contentItems;

        /// <summary>
        /// 棚番号
        /// </summary>
        public List<ComboBoxItemViewModel> ContentItems
        {
            get { return _contentItems; }
            set { this.SetProperty(ref this._contentItems, value); }
        }

        /// <summary>
        /// 棚番号 selected
        /// </summary>
        private ComboBoxItemViewModel _contentSelected;

        /// <summary>
        /// 棚番号 selected
        /// </summary>
        public ComboBoxItemViewModel ContentSelected
        {
            get { return _contentSelected; }
            set
            {
                SearchParams.Contents = value != null ? value.Value.ToString() : null;
                this.SetProperty(ref this._contentSelected, value);
            }
        }

        private bool _isPickerOpen;
        public bool IsPickerOpen
        {
            get { return _isPickerOpen; }
            set { this.SetProperty(ref this._isPickerOpen, value); }
        }

        private List<CustodyItemViewModel> _custodies;
        #endregion

        #region Command

        public ICommand SearchCommand { get; }
        public ICommand SetToDayCommand { get; }
        public ICommand OpenPickerCommand { get; }
        public ICommand OkSelectDateCommand { get; }

        #endregion

        private IPleasanterService _pleasanterService;

        public CustodySearchViewModel(ContentPage owner) : base(owner)
        {
            SearchCommand = new Command(CustodySearch);
            SetToDayCommand = new Command(SetToDay);
            OpenPickerCommand = new Command(OpenPicker);
            OkSelectDateCommand = new Command(OkSelectDate);

            _pleasanterService = DependencyService.Get<IPleasanterService>();

            _searchParams = new CustodySearchParamViewModel();

            GetContents();
        }

        /// <summary>
        /// Search customer
        /// </summary>
        private async void CustodySearch()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.Custody))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0008);
                return;
            }

            await Search();

            if (_custodies.Count == 1)
            {
                CustodyPage custodyPage = new CustodyPage(_custodies[0]);
                await this.Owner.Navigation.PushAsync(custodyPage, true);
                return;
            }

            CustodySearchResultPage custodySearchResultPage = new CustodySearchResultPage(_custodies);
            await this.Owner.Navigation.PushAsync(custodySearchResultPage, true);
        }

        private async Task Search()
        {
            var items = await _pleasanterService.GetCustody(_searchParams.GetCustodyBodySearch(), _searchParams.GetCustodyDetailBodySearch());

            _custodies = items.Select(s => new CustodyItemViewModel(s)).ToList();
        }

        /// <summary>
        /// Set 預り予定日 = ToDay
        /// </summary>
        private void SetToDay()
        {
            SearchParams.SelectDate = DateTime.Now;
            SearchParams.InputDate = SearchParams.SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }

        private async void GetContents()
        {
            var items = await _pleasanterService.GetContents();

            ContentItems = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = s.contents,
                Value = s.contents
            }).ToList();
        }

        private void OpenPicker()
        {
            IsPickerOpen = true;
        }

        private void OkSelectDate()
        {
            SearchParams.InputDate = SearchParams.SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }
    }
}
