using System;
using System.Collections.Generic;
using System.Text;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.Android.Services.Pleasanter
{
    public class CustodyItemModel
    {
        public CustodyBody Custody { get; set;}

        public CustodyDetailBody CustodyDetail { get; set; }

        public CustodyItemModel()
        {

        }

        public CustodyItemModel(CustodyBody custody, CustodyDetailBody custodyDetail)
        {
            Custody = custody;
            CustodyDetail = custodyDetail;
        }
    }
}
