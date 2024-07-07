using CsvHelper;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Android.ViewModels.Custodies;
using ReceivingManagementSystem.Android.ViewModels.Delivery;
using ReceivingManagementSystem.Android.Interfaces;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace ReceivingManagementSystem.Android.ViewModels.Inventory
{
    public class InventoryUpdateNormalViewModel : BaseViewModel
    {
        #region Properties

        private InventoryCustodyItemViewModel _item;
        public InventoryCustodyItemViewModel Item
        {
            get { return _item; }
            set { this.SetProperty(ref this._item, value); }
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
                this.SetProperty(ref this._statusSelected, value);
            }
        }

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; }

        #endregion

        #region Command

        public ICommand RenewCommand { get; }

        #endregion

        private IPleasanterService _pleasanterService;

        public InventoryUpdateNormalViewModel(InventoryCustodyItemViewModel item, bool Inventory, ContentPage owner) : base(owner)
        {
            if(Inventory)
            {
                Title = "預り業務-棚卸更新";
            }
            else {
                Title = "預り業務-状態更新";
            }

            RenewCommand = new Command(Renew);

            _pleasanterService = DependencyService.Get<IPleasanterService>();

            Item = item;

            GetStatus();
        }

        /// <summary>
        /// Delivery
        /// </summary>
        private async void Renew()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.CustodyDetail))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0008);
                return;
            }

            bool isValidate = await Validate();

            if (!isValidate)
            {
                return;
            }

            var custodyDetailBody = _item.GetCustodyDetail();
            custodyDetailBody.InventoryDate = DateTime.Now;
            custodyDetailBody.status = _statusSelected.Value.ToString();

            bool result = await _pleasanterService.UpdateCustodyDetail(custodyDetailBody);

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK, MessageResourceKey.E0003, TextResourceKey.Inventory);
            }
            else
            {
                _item.Status = custodyDetailBody.status;
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.Inventory);
                Close(true);
            }
        }

        /// <summary>
        /// Validate data require
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Validate()
        {
            List<string> errors = new List<string>();

            if (_statusSelected == null)
            {
                errors.Add(ResourceProvider.GetMesResource(MessageResourceKey.E0001, TextResourceKey.Status));
            }

            if (errors.Count > 0)
            {
                await ShowActionSheet(TextResourceKey.NotificationTitle, TextResourceKey.Cancel, errors);
                return false;
            }

            return true;
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
