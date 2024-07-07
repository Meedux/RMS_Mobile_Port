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
    public class PleasanterSettingParamViewModel : BaseModel
    {
        /// <summary>
        /// プリザンターURL
        /// </summary>
        private string _url;

        /// <summary>
        /// プリザンターURL
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { this.SetProperty(ref this._url, value); }
        }

        /// <summary>
        /// APIキー
        /// </summary>
        private string _apiKey;

        /// <summary>
        /// APIキー
        /// </summary>
        public string ApiKey
        {
            get { return _apiKey; }
            set { this.SetProperty(ref this._apiKey, value); }
        }

        /// <summary>
        /// サイトID
        /// </summary>
        private string _siteId;

        /// <summary>
        /// サイトID
        /// </summary>
        public string SiteId
        {
            get { return _siteId; }
            set { this.SetProperty(ref this._siteId, value); }
        }

        /// <summary>
        /// 預り
        /// </summary>
        private string _custody;

        /// <summary>
        /// 預り
        /// </summary>
        public string Custody
        {
            get { return _custody; }
            set { this.SetProperty(ref this._custody, value); }
        }

        /// <summary>
        /// 預り明細
        /// </summary>
        private string _custodyDetail;

        /// <summary>
        /// 預り明細
        /// </summary>
        public string CustodyDetail
        {
            get { return _custodyDetail; }
            set { this.SetProperty(ref this._custodyDetail, value); }
        }

        /// <summary>
        /// 得意先
        /// </summary>
        private string _customer;

        /// <summary>
        /// 得意先
        /// </summary>
        public string Customer
        {
            get { return _customer; }
            set { this.SetProperty(ref this._customer, value); }
        }

        /// <summary>
        /// 得意先
        /// </summary>
        private string _shelfNumber;

        /// <summary>
        /// 得意先
        /// </summary>
        public string ShelfNumber
        {
            get { return _shelfNumber; }
            set { this.SetProperty(ref this._shelfNumber, value); }
        }

        /// <summary>
        /// 内容
        /// </summary>
        private string _content;

        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get { return _content; }
            set { this.SetProperty(ref this._content, value); }
        }

        /// <summary>
        /// 商品マスタ
        /// </summary>
        private string _itemMaster;

        /// <summary>
        /// 商品マスタ
        /// </summary>
        public string ItemMaster
        {
            get { return _itemMaster; }
            set { this.SetProperty(ref this._itemMaster, value); }
        }

        /// <summary>
        /// パレットマスタ
        /// </summary>
        private string _palletMaster;

        /// <summary>
        /// パレットマスタ
        /// </summary>
        public string PalletMaster
        {
            get { return _palletMaster; }
            set { this.SetProperty(ref this._palletMaster, value); }
        }

        /// <summary>
        /// 定期便マスタ
        /// </summary>
        private string _subscServiceMaster;

        /// <summary>
        /// 定期便マスタ
        /// </summary>
        public string SubscServiceMaster
        {
            get { return _subscServiceMaster; }
            set { this.SetProperty(ref this._subscServiceMaster, value); }
        }

        /// <summary>
        /// 入出荷
        /// </summary>
        private string _receivingAndShipping;

        /// <summary>
        /// 入出荷
        /// </summary>
        public string ReceivingAndShipping
        {
            get { return _receivingAndShipping; }
            set { this.SetProperty(ref this._receivingAndShipping, value); }
        }

        /// <summary>
        /// 商品在庫
        /// </summary>
        private string _itemInventory;

        /// <summary>
        /// 商品在庫
        /// </summary>
        public string ItemInventory
        {
            get { return _itemInventory; }
            set { this.SetProperty(ref this._itemInventory, value); }
        }

        /// <summary>
        /// 棚卸
        /// </summary>
        private string _itemInventoryCount;

        /// <summary>
        /// 棚卸
        /// </summary>
        public string ItemInventoryCount
        {
            get { return _itemInventoryCount; }
            set { this.SetProperty(ref this._itemInventoryCount, value); }
        }

        public PleasanterSettingParamViewModel()
        {

        }
    }
}
