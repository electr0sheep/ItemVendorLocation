using Dalamud.Logging;
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

        public void FilterGCResults()
        {
            if (Service.Configuration.FilterGCResults)
            {
                // filter gc vendors that accept gc seals
                List<uint> otherGcVendorIds = new();
                unsafe
                {
                    byte playerGC = UIState.Instance()->PlayerState.GrandCompany;
                    otherGcVendorIds = Service.Plugin.GcVendorIdMap.Values.Where(i => i != Service.Plugin.GcVendorIdMap[playerGC]).ToList();
                }
                _ = NpcInfos.RemoveAll(i => otherGcVendorIds.Contains(i.Id));

                // filter fc gc vendors
                List<uint> otherOicVendorIds = new();
                unsafe
                {
                    InfoProxyInterface* infoProxy = Framework.Instance()->UIModule->GetInfoModule()->GetInfoProxyById(InfoProxyId.FreeCompany);
                    if (infoProxy != null)
                    {
                        InfoProxyFreeCompany* freeCompanyInfoProxy = (InfoProxyFreeCompany*)infoProxy;
                        GrandCompany playerFreeCompanyGC = *(&freeCompanyInfoProxy->GrandCompany + 0x25);
                        otherOicVendorIds = Service.Plugin.OicVendorIdMap.Values.Where(i => i != Service.Plugin.OicVendorIdMap[playerFreeCompanyGC]).ToList();
                    }
                }
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