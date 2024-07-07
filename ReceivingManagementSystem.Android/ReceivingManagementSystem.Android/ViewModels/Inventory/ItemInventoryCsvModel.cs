using System;
using System.Collections.Generic;
using System.Text;

namespace ReceivingManagementSystem.Android.ViewModels.Inventory
{
    public class ItemInventoryCsvModel
    {
        public string LastCountDate { get; set; }
        public string Rfid { get; set; }
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public string Standard { get; set; }
        public string Dimensions { get; set; }
        public string PalletKind { get; set; }
        public string Inventory { get; set; }

    }
}
