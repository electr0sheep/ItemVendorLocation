using System;
using System.Collections.Generic;

namespace DebugOnly
{
    internal class main
    {
        public static void Main()
        {
            string itemName = "Astral Nodule";
            List<GarlandToolsWrapper.Models.ItemSearchResult> items = GarlandToolsWrapper.WebRequests.ItemSearch(itemName);
            GarlandToolsWrapper.Models.ItemSearchResult item = items.Find(i => string.Equals(i.obj.n, itemName, StringComparison.OrdinalIgnoreCase))!;
            List<ItemVendorLocation.Models.Vendor> vendorResults = new();
            vendorResults = GetVendors((ulong)item.obj.i);

            Console.WriteLine("END");
        }

        public static List<ItemVendorLocation.Models.Vendor> GetVendors(ulong itemId)
        {
            //get preliminary data
            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(itemId);
            List<ItemVendorLocation.Models.Vendor> vendorResults = new();

            // gil vendor
            if (itemDetails.item.vendors != null)
            {
                foreach (ulong vendorId in itemDetails.item.vendors)
                {
                    GarlandToolsWrapper.Models.Partial? vendor = itemDetails.partials.Find(i => (ulong)i.obj.i == vendorId);
                    List<ItemVendorLocation.Models.Currency> currencies = new();

                    if (vendor != null)
                    {
                        // get rid of any vendor that doesn't have a location
                        // typically man/maid servants
                        string name = vendor.obj.n;
                        currencies.Add(new ItemVendorLocation.Models.Currency("Gil", itemDetails.item.price));
                        if (vendor.obj.l == null)
                        {
                            vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, "No Location", currencies));
                            break;
                        }

                        string location = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[vendor.obj.l.ToString()].name;
                        uint[] internalLocationIndex = ItemVendorLocation.VendorPlugin.CommonLocationNameToInternalCoords[location];

                        vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, location, currencies));
                    }
                }
            }
            // special currency vendor
            if (itemDetails.item.tradeShops != null)
            {
                List<GarlandToolsWrapper.Models.TradeShop> tradeShops = itemDetails.item.tradeShops;

                foreach (GarlandToolsWrapper.Models.TradeShop tradeShop in tradeShops)
                {
                    if (tradeShop.npcs.Count > 0)
                    {
                        foreach (ulong npcId in tradeShop.npcs)
                        {
                            List<ItemVendorLocation.Models.Currency> currencies = new();
                            GarlandToolsWrapper.Models.Partial? tradeShopNpc = itemDetails.partials.Find(i => (ulong)i.obj.i == npcId);
                            if (tradeShopNpc != null)
                            {
                                string name = tradeShopNpc.obj.n;
                                //ulong cost = tradeShop.listings[0].currency[0].amount;
                                foreach (GarlandToolsWrapper.Models.Currency currency in tradeShop.listings[0].currency)
                                {
                                    string currencyName = itemDetails.partials.Find(i => i.id == currency.id && i.type == "item")!.obj.n;
                                    currencies.Add(new ItemVendorLocation.Models.Currency(currencyName, currency.amount));
                                }
                                //string currency = itemDetails.partials.Find(i => i.id == tradeShop.listings[0].currency[0].id && i.type == "item")!.obj.n;

                                if (tradeShopNpc.obj.l == null)
                                {
                                    vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, "No Location", currencies));
                                    break;
                                }

                                string location = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[tradeShopNpc.obj.l.ToString()].name;
                                uint[] internalLocationIndex = ItemVendorLocation.VendorPlugin.CommonLocationNameToInternalCoords[location];

                                vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, location, currencies));
                            }
                        }
                    }
                    else
                    {
                        List<ItemVendorLocation.Models.Currency> currencies = new();
                        string name = tradeShop.shop;
                        ulong cost = tradeShop.listings[0].currency[0].amount;
                        foreach (GarlandToolsWrapper.Models.Currency currency in tradeShop.listings[0].currency)
                        {
                            string currencyName = itemDetails.partials.Find(i => i.id == currency.id && i.type == "item")!.obj.n;
                            currencies.Add(new ItemVendorLocation.Models.Currency(currencyName, currency.amount));
                        }

                        vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, "Unknown", currencies));
                    }
                }
            }

            return vendorResults;
        }
    }
}
