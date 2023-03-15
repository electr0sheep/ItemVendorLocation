using FFXIVClientStructs.FFXIV.Client.Game.UI;
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
    }

    public class ItemInfo
    {
        public uint Id;
        public string Name;
        public List<NpcInfo> NpcInfos;
        public ItemType Type;
        public string AchievementDescription;

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
                List<uint> otherGcVendorIds = new();
                unsafe
                {
                    otherGcVendorIds = Service.Plugin.GcVendorIdMap.Values.Where(i => i != Service.Plugin.GcVendorIdMap[UIState.Instance()->PlayerState.GrandCompany]).ToList();
                }
                _ = NpcInfos.RemoveAll(i => otherGcVendorIds.Contains(i.Id));
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
            NpcInfos = NpcInfos.DistinctBy(i => i.Name).ToList();
        }

    }
}