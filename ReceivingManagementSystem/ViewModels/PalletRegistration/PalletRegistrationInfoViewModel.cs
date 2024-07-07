using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;
using static RMS_Pleasanter.PalletMaster;

namespace ReceivingManagementSystem.ViewModels.PalletRegistration
{
    public class PalletRegistrationInfoViewModel : BaseModel
    {
        /// <summary>
        /// 商品種別/商品名/品番
        /// </summary>
        private List<ComboBoxItemViewModel> _items;

        /// <summary>
        /// 商品種別/商品名/品番
        /// </summary>
        public List<ComboBoxItemViewModel> Items
        {
            get { return _items; }
            set { this.SetProperty(ref this._items, value); }
        }

        /// <summary>
        /// 商品種別/商品名/品番 selected
        /// </summary>
        private ComboBoxItemViewModel _itemSelected;

        /// <summary>
        /// 商品種別/商品名/品番 selected
        /// </summary>
        public ComboBoxItemViewModel ItemSelected
        {
            get { return _itemSelected; }
            set
            {
                this.SetProperty(ref this._itemSelected, value);
            }
        }

        /// <summary>
        /// パレット番号
        /// </summary>
        private string _palletNumber;

        /// <summary>
        /// パレット番号
        /// </summary>
        public string PalletNumber
        {
            get { return _palletNumber; }
            set { this.SetProperty(ref this._palletNumber, value); }
        }

        /// <summary>
        /// 規格
        /// </summary>
        private string _standard;

        /// <summary>
        /// 規格
        /// </summary>
        public string Standard
        {
            get { return _standard; }
            set { this.SetProperty(ref this._standard, value); }
        }

        /// <summary>
        /// 寸法
        /// </summary>
        private string _dimensions1;

        /// <summary>
        /// 寸法
        /// </summary>
        public string Dimensions1
        {
            get { return _dimensions1; }
            set { this.SetProperty(ref this._dimensions1, value); }
        }

        // 寸法
        /// </summary>
        private string _dimensions2;

        /// <summary>
        /// 寸法
        /// </summary>
        public string Dimensions2
        {
            get { return _dimensions2; }
            set { this.SetProperty(ref this._dimensions2, value); }
        }

        // 寸法
        /// </summary>
        private string _dimensions3;

        /// <summary>
        /// 寸法
        /// </summary>
        public string Dimensions3
        {
            get { return _dimensions3; }
            set { this.SetProperty(ref this._dimensions3, value); }
        }

        /// <summary>
        /// パレット種類
        /// </summary>
        private string _palletKind;

        /// <summary>
        /// パレット種類
        /// </summary>
        public string PalletKind
        {
            get { return _palletKind; }
            set { this.SetProperty(ref this._palletKind, value); }
        }

        /// <summary>
        /// 最大商品個数
        /// </summary>
        private string _maxNumOfItems;

        /// <summary>
        /// 最大商品個数
        /// </summary>
        public string MaxNumOfItems
        {
            get { return _maxNumOfItems; }
            set { this.SetProperty(ref this._maxNumOfItems, value); }
        }

        public PalletRegistrationInfoViewModel() : base()
        {
        }

        public PalletMasterBody GetPalletMasterBody()
        {
            PalletMasterBody palletMasterBody = new PalletMasterBody()
            {
                dimensions = $"{_dimensions1}x{_dimensions2}x{_dimensions3}",
                maxNumOfItems = string.IsNullOrEmpty(_maxNumOfItems) ? (uint?)null: uint.Parse(_maxNumOfItems),
                palletKind = _palletKind,
                palletNumber =_palletNumber,
                standard = _standard,
                itemId = _itemSelected.Value.ToString()
            };

            return palletMasterBody;
        }

    }
}
