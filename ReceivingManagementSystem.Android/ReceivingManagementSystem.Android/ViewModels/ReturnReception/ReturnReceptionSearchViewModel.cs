using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Custodies;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.ReturnReception;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Interfaces;
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

namespace ReceivingManagementSystem.Android.ViewModels.Custodies
{
    public class ReturnReceptionSearchViewModel : BaseViewModel
    {
        #region Properties

        private ReturnReceptionSearchParamViewModel _searchParams;
        public ReturnReceptionSearchParamViewModel SearchParams
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
        public ICommand ImportCSVCommand { get; }
        public ICommand OpenPickerCommand { get; }
        public ICommand OkSelectDateCommand { get; }

        #endregion

        private IPleasanterService _pleasanterService;
        private IFileWrapper _fileWrapper;

        public ReturnReceptionSearchViewModel(ContentPage owner) : base(owner)
        {
            SearchCommand = new Command(CustodySearch);
            ImportCSVCommand = new Command(ChooseFile);
            OpenPickerCommand = new Command(OpenPicker);
            OkSelectDateCommand = new Command(OkSelectDate);

            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _fileWrapper = DependencyService.Get<IFileWrapper>();

            _searchParams = new ReturnReceptionSearchParamViewModel();

            GetContents();
        }

        /// <summary>
        /// Search customer
        /// </summary>
        private async void CustodySearch()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.Custody, PleasanterObjectTypeEnum.CustodyDetail))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0008);
                return;
            }

            await Search();

            if (_custodies.Count == 1)
            {
                ReturnReceptionPage custodyPage = new ReturnReceptionPage(_custodies[0]);
                await this.Owner.Navigation.PushAsync(custodyPage, true);
                return;
            }
           
            ReturnReceptionSearchResultPage returnReceptionSearchResultPage = new ReturnReceptionSearchResultPage(_custodies);
            await this.Owner.Navigation.PushAsync(returnReceptionSearchResultPage, true);
        }

        private async Task Search()
        {
            var items = await _pleasanterService.GetCustody(_searchParams.GetCustodyBodySearch(), _searchParams.GetCustodyDetailBodySearch());

            _custodies = items.Select(s => new CustodyItemViewModel(s)).ToList();
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

        private async void ChooseFile()
        {
            try
            {
                var result = await _fileWrapper.ChooseFile(".csv");
                if (result == null)
                {
                    return;
                }

                var records = await ReadFileCSV(result.FileStream);

                if (records == null)
                {
                    return;
                }

                foreach (var item in records)
                {
                    // TODO
                }
            }
            catch (Exception /* ex */)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.Cancel,
                    MessageResourceKey.E0005, string.Empty);
            }
        }

        private async Task<List<CustodyBody>> ReadFileCSV(Stream fileName)
        {
            var records = new List<CustodyBody>();
            try
            {

                using (var reader = new StreamReader(fileName, Encoding.GetEncoding(932)))
                using (var csv = new CsvReader(reader, new CultureInfo("ja-JP")))
                {

                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var record = new CustodyBody
                        {
                            //date = csv.GetField<DateTime>("受注日"),
                            //address = $"{csv.GetField<string>("都道府県(名称)")} {csv.GetField<string>("住所1")} {csv.GetField<string>("住所2")}",
                            //companyName = csv.GetField<string>("会社名"),
                            //customerName = $"{csv.GetField<string>("お名前(姓)")} {csv.GetField<string>("お名前(名)")}",
                            //postCode = csv.GetField<string>("郵便番号"),
                            //telephoneNumber = csv.GetField<string>("TEL"),
                            //custodyDate = csv.GetField<DateTime>("受注日"),
                            //code = csv.GetField<string>("商品ID")
                        };

                        records.Add(record);
                    }
                }
            }
            catch (Exception /* ex */)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.Cancel,
                     MessageResourceKey.E0005, string.Empty);

                return null;
            }

            return records;
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
