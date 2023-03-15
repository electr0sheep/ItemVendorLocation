using System;
using System.Collections.Generic;

namespace ItemVendorLocation.Models
{
    public class NpcInfo
    {
        public uint Id;
        public string Name;
        public string ShopName;
        public List<Tuple<uint, string>> Costs;
        public NpcLocation Location;
    }
}
