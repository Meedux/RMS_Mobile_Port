using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Custodies;
using ReceivingManagementSystem.Inventory;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.ReturnReception;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.ViewModels.Custodies;
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
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.ViewModels.Inventory
{
    public class SearchViewModel : BaseViewModel
    {
        #region Properties

        private SearchParamViewModel _searchParams;
        public SearchParamViewModel SearchParams
        {
            get { return _searchParams; }
            set { this.SetProperty(ref this._searchParams, value); }
        }

        /// <summary>
        /// 内容
        /// </summary>
        private List<ComboBoxItemViewModel> _contentItems;

        /// <summary>
        /// 内容
        /// </summary>
        public List<ComboBoxItemViewModel> ContentItems
        {
            get { return _contentItems; }
            set { this.SetProperty(ref this._contentItems, value); }
        }

        /// <summary>
        /// 内容 selected
        /// </summary>
        private ComboBoxItemViewModel _contentSelected;

        /// <summary>
        /// 内容 selected
        /// </summary>
        public ComboBoxItemViewModel ContentSelected
        {
            get { return _contentSelected; }
            set
            {
                _searchParams.Contents = value != null ? value.Value.ToString() : string.Empty;
                this.SetProperty(ref this._contentSelected, value);
            }
        }

        private bool _isPickerOpen;
        public bool IsPickerOpen
        {
            get { return _isPickerOpen; }
            set { this.SetProperty(ref this._isPickerOpen, value); }
        }

        /// <summary>
        /// 預り予定日
        /// </summary>
        private DateTime _selectDate;

        /// <summary>
        /// 預り予定日
        /// </summary>
        public DateTime SelectDate
        {
            get { return _selectDate; }
            set
            {
                this.SetProperty(ref this._selectDate, value);
            }
        }

        /// <summary>
        /// 状態リスト
        /// </summary>
        private List<ComboBoxItemViewModel> _statusList;

        /// <summary>
        /// 状態リスト
        /// </summary>
        public List<ComboBoxItemViewModel> StatusList
        {
            get { return _statusList; }
            set { this.SetProperty(ref this._statusList, value); }
        }

        /// <summary>
        /// 状態 selected
        /// </summary>
        private ComboBoxItemViewModel _statusSelected;

        /// <summary>
        /// 状態 selected
        /// </summary>
        public ComboBoxItemViewModel StatusSelected
        {
            get { return _statusSelected; }
            set
            {
                _searchParams.Status = value != null ? value.Value.ToString() : string.Empty;
                this.SetProperty(ref this._statusSelected, value);
            }
        }

        #endregion

        #region Command

        public ICommand SearchCommand { get; }
        public ICommand SetToDayCommand { get; }
        public ICommand OpenPickerCommand { get; }
        public ICommand OkSelectDateCommand { get; }

        #endregion

        private IPleasanterService _pleasanterService;
        private string dateType;
        private List<InventoryCustodyItemViewModel> _custodies;

        public SearchViewModel(ContentPage owner) : base(owner)
        {
            SearchCommand = new Command(CustodySearch);
            OpenPickerCommand = new Command<string>(OpenPicker);
            OkSelectDateCommand = new Command(OkSelectDate);

            _pleasanterService = DependencyService.Get<IPleasanterService>();

            _searchParams = new SearchParamViewModel();

            GetContents();

            GetStatus();

            StatusSelected = StatusList.First(s => { return s.DisplayValue == "収納"; });
        }

        /// <summary>
        /// Search customer
        /// </summary>
        private async void CustodySearch()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.Custody))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0008);
                return;
            }

            List<CustodyItemModel> items = await _pleasanterService.SearchCustodyAndDetail(_searchParams.GetCustodyBodySearch(), _searchParams.GetCustodyDetailBodySearch());
            if (items == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.Cancel, MessageResourceKey.E0002, TextResourceKey.Custody);

                return;
            }
            foreach (var item in items)
            {
                if (item.CustodyDetail.InventoryDate.ToString() == "1899/12/30 0:00:00")
                    item.CustodyDetail.InventoryDate = null;
                if (item.CustodyDetail.returnDate.ToString() == "1899/12/30 0:00:00")
                    item.CustodyDetail.returnDate = null;
            }

            _custodies = items.Select(s => new InventoryCustodyItemViewModel(s)).ToList();
            SearchResultPage inventorySearchResultPage = new SearchResultPage(_custodies);
            await this.Owner.Navigation.PushAsync(inventorySearchResultPage, true);
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

        private void OpenPicker(string dateType)
        {
            DateTime? selectDate = null;
            if (dateType == "1" && !string.IsNullOrEmpty(SearchParams.CustodyDateFrom))
            {
                selectDate = DateHelper.GetDateByInputDate(SearchParams.CustodyDateFrom);
            }
            else if (dateType == "2" && !string.IsNullOrEmpty(SearchParams.CustodyDateTo))
            {
                selectDate = DateHelper.GetDateByInputDate(SearchParams.CustodyDateTo);
            }
            else if (dateType == "3" && !string.IsNullOrEmpty(SearchParams.ReturnDateFrom))
            {
                selectDate = DateHelper.GetDateByInputDate(SearchParams.ReturnDateFrom);
            }
            else if (dateType == "4" && !string.IsNullOrEmpty(SearchParams.ReturnDateTo))
            {
                selectDate = DateHelper.GetDateByInputDate(SearchParams.ReturnDateTo);
            }
            else if (dateType == "5" && !string.IsNullOrEmpty(SearchParams.InventoryDateFrom))
            {
                selectDate = DateHelper.GetDateByInputDate(SearchParams.InventoryDateFrom);
            }
            else if (dateType == "5" && !string.IsNullOrEmpty(SearchParams.InventoryDateTo))
            {
                selectDate = DateHelper.GetDateByInputDate(SearchParams.InventoryDateTo);
            }

            SelectDate = selectDate.HasValue ? selectDate.Value : DateTime.Now;

            this.dateType = dateType;
            IsPickerOpen = true;
        }

        private void OkSelectDate()
        {
            if (dateType == "1")
            {
                SearchParams.CustodyDateFrom = SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
            }
            else if (dateType == "2")
            {
                SearchParams.CustodyDateTo = SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
            }
            else if (dateType == "3")
            {
                SearchParams.ReturnDateFrom = SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
            }
            else if (dateType == "4")
            {
                SearchParams.ReturnDateTo = SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
            }
            else if (dateType == "5")
            {
                SearchParams.InventoryDateFrom = SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
            }
            else if (dateType == "6")
            {
                SearchParams.InventoryDateTo = SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
            }
        }

        /// <summary>
        /// 状態の取得
        /// </summary>
        private void GetStatus()
        {
            StatusList = new List<ComboBoxItemViewModel>();

            var status = CustodyStatusEnum.GetStatus();

            foreach (var item in status)
            {
                StatusList.Add(new ComboBoxItemViewModel()
                {
                    DisplayValue = item,
                    Value = item
                });
            }

        }
    }
}
