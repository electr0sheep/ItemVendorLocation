using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System.Collections.Generic;
using System.Linq;

namespace ItemVendorLocation.Models
{
    public enum ItemType
    {
        GilShop,
        SpecialShop,
        GcShop,
        Achievement,
        FcShop,
        QuestReward,
        CollectableExchange,
    }

    public class ItemInfo
    {
        public uint Id;
        public string Name;
        public List<NpcInfo> NpcInfos;
        public ItemType Type;
        public string AchievementDescription;

        public bool HasShopNames()
        {
            return NpcInfos.Any(i => i.ShopName != null);
        }

        public void ApplyFilters()
        {
            FilterDuplicates();
            FilterNoLocationNPCs();
            FilterGCResults();
        }

        public unsafe void FilterGCResults()
        {
            if (!Service.Configuration.FilterGCResults)
            {
                return;
            }

            // filter gc vendors that accept gc seals
            // we remove non player affiliated gc vendors rather thank keeping player affiliated gc vendors
            // because there could be other vendors in the list
            var playerGC = UIState.Instance()->PlayerState.GrandCompany;
            var otherGcVendorIds = Dictionaries.GcVendorIdMap.Values.Where(i => i != Dictionaries.GcVendorIdMap[playerGC]);
            // Only remove items if doing so doesn't remove all the results
            if (NpcInfos.Any(i => !otherGcVendorIds.Contains(i.Id)))
            {
                _ = NpcInfos.RemoveAll(i => otherGcVendorIds.Contains(i.Id));
            }

            // filter fc gc vendors
            var infoProxy = Framework.Instance()->UIModule->GetInfoModule()->GetInfoProxyById(InfoProxyId.FreeCompany);
            if (infoProxy == null)
            {
                return;
            }

            var freeCompanyInfoProxy = (InfoProxyFreeCompany*)infoProxy;
            var playerFreeCompanyGC = freeCompanyInfoProxy->GrandCompany;
            var otherOicVendorIds = Dictionaries.OicVendorIdMap.Values.Where(i => i != Dictionaries.OicVendorIdMap[playerFreeCompanyGC]);

            if (otherOicVendorIds != null && NpcInfos.Any(i => !otherOicVendorIds.Contains(i.Id)))
            {
                _ = NpcInfos.RemoveAll(i => otherOicVendorIds.Contains(i.Id));
            }
        }

        public void FilterNoLocationNPCs()
        {
            if (Service.Configuration.FilterNPCsWithNoLocation)
            {
                _ = NpcInfos.RemoveAll(i => i.Location == null);
            }
        }

        public void FilterDuplicates()
        {
            if (Service.Configuration.FilterDuplicates)
            {
                NpcInfos = NpcInfos.GroupBy(i => new { i.Name, i.Location?.TerritoryType, i.Location?.X, i.Location?.Y }).Select(i => i.First()).ToList();
            }
        }
    }
}