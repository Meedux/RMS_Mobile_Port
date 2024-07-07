using CsvHelper;
using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Android.Orders;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using ReceivingManagementSystem.Android.Services.Rfid;
using ReceivingManagementSystem.Views.Android.Settings;
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

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class PleasanterSettingViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// Param
        /// </summary>
        private PleasanterSettingParamViewModel _plesanterSetting;

        /// <summary>
        /// Param
        /// </summary>
        public PleasanterSettingParamViewModel PlesanterSetting
        {
            get { return _plesanterSetting; }

            set { this.SetProperty(ref this._plesanterSetting, value); }
        }


        #endregion

        #region Command

        public ICommand OkCommand { get; }

        #endregion

        private ISaveSettingsWrapper _pSaveSettingsWrapper;
        private IPleasanterService _pleasanterService;

        public PleasanterSettingViewModel(ContentPage owner) : base(owner)
        {
            OkCommand = new Command(Ok);

            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();

            GetSetting();
        }

        private void GetSetting()
        {
            PlesanterSetting = new PleasanterSettingParamViewModel();

            long? defautSiteId = null;
            var content = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Content, defautSiteId);
            PlesanterSetting.Content = content.HasValue ? content.Value.ToString() : String.Empty;

            var custody = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Custody, defautSiteId);
            PlesanterSetting.Custody = custody.HasValue ? custody.Value.ToString() : String.Empty;

            var custodyDetail = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Custody_Detail, defautSiteId);
            PlesanterSetting.CustodyDetail = custodyDetail.HasValue ? custodyDetail.Value.ToString() : String.Empty;

            var customer = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Customer, defautSiteId);
            PlesanterSetting.Customer = customer.HasValue ? customer.Value.ToString() : String.Empty;

            var shelfNumber = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Shelf_Number, defautSiteId);
            PlesanterSetting.ShelfNumber = shelfNumber.HasValue ? shelfNumber.Value.ToString() : String.Empty;


            var itemMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Master, defautSiteId);
            PlesanterSetting.ItemMaster = itemMaster.HasValue ? itemMaster.Value.ToString() : String.Empty;

            var palletMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Pallet_Master, defautSiteId);
            PlesanterSetting.PalletMaster = palletMaster.HasValue ? palletMaster.Value.ToString() : String.Empty;

            var receivingAndShipping = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Receipt_Shipment, defautSiteId);
            PlesanterSetting.ReceivingAndShipping = receivingAndShipping.HasValue ? receivingAndShipping.Value.ToString() : String.Empty;

            var itemInventory = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Inventory, defautSiteId);
            PlesanterSetting.ItemInventory = itemInventory.HasValue ? itemInventory.Value.ToString() : String.Empty;

            var itemInventoryCount = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Inventory_Count, defautSiteId);
            PlesanterSetting.ItemInventoryCount = itemInventoryCount.HasValue ? itemInventoryCount.Value.ToString() : String.Empty;

            var subscServiceMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Subsc_Service_Master, defautSiteId);
            PlesanterSetting.SubscServiceMaster = subscServiceMaster.HasValue ? subscServiceMaster.Value.ToString() : String.Empty;

            PlesanterSetting.ApiKey = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Api_Key, string.Empty);
            PlesanterSetting.Url = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Url, string.Empty);
        }

        private async void Ok()
        {
            List<SaveSettingsParam> settingsParams = new List<SaveSettingsParam>();

            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Content, GetLongValue(_plesanterSetting.Content)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Custody, GetLongValue(_plesanterSetting.Custody)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Custody_Detail, GetLongValue(_plesanterSetting.CustodyDetail)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Customer, GetLongValue(_plesanterSetting.Customer)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Shelf_Number, GetLongValue(_plesanterSetting.ShelfNumber)));

            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
               ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Master, GetLongValue(_plesanterSetting.ItemMaster)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Pallet_Master, GetLongValue(_plesanterSetting.PalletMaster)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Receipt_Shipment, GetLongValue(_plesanterSetting.ReceivingAndShipping)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Inventory, GetLongValue(_plesanterSetting.ItemInventory)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Inventory_Count, GetLongValue(_plesanterSetting.ItemInventoryCount)));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.LONG,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Subsc_Service_Master, GetLongValue(_plesanterSetting.SubscServiceMaster)));

            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Url, _plesanterSetting.Url));
            settingsParams.Add(new SaveSettingsParam(SaveSettingsParam.SaveTypes.STRING,
                ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Api_Key, _plesanterSetting.ApiKey));

            var result = _pSaveSettingsWrapper.SaveSettings(settingsParams.ToArray());

            if (!result)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                     MessageResourceKey.E0003, TextResourceKey.Setting);
            }
            else
            {
                ShowMessage(MessageResourceKey.E0004, TextResourceKey.Setting);

                _pleasanterService.GetSetting();

                Close();
            }
        }

        private long? GetLongValue(string value)
        {
            long? longvalue = null;
            if (string.IsNullOrEmpty(value))
            {
                return longvalue;
            }

            try
            {
                longvalue = long.Parse(value);
            }
            catch
            {

            }

            return longvalue;
        }
    }
}
