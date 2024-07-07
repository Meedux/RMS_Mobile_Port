using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.ViewModels.Orders
{
    public class OrderReceiptInfoViewModel : BaseModel
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
            set {
                if(value.Length >= 4)
                    this.SetProperty(ref this._postCode, value.Substring(0, 3) + "-" + value.Substring(3));
                else this.SetProperty(ref this._postCode, value);
            }
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
        /// Id
        /// </summary>
        private decimal? _id;

        /// <summary>
        /// Id
        /// </summary>
        public decimal? Id
        {
            get { return _id; }
            set { this.SetProperty(ref this._id, value); }
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
        /// Select date
        /// </summary>
        private DateTime _selectDate;

        /// <summary>
        /// Select date
        /// </summary>
        public DateTime SelectDate
        {
            get { return _selectDate; }
            set
            {
                this.SetProperty(ref this._selectDate, value);
            }
        }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// DetailNumber
        /// </summary>
        public decimal DetailNumber { get; set; }

        public OrderReceiptInfoViewModel() : base()
        {
            _selectDate = DateTime.Now;
            DetailNumber = 1;
        }

        public CustodyBody GetCustodyBody()
        {
            CustodyBody custodyBody = new CustodyBody()
            {
                address = _address,
                companyName = _companyName,
                customerName = _customerName,
                postCode = _postCode,
                telephoneNumber = _telephoneNumber,
                code = Code,
                date = DateTime.Now
            };

            if (!string.IsNullOrEmpty(_inputDate))
            {
                custodyBody.custodyDate = DateTime.ParseExact($"{_inputDate} {DateTime.Now.ToString(DateHelper.Date_Format_HHMMSS)}",
                    DateHelper.Date_Format_YYYYMMDDHHMMSS, CultureInfo.CurrentCulture);
            }

            return custodyBody;
        }

        public CustodyDetailBody GetCustodyDetailBody()
        {
            CustodyDetailBody custodyBody = new CustodyDetailBody()
            {
                rfid = _rfid,
                code = Code,
                status = string.IsNullOrEmpty(_rfid) ? CustodyStatusEnum.Order_Received.Value : CustodyStatusEnum.Custody.Value,
                contents = _contents,
                detailNumber = DetailNumber.ToString(),
            };

            return custodyBody;
        }
    }
}
