using System;
using System.Collections.Generic;
using System.Text;

namespace ReceivingManagementSystem.ViewModels.Inventory
{
    public class InventoryCsvModel
    {
        /// <summary>
        /// 最終棚卸日
        /// </summary>
        public string InventoryDate { get; set; }
        /// <summary>
        /// 状態
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// RFID
        /// </summary>
        public string Rfid { get; set; }
        /// <summary>
        /// 棚番号
        /// </summary>
        public string ShelfNumber { get; set; }
        /// <summary>
        /// 預り日
        /// </summary>
        public string CustodyDate { get; set; }
        /// <summary>
        /// 返却日
        /// </summary>
        public string ReturnDate { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Contents { get; set; }
        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 氏名
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 電話番号
        /// </summary>
        public string TelephoneNumber { get; set; }

    }
}
