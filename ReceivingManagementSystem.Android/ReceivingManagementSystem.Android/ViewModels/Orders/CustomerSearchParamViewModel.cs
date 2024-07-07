using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using static RMS_Pleasanter.Client;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;

namespace ReceivingManagementSystem.Android.ViewModels.Orders
{
    public class CustomerSearchParamViewModel : BaseModel
    {
        /// <summary>
        /// 得意先コード
        /// </summary>
        private string _customerCode;

        /// <summary>
        /// 得意先コード
        /// </summary>
        public string CustomerCode
        {
            get { return _customerCode; }
            set { this.SetProperty(ref this._customerCode, value); }
        }

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
        /// メールアドレス
        /// </summary>
        private string _mailAddress;

        /// <summary>
        /// メールアドレス
        /// </summary>
        public string MailAddress
        {
            get { return _mailAddress; }
            set { this.SetProperty(ref this._mailAddress, value); }
        }

        public ClientBody GetClientBody()
        {
            ClientBody custodyBody = new ClientBody()
            {
                address = _address,
                companyName = _companyName,
                customerName = _customerName,
                postCode = _postCode,
                telephoneNumber = _telephoneNumber,
                code = _customerCode,
                emailAddress = _mailAddress
            };

            return custodyBody;
        }

        public CustomerSearchParamViewModel()
        {

        }

        public CustomerSearchParamViewModel(ClientBody clientBody)
        {
            _address = clientBody.address;
            _companyName = clientBody.companyName;
            _customerName = clientBody.customerName;
            _postCode = clientBody.postCode;
            _telephoneNumber = clientBody.telephoneNumber;
            CustomerCode = clientBody.code;
            _mailAddress = clientBody.emailAddress;
        }
    }
}
