using ReceivingManagementSystem.Common.Resources;
using ReceivingManagementSystem.Orders;
using ReceivingManagementSystem.ViewModels.Custodies;
using ReceivingManagementSystem.Services.Rfid;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using static RMS_Pleasanter.CustodyDetail;
using ReceivingManagementSystem.Services.Pleasanter;
using static RMS_Pleasanter.Custody;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Inventory;
using ReceivingManagementSystem.Wrapper;
using System.IO;
using CsvHelper;
using System.Globalization;
using PleasanterOperation;
using static RMS_Pleasanter.ItemMaster;
using static RMS_Pleasanter.PalletMaster;
using static RMS_Pleasanter.ItemInventoryCount;
using CsvHelper.Configuration;
using Serilog;

namespace ReceivingManagementSystem.ViewModels.Inventory
{
    public class ItemInventorySearchResultViewModel : BaseViewModel
    {
        #region Properties

        private ObservableCollection<ItemInventoryItemViewModel> _items;

        public ObservableCollection<ItemInventoryItemViewModel> Items
        {
            get { return _items; }
            set { this.SetProperty(ref this._items, value); }
        }

        private ItemInventoryItemViewModel _itemSelected;

        public ItemInventoryItemViewModel ItemSelected
        {
            get { return _itemSelected; }
            set
            {
                this.SetProperty(ref this._itemSelected, value);
            }
        }

        #endregion

        #region Command

        public ICommand DoubleClickCommand { get; set; }
        public ICommand AppearingCommand { get; set; }
        public ICommand OkCommand { get; set; }
        public ICommand ExportCsvCommand { get; set; }
        #endregion

        private IReadRfidService _readRfidService;
        private IPleasanterService _pleasanterService;
        private IFileWrapper _fileWrapper;

        public ItemInventorySearchResultViewModel(ContentPage owner) : base(owner)
        {
            DoubleClickCommand = new Command(Update);
            OkCommand = new Command(Update);
            ExportCsvCommand = new Command(ExportCsv);

            _readRfidService = DependencyService.Get<IReadRfidService>();
            _pleasanterService = DependencyService.Get<IPleasanterService>();
            _fileWrapper = DependencyService.Get<IFileWrapper>();

            Items = new ObservableCollection<ItemInventoryItemViewModel>();

            Search();
        }

        private async void Search()
        {
            if (!_pleasanterService.CheckSetting(PleasanterObjectTypeEnum.ItemInventory, PleasanterObjectTypeEnum.Item, PleasanterObjectTypeEnum.PalletMaster,
                PleasanterObjectTypeEnum.ItemInventory, PleasanterObjectTypeEnum.ItemInventoryCount))
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.OK,
                    MessageResourceKey.E0008);
                return;
            }

            var itemInventories = await _pleasanterService.GetItemInventory();
            var items = await _pleasanterService.GetItems(); 

            List<string> palletNumbers = itemInventories.GroupBy(g => g.palletNumber).Select(s => s.Key).ToList();
            List<string> ids = itemInventories.GroupBy(g => g.itemId).Select(s => s.Key).ToList();

            List<SearchInfoModel> searchInfo = new List<SearchInfoModel>();
            searchInfo = new List<SearchInfoModel>();
            searchInfo.Add(new SearchInfoModel
            {
                FieldName = "itemId",
                Values = ids.Cast<object>().ToList(),
                ValueType = SearchInfoModel.ValueTypeEnum.String,
                SearchType = SearchInfoModel.SearchTypeEnum.In
            });

            var pallets = await _pleasanterService.SearchPallets(searchInfo);

            var itemInventoryCounts = await _pleasanterService.SearchItemInventoryCounts(searchInfo);

            Items = new ObservableCollection<ItemInventoryItemViewModel>();
            ItemInventoryItemViewModel itemInventoryItemViewModel;

            ItemBody itemBody;
            PalletMasterBody palletMasterBody;
            ItemInventoryCountBody itemInventoryCountBody;
            foreach (var itemInventory in itemInventories)
            {
                itemBody = items.FirstOrDefault(r => r.itemId.Equals(itemInventory.itemIdOfItem));
                palletMasterBody = pallets.FirstOrDefault(r => r.itemId.Equals(itemInventory.itemId) && r.palletNumber == itemInventory.palletNumber);
                itemInventoryCountBody = itemInventoryCounts.Where(r => r.itemId == itemInventory.itemId
                    && r.palletNumber == itemInventory.palletNumber
                    && r.RFID == itemInventory.RFID).OrderByDescending(r => r.date).FirstOrDefault();

                itemInventoryItemViewModel = new ItemInventoryItemViewModel(itemInventory, itemBody, palletMasterBody, itemInventoryCountBody);
                Items.Add(itemInventoryItemViewModel);
            }
        }

        /// <summary>
        /// Double click row
        /// </summary>
        private async void Update()
        {
            if (_itemSelected == null)
            {
                await ShowAlert(TextResourceKey.NotificationTitle, TextResourceKey.Cancel, MessageResourceKey.E0006);

                return;
            }

            InputNumberInventoryPage custodyPage = new InputNumberInventoryPage(_itemSelected);
            custodyPage.OnCloseOk += UpdatePage_Disappearing;
            await this.Owner.Navigation.PushAsync(custodyPage, true);
        }

        private void UpdatePage_Disappearing(object sender, EventArgs e)
        {
            InputNumberInventoryPage custodyPage = (InputNumberInventoryPage)sender;

            var viewModel = (InputNumberInventoryViewModel)custodyPage.BindingContext;

            if (viewModel.IsOK)
            {
                // 更新する
                //Items.Remove(_itemSelected);
                //ItemSelected = null;
            }
        }

        private async void ExportCsv()
        {
            try
            {
                List<ItemInventoryCsvModel> inventory = _items.Select(s => new ItemInventoryCsvModel
                {
                    LastCountDate = s.InventoryDateView,
                    Rfid = s.Rfid,
                    Dimensions = s.Dimensions,
                    Inventory = s.Inventory.HasValue ? s.Inventory.ToString() : "",
                    ItemName = s.ItemName,
                    ItemNumber = s.ItemNumber,
                    ItemType = s.ItemType,
                    PalletKind = s.PalletKind,
                    Standard = s.Standard
                }).ToList();

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                    Encoding = Encoding.GetEncoding(932)
                };
                List<string> header = new List<string>() { "最終棚卸日", "RFID", "商品種別", "商品名", "品番", "規格", "寸法", "パレット種類", "在庫数" };

                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(memoryStream, Encoding.GetEncoding(932)))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteField(header);
                        csv.NextRecord();
                        csv.WriteRecords(inventory);

                    }

                    await _fileWrapper.ExportFile(memoryStream);
                }

                ShowMessage(MessageResourceKey.E0007);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }
    }
}
