using System;
using System.Collections.Generic;

namespace ItemVendorLocation.Models
{
    public enum ItemType
    {
        GilShop,
        SpecialShop,
        GcShop,
    }

    public class ItemInfo
    {
        public uint Id;
        public string Name;
        public List<NpcInfo> NpcInfos;
        public ItemType Type;

        /*
        public uint CostAmount;
        public string CostItemName;*/
        public List<Tuple<uint, string>> Costs;
    }
}