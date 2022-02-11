using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ItemVendorLocation
{
    public class Models
    {
        public class Vendor
        {
            public string? name = null;
            public MapLinkPayload? location = null;
            public ulong? cost = null;
            public string? currency = null;

            public Vendor(string name, MapLinkPayload location, ulong cost, string currency)
            {
                this.name = name;
                this.location = location;
                this.cost = cost;
                this.currency = currency;
            }
        }

        public class TestVendor
        {
            public string? name = null;
            public string? location = null;
            public ulong? cost = null;
            public string? currency = null;

            public TestVendor(string name, string location, ulong cost, string currency)
            {
                this.name = name;
                this.location = location;
                this.cost = cost;
                this.currency = currency;
            }
        }
    }
}
