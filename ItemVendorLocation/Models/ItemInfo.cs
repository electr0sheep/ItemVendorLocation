using System;
using System.Collections.Generic;

namespace ItemVendorLocation.Models
{
    public enum ItemType
    {
        GilShop,
        SpecialShop,
        GcShop,
        FccShop,
        Achievement,
    }

    public class ItemInfo
    {
        public uint Id;
        public string Name;
        public List<NpcInfo> NpcInfos;
        public ItemType Type;
        public List<Tuple<uint, string>> Costs;
        public string AchievementDescription;
    }
}