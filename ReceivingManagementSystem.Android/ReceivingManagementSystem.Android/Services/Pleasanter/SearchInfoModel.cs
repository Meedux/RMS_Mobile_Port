using System;
using System.Collections.Generic;
using System.Text;

namespace ReceivingManagementSystem.Android.Services.Pleasanter
{
    public class SearchInfoModel
    {
        public object Value { get; set; }
        public List<object> Values { get; set; }
        public string FieldName { get; set; }
        public ValueTypeEnum ValueType { get; set; }
        public SearchTypeEnum SearchType { get; set; }

        public enum ValueTypeEnum
        { 
            String = 0,
            Int = 1,
        }

        public enum SearchTypeEnum
        {
            Equals = 0,
            In = 1,
            FromTo = 2,
            Contains = 3,
        }

        public SearchInfoModel()
        {
            Values = new List<object>();
        }
    }
}
