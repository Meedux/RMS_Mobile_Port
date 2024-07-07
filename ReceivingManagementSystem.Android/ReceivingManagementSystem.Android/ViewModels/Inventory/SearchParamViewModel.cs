using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using System;
using System.Collections.Generic;
using System.Text;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.Android.ViewModels.Inventory
{
    public class SearchParamViewModel : BaseModel
    {
        /// <summary>
        /// 会社名
        /// </summary>
        private string _companyName;

        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName
        {
            get { return _companyName; }
            set { this.SetProperty(ref this._companyName, value); }
        }

        /// <summary>
        /// 氏名
        /// </summary>
        private string _customerName;

        /// <summary>
        /// 氏名
        /// </summary>
        public string CustomerName
        {
            get { return _customerName; }
            set { this.SetProperty(ref this._customerName, value); }
        }

        /// <summary>
        /// 郵便番号
        /// </summary>
        private string _postCode;

        /// <summary>
        /// 郵便番号
        /// </summary>
        public string PostCode
        {
            get { return _postCode; }
            set { this.SetProperty(ref this._postCode, value); }
        }

        /// <summary>
        /// 住所
        /// </summary>
        private string _address;

        /// <summary>
        /// 住所
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { this.SetProperty(ref this._address, value); }
        }

        /// <summary>
        /// 電話番号
        /// </summary>
        private string _telephoneNumber;

        /// <summary>
        /// 電話番号
        /// </summary>
        public string TelephoneNumber
        {
            get { return _telephoneNumber; }
            set { this.SetProperty(ref this._telephoneNumber, value); }
        }

        /// <summary>
        /// 内容
        /// </summary>
        private string _contents;

        /// <summary>
        /// 内容
        /// </summary>
        public string Contents
        {
            get { return _contents; }
            set { this.SetProperty(ref this._contents, value); }
        }

        /// <summary>
        /// 状態
        /// </summary>
        private string _status;

        /// <summary>
        /// 内容
        /// </summary>
        public string Status
        {
            get { return _status; }
            set { this.SetProperty(ref this._status, value); }
        }

        /// <summary>
        /// Input Date
        /// </summary>
        private string _custodyDateFrom;

        /// <summary>
        /// Input Date
        /// </summary>
        public string CustodyDateFrom
        {
            get { return _custodyDateFrom; }
            set { this.SetProperty(ref this._custodyDateFrom, value); }
        }

        /// <summary>
        /// Input Date
        /// </summary>
        private string _custodyDateTo;

        /// <summary>
        /// Input Date
        /// </summary>
        public string CustodyDateTo
        {
            get { return _custodyDateTo; }
            set { this.SetProperty(ref this._custodyDateTo, value); }
        }

        /// <summary>
        /// Input Date
        /// </summary>
        private string _returnDateFrom;

        /// <summary>
        /// Input Date
        /// </summary>
        public string ReturnDateFrom
        {
            get { return _returnDateFrom; }
            set { this.SetProperty(ref this._returnDateFrom, value); }
        }

        /// <summary>
        /// Input Date
        /// </summary>
        private string _returnDateTo;

        /// <summary>
        /// Input Date
        /// </summary>
        public string ReturnDateTo
        {
            get { return _returnDateTo; }
            set { this.SetProperty(ref this._returnDateTo, value); }
        }

        /// <summary>
        /// Input Date
        /// </summary>
        private string _inventoryDateFrom;

        /// <summary>
        /// Input Date
        /// </summary>
        public string InventoryDateFrom
        {
            get { return _inventoryDateFrom; }
            set { this.SetProperty(ref this._inventoryDateFrom, value); }
        }

        /// <summary>
        /// Input Date
        /// </summary>
        private string _inventoryDateTo;

        /// <summary>
        /// Input Date
        /// </summary>
        public string InventoryDateTo
        {
            get { return _inventoryDateTo; }
            set { this.SetProperty(ref this._inventoryDateTo, value); }
        }

        public List<SearchInfoModel> GetCustodyBodySearch()
        {
            List<SearchInfoModel> searchInfos = new List<SearchInfoModel>();

            searchInfos.Add(new SearchInfoModel()
            {
                FieldName = "address",
                SearchType = SearchInfoModel.SearchTypeEnum.Contains,
                Value = _address,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            });

            searchInfos.Add(new SearchInfoModel()
            {
                FieldName = "companyName",
                SearchType = SearchInfoModel.SearchTypeEnum.Contains,
                Value = _companyName,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            });

            searchInfos.Add(new SearchInfoModel()
            {
                FieldName = "customerName",
                SearchType = SearchInfoModel.SearchTypeEnum.Contains,
                Value = _customerName,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            });

            searchInfos.Add(new SearchInfoModel()
            {
                FieldName = "telephoneNumber",
                SearchType = SearchInfoModel.SearchTypeEnum.Contains,
                Value = _telephoneNumber,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            });

            SearchInfoModel searchInfoModel = new SearchInfoModel()
            {
                FieldName = "custodyDate",
                SearchType = SearchInfoModel.SearchTypeEnum.In,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            };

            bool isDate = false;
            DateTime? selectDate = null;
            if (!string.IsNullOrEmpty(_custodyDateFrom))
            {
                selectDate = DateHelper.GetDateByInputDate(_custodyDateFrom);

                if (selectDate.HasValue)
                {
                    isDate = true;
                    searchInfoModel.Values.Add(selectDate.Value.ToStringYYYYMMDD());
                }
                else
                {
                    searchInfoModel.Values.Add(string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(_custodyDateTo))
            {
                selectDate = DateHelper.GetDateByInputDate(_custodyDateTo);
                if (selectDate.HasValue)
                {
                    isDate = true;
                    searchInfoModel.Values.Add(selectDate.Value.ToStringYYYYMMDD());
                }
                else
                {
                    searchInfoModel.Values.Add(string.Empty);
                }
            }

            if (isDate)
            {
                searchInfos.Add(searchInfoModel);
            }

            return searchInfos;
        }

        public List<SearchInfoModel> GetCustodyDetailBodySearch()
        {
            List<SearchInfoModel> searchInfos = new List<SearchInfoModel>();

            searchInfos.Add(new SearchInfoModel()
            {
                FieldName = "contents",
                SearchType = SearchInfoModel.SearchTypeEnum.Equals,
                Value = _contents,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            });

            searchInfos.Add(new SearchInfoModel()
            {
                FieldName = "status",
                SearchType = SearchInfoModel.SearchTypeEnum.Equals,
                Value = _status,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            });

            SearchInfoModel searchInfoModel = new SearchInfoModel()
            {
                FieldName = "returnDate",
                SearchType = SearchInfoModel.SearchTypeEnum.In,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            };


            bool isDate = false;
            DateTime? selectDate = null;
            if (!string.IsNullOrEmpty(_returnDateFrom))
            {
                selectDate = DateHelper.GetDateByInputDate(_returnDateFrom);
                if (selectDate.HasValue)
                {
                    isDate = true;
                    searchInfoModel.Values.Add(selectDate.Value.ToStringYYYYMMDD());
                }
                else
                {
                    searchInfoModel.Values.Add(string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(_returnDateTo))
            {
                selectDate = DateHelper.GetDateByInputDate(_returnDateTo);
                if (selectDate.HasValue)
                {
                    isDate = true;
                    searchInfoModel.Values.Add(selectDate.Value.ToStringYYYYMMDD());
                }
                else
                {
                    searchInfoModel.Values.Add(string.Empty);
                }
            }

            if (isDate)
            {
                searchInfos.Add(searchInfoModel);
            }

            isDate = false;
            searchInfoModel = new SearchInfoModel()
            {
                FieldName = "InventoryDate",
                SearchType = SearchInfoModel.SearchTypeEnum.In,
                ValueType = SearchInfoModel.ValueTypeEnum.String
            };

            if (!string.IsNullOrEmpty(_inventoryDateFrom))
            {
                selectDate = DateHelper.GetDateByInputDate(_inventoryDateFrom);
                if (selectDate.HasValue)
                {
                    isDate = true;
                    searchInfoModel.Values.Add(selectDate.Value.ToStringYYYYMMDD());
                }
                else
                {
                    searchInfoModel.Values.Add(string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(_inventoryDateTo))
            {
                selectDate = DateHelper.GetDateByInputDate(_inventoryDateTo);
                if (selectDate.HasValue)
                {
                    isDate = true;
                    searchInfoModel.Values.Add(selectDate.Value.ToStringYYYYMMDD());
                }
                else
                {
                    searchInfoModel.Values.Add(string.Empty);
                }
            }

            if (isDate)
            {
                searchInfos.Add(searchInfoModel);
            }

            return searchInfos;
        }
    }
}
