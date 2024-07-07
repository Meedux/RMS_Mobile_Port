using CsvHelper;
using PleasanterOperation;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.Services.Pleasanter;
using ReceivingManagementSystem.Services.Rfid;
using ReceivingManagementSystem.ViewModels.PalletRegistration;
using ReceivingManagementSystem.Wrapper;
using RMS_Pleasanter;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.ViewModels.PalletRegistration
{
    public class PalletRegistrationViewModel : BaseViewModel
    {
        #region Properties

        private PalletRegistrationInfoViewModel _palletInfo;
        public PalletRegistrationInfoViewModel PalletInfo
        {
            get { return _palletInfo; }
            set { this.SetProperty(ref this._palletInfo, value); }
        }

        #endregion

        #region Command

        public ICommand RegisterCommand { get; }
        #endregion

        private IPleasanterService _pleasanterService;

        public PalletRegistrationViewModel(ContentPage owner) : base(owner)
        {
            RegisterCommand = new Command(ShowConfirmInfo);

            _pleasanterService = DependencyService.Get<IPleasanterService>();

            PalletInfo = new PalletRegistrationInfoViewModel();

            GetItems();
        }

        public async void ItemSelectedChanged(object item)
        {
            var pallets = await _pleasanterService.GetPalletByItemId(((ComboBoxItemViewModel)item).Id.ToString());
            PalletInfo.PalletNumber = (pallets.Count() + 1).ToString();
        }

        /// <summary>
        /// ShowConfirm Order
        /// </summary>
        private async void ShowConfirmInfo()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.Item, PleasanterObjectTypeEnum.PalletMaster))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0008);
                return;
            }

            bool isValidate = await Validate();

            if (!isValidate)
            {
                return;
            }

            bool result = await Register(); 
            
            if (result)
            {
                Close();
            }
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            if (_palletInfo.ItemSelected == null)
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Item));
            }

            if (string.IsNullOrEmpty(_palletInfo.PalletNumber))
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.PalletNumber));
            }

            if (errors.Count > 0)
            {
                string action = await Owner.DisplayActionSheet(ResourceProvider.GetResourceByName(TextResourceKey.NotificationTitle),
                   ResourceProvider.GetResourceByName(TextResourceKey.Cancel), null, errors.ToArray());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Register pallet
        /// </summary>
        private async Task<bool> Register()
        {
            decimal? custodyId = await _pleasanterService.CreatePalletMaster(_palletInfo.GetPalletMasterBody());

            if (!custodyId.HasValue)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.Pallet);

                return false;
            }

            ShowMessage(MessageResourceKey.E0004, TextResourceKey.Pallet);
            return true;
        }

        /// <summary>
        /// Get items
        /// </summary>
        private async void GetItems()
        {
            var items = await _pleasanterService.GetItems();

            PalletInfo.Items = items.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = $"{s.itemNumber} / {s.itemName} / {s.itemType}",
                Value = s.id.Value,
                Id = s.id.Value,
            }).ToList();
        }

    }
}
