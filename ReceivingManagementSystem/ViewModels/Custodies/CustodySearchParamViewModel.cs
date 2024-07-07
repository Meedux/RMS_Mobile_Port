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

namespace ReceivingManagementSystem.ViewModels.Custodies
{
    public class CustodySearchParamViewModel : BaseModel
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
        /// 郵便番号*
        /// </summary>
        private string _postCode;

        /// <summary>
        /// 郵便番号*
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
        /// 預り予定日
        /// </summary>
        private DateTime? _custodyDate;

        /// <summary>
        /// 預り予定日
        /// </summary>
        public DateTime? CustodyDate
        {
            get { return _custodyDate; }
            set { this.SetProperty(ref this._custodyDate, value); }
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
        /// 返却予定日
        /// </summary>
        private DateTime? _returnDate;

        /// <summary>
        /// 返却予定日
        /// </summary>
        public DateTime? ReturnDate
        {
            get { return _returnDate; }
            set { this.SetProperty(ref this._returnDate, value); }
        }

        /// <summary>
        /// 受注日
        /// </summary>
        private DateTime? _orderDate;

        /// <summary>
        /// 受注日
        /// </summary>
        public DateTime? OrderDate
        {
            get { return _orderDate; }
            set { this.SetProperty(ref this._orderDate, value); }
        }

        /// <summary>
        /// 棚番号
        /// </summary>
        private string _shelfNumber;

        /// <summary>
        /// 棚番号
        /// </summary>
        public string ShelfNumber
        {
            get { return _shelfNumber; }
            set
            {
                this.SetProperty(ref this._shelfNumber, value);
            }
        }

        /// <summary>
        /// 状態
        /// </summary>
        private string _status;

        /// <summary>
        /// 状態
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                this.SetProperty(ref this._status, value);
            }
        }

        /// <summary>
        /// Input Date
        /// </summary>
        private string _inputDate;

        /// <summary>
        /// Input Date
        /// </summary>
        public string InputDate
        {
            get { return _inputDate; }
            set { this.SetProperty(ref this._inputDate, value); }
        }

        /// <summary>
        /// 預り予定日
        /// </summary>
        private DateTime _selectDate;

        /// <summary>
        /// 預り予定日
        /// </summary>
        public DateTime SelectDate
        {
            get { return _selectDate; }
            set
            {
                this.SetProperty(ref this._selectDate, value);
            }
        }

        public CustodySearchParamViewModel() : base()
        {
            _selectDate = DateTime.Now;
        }

        public CustodyBody GetCustodyBodySearch()
        {
            CustodyBody custodyBody = new CustodyBody()
            {
                address = _address,
                companyName = _companyName,
                customerName = _customerName,
                custodyDate = _custodyDate,
                postCode = _postCode,
                telephoneNumber = _telephoneNumber,
            };

            if (!string.IsNullOrEmpty(_inputDate))
            {
                custodyBody.custodyDate = DateHelper.GetDateByInputDate(_inputDate);
            }

            return custodyBody;
        }

        public CustodyDetailBody GetCustodyDetailBodySearch()
        {
            CustodyDetailBody custodyBody = new CustodyDetailBody()
            {
                contents = _contents,
                status = CustodyStatusEnum.Order_Received.Value
            };

            return custodyBody;
        }
    }
}
