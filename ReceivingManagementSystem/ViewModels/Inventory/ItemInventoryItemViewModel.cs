using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Services.Pleasanter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;
using static RMS_Pleasanter.ItemInventory;
using static RMS_Pleasanter.ItemInventoryCount;
using static RMS_Pleasanter.ItemMaster;
using static RMS_Pleasanter.PalletMaster;

namespace ReceivingManagementSystem.ViewModels.Custodies
{
    public class ItemInventoryItemViewModel : BaseModel
    {
        /// <summary>
        /// 棚卸日
        /// </summary>
        private DateTime? _inventoryDate;

        /// <summary>
        /// 棚卸日
        /// </summary>
        public DateTime? InventoryDate
        {
            get { return _inventoryDate; }
            set { this.SetProperty(ref this._inventoryDate, value); }
        }

        /// <summary>
        /// 受注日 view
        /// </summary>
        private string _inventoryDateView;

        /// <summary>
        /// 受注日 view
        /// </summary>
        public string InventoryDateView
        {
            get { return _inventoryDateView; }
            set { this.SetProperty(ref this._inventoryDateView, value); }
        }


        /// <summary>
        /// 商品種別
        /// </summary>
        private string _itemType;

        /// <summary>
        /// 商品種別
        /// </summary>
        public string ItemType
        {
            get { return _itemType; }
            set { this.SetProperty(ref this._itemType, value); }
        }

        /// <summary>
        /// 商品名
        /// </summary>
        private string _itemName;

        /// <summary>
        /// 商品名
        /// </summary>
        public string ItemName
        {
            get { return _itemName; }
            set { this.SetProperty(ref this._itemName, value); }
        }

        /// <summary>
        /// 商品名規格/
        /// </summary>
        private string _itemNameStandard;

        /// <summary>
        /// 商品名/規格
        /// </summary>
        public string ItemNameStandard
        {
            get { return _itemNameStandard; }
            set { this.SetProperty(ref this._itemNameStandard, value); }
        }

        /// <summary>
        /// 規格
        /// </summary>
        private string _standard;

        /// <summary>
        /// 規格
        /// </summary>
        public string Standard
        {
            get { return _standard; }
            set { this.SetProperty(ref this._standard, value); }
        }

        /// <summary>
        /// 寸法
        /// </summary>
        private string _dimensions;

        /// <summary>
        /// 寸法
        /// </summary>
        public string Dimensions
        {
            get { return _dimensions; }
            set { this.SetProperty(ref this._dimensions, value); }
        }

        /// <summary>
        /// パレット種類
        /// </summary>
        private string _palletKind;

        /// <summary>
        /// パレット種類
        /// </summary>
        public string PalletKind
        {
            get { return _palletKind; }
            set { this.SetProperty(ref this._palletKind, value); }
        }

        /// <summary>
        /// 品番
        /// </summary>
        private string _itemNumber;

        /// <summary>
        /// 品番
        /// </summary>
        public string ItemNumber
        {
            get { return _itemNumber; }
            set { this.SetProperty(ref this._itemNumber, value); }
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
        /// 在庫数
        /// </summary>
        private uint? _inventory;

        /// <summary>
        /// 在庫数
        /// </summary>
        public uint? Inventory
        {
            get { return _inventory; }
            set { this.SetProperty(ref this._inventory, value); }
        }

        /// <summary>
        /// 棚卸数
        /// </summary>
        private uint? _inventoryCount;

        /// <summary>
        /// 棚卸数
        /// </summary>
        public uint? InventoryCount
        {
            get { return _inventoryCount; }
            set 
            { 
                this.SetProperty(ref this._inventoryCount, value);
                IsDiscrepancy = (_inventoryCount != _inventory);
            }
        }

        private bool _isDiscrepancy = true;

        public bool IsDiscrepancy
        {
            get { return _isDiscrepancy; }
            set { this.SetProperty(ref this._isDiscrepancy, value); }
        }

        /// <summary>
        /// 理由
        /// </summary>
        private string _reason;

        /// <summary>
        /// 理由
        /// </summary>
        public string Reason
        {
            get { return _reason; }
            set { this.SetProperty(ref this._reason, value); }
        }

        /// <summary>
        /// _columnEmpty
        /// </summary>
        private string _columnEmpty;

        /// <summary>
        /// _columnEmpty
        /// </summary>
        public string ColumnEmpty
        {
            get { return _columnEmpty; }
            set { this.SetProperty(ref this._columnEmpty, value); }
        }


        public decimal? Id;

        private ItemInventoryBody itemInventoryBody;
        private ItemBody itemBody;
        private PalletMasterBody palletMasterBody;
        private ItemInventoryCountBody itemInventoryCountBody;

        public ItemInventoryItemViewModel() : base()
        {
        }

        public ItemInventoryItemViewModel(ItemInventoryBody itemInventoryBody, ItemBody itemBody, PalletMasterBody palletMasterBody, ItemInventoryCountBody itemInventoryCountBody) : base()
        {
            this.itemInventoryBody = itemInventoryBody;
            this.itemBody = itemBody;
            this.palletMasterBody = palletMasterBody;
            this.itemInventoryCountBody = itemInventoryCountBody;

            Rfid = itemInventoryBody.RFID;
            Inventory = itemInventoryBody.inventory;


            if (itemInventoryCountBody != null)
            {
                InventoryDate = itemInventoryCountBody.date;
                InventoryDateView = InventoryDate.HasValue ? InventoryDate.Value.ToString("yyyy/MM/dd") : string.Empty;
            }

            if (itemBody != null)
            {
                ItemType = itemBody.itemType;
                ItemName = itemBody.itemName;
                ItemNumber = itemBody.itemNumber;
            }

            if (palletMasterBody != null)
            {
                Standard = palletMasterBody.standard;
                Dimensions = palletMasterBody.dimensions;
                PalletKind = palletMasterBody.palletKind;
            }

            ItemNameStandard = $"{ItemName} / {Standard}";
        }

        public ItemInventoryBody GetItemInventoryBody()
        {
            itemInventoryBody.inventory = InventoryCount;

            return itemInventoryBody;
        }

        public ItemInventoryCountBody GetItemInventoryCountBody()
        {
            ItemInventoryCountBody itemCount = new ItemInventoryCountBody()
            {
                incDec = InventoryCount,
                date = DateTime.Now,
                description = Reason,
                itemId = itemInventoryBody.itemId,
                palletNumber = itemInventoryBody.palletNumber,
                RFID = itemInventoryBody.RFID
            };

            return itemCount;
        }
    }
}
