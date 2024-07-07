using PleasanterOperation;
using ReceivingManagementSystem.Common.Enums;
using ReceivingManagementSystem.Wrapper;
using RMS_Pleasanter;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static PleasanterOperation.OperationData;
using static RMS_Pleasanter.Client;
using static RMS_Pleasanter.Contents;
using static RMS_Pleasanter.Custody;
using static RMS_Pleasanter.CustodyDetail;
using static RMS_Pleasanter.ItemInventory;
using static RMS_Pleasanter.ItemInventoryCount;
using static RMS_Pleasanter.ItemMaster;
using static RMS_Pleasanter.PalletMaster;
using static RMS_Pleasanter.ReceivingAndShipping;
using static RMS_Pleasanter.ShelfNumber;
using static RMS_Pleasanter.SubscServiceMaster;

namespace ReceivingManagementSystem.Services.Pleasanter
{
    public class PleasanterService : IPleasanterService
    {
        private Custody _custody;
        private CustodyDetail _custodyDetail;
        private ShelfNumber _shelfNumber;
        private Client _client;
        private Contents _contents;
        private PalletMaster _palletMaster;
        private ItemMaster _itemMaster;
        private SubscServiceMaster _subscServiceMaster;
        private ReceivingAndShipping _receivingAndShipping;
        private ItemInventory _itemInventory;
        private ItemInventoryCount _itemInventoryCount;

        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public PleasanterService()
        {
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>(); ;
            GetSetting();
        }

        public void GetSetting()
        {
            string apiKey = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Api_Key, string.Empty);
            string url = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Url, string.Empty);

