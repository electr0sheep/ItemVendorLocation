using Dalamud.Game.Text.SeStringHandling.Payloads;
using ItemVendorLocation.Models;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ItemVendorLocation
{
    internal class LegacyStuff
    {
        private readonly Dictionary<string, uint[]> CommonLocationNameToInternalCoords = new()
        {
            { "Amh Araeng", new uint[] { 815, 493 } },
            { "Azys Lla", new uint[] { 402, 216 } },
            { "Bozjan Southern Front", new uint[] { 920, 606 } },
            { "Central Shroud", new uint[] { 148, 4 } },
            { "Central Thanalan", new uint[] { 141, 21 } },
            { "Coerthas Central Highlands", new uint[] { 155, 53 } },
            { "Coerthas Western Highlands", new uint[] { 397, 211 } },
            { "East Shroud", new uint[] { 152, 5 } },
            { "Eastern La Noscea", new uint[] { 137, 17 } },
            { "Eastern Thanalan", new uint[] { 142, 22 } },
            { "Elpis", new uint[] { 961, 700 } },
            { "Empyreum", new uint[] { 979, 679 } },
            { "Eulmore - The Buttress", new uint[] { 820, 498 } },
            { "Eureka Anemos", new uint[] { 732, 414 } },
            { "Eureka Hydatos", new uint[] { 827, 515 } },
            { "Eureka Pagos", new uint[] { 763, 467 } },
            { "Eureka Pyros", new uint[] { 795, 484 } },
            { "Foundation", new uint[] { 418, 218 } },
            { "Garlemald", new uint[] { 958, 697 } },
            { "Idyllshire", new uint[] { 478, 257 } },
            { "Il Mheg", new uint[] { 816, 494 } },
            { "Ingleside Apartment Lobby", new uint[] { 985, 681 } },
            { "Kholusia", new uint[] { 814, 492 } },
            { "Kobai Goten Apartment Lobby", new uint[] { 654, 388 } },
            { "Kugane", new uint[] { 628, 370 } },
            { "Labyrinthos", new uint[] { 956, 695 } },
            { "Lakeland", new uint[] { 813, 491 } },
            { "Lily Hills Apartment Lobby", new uint[] { 574, 321 } },
            { "Limsa Lominsa Lower Decks", new uint[] { 129, 12 } },
            { "Limsa Lominsa Upper Decks", new uint[] { 128, 11 } },
            { "Lower La Noscea", new uint[] { 135, 16 } },
            { "Mare Lamentorum", new uint[] { 959, 698 } },
            { "Matoya's Cave", new uint[] { 463, 253 } },
            { "Middle La Noscea", new uint[] { 134, 15 } },
            { "Mist", new uint[] { 339, 72 } },
            { "Mor Dhona", new uint[] { 156, 25 } },
            { "New Gridania", new uint[] { 132, 2 } },
            { "North Shroud", new uint[] { 154, 7 } },
            { "Northern Thanalan", new uint[] { 147, 24 } },
            { "Old Gridania", new uint[] { 133, 3 } },
            { "Old Sharlayan", new uint[] { 962, 693 } },
            { "Outer La Noscea", new uint[] { 180, 30 } },
            { "Radz-at-Han", new uint[] { 963, 694 } },
            { "Rhalgr's Reach", new uint[] { 635, 366 } },
            { "South Shroud", new uint[] { 153, 6 } },
            { "Southern Thanalan", new uint[] { 146, 23 } },
            { "Sultana's Breath Apartment Lobby", new uint[] { 575, 322 } },
            { "Shirogane", new uint[] { 641, 364 } },
            { "Thavnair", new uint[] { 957, 696 } },
            { "The Azim Steppe", new uint[] { 622, 372 } },
            { "The Churning Mists", new uint[] { 400, 214 } },
            { "The Crystarium", new uint[] { 819, 497 } },
            { "The Diadem", new uint[] { 939, 584 } },
            { "The Doman Enclave", new uint[] { 759, 463 } },
            { "The Dravanian Forelands", new uint[] { 398, 212 } },
            { "The Endeavor", new uint[] { 900, 604 } },
            { "The Firmament", new uint[] { 886, 574 } },
            { "The Fringes", new uint[] { 612, 367 } },
            { "The Goblet", new uint[] { 341, 83 } },
            { "The Gold Saucer", new uint[] { 144, 196 } },
            { "The Lavender Beds", new uint[] { 340, 82 } },
            { "The Lochs", new uint[] { 621, 369 } },
            { "The Mists", new uint[] { 339, 72 } },
            { "The Peaks", new uint[] { 620, 368 } },
            { "The Pillars", new uint[] { 419, 219 } },
            { "The Rak'tika Greatwood", new uint[] { 817, 495 } },
            { "The Ruby Sea", new uint[] { 613, 371 } },
            { "The Sea of Clouds", new uint[] { 401, 215 } },
            { "The Tempest", new uint[] { 818, 496 } },
            { "The Waking Sands", new uint[] { 212, 80 } },
            { "Topmast Apartment Lobby", new uint[] { 573, 320 } },
            { "Ul'dah - Steps of Nald", new uint[] { 130, 13 } },
            { "Ul'dah - Steps of Thal - Hustings Strip", new uint[] { 131, 73 } },
            { "Ul'dah - Steps of Thal - Merchant Strip", new uint[] { 131, 14 } },
            { "Ultima Thule", new uint[] { 960, 699 } },
            { "Upper La Noscea", new uint[] { 139, 19 } },
            { "Western La Noscea", new uint[] { 138, 18 } },
            { "Western Thanalan", new uint[] { 140, 20 } },
            { "Wolves' Den Pier", new uint[] { 250, 51 } },
            { "Yanxia", new uint[] { 614, 354 } },
            { "Zadnor", new uint[] { 975, 665 } }
        };

        private class Currency
        {
            public string name;
            public ulong cost;

            public Currency(string name, ulong cost)
            {
                this.name = name;
                this.cost = cost;
            }
        }

        private class Vendor
        {
            public string name = "";
            public MapLinkPayload mapLink = null;
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

        private readonly ExcelSheet<Item> _items;
        private readonly ExcelSheet<GilShopItem> _gilShopItems;
        private readonly ExcelSheet<GCScripShopItem> _gcScripShopItems;
        private readonly ExcelSheet<SpecialShop> _specialShopItems;
        private readonly ExcelSheet<FccShop> _fccShopItems;

        public LegacyStuff()
        {
            _items = Service.DataManager.GetExcelSheet<Item>();
            _gilShopItems = Service.DataManager.GetExcelSheet<GilShopItem>();
            _gcScripShopItems = Service.DataManager.GetExcelSheet<GCScripShopItem>();
            _specialShopItems = Service.DataManager.GetExcelSheet<SpecialShop>();
            _fccShopItems = Service.DataManager.GetExcelSheet<FccShop>();
        }

        public bool IsItemSoldByGilVendor(uint itemId)
        {
            return _gilShopItems.Any(i => i.Item.Row == itemId);
        }

        public bool IsItemSoldByGCVendor(uint itemId)
        {
            return _gcScripShopItems.Any(i => i.Item.Row == itemId);
        }

        public bool IsItemSoldByFcVendor(uint itemId)
        {
            return _fccShopItems.Any(i => i.Item.Any(i => i == itemId));
        }

        public bool IsItemSoldBySpecialVendor(uint itemId)
        {
            return _specialShopItems.Any(i =>
            i.UnkData1[0].ItemReceive == itemId ||
            i.UnkData1[0].CountReceive == itemId ||
            i.UnkData1[0].SpecialShopItemCategory == itemId ||
            i.UnkData1[0].HQReceive == itemId ||
            i.UnkData1[1].ItemReceive == itemId ||
            i.UnkData1[1].CountReceive == itemId ||
            i.UnkData1[1].SpecialShopItemCategory == itemId ||
            i.UnkData1[1].HQReceive == itemId ||
            i.Unknown9 == itemId ||
            i.Unknown10 == itemId ||
            i.Unknown11 == itemId ||
            i.Unknown12 == itemId ||
            i.Unknown13 == itemId ||
            i.Unknown14 == itemId ||
            i.Unknown15 == itemId ||
            i.Unknown16 == itemId ||
            i.Unknown17 == itemId ||
            i.Unknown18 == itemId ||
            i.Unknown19 == itemId ||
            i.Unknown20 == itemId ||
            i.Unknown21 == itemId ||
            i.Unknown22 == itemId ||
            i.Unknown23 == itemId ||
            i.Unknown24 == itemId ||
            i.Unknown25 == itemId ||
            i.Unknown26 == itemId ||
            i.Unknown27 == itemId ||
            i.Unknown28 == itemId ||
            i.Unknown29 == itemId ||
            i.Unknown30 == itemId ||
            i.Unknown31 == itemId ||
            i.Unknown32 == itemId ||
            i.Unknown33 == itemId ||
            i.Unknown34 == itemId ||
            i.Unknown35 == itemId ||
            i.Unknown36 == itemId ||
            i.Unknown37 == itemId ||
            i.Unknown38 == itemId ||
            i.Unknown39 == itemId ||
            i.Unknown40 == itemId ||
            i.Unknown41 == itemId ||
            i.Unknown42 == itemId ||
            i.Unknown43 == itemId ||
            i.Unknown44 == itemId ||
            i.Unknown45 == itemId ||
            i.Unknown46 == itemId ||
            i.Unknown47 == itemId ||
            i.Unknown48 == itemId ||
            i.Unknown49 == itemId ||
            i.Unknown50 == itemId ||
            i.Unknown51 == itemId ||
            i.Unknown52 == itemId ||
            i.Unknown53 == itemId ||
            i.Unknown54 == itemId ||
            i.Unknown55 == itemId ||
            i.Unknown56 == itemId ||
            i.Unknown57 == itemId ||
            i.Unknown58 == itemId ||
            i.Unknown59 == itemId ||
            i.Unknown60 == itemId);
        }

        public bool IsItemSoldByAnyVendor(uint itemId)
        {
            return IsItemSoldByGilVendor(itemId) || IsItemSoldBySpecialVendor(itemId) || IsItemSoldByFcVendor(itemId) || IsItemSoldByGCVendor(itemId);
        }

        public ItemInfo GetItemInfo(uint itemId)
        {
            Item item = _items.GetRow(itemId);
            ItemInfo itemInfo = new()
            {
                Id = itemId,
                Name = item.Name,
                Type = IsItemSoldByGilVendor(itemId) ? ItemType.GilShop :
                IsItemSoldByFcVendor(itemId) ? ItemType.SpecialShop :
                IsItemSoldByGCVendor(itemId) ? ItemType.GcShop :
                IsItemSoldBySpecialVendor(itemId) ? ItemType.SpecialShop :
                throw new Exception("Could not determine ItemType"),
                NpcInfos = new(),
            };
            //get preliminary data
            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(itemId);

            // gil vendor
            if (itemDetails.item.vendors != null)
            {
                foreach (ulong vendorId in itemDetails.item.vendors)
                {
                    GarlandToolsWrapper.Models.Partial vendor = itemDetails.partials.Find(i => (ulong)i.obj.i == vendorId);
                    List<Currency> currencies = new();

                    if (vendor != null)
                    {
                        // get rid of any vendor that doesn't have a location
                        // typically man/maid servants
                        string name = vendor.obj.n;
                        currencies.Add(new Currency("Gil", itemDetails.item.price));
                        if (vendor.obj.l == null)
                        {
                            itemInfo.NpcInfos.Add(new()
                            {
                                Name = name,
                                Location = null,
                                Costs = new()
                                {
                                    new((uint)itemDetails.item.price, "Gil")
                                }
                            });
                            break;
                        }

                        string location = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[vendor.obj.l.ToString()].name;
                        uint[] internalLocationIndex = CommonLocationNameToInternalCoords[location];
                        MapLinkPayload mapLink = null;
                        if (vendor.obj.CIsValid())
                        {
                            mapLink = new(internalLocationIndex[0], internalLocationIndex[1], (float)vendor.obj.c[0], (float)vendor.obj.c[1]);
                        }
                        else
                        {
                            // For now, we'll just set 0,0 as the coords for those vendors that Garland Tools doesn't have actual coords for
                            mapLink = new(internalLocationIndex[0], internalLocationIndex[1], 0f, 0f);
                        }

                        List<Tuple<uint, string>> costs = new();
                        foreach (Currency currency in currencies)
                        {
                            costs.Add(new((uint)currency.cost, currency.name));
                        }
                        itemInfo.NpcInfos.Add(new()
                        {
                            Name = name,
                            Location = new(mapLink.XCoord, mapLink.YCoord, mapLink.TerritoryType),
                            Costs = costs,
                        });
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
                            List<Currency> currencies = new();
                            GarlandToolsWrapper.Models.Partial? tradeShopNpc = itemDetails.partials.Find(i => (ulong)i.obj.i == npcId);
                            if (tradeShopNpc != null)
                            {
                                string name = tradeShopNpc.obj.n;
                                foreach (GarlandToolsWrapper.Models.Currency currency in tradeShop.listings[0].currency)
                                {
                                    string currencyName = itemDetails.partials.Find(i => i.id == currency.id && i.type == "item")!.obj.n;
                                    currencies.Add(new Currency(currencyName, currency.amount));
                                }

                                List<Tuple<uint, string>> costs = new();
                                foreach (Currency currency in currencies)
                                {
                                    costs.Add(new((uint)currency.cost, currency.name));
                                }

                                if (tradeShopNpc.obj.l == null)
                                {
                                    itemInfo.NpcInfos.Add(new()
                                    {
                                        Name = name,
                                        Location = null,
                                        Costs = costs,
                                    });
                                    break;
                                }

                                string location = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[tradeShopNpc.obj.l.ToString()].name;
                                uint[] internalLocationIndex = CommonLocationNameToInternalCoords[location];
                                MapLinkPayload? mapLink = null;
                                if (tradeShopNpc.obj.CIsValid())
                                {
                                    mapLink = new(internalLocationIndex[0], internalLocationIndex[1], (float)tradeShopNpc.obj.c[0], (float)tradeShopNpc.obj.c[1]);
                                }
                                else
                                {
                                    // For now, we'll just set 0,0 as the coords for those vendors that Garland Tools doesn't have actual coords for
                                    mapLink = new(internalLocationIndex[0], internalLocationIndex[1], 0f, 0f);
                                }

                                itemInfo.NpcInfos.Add(new()
                                {
                                    Name = name,
                                    Location = new(mapLink.XCoord, mapLink.YCoord, mapLink.TerritoryType),
                                    Costs = costs,
                                });
                            }
                        }
                    }
                    else
                    {
                        List<Currency> currencies = new();
                        string name = tradeShop.shop;
                        ulong cost = tradeShop.listings[0].currency[0].amount;
                        foreach (GarlandToolsWrapper.Models.Currency currency in tradeShop.listings[0].currency)
                        {
                            string currencyName = itemDetails.partials.Find(i => i.id == currency.id && i.type == "item")!.obj.n;
                            currencies.Add(new Currency(currencyName, currency.amount));
                        }

                        List<Tuple<uint, string>> costs = new();
                        foreach (Currency currency in currencies)
                        {
                            costs.Add(new((uint)currency.cost, currency.name));
                        }

                        itemInfo.NpcInfos.Add(new()
                        {
                            Name = name,
                            Location = null,
                            Costs = costs,
                        });
                    }
                }
            }

            return itemInfo;
        }
    }
}
