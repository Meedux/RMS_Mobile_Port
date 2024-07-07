using ReceivingManagementSystem.Common;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Common.Helpers;
using ReceivingManagementSystem.Android.Services.Pleasanter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;
using static RMS_Pleasanter.PalletMaster;
using static RMS_Pleasanter.ReceivingAndShipping;

namespace ReceivingManagementSystem.Android.ViewModels.Shipping
{
    public class ShippingInfoViewModel : BaseModel
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
                _ = GetPallet();
            }
        }

        /// <summary>
        /// 商品種別/商品名/品番
        /// </summary>
        private List<ComboBoxItemViewModel> _pallets;

        /// <summary>
        /// 商品種別/商品名/品番
        /// </summary>
        public List<ComboBoxItemViewModel> Pallets
        {
            get { return _pallets; }
            set { this.SetProperty(ref this._pallets, value); }
        }

        /// <summary>
        /// 商品種別/商品名/品番 selected
        /// </summary>
        private ComboBoxItemViewModel _palletSelected;

        /// <summary>
        /// 商品種別/商品名/品番 selected
        /// </summary>
        public ComboBoxItemViewModel PalletSelected
        {
            get { return _palletSelected; }
            set
            {
                this.SetProperty(ref this._palletSelected, value);
            }
        }

        /// 商品種別/商品名/品番
        /// </summary>
        private List<ComboBoxItemViewModel> _services;

        /// <summary>
        /// 商品種別/商品名/品番
        /// </summary>
        public List<ComboBoxItemViewModel> Services
        {
            get { return _services; }
            set { this.SetProperty(ref this._services, value); }
        }

        /// <summary>
        /// 商品種別/商品名/品番 selected
        /// </summary>
        private ComboBoxItemViewModel _serviceSelected;

        /// <summary>
        /// 商品種別/商品名/品番 selected
        /// </summary>
        public ComboBoxItemViewModel ServiceSelected
        {
            get { return _serviceSelected; }
            set
            {
                this.SetProperty(ref this._serviceSelected, value);
            }
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
        /// 出荷数
        /// </summary>
        private uint? _shippedNumber;

        /// <summary>
        /// 出荷数
        /// </summary>
        public uint? ShippedNumber
        {
            get { return _shippedNumber; }
            set { this.SetProperty(ref this._shippedNumber, value); }
        }

        /// <summary>
        /// 発地
        /// </summary>
        private string _origin;

        /// <summary>
        /// 発地
        /// </summary>
        public string Origin
        {
            get { return _origin; }
            set { this.SetProperty(ref this._origin, value); }
        }

        /// <summary>
        // 着地
        /// </summary>
        private string _destination;

        /// <summary>
        /// 着地
        /// </summary>
        public string Destination
        {
            get { return _destination; }
            set { this.SetProperty(ref this._destination, value); }
        }

        /// <summary>
        // デバンニング(20ft)
        /// </summary>
        private bool _devanning20;

        /// <summary>
        /// デバンニング(20ft)
        /// </summary>
        public bool Devanning20
        {
            get { return _devanning20; }
            set { this.SetProperty(ref this._devanning20, value); }
        }

        /// <summary>
        // デバンニング(40ft)
        /// </summary>
        private bool _devanning40;

        /// <summary>
        /// デバンニング(40ft)
        /// </summary>
        public bool Devanning40
        {
            get { return _devanning40; }
            set { this.SetProperty(ref this._devanning40, value); }
        }

        /// <summary>
        // なし
        /// </summary>
        private bool _none;

        /// <summary>
        /// なし
        /// </summary>
        public bool None
        {
            get { return _none; }
            set { this.SetProperty(ref this._none, value); }
        }

        /// <summary>
        /// 高速代金
        /// </summary>
        private uint? _highwayCharges;

        /// <summary>
        /// 高速代金
        /// </summary>
        public uint? HighwayCharges
        {
            get { return _highwayCharges; }
            set { this.SetProperty(ref this._highwayCharges, value); }
        }

        /// <summary>
        /// 配送料
        /// </summary>
        private uint? _shippingCharges;

        /// <summary>
        /// 配送料
        /// </summary>
        public uint? ShippingCharges
        {
            get { return _shippingCharges; }
            set { this.SetProperty(ref this._shippingCharges, value); }
        }

        private IPleasanterService _pleasanterService;

        public ShippingInfoViewModel() : base()
        {
            _none = true;
            _pleasanterService = DependencyService.Get<IPleasanterService>();
        }

        public async Task GetPallet()
        {
            Pallets = new List<ComboBoxItemViewModel>();
            if (_itemSelected == null)
            {
                return;
            }

            var pallets = await _pleasanterService.GetPalletByItemId(_itemSelected.Id.ToString());

            Pallets = pallets.Select(s => new ComboBoxItemViewModel()
            {
                DisplayValue = s.palletNumber,
                Value = s.palletNumber,
                Id = s.id.Value
            }).ToList();
        }

        public ReceivingAndShippingBody GetReceivingAndShipping()
        {
            ReceivingAndShippingBody receivingAndShippingBody = new ReceivingAndShippingBody()
            {
                destination = _destination,
                devanning_20 = _devanning20,
                devanning_40 = _devanning40,
                highwayCharges = _highwayCharges,
                origin = _origin,
                shippedNumber = _shippedNumber,
                shippingCharges = _shippingCharges,
                RFID = _rfid,
                itemId = _itemSelected.Id.ToString(),
                date = DateTime.Now
            };

            if (_palletSelected != null)
            {
                receivingAndShippingBody.palletNumber = _palletSelected.Id.ToString();
            }

            if (_serviceSelected != null)
            {
                receivingAndShippingBody.serviceName = _serviceSelected.Id.ToString();
            }

            return receivingAndShippingBody;
        }

    }
}
