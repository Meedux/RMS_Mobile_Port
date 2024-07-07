using System;
using System.Collections.Generic;
using System.Text;

namespace ReceivingManagementSystem.Common.Enums
{
    public class CustodyStatusEnum : EnumerationBase<string>
    {
        public static CustodyStatusEnum Order_Received = new CustodyStatusEnum("受注", "受注");
        public static CustodyStatusEnum Custody = new CustodyStatusEnum("預り", "預り");
        public static CustodyStatusEnum Storage = new CustodyStatusEnum("収納", "収納");
        public static CustodyStatusEnum Return_Reception = new CustodyStatusEnum("返却受付", "返却受付");
        public static CustodyStatusEnum Delivery = new CustodyStatusEnum("出庫", "出庫");
        public static CustodyStatusEnum Return = new CustodyStatusEnum("返却", "返却");
        public static CustodyStatusEnum Waste = new CustodyStatusEnum("廃棄", "廃棄");
        public static CustodyStatusEnum Unknow = new CustodyStatusEnum("不明", "不明");

        public CustodyStatusEnum(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public static List<string> GetStatus()
        {
            List<string> statusList = new List<string>();

            statusList.Add(Order_Received.Value);
            statusList.Add(Custody.Value);
            statusList.Add(Storage.Value);
            statusList.Add(Return_Reception.Value);
            statusList.Add(Delivery.Value);
            statusList.Add(Return.Value);
            statusList.Add(Waste.Value);
            statusList.Add(Unknow.Value);

            return statusList;
        }
    }
}
