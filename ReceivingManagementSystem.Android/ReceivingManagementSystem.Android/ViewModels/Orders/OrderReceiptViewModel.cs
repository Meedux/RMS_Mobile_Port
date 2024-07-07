using CsvHelper;
using PleasanterOperation;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Views;
using ReceivingManagementSystem.Android.Interfaces;
using RMS_Pleasanter;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class OrderReceiptViewModel : BaseViewModel
    {
        #region Properties

        private OrderReceiptInfoViewModel _orderInfo;
        public OrderReceiptInfoViewModel OrderInfo
        {
            get { return _orderInfo; }
            set { this.SetProperty(ref this._orderInfo, value); }
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
                OrderInfo.Contents = value != null ? value.Value.ToString() : null;
                this.SetProperty(ref this._contentSelected, value);
            }
        }

        /// <summary>
        /// 内容 selected
        /// </summary>
        private string _contentText;

        /// <summary>
        /// 内容 selected
        /// </summary>
        public string ContentText
        {
            get { return _contentText; }
            set
            {
                OrderInfo.Contents = value;
                this.SetProperty(ref this._contentText, value);
            }
        }

        /// <summary>
        /// RFID
        /// </summary>
        private bool _isRfid;

        /// <summary>
        /// RFID
        /// </summary>
        public bool IsRfid
        {
            get { return _isRfid; }

            set { this.SetProperty(ref this._isRfid, value); }
        }


        /// <summary>
        /// 複数登録
        /// </summary>
        private bool _isEnabled;

        /// <summary>
        /// 複数登録
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }

            set { this.SetProperty(ref this._isEnabled, value); }
        }

        private bool _isPickerOpen;
        public bool IsPickerOpen
        {
            get { return _isPickerOpen; }
            set { this.SetProperty(ref this._isPickerOpen, value); }
        }

        private List<CustomerSearchParamViewModel> _clients;

        private string _device;
        #endregion

        #region Command

        public ICommand RegisterCommand { get; }
        public ICommand CustomerSearchCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand SetToDayCommand { get; }
        public ICommand ImportCSVCommand { get; }
        public ICommand OpenPickerCommand { get; }
        public ICommand OkSelectDateCommand { get; }
        #endregion

        private IResourceProvider _resourceProvider;
        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private IFileWrapper _fileWrapper;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public OrderReceiptViewModel(ContentPage owner) : base(owner)
        {
            RegisterCommand = new Command(ShowConfirmInfo);
            CustomerSearchCommand = new Command(CustomerSearch);
            ReadRFIDCommand = new Command(ReadRFID);
            SetToDayCommand = new Command(SetToDay);
            ImportCSVCommand = new Command(ChooseFile);
            OpenPickerCommand = new Command(OpenPicker);
            OkSelectDateCommand = new Command(OkSelectDate);

            _resourceProvider = DependencyService.Get<IResourceProvider>();
            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _fileWrapper = DependencyService.Get<IFileWrapper>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            OrderInfo = new OrderReceiptInfoViewModel();

            GetContents();

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");
            IsEnabled = true;

            Log.Information("Start order");
        }

        public void RfidInit()
        {
            _readRfidService.OnReadRfid += OnReadRfid;
            _readRfidService.OnInit();
        }

        public void RfidStop()
        {
            _readRfidService.OnReadRfid -= OnReadRfid;
            _readRfidService.Stop();
        }

        /// <summary>
        /// ShowConfirm Order
        /// </summary>
        private async void ShowConfirmInfo()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.Custody, PleasanterObjectTypeEnum.CustodyDetail))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0008);
                return;
            }

            bool isValidate = await Validate();

            if (!isValidate)
            {
                return;
            }

            if (!_isEnabled)
            {
                await RegisterDetail();

                ClearDataMultiRegister();

                return;
            }

            if (!string.IsNullOrEmpty(_orderInfo.TelephoneNumber) && ulong.TryParse(_orderInfo.TelephoneNumber, out _))
            {
                _orderInfo.TelephoneNumber = FormatPhoneNumber(_orderInfo.TelephoneNumber);
            }

            OrderReceiptPopup orderReceiptPopup = new OrderReceiptPopup(new OrderReceiptConfirmViewModel(_orderInfo));
            string result = await this.Owner.Navigation.ShowPopupAsync(orderReceiptPopup);

            var insertResult = await Register();

            if (!insertResult)
            {
                return;
            }

            if ("1".Equals(result))
            {
                ClearDataMultiRegister();
                IsEnabled = false;
            }
            else if ("0".Equals(result))
            {
                Close();
            }
        }

        private void ClearDataMultiRegister()
        {
            ContentSelected = null;
            ContentText = string.Empty;
            _orderInfo.Contents = null;
            _orderInfo.SelectDate = DateTime.Now;
            _orderInfo.InputDate = String.Empty;
            _orderInfo.Rfid = null;
            _orderInfo.DetailNumber++;
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(_orderInfo.CustomerName))
            {
                errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Content));
            }

            if (string.IsNullOrEmpty(_orderInfo.PostCode))
            {
                errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.PostCode));
            }

            if (string.IsNullOrEmpty(_orderInfo.Address))
            {
                errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Address));
            }

            if (string.IsNullOrEmpty(_orderInfo.TelephoneNumber))
            {
                errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.TelephoneNumber));
            }
            else if (!ValidateHelper.CheckPhoneNumber(_orderInfo.TelephoneNumber))
            {
                errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0009, TextResourceKey.TelephoneNumber, "電話番号"));
            }

            if (!DateHelper.CheckInputDate(_orderInfo.InputDate))
            {
                errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.CustodyDate));
            }

            if (string.IsNullOrEmpty(_contentText))
            {
                errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Content));
            }

            if (!string.IsNullOrEmpty(_orderInfo.Rfid))
            {
                if (_orderInfo.Rfid.Any(char.IsLower))
                {
                    errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0010));
                }

                var items = await _pleasanterService.GetCustodyDetail(new CustodyDetailBody()
                {
                    rfid = _orderInfo.Rfid
                });

                if (items.Count > 0)
                {
                    errors.Add(_resourceProvider.GetMesResource(MessageResourceKey.E0011, TextResourceKey.Rfid));
                }
            }

            if (errors.Count > 0)
            {
                string action = await Owner.DisplayActionSheet(_resourceProvider.GetResourceByName(TextResourceKey.NotificationTitle),
                   _resourceProvider.GetResourceByName(TextResourceKey.Cancel), null, errors.ToArray());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Register order
        /// </summary>
        private async Task<bool> Register()
        {
            decimal? custodyId = await _pleasanterService.CreateCustody(_orderInfo.GetCustodyBody());

            if (!custodyId.HasValue)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Order);

                return false;
            }

            var custody = await _pleasanterService.GetCustodyById(custodyId.Value);
            if (custody == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Order);

                return false;
            }

            _orderInfo.Id = custody.id;
            _orderInfo.Code = custody.code;

            return await RegisterDetail();
        }

        /// <summary>
        /// Gen code on Day
        /// </summary>
        /// <returns></returns>
        private async Task<string> GenCode()
        {
            List<SearchInfoModel> searchInfos = new List<SearchInfoModel>();
            SearchInfoModel searchInfoModel = new SearchInfoModel
            {
                FieldName = "custodyDate",
                ValueType = SearchInfoModel.ValueTypeEnum.String,
                SearchType = SearchInfoModel.SearchTypeEnum.FromTo
            };
            searchInfoModel.Values.Add(DateTime.Now.ToSartDateYYYYMMDD());
            searchInfoModel.Values.Add(DateTime.Now.ToEndDateYYYYMMDD());
            searchInfos.Add(searchInfoModel);

            var custodys = await _pleasanterService.SearchCustody(searchInfos);

            int index = 0;
            if (custodys.Count == 0)
            {
                index = 1;
            }
            else
            {
                string codeMax = custodys.Max(m => m.code);

                if (codeMax.Length > 4)
                {
                    try
                    {
                        string indexMax = codeMax.Substring(codeMax.Length - 4, 4);

                        index = int.Parse(indexMax) + 1;
                    }
                    catch
                    {
                        index = 1;
                    }
                }
                else
                {
                    index = 1;
                }
            }

            return $"{DateTime.Now.ToString("yyyyMMdd")}{string.Format("{0:0000}", index)}";
        }

        /// <summary>
        /// Register order detail
        /// </summary>
        private async Task<bool> RegisterDetail()
        {
            decimal? custodyDetailId = await _pleasanterService.CreateCustodyDetail(_orderInfo.GetCustodyDetailBody());
            if (!custodyDetailId.HasValue)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Order);

                await _pleasanterService.CreateCustody(_orderInfo.GetCustodyBody());
                return false;
            }

            ShowMessage(MessageResourceKey.E0004, TextResourceKey.Order);
            return true;
        }

        /// <summary>
        /// Search customer
        /// </summary>
        private async void CustomerSearch()
        {
            CustomerSearchPopup orderReceiptPopup = new CustomerSearchPopup();
            object result = await this.Owner.Navigation.ShowPopupAsync(orderReceiptPopup);
            if (result == null)
            {
                return;
            }

            await Search((CustomerSearchParamViewModel)result);

            if (_clients == null && _clients.Count == 0)
            {
                return;
            }
            if (_clients.Count == 1)
            {
                SetClientInfo(_clients[0]);
            }

            CustomerSearchResultPopup customerSearchResultPopup = new CustomerSearchResultPopup(_clients);
            object clientSelected = await this.Owner.Navigation.ShowPopupAsync(customerSearchResultPopup);

            if (clientSelected != null)
            {
                SetClientInfo((CustomerSearchParamViewModel)clientSelected);
            }
        }

        private void SetClientInfo(CustomerSearchParamViewModel client)
        {
            OrderInfo.CustomerName = client.CustomerName;
            OrderInfo.TelephoneNumber = client.TelephoneNumber;
            OrderInfo.Address = client.Address;
            OrderInfo.CompanyName = client.CompanyName;
            OrderInfo.PostCode = client.PostCode;
        }

        private async Task Search(CustomerSearchParamViewModel searchParams)
        {
            var items = await _pleasanterService.GetClients(searchParams.GetClientBody());

            _clients = items.Select(s => new CustomerSearchParamViewModel(s)).ToList();
        }

        /// <summary>
        /// Read RFID
        /// </summary>
        private void ReadRFID()
        {
            if ("SP1".Equals(_device))
            {
                _readRfidService.ReadRfid();
            }
            else
            {
                OrderInfo.Rfid = string.Empty;
            }
        }

        /// <summary>
        /// Set 預り予定日 = ToDay
        /// </summary>
        private void SetToDay()
        {
            OrderInfo.SelectDate = DateTime.Now;
            OrderInfo.InputDate = OrderInfo.SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }

        private async void GetContents()
        {
            //await _pleasanterService.InitData();

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

                    Decimal? custodyId = await _pleasanterService.CreateCustody(item);
                    if (!custodyId.HasValue)
                    {
                        await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                             MessageResourceKey.E0003, TextResourceKey.Order);

                        break;
                    }

                    Decimal? custodyDetailId = await _pleasanterService.CreateCustodyDetail(new CustodyDetailBody
                    {
                        code = item.code,
                        status = CustodyStatusEnum.Order_Received.Value
                    });

                    if (!custodyDetailId.HasValue)
                    {
                        await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                             MessageResourceKey.E0003, TextResourceKey.Order);

                        break;
                    }
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
                            date = csv.GetField<DateTime>("受注日"),
                            address = $"{csv.GetField<string>("都道府県(名称)")} {csv.GetField<string>("住所1")} {csv.GetField<string>("住所2")}",
                            companyName = csv.GetField<string>("会社名"),
                            customerName = $"{csv.GetField<string>("お名前(姓)")} {csv.GetField<string>("お名前(名)")}",
                            postCode = csv.GetField<string>("郵便番号"),
                            telephoneNumber = csv.GetField<string>("TEL"),
                            custodyDate = csv.GetField<DateTime>("受注日"),
                            code = csv.GetField<string>("商品ID")
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

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            OrderInfo.Rfid = args.Rfid;
        }

        private void OpenPicker()
        {
            IsPickerOpen = true;
        }

        private void OkSelectDate()
        {
            OrderInfo.InputDate = OrderInfo.SelectDate.ToString(DateHelper.Date_Format_YYYYMMDD);
        }

        public static string FormatPhoneNumber(string input)
        {
            return FormatPhoneNumber(input, false);
        }

        public static string FormatPhoneNumber(string input, bool strict)
        {
            Dictionary<int, Dictionary<string, int>> groups = new Dictionary<int, Dictionary<string, int>>();
            {
                Dictionary<string, int> group5 = new Dictionary<string, int>();
                group5.Add("01564", 1);
                group5.Add("01558", 1);
                group5.Add("01586", 1);
                group5.Add("01587", 1);
                group5.Add("01634", 1);
                group5.Add("01632", 1);
                group5.Add("01547", 1);
                group5.Add("05769", 1);
                group5.Add("04992", 1);
                group5.Add("04994", 1);
                group5.Add("01456", 1);
                group5.Add("01457", 1);
                group5.Add("01466", 1);
                group5.Add("01635", 1);
                group5.Add("09496", 1);
                group5.Add("08477", 1);
                group5.Add("08512", 1);
                group5.Add("08396", 1);
                group5.Add("08388", 1);
                group5.Add("08387", 1);
                group5.Add("08514", 1);
                group5.Add("07468", 1);
                group5.Add("01655", 1);
                group5.Add("01648", 1);
                group5.Add("01656", 1);
                group5.Add("01658", 1);
                group5.Add("05979", 1);
                group5.Add("04996", 1);
                group5.Add("01654", 1);
                group5.Add("01372", 1);
                group5.Add("01374", 1);
                group5.Add("09969", 1);
                group5.Add("09802", 1);
                group5.Add("09912", 1);
                group5.Add("09913", 1);
                group5.Add("01398", 1);
                group5.Add("01377", 1);
                group5.Add("01267", 1);
                group5.Add("04998", 1);
                group5.Add("01397", 1);
                group5.Add("01392", 1);
                groups.Add(5, group5);
                Dictionary<string, int> group4 = new Dictionary<string, int>();
                group4.Add("0768", 2);
                group4.Add("0770", 2);
                group4.Add("0772", 2);
                group4.Add("0774", 2);
                group4.Add("0773", 2);
                group4.Add("0767", 2);
                group4.Add("0771", 2);
                group4.Add("0765", 2);
                group4.Add("0748", 2);
                group4.Add("0747", 2);
                group4.Add("0746", 2);
                group4.Add("0826", 2);
                group4.Add("0749", 2);
                group4.Add("0776", 2);
                group4.Add("0763", 2);
                group4.Add("0761", 2);
                group4.Add("0766", 2);
                group4.Add("0778", 2);
                group4.Add("0824", 2);
                group4.Add("0797", 2);
                group4.Add("0796", 2);
                group4.Add("0555", 2);
                group4.Add("0823", 2);
                group4.Add("0798", 2);
                group4.Add("0554", 2);
                group4.Add("0820", 2);
                group4.Add("0795", 2);
                group4.Add("0556", 2);
                group4.Add("0791", 2);
                group4.Add("0790", 2);
                group4.Add("0779", 2);
                group4.Add("0558", 2);
                group4.Add("0745", 2);
                group4.Add("0794", 2);
                group4.Add("0557", 2);
                group4.Add("0799", 2);
                group4.Add("0738", 2);
                group4.Add("0567", 2);
                group4.Add("0568", 2);
                group4.Add("0585", 2);
                group4.Add("0586", 2);
                group4.Add("0566", 2);
                group4.Add("0564", 2);
                group4.Add("0565", 2);
                group4.Add("0587", 2);
                group4.Add("0584", 2);
                group4.Add("0581", 2);
                group4.Add("0572", 2);
                group4.Add("0574", 2);
                group4.Add("0573", 2);
                group4.Add("0575", 2);
                group4.Add("0576", 2);
                group4.Add("0578", 2);
                group4.Add("0577", 2);
                group4.Add("0569", 2);
                group4.Add("0594", 2);
                group4.Add("0827", 2);
                group4.Add("0736", 2);
                group4.Add("0735", 2);
                group4.Add("0725", 2);
                group4.Add("0737", 2);
                group4.Add("0739", 2);
                group4.Add("0743", 2);
                group4.Add("0742", 2);
                group4.Add("0740", 2);
                group4.Add("0721", 2);
                group4.Add("0599", 2);
                group4.Add("0561", 2);
                group4.Add("0562", 2);
                group4.Add("0563", 2);
                group4.Add("0595", 2);
                group4.Add("0596", 2);
                group4.Add("0598", 2);
                group4.Add("0597", 2);
                group4.Add("0744", 2);
                group4.Add("0852", 2);
                group4.Add("0956", 2);
                group4.Add("0955", 2);
                group4.Add("0954", 2);
                group4.Add("0952", 2);
                group4.Add("0957", 2);
                group4.Add("0959", 2);
                group4.Add("0966", 2);
                group4.Add("0965", 2);
                group4.Add("0964", 2);
                group4.Add("0950", 2);
                group4.Add("0949", 2);
                group4.Add("0942", 2);
                group4.Add("0940", 2);
                group4.Add("0930", 2);
                group4.Add("0943", 2);
                group4.Add("0944", 2);
                group4.Add("0948", 2);
                group4.Add("0947", 2);
                group4.Add("0946", 2);
                group4.Add("0967", 2);
                group4.Add("0968", 2);
                group4.Add("0987", 2);
                group4.Add("0986", 2);
                group4.Add("0985", 2);
                group4.Add("0984", 2);
                group4.Add("0993", 2);
                group4.Add("0994", 2);
                group4.Add("0997", 2);
                group4.Add("0996", 2);
                group4.Add("0995", 2);
                group4.Add("0983", 2);
                group4.Add("0982", 2);
                group4.Add("0973", 2);
                group4.Add("0972", 2);
                group4.Add("0969", 2);
                group4.Add("0974", 2);
                group4.Add("0977", 2);
                group4.Add("0980", 2);
                group4.Add("0979", 2);
                group4.Add("0978", 2);
                group4.Add("0920", 2);
                group4.Add("0898", 2);
                group4.Add("0855", 2);
                group4.Add("0854", 2);
                group4.Add("0853", 2);
                group4.Add("0553", 2);
                group4.Add("0856", 2);
                group4.Add("0857", 2);
                group4.Add("0863", 2);
                group4.Add("0859", 2);
                group4.Add("0858", 2);
                group4.Add("0848", 2);
                group4.Add("0847", 2);
                group4.Add("0835", 2);
                group4.Add("0834", 2);
                group4.Add("0833", 2);
                group4.Add("0836", 2);
                group4.Add("0837", 2);
                group4.Add("0846", 2);
                group4.Add("0845", 2);
                group4.Add("0838", 2);
                group4.Add("0865", 2);
                group4.Add("0866", 2);
                group4.Add("0892", 2);
                group4.Add("0889", 2);
                group4.Add("0887", 2);
                group4.Add("0893", 2);
                group4.Add("0894", 2);
                group4.Add("0897", 2);
                group4.Add("0896", 2);
                group4.Add("0895", 2);
                group4.Add("0885", 2);
                group4.Add("0884", 2);
                group4.Add("0869", 2);
                group4.Add("0868", 2);
                group4.Add("0867", 2);
                group4.Add("0875", 2);
                group4.Add("0877", 2);
                group4.Add("0883", 2);
                group4.Add("0880", 2);
                group4.Add("0879", 2);
                group4.Add("0829", 2);
                group4.Add("0550", 2);
                group4.Add("0228", 2);
                group4.Add("0226", 2);
                group4.Add("0225", 2);
                group4.Add("0224", 2);
                group4.Add("0229", 2);
                group4.Add("0233", 2);
                group4.Add("0237", 2);
                group4.Add("0235", 2);
                group4.Add("0234", 2);
                group4.Add("0223", 2);
                group4.Add("0220", 2);
                group4.Add("0192", 2);
                group4.Add("0191", 2);
                group4.Add("0187", 2);
                group4.Add("0193", 2);
                group4.Add("0194", 2);
                group4.Add("0198", 2);
                group4.Add("0197", 2);
                group4.Add("0195", 2);
                group4.Add("0238", 2);
                group4.Add("0240", 2);
                group4.Add("0260", 2);
                group4.Add("0259", 2);
                group4.Add("0258", 2);
                group4.Add("0257", 2);
                group4.Add("0261", 2);
                group4.Add("0263", 2);
                group4.Add("0266", 2);
                group4.Add("0265", 2);
                group4.Add("0264", 2);
                group4.Add("0256", 2);
                group4.Add("0255", 2);
                group4.Add("0243", 2);
                group4.Add("0242", 2);
                group4.Add("0241", 2);
                group4.Add("0244", 2);
                group4.Add("0246", 2);
                group4.Add("0254", 2);
                group4.Add("0248", 2);
                group4.Add("0247", 2);
                group4.Add("0186", 2);
                group4.Add("0185", 2);
                group4.Add("0144", 2);
                group4.Add("0143", 2);
                group4.Add("0142", 2);
                group4.Add("0139", 2);
                group4.Add("0145", 2);
                group4.Add("0146", 2);
                group4.Add("0154", 2);
                group4.Add("0153", 2);
                group4.Add("0152", 2);
                group4.Add("0138", 2);
                group4.Add("0137", 2);
                group4.Add("0125", 2);
                group4.Add("0124", 2);
                group4.Add("0123", 2);
                group4.Add("0126", 2);
                group4.Add("0133", 2);
                group4.Add("0136", 2);
                group4.Add("0135", 2);
                group4.Add("0134", 2);
                group4.Add("0155", 2);
                group4.Add("0156", 2);
                group4.Add("0176", 2);
                group4.Add("0175", 2);
                group4.Add("0174", 2);
                group4.Add("0178", 2);
                group4.Add("0179", 2);
                group4.Add("0184", 2);
                group4.Add("0183", 2);
                group4.Add("0182", 2);
                group4.Add("0173", 2);
                group4.Add("0172", 2);
                group4.Add("0162", 2);
                group4.Add("0158", 2);
                group4.Add("0157", 2);
                group4.Add("0163", 2);
                group4.Add("0164", 2);
                group4.Add("0167", 2);
                group4.Add("0166", 2);
                group4.Add("0165", 2);
                group4.Add("0267", 2);
                group4.Add("0250", 2);
                group4.Add("0533", 2);
                group4.Add("0422", 2);
                group4.Add("0532", 2);
                group4.Add("0531", 2);
                group4.Add("0436", 2);
                group4.Add("0428", 2);
                group4.Add("0536", 2);
                group4.Add("0299", 2);
                group4.Add("0294", 2);
                group4.Add("0293", 2);
                group4.Add("0475", 2);
                group4.Add("0295", 2);
                group4.Add("0297", 2);
                group4.Add("0296", 2);
                group4.Add("0495", 2);
                group4.Add("0438", 2);
                group4.Add("0466", 2);
                group4.Add("0465", 2);
                group4.Add("0467", 2);
                group4.Add("0478", 2);
                group4.Add("0476", 2);
                group4.Add("0470", 2);
                group4.Add("0463", 2);
                group4.Add("0479", 2);
                group4.Add("0493", 2);
                group4.Add("0494", 2);
                group4.Add("0439", 2);
                group4.Add("0268", 2);
                group4.Add("0480", 2);
                group4.Add("0460", 2);
                group4.Add("0538", 2);
                group4.Add("0537", 2);
                group4.Add("0539", 2);
                group4.Add("0279", 2);
                group4.Add("0548", 2);
                group4.Add("0280", 2);
                group4.Add("0282", 2);
                group4.Add("0278", 2);
                group4.Add("0277", 2);
                group4.Add("0269", 2);
                group4.Add("0270", 2);
                group4.Add("0274", 2);
                group4.Add("0276", 2);
                group4.Add("0283", 2);
                group4.Add("0551", 2);
                group4.Add("0289", 2);
                group4.Add("0287", 2);
                group4.Add("0547", 2);
                group4.Add("0288", 2);
                group4.Add("0544", 2);
                group4.Add("0545", 2);
                group4.Add("0284", 2);
                group4.Add("0291", 2);
                group4.Add("0285", 2);
                group4.Add("0120", 3);
                group4.Add("0570", 3);
                group4.Add("0800", 3);
                group4.Add("0990", 3);
                groups.Add(4, group4);
                Dictionary<string, int> group3 = new Dictionary<string, int>();
                group3.Add("099", 3);
                group3.Add("054", 3);
                group3.Add("058", 3);
                group3.Add("098", 3);
                group3.Add("095", 3);
                group3.Add("097", 3);
                group3.Add("052", 3);
                group3.Add("053", 3);
                group3.Add("011", 3);
                group3.Add("096", 3);
                group3.Add("049", 3);
                group3.Add("015", 3);
                group3.Add("048", 3);
                group3.Add("072", 3);
                group3.Add("084", 3);
                group3.Add("028", 3);
                group3.Add("024", 3);
                group3.Add("076", 3);
                group3.Add("023", 3);
                group3.Add("047", 3);
                group3.Add("029", 3);
                group3.Add("075", 3);
                group3.Add("025", 3);
                group3.Add("055", 3);
                group3.Add("026", 3);
                group3.Add("079", 3);
                group3.Add("082", 3);
                group3.Add("027", 3);
                group3.Add("078", 3);
                group3.Add("077", 3);
                group3.Add("083", 3);
                group3.Add("022", 3);
                group3.Add("086", 3);
                group3.Add("089", 3);
                group3.Add("045", 3);
                group3.Add("044", 3);
                group3.Add("092", 3);
                group3.Add("046", 3);
                group3.Add("017", 3);
                group3.Add("093", 3);
                group3.Add("059", 3);
                group3.Add("073", 3);
                group3.Add("019", 3);
                group3.Add("087", 3);
                group3.Add("042", 3);
                group3.Add("018", 3);
                group3.Add("043", 3);
                group3.Add("088", 3);
                group3.Add("050", 4);
                groups.Add(3, group3);
                Dictionary<string, int> group2 = new Dictionary<string, int>();
                group2.Add("04", 4);
                group2.Add("03", 4);
                group2.Add("06", 4);
                groups.Add(2, group2);
                if (strict)
                {
                    group3.Add("020", 3);
                    group3.Add("070", 3);
                    group3.Add("080", 3);
                    group3.Add("090", 3);
                }
                else
                {
                    group3.Add("020", 4);
                    group3.Add("070", 4);
                    group3.Add("080", 4);
                    group3.Add("090", 4);
                }
            }

            string number = Regex.Replace(input, @"[^\d]+", "");

            foreach (int _ in groups.Keys)
            {
                Dictionary<string, int> group = groups[_];
                int len = _;
                string area = number.Substring(0, len);
                if (group.ContainsKey(area))
                {
                    string formatted1 = number.Substring(len, group[area]);
                    string formatted2 = number.Substring(len + group[area]);
                    string formatted = area + "-" + formatted1 + "-" + formatted2;
                    if (formatted[formatted.Length - 1] != '-')
                        return formatted;
                    else
                        return input;
                }
            }

            string pattern = @"\A(00(?:[013-8]|2\d|91[02-9])\d)(\d+)\z";
            Match matches = Regex.Match(number, pattern);
            if (matches.Success)
            {
                return matches.Groups[1] + "-" + matches.Groups[2];
            }

            return input;
        }
    }
}
