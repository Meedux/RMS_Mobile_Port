using PleasanterOperation;
using ReceivingManagementSystem.Common.Enums;
using RMS_Pleasanter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    public interface IPleasanterService
    {
        Task<List<CustodyItemModel>> GetCustody(CustodyBody custodyBodyParams, CustodyDetailBody custodyDetailParams);
        Task<CustodyItemModel> GetCustodyItemByDetail(CustodyDetailBody custodyDetailParams);
        Task<List<CustodyDetailBody>> GetCustodyDetail(CustodyDetailBody custodyDetailBody);

        Task<ShelfNumberBody> GetShelfNumberByRfid(string rfid);

        Task<bool> UpdateCustodyDetail(CustodyDetailBody custodyDetailBody);

        Task<List<ClientBody>> GetClients(ClientBody clientBody);
        Task<List<ContentsBody>> GetContents();
        Task<List<ShelfNumberBody>> GetShelfNumbers();
        void GetSetting();
        Task InitData();
        Task<decimal?> CreateCustody(CustodyBody custodyBody);
        Task<CustodyBody> GetCustodyById(decimal id);
        Task<decimal?> CreateCustodyDetail(CustodyDetailBody custodyDetailBody);


        Task<decimal?> DeleteCustody(CustodyBody custodyBody);
        Task<List<CustodyBody>> SearchCustody(List<SearchInfoModel> searchInfo);
        Task<decimal?> CreatePalletMaster(PalletMasterBody palletMasterBody);
        Task<List<ItemBody>> GetItems();
        Task<List<ItemBody>> GetItems(ItemBody itemBody);
        Task<List<PalletMasterBody>> GetPallets();
        Task<PalletMasterBody> GetPalletById(decimal Id);
        Task<List<PalletMasterBody>> GetPalletByItemId(string itemId);
        Task<List<SubscServiceMasterBody>> GetServices();
        Task<decimal?> CreateReceivingAndShipping(ReceivingAndShippingBody peceivingAndShippingBody);
        Task<List<ItemInventoryBody>> GetItemInventoryByItemIdAndRfid(string itemId, string rfid);
        Task<List<ItemInventoryBody>> GetItemInventoryByRfid(string rfid);
        Task<bool> UpdateItemInventory(ItemInventoryBody itemInventoryBody);
        Task<decimal?> CreateItemInventory(ItemInventoryBody itemInventoryBody);
        Task<List<ItemInventoryBody>> GetItemInventory();
        Task<List<ItemBody>> SearchItems(List<SearchInfoModel> searchInfo);
        Task<List<PalletMasterBody>> SearchPallets(List<SearchInfoModel> searchInfo);
        Task<List<ItemInventoryCountBody>> SearchItemInventoryCounts(List<SearchInfoModel> searchInfo);
        Task<bool> UpdateItemInventoryCount(ItemInventoryCountBody itemInventoryBody);
        Task<decimal?> CreateItemInventoryCount(ItemInventoryCountBody itemInventoryBody);

        bool CheckSetting(params PleasanterObjectTypeEnum[] objectTypes);
        Task<List<CustodyItemModel>> SearchCustodyAndDetail (List<SearchInfoModel> searchCustodyInfo, List<SearchInfoModel> searchCustodyDetailInfo);
    }
}