            long? siteCustomer = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Customer, 0);
            long? siteContent = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Content, 0);
            long? siteCustody = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Custody, 0);
            long? siteCustodyDetail = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Custody_Detail, 0);
            long? siteShelfNumber = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Shelf_Number, 0);

            long? palletMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Pallet_Master, 0);
            long? itemMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Master, 0);
            long? subscServiceMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Subsc_Service_Master, 0);
            long? receivingAndShipping = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Receipt_Shipment, 0);
            long? itemInventory = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Inventory, 0);
            long? itemInventoryCount = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Inventory_Count, 0);

            _custody = new Custody(apiKey, url, siteCustody.Value);
            _custodyDetail = new CustodyDetail(apiKey, url, siteCustodyDetail.Value);
            _shelfNumber = new ShelfNumber(apiKey, url, siteShelfNumber.Value);
            _contents = new Contents(apiKey, url, siteContent.Value);
            _client = new Client(apiKey, url, siteCustomer.Value);
            _palletMaster = new PalletMaster(apiKey, url, palletMaster.Value);
            _itemMaster = new ItemMaster(apiKey, url, itemMaster.Value);
            _subscServiceMaster = new SubscServiceMaster(apiKey, url, subscServiceMaster.Value);
            _receivingAndShipping = new ReceivingAndShipping(apiKey, url, receivingAndShipping.Value);
            _itemInventory = new ItemInventory(apiKey, url, itemInventory.Value);
            _itemInventoryCount = new ItemInventoryCount(apiKey, url, itemInventoryCount.Value);
        }

        public async Task<List<ClientBody>> GetClients(ClientBody clientBody)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _client.Get(clientBody);
                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ClientBody>().ToList();
        }

        public async Task<List<ContentsBody>> GetContents()
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _contents.Get(new ContentsBody());

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ContentsBody>().ToList();
        }

        public async Task<CustodyItemModel> GetCustodyItemByDetail(CustodyDetailBody custodyDetailParams)
        {
            try
            {
                List<DataBody> custodyDetailBodyResults = await _custodyDetail.Get(custodyDetailParams);
                if (custodyDetailBodyResults.Count == 0)
                {
                    return null;
                }

                CustodyDetailBody custodyDetailBody = (CustodyDetailBody)custodyDetailBodyResults[0];

                CustodyBody custodyBodyParams = new CustodyBody()
                {
                    code = custodyDetailBody.code
                };

                List<DataBody> custodyResult = await _custody.Get(custodyBodyParams);

                if (custodyResult.Count == 0)
                {
                    return null;
                }

                CustodyBody custodyBody = (CustodyBody)custodyResult[0];

                return new CustodyItemModel()
                {
                    Custody = custodyBody,
                    CustodyDetail = custodyDetailBody
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
            return null;
        }

        public async Task<ShelfNumberBody> GetShelfNumberByRfid(string rfid)
        {
            try
            {
                ShelfNumberBody shelfNumberParams = new ShelfNumberBody()
                {
                    shelfRfid = rfid
                };

                List<DataBody> results = await _shelfNumber.Get(shelfNumberParams);

                if (results.Count == 0)
                {
                    return null;
                }

                return (ShelfNumberBody)results[0];
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return null;
        }

        public async Task<bool> UpdateCustodyDetail(CustodyDetailBody custodyDetailBody)
        {
            decimal? result = await _custodyDetail.Update(custodyDetailBody);

            return result == null || result == 0 ? false : true;
        }

        public async Task InitData()
        {
            try
            {
                var contents = await GetContents();
                if (contents.Count == 0)
                {
                    await _contents.Create(new ContentsBody() { contents = "Content1" });
                    await _contents.Create(new ContentsBody() { contents = "Content2" });
                }

                var clients = await _client.Get(new ClientBody());
                if (clients.Count == 0)
                {
                    await _client.Create(new ClientBody()
                    {
                        address = "Address1",
                        code = "Code1",
                        companyName = "Company1",
                        customerName = "Customer1",
                        emailAddress = "Email1",
                        postCode = "10000",
                        telephoneNumber = "0988888888"
                    });

                    await _client.Create(new ClientBody()
                    {
                        address = "Address2",
                        code = "Code2",
                        companyName = "Company2",
                        customerName = "Customer2",
                        emailAddress = "Email2",
                        postCode = "10000",
                        telephoneNumber = "0944444444"
                    });
                }

                var shelfNumbers = await _shelfNumber.Get(new ShelfNumberBody());
                if (shelfNumbers.Count == 0)
                {
                    await _shelfNumber.Create(new ShelfNumberBody()
                    {
                        shelfNumber = "A1",
                        shelfRfid = "SH0001"
                    });

                    await _shelfNumber.Create(new ShelfNumberBody()
                    {
                        shelfNumber = "A2",
                        shelfRfid = "SH0002"
                    });
                    await _shelfNumber.Create(new ShelfNumberBody()
                    {
                        shelfNumber = "A3",
                        shelfRfid = "SH0003"
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }

        public async Task<decimal?> CreateCustody(CustodyBody custodyBody)
        {
            try
            {
                return await _custody.Create(custodyBody);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        public async Task<decimal?> CreateCustodyDetail(CustodyDetailBody custodyDetailBody)
        {
            try
            {
                return await _custodyDetail.Create(custodyDetailBody);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        public async Task<List<CustodyItemModel>> GetCustody(CustodyBody custodyBodyParams, CustodyDetailBody custodyDetailParams)
        {
            List<CustodyItemModel> items = new List<CustodyItemModel>();
            try
            {
                List<DataBody> custodyDetailResults = await _custodyDetail.Get(custodyDetailParams);
                if (custodyDetailResults.Count == 0)
                {
                    return items;
                }

                List<DataBody> custodyResults = await _custody.Get(custodyBodyParams);

                if (custodyResults.Count == 0)
                {
                    return items;
                }

                var custodyBodyResults = custodyResults.Cast<CustodyBody>().ToList();


                CustodyDetailBody custodyDetailBody;
                CustodyBody custodyBody;
                foreach (DataBody dataBody in custodyDetailResults)
                {
                    custodyDetailBody = (CustodyDetailBody)dataBody;

                    custodyBody = custodyBodyResults.FirstOrDefault(r => r.code.Equals(custodyDetailBody.code));

                    if (custodyBody == null)
                    {
                        continue;
                    }

                    items.Add(new CustodyItemModel
                    {
                        Custody = custodyBody,
                        CustodyDetail = custodyDetailBody
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items;
        }

        public async Task<List<ShelfNumberBody>> GetShelfNumbers()
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _shelfNumber.Get(new ShelfNumberBody());

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ShelfNumberBody>().ToList();
        }

        public async Task<decimal?> DeleteCustody(CustodyBody custodyBody)
        {
            try
            {
                return await _custody.Delete(custodyBody);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        public async Task<List<CustodyDetailBody>> GetCustodyDetail(CustodyDetailBody custodyDetailBody)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _custodyDetail.Get(custodyDetailBody);

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<CustodyDetailBody>().ToList();
        }

        public async Task<List<CustodyBody>> SearchCustody(List<SearchInfoModel> searchInfo)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _custody.Search(GetColumnFilterHash(searchInfo, Custody.GetColumnNames(), _custody.SelectColumns()), GetColumnFilterSearchTypes(searchInfo, Custody.GetColumnNames()));

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<CustodyBody>().ToList();
        }

        public async Task<decimal?> CreatePalletMaster(PalletMaster.PalletMasterBody palletMasterBody)
        {
            try
            {
                return await _palletMaster.Create(palletMasterBody);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        public async Task<List<ItemBody>> GetItems()
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _itemMaster.Get(new ItemBody());

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ItemBody>().ToList();
        }

        public async Task<List<ItemBody>> GetItems(ItemBody itemBody)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _itemMaster.Get(itemBody);

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ItemBody>().ToList();
        }

        public async Task<List<PalletMasterBody>> GetPallets()
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _palletMaster.Get(new PalletMasterBody());

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<PalletMasterBody>().ToList();
        }

        public async Task<List<SubscServiceMasterBody>> GetServices()
        {
            List<DataBody> services = new List<DataBody>();
            try
            {
                services = await _subscServiceMaster.Get(new SubscServiceMasterBody());

                if (services == null)
                {
                    services = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return services.Cast<SubscServiceMasterBody>().ToList();
        }

        public async Task<decimal?> CreateReceivingAndShipping(ReceivingAndShippingBody receivingAndShippingBody)
        {
            try
            {
                return await _receivingAndShipping.Create(receivingAndShippingBody);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        public async Task<PalletMasterBody> GetPalletById(decimal Id)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _palletMaster.Get(Id);

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<PalletMasterBody>().First();
        }

        public async Task<List<PalletMasterBody>> GetPalletByItemId(string itemId)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _palletMaster.Get(new PalletMasterBody()
                {
                    itemId = itemId
                });

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<PalletMasterBody>().ToList();
        }

        public async Task<List<ItemInventoryBody>> GetItemInventoryByItemIdAndRfid(string itemId, string rfid)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _itemInventory.Get(new ItemInventoryBody()
                {
                    itemId = itemId,
                    RFID = rfid
                });

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ItemInventoryBody>().ToList();
        }

        public async Task<List<ItemInventoryBody>> GetItemInventoryByRfid(string rfid)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _itemInventory.Get(new ItemInventoryBody()
                {
                    RFID = rfid
                });

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ItemInventoryBody>().ToList();
        }

        public async Task<bool> UpdateItemInventory(ItemInventoryBody itemInventoryBody)
        {
            try
            {
                decimal? result = await _itemInventory.Update(itemInventoryBody);

                return result == null || result == 0 ? false : true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);

                return false;
            }
        }

        public async Task<decimal?> CreateItemInventory(ItemInventoryBody itemInventoryBody)
        {
            try
            {
                return await _itemInventory.Create(itemInventoryBody);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        public async Task<List<ItemInventoryBody>> GetItemInventory()
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _itemInventory.Get(new ItemInventoryBody());

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ItemInventoryBody>().ToList();
        }

        public async Task<List<ItemBody>> SearchItems(List<SearchInfoModel> searchInfo)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _itemMaster.Search(GetColumnFilterHash(searchInfo, ItemMaster.GetColumnNames(), _itemMaster.SelectColumns()), GetColumnFilterSearchTypes(searchInfo, ItemMaster.GetColumnNames()));

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ItemBody>().ToList();
        }

        public async Task<List<PalletMasterBody>> SearchPallets(List<SearchInfoModel> searchInfo)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _palletMaster.Search(GetColumnFilterHash(searchInfo, PalletMaster.GetColumnNames(), _palletMaster.SelectColumns()), GetColumnFilterSearchTypes(searchInfo, ItemMaster.GetColumnNames()));

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<PalletMasterBody>().ToList();
        }

        public async Task<List<ItemInventoryCountBody>> SearchItemInventoryCounts(List<SearchInfoModel> searchInfo)
        {
            List<DataBody> items = new List<DataBody>();
            try
            {
                items = await _itemInventoryCount.Search(GetColumnFilterHash(searchInfo, ItemInventoryCount.GetColumnNames(), _itemInventoryCount.SelectColumns()), GetColumnFilterSearchTypes(searchInfo, ItemMaster.GetColumnNames()));

                if (items == null)
                {
                    items = new List<DataBody>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items.Cast<ItemInventoryCountBody>().ToList();
        }

        public async Task<bool> UpdateItemInventoryCount(ItemInventoryCountBody itemInventoryCountBody)
        {
            try
            {
                decimal? result = await _itemInventoryCount.Update(itemInventoryCountBody);

                return result == null || result == 0 ? false : true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);

                return false;
            }
        }

        public async Task<decimal?> CreateItemInventoryCount(ItemInventoryCountBody itemInventoryCountBody)
        {
            try
            {
                return await _itemInventoryCount.Create(itemInventoryCountBody);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);

                return null;
            }
        }

        private Dictionary<string, string> GetColumnFilterHash(List<SearchInfoModel> searchInfos, Dictionary<string, string> columnNames, Dictionary<string, bool> selectColumns)
        {
            Dictionary<string, string> ColumnFilterHash = new Dictionary<string, string>();

            string value;
            foreach (var searchInfo in searchInfos)
            {
                if ((searchInfo.Value != null && !string.IsNullOrEmpty(searchInfo.Value.ToString())) || searchInfo.Values.Count > 0)
                {
                    value = GetValueSearch(searchInfo, columnNames, selectColumns);

                    if (!string.IsNullOrEmpty(value))
                    {
                        ColumnFilterHash.Add(columnNames[searchInfo.FieldName], value);
                    }
                }
            }

            return ColumnFilterHash;
        }

        private string GetValueSearch(SearchInfoModel searchInfo, Dictionary<string, string> columnNames, Dictionary<string, bool> selectColumns)
        {
            switch (searchInfo.SearchType)
            {
                case SearchInfoModel.SearchTypeEnum.Equals:
                    {
                        if (selectColumns != null && selectColumns.ContainsKey(columnNames[searchInfo.FieldName]) && selectColumns[columnNames[searchInfo.FieldName]])
                        {
                            return $"[\"{searchInfo.Value.ToString()}\"]";
                        }
                        else
                        {
                            return searchInfo.Value.ToString();
                        }
                    }
                case SearchInfoModel.SearchTypeEnum.In:
                    {
                        return GenInValueSearch(searchInfo, columnNames, selectColumns);
                    }
                case SearchInfoModel.SearchTypeEnum.FromTo:
                    {
                        return $"[\"{string.Join(",", searchInfo.Values)}\"]";
                    }
                case SearchInfoModel.SearchTypeEnum.Contains:
                    {
                        return searchInfo.Value.ToString();
                    }
                default:
                    break;
            }

            return string.Empty;
        }

        private string GenInValueSearch(SearchInfoModel searchInfo, Dictionary<string, string> columnNames, Dictionary<string, bool> selectColumns)
        {
            switch (searchInfo.ValueType)
            {
                case SearchInfoModel.ValueTypeEnum.String:
                    {
                        if (selectColumns != null && selectColumns.ContainsKey(columnNames[searchInfo.FieldName]) && selectColumns[columnNames[searchInfo.FieldName]])
                        {
                            return $"[\"{string.Join("\",\"", searchInfo.Values)}\"]";
                        }
                        else
                        {
                            return $"[{string.Join(",", searchInfo.Values)}]";
                        }
                    }
                case SearchInfoModel.ValueTypeEnum.Int:
                    {
                        return $"[{string.Join(",", searchInfo.Values)}]";
                    }
                default:
                    break;
            }

            return string.Empty;
        }

        private Dictionary<string, eColumnFilterSearchType> GetColumnFilterSearchTypes(List<SearchInfoModel> searchInfos, Dictionary<string, string> columnNames)
        {
            Dictionary<string, eColumnFilterSearchType> ColumnFilterSearchTypes = new Dictionary<string, eColumnFilterSearchType>();
            foreach (var searchInfo in searchInfos)
            {
                if ((searchInfo.Value != null && !string.IsNullOrEmpty(searchInfo.Value.ToString())) || searchInfo.Values.Count > 0)
                {
                    switch (searchInfo.SearchType)
                    {
                        case SearchInfoModel.SearchTypeEnum.Equals:
                        case SearchInfoModel.SearchTypeEnum.In:
                            {
                                ColumnFilterSearchTypes.Add(columnNames[searchInfo.FieldName], eColumnFilterSearchType.ExactMatch);
                                break;
                            }
                        case SearchInfoModel.SearchTypeEnum.Contains:
                            {
                                ColumnFilterSearchTypes.Add(columnNames[searchInfo.FieldName], eColumnFilterSearchType.PartialMatch);
                                break;
                            }
                    }
                }

            }

            return ColumnFilterSearchTypes;
        }

        public bool CheckSetting(params PleasanterObjectTypeEnum[] objectTypes)
        {
            string apiKey = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Api_Key, string.Empty);
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            string url = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Url, string.Empty);
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            foreach (var objectType in objectTypes)
            {
                switch (objectType)
                {
                    case PleasanterObjectTypeEnum.Contents:
                        {
                            long? siteContent = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Content, null);

                            if (!siteContent.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.Client:
                        {
                            long? siteCustomer = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Customer, null);
                            if (!siteCustomer.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.Item:
                        {
                            long? itemMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Master, null);
                            if (!itemMaster.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.ItemInventoryCount:
                        {
                            long? itemInventoryCount = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Inventory_Count, null);
                            if (!itemInventoryCount.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.ItemInventory:
                        {
                            long? itemInventory = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Item_Inventory, null);
                            if (!itemInventory.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.Custody:
                        {
                            long? siteCustody = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Custody, null);
                            if (!siteCustody.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.CustodyDetail:
                        {
                            long? siteCustodyDetail = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Custody_Detail, null);
                            if (!siteCustodyDetail.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.PalletMaster:
                        {
                            long? palletMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Pallet_Master, null);
                            if (!palletMaster.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.ShelfNumber:
                        {
                            long? siteShelfNumber = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Shelf_Number, null);
                            if (!siteShelfNumber.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.SubscServiceMaster:
                        {
                            long? subscServiceMaster = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Subsc_Service_Master, null);
                            if (!subscServiceMaster.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                    case PleasanterObjectTypeEnum.ReceivingAndShipping:
                        {
                            long? receivingAndShipping = _pSaveSettingsWrapper.GetLong(ReceivingManagementSystem.Common.Constants.Setting_Pleasanter_Site_Receipt_Shipment, null);
                            if (!receivingAndShipping.HasValue)
                            {
                                return false;
                            }
                            break;
                        }
                }
            }

            return true;
        }

        public async Task<List<CustodyItemModel>> SearchCustodyAndDetail(List<SearchInfoModel> searchCustodyInfo, List<SearchInfoModel> searchCustodyDetailInfo)
        {
            List<CustodyItemModel> items = new List<CustodyItemModel>();
            try
            {
                List<DataBody> custodyDetailResults = await _custodyDetail.Search(GetColumnFilterHash(searchCustodyDetailInfo, CustodyDetail.GetColumnNames(), _custodyDetail.SelectColumns()), GetColumnFilterSearchTypes(searchCustodyDetailInfo, CustodyDetail.GetColumnNames()));

                if (custodyDetailResults.Count == 0)
                {
                    return items;
                }

                List<DataBody> custodyResults = await _custody.Search(GetColumnFilterHash(searchCustodyInfo, Custody.GetColumnNames(), _custody.SelectColumns()), GetColumnFilterSearchTypes(searchCustodyInfo, Custody.GetColumnNames()));

                if (custodyResults.Count == 0)
                {
                    return items;
                }

                var custodyBodyResults = custodyResults.Cast<CustodyBody>().ToList();


                CustodyDetailBody custodyDetailBody;
                CustodyBody custodyBody;
                foreach (DataBody dataBody in custodyDetailResults)
                {
                    custodyDetailBody = (CustodyDetailBody)dataBody;

                    custodyBody = custodyBodyResults.FirstOrDefault(r => r.code.Equals(custodyDetailBody.code));

                    if (custodyBody == null)
                    {
                        continue;
                    }

                    items.Add(new CustodyItemModel
                    {
                        Custody = custodyBody,
                        CustodyDetail = custodyDetailBody
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

            return items;
        }

        public async Task<CustodyBody> GetCustodyById(decimal id)
        {
            try
            {
                List<DataBody> custody = await _custody.Get(id);

                if (custody.Count == 0)
                {
                    return null;
                }

                return (CustodyBody)custody[0];
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }
    }
}
