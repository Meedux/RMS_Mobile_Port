using CsvHelper;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.ViewModels.Warehousing;
using ReceivingManagementSystem.Views;
using ReceivingManagementSystem.Warehousing;
using ReceivingManagementSystem.Wrapper;
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

namespace ReceivingManagementSystem.ViewModels.Warehousing
{
    public class WarehousingShelfNumberInputViewModel : BaseViewModel
    {
        #region Properties

        private CustodyItemViewModel _custodyItem;
        public CustodyItemViewModel CustodyItem
        {
            get { return _custodyItem; }
            set { this.SetProperty(ref this._custodyItem, value); }
        }

        /// <summary>
        /// Rfid
        /// </summary>
        private string _rfid;

        /// <summary>
        /// Rfid
        /// </summary>
        public string Rfid
        {
            get { return _rfid; }

            set { this.SetProperty(ref this._rfid, value); }
        }

        /// <summary>
        /// 棚番号
        /// </summary>
        private List<ComboBoxItemViewModel> _shelfNumbers;

        /// <summary>
        /// 棚番号
        /// </summary>
        public List<ComboBoxItemViewModel> ShelfNumbers
        {
            get { return _shelfNumbers; }
            set { this.SetProperty(ref this._shelfNumbers, value); }
        }

        /// <summary>
        /// 棚番号 selected
        /// </summary>
        private ComboBoxItemViewModel _shelfNumberSelected;

        /// <summary>
        /// 棚番号 selected
        /// </summary>
        public ComboBoxItemViewModel ShelfNumberSelected
        {
            get { return _shelfNumberSelected; }
            set
            {
                CustodyItem.ShelfNumber = value != null ? value.Value.ToString() : null;
                this.SetProperty(ref this._shelfNumberSelected, value);
            }
        }

        private string _device;
        #endregion

        #region Command

        public ICommand OkCommand { get; }
        public ICommand ReadRFIDCommand { get; }
        public ICommand SelectShelfCommand { get; }

        #endregion

        #region Dependency

        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        #endregion

        public WarehousingShelfNumberInputViewModel(CustodyItemViewModel itemViewModel, ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);
            ReadRFIDCommand = new Command(ReadRFID);
            SelectShelfCommand = new Command(GetShelfNumberByRfid);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();

            CustodyItem = itemViewModel;

            GetShelfNumbers();

            _device = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Device, "SP1");
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
        private async void Ok()
        {
            bool isValidate = await Validate();

            if (!isValidate)
            {
                return;
            }

            WarehousingConfirmationPage warehousingConfirmationPage = new WarehousingConfirmationPage(_custodyItem);
            warehousingConfirmationPage.OnCloseOk += CustodyPage_Disappearing;
            await this.Owner.Navigation.PushAsync(warehousingConfirmationPage, true);
        }

        private void CustodyPage_Disappearing(object sender, EventArgs e)
        {
            WarehousingConfirmationPage custodyPage = (WarehousingConfirmationPage)sender;

            var viewModel = (WarehousingConfirmationViewModel)custodyPage.BindingContext;

            if (viewModel.IsOK)
            {
                Close(true);
                ((WarehousingShelfNumberInputPage)this.Owner).OnClose();
            }
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            if (ShelfNumberSelected == null)
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.ShelfNumber));
            }

            if (errors.Count > 0)
            {
                await Owner.DisplayActionSheet(ResourceProvider.GetResourceByName(TextResourceKey.NotificationTitle),
                   ResourceProvider.GetResourceByName(TextResourceKey.OK), null, errors.ToArray());
                return false;
            }

            return true;
        }

        private async void GetShelfNumbers()
        {
            var items = await _pleasanterService.GetShelfNumbers();

            ShelfNumbers = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = s.shelfNumber,
                Value = s.shelfNumber,
                Id = s.shelfRfid
            }).ToList();
        }

        /// <summary>
        /// Read RFID
        /// </summary>
        private void ReadRFID()
        {
            Rfid = string.Empty;
            CustodyItem.ShelfNumber = string.Empty;

            if ("SP1".Equals(_device))
            {
                _readRfidService.ReadRfid();
            }
        }

        private void OnReadRfid(object sender, RfidResultEventArgs args)
        {
            Rfid = args.Rfid + "\r\n";
        }

        private async void GetShelfNumberByRfid()
        {
            var shelfNumber = _shelfNumbers.FirstOrDefault(r => r.Id.ToString().Equals(_rfid));

            if (shelfNumber == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0002, TextResourceKey.ShelfNumber);
            }
            else {
                ShelfNumberSelected = shelfNumber;
            }
        }
    }
}
