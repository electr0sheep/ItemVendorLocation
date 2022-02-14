using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ItemVendorLocation
{
    public class Models
    {
        /// <summary>
        /// Models vendor information displayed in the plugin results window
        /// </summary>
        public class Vendor
        {
            public string name = "";
            public MapLinkPayload? mapLink = null;
            public string location = "";
            public ulong cost = 0;
            public string currency = "";

            public Vendor(string name, MapLinkPayload mapLink, string location, ulong cost, string currency)
            {
                this.name = name;
                this.mapLink = mapLink;
                this.location = location;
                this.cost = cost;
                this.currency = currency;
            }
        }

        /// <summary>
        /// Models vendor information displayed in the UITest project
        /// </summary>
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
