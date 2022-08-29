using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;

namespace ItemVendorLocation
{
    public class Models
    {
        /// <summary>
        /// Models currency information displayed in the plugin results window
        /// </summary>
        public class Currency
        {
            public string name;
            public ulong cost;

            public Currency(string name, ulong cost)
            {
                this.name = name;
                this.cost = cost;
            }
        }

        /// <summary>
        /// Models vendor information displayed in the plugin results window
        /// </summary>
        public class Vendor
        {
            public string name = "";
            public MapLinkPayload? mapLink = null;
            public string location = "";
            public List<Currency> currencies;

            public Vendor(string name, MapLinkPayload? mapLink, string location, List<Currency> currencies)
            {
                this.name = name;
                this.mapLink = mapLink;
                this.location = location;
                this.currencies = currencies;
            }
        }
    }
}
