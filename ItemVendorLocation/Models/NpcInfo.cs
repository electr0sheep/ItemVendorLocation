using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemVendorLocation.Models
{
    public class NpcInfo
    {
        public uint Id;
        public string Name;
        public List<Tuple<uint, string>> Costs;
        public NpcLocation Location;
    }
}
