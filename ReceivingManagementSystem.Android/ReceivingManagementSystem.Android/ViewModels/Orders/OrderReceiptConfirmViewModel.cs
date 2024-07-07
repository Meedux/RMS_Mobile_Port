using ReceivingManagementSystem.Common.Helpers;
using Xamarin.Forms;

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class OrderReceiptConfirmViewModel : BaseModel
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
        private string _custodyDate;

        /// <summary>
        /// 預り予定日
        /// </summary>
        public string CustodyDate
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
        /// Rfid
        /// </summary>
        private bool _isVisibleRfid;

        /// <summary>
        /// Rfid
        /// </summary>
        public bool IsVisibleRfid
        {
            get { return _isVisibleRfid; }
            set { this.SetProperty(ref this._isVisibleRfid, value); }
        }

        public OrderReceiptConfirmViewModel(ContentPage owner) : base(owner)
        {

        }

        public OrderReceiptConfirmViewModel(OrderReceiptInfoViewModel order) : base()
        {
            _companyName = order.CompanyName;
            _customerName = order.CustomerName;
            _postCode = order.PostCode;
            _address = order.Address;
            _telephoneNumber = order.TelephoneNumber;
            _contents = order.Contents;
            _custodyDate = order.InputDate;
            _companyName = order.CompanyName;
            _rfid = order.Rfid;

            if (!string.IsNullOrEmpty(order.Rfid))
            {
                IsVisibleRfid = true;
            }
        }
    }
}
