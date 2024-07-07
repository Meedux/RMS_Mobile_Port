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
    public class InventoryCustodyItemViewModel : BaseModel
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
        /// 預り予定日 View
        /// </summary>
        private string _custodyDateView;

        /// <summary>
        /// 預り予定日
        /// </summary>
        public string CustodyDateView
        {
            get { return _custodyDateView; }
            set { this.SetProperty(ref this._custodyDateView, value); }
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
        /// 返却予定日 View
        /// </summary>
        private string _returnDateView;

        /// <summary>
        /// 返却予定日 View
        /// </summary>
        public string ReturnDateView
        {
            get { return _returnDateView; }
            set { this.SetProperty(ref this._returnDateView, value); }
        }

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
                _custodyItemModel.CustodyDetail.shelfNumber = value;
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

        private bool _isError;
        public bool IsError
        {
            get { return _isError; }
            set {
                TextColor = value ? Color.Red : Color.Black;
                this.SetProperty(ref this._isError, value);
            }
        }

        public bool IsExist { get; set; }

        private Color _textColor = Color.Black;
        public Color TextColor
        {
            get { return _textColor; }
            set { this.SetProperty(ref this._textColor, value); }
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

        private CustodyItemModel _custodyItemModel;

        public InventoryCustodyItemViewModel() : base()
        {
        }

        public InventoryCustodyItemViewModel(CustodyItemModel custodyItemModel) : base()
        {
            _companyName = custodyItemModel.Custody.companyName;
            _contents = custodyItemModel.CustodyDetail.contents;
            _custodyDate = custodyItemModel.Custody.custodyDate;
            _custodyDateView = _custodyDate.HasValue ? _custodyDate.Value.ToString(DateHelper.Date_Format_YYYYMMDD) : string.Empty;
            _customerName = custodyItemModel.Custody.customerName;
            _inventoryDate = custodyItemModel.CustodyDetail.InventoryDate;
            _inventoryDateView = _inventoryDate.HasValue ? _inventoryDate.Value.ToString(DateHelper.Date_Format_YYYYMMDD) : string.Empty;
            _rfid = custodyItemModel.CustodyDetail.rfid;
            _shelfNumber = custodyItemModel.CustodyDetail.shelfNumber;
            _returnDate = custodyItemModel.CustodyDetail.returnDate;
            _returnDateView = _returnDate.HasValue ? _returnDate.Value.ToString(DateHelper.Date_Format_YYYYMMDD) : string.Empty;
            _address = custodyItemModel.Custody.address;
            _postCode = custodyItemModel.Custody.postCode;
            _telephoneNumber = custodyItemModel.Custody.telephoneNumber;
            _status = custodyItemModel.CustodyDetail.status;
            _columnEmpty = "    ";
            IsExist = true;

            _custodyItemModel = custodyItemModel;
            Id = custodyItemModel.CustodyDetail.id;
        }

        public CustodyDetailBody GetCustodyDetail()
        {
            return _custodyItemModel.CustodyDetail;
        }

        public DateTime GetInputDate()
        {
            return DateTime.ParseExact($"{_inputDate} {DateTime.Now.ToString(DateHelper.Date_Format_HHMMSS)}",
                    DateHelper.Date_Format_YYYYMMDDHHMMSS, CultureInfo.CurrentCulture);
        }
    }
}
