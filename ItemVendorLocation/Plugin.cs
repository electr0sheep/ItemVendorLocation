using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Reflection;
using XivCommon;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;
using System.Linq;
using XivCommon.Functions.Tooltips;
using System.Threading.Tasks;

namespace ItemVendorLocation
{
    public class VendorPlugin : IDalamudPlugin
    {
        /// <summary>
        /// XivCommon library instance.
        /// </summary>
        private readonly XivCommonBase XivCommon;

        public string Name => "Item Vendor Location";

        private Lumina.Excel.GeneratedSheets.Item? selectedItem;

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        [PluginService] public static GameGui GameGui { get; private set; } = null!;
        [PluginService] public static Dalamud.Data.DataManager DataManager { get; private set; } = null!;

        public class VendorInformation
        {
            public VendorInformation(string name, MapLinkPayload location)
            {
                this.name = name;
                this.location = location;
            }
            public string name;
            public MapLinkPayload location;
        }

        //// key is our mock gc name
        //// value is what the data looks like in Garland Tools
        //public static Dictionary<string, string> GcName = new()
        //{
        //    { "Maelstrom", "Maelstrom" },
        //    { "Adder", "Order of the Twin Adder" },
        //    { "Flames", "Immortal Flames" },
        //};

        // mock for the player's gc
        //public static string TestGcName = "Maelstrom";


        // frustratingly, it seems like there are multiple entries for the same thing in the placename table
        // for example, there are at least 2 Kuganes, one that has an entry in the territorytype table, one that
        // does not. I could potentially just catch InvalidOperationException s, but I don't know enough about
        // the tables to want to continue to go down this path for now.
        //public static uint[] LookupInternalCoordsByPlaceName(string name)
        //{
        //    string[] subName = name.Split(" - ");
        //    if (subName.Length > 2)
        //    {
        //        string filteredName = $"{subName[0]} - {subName[1]}";
        //        var test = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()!.GetRow(573);
        //        Lumina.Excel.GeneratedSheets.PlaceName placeName = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>()!.First(i => i.Name == filteredName);
        //        Lumina.Excel.GeneratedSheets.TerritoryType territoryType = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()!.First(i => i.PlaceName.Row == placeName.RowId);
        //        return new uint[] { territoryType.RowId, territoryType.Map.Row };
        //    }
        //    else
        //    {
        //        var test = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()!.GetRow(573);
        //        Lumina.Excel.GeneratedSheets.PlaceName placeName = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>()!.First(i => i.Name == name);
        //        Lumina.Excel.GeneratedSheets.TerritoryType territoryType = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()!.First(i => i.PlaceName.Row == placeName.RowId);
        //        return new uint[] { territoryType.RowId, territoryType.Map.Row };
        //    }
        //}

        public static readonly Dictionary<string, uint[]> CommonLocationNameToInternalCoords = new()
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
            { "Ul'dah - Steps of Thal - Hustings Strip", new uint[] { 131, 14 } },
            { "Ul'dah - Steps of Thal - Merchant Strip", new uint[] { 131, 14 } },
            { "Ultima Thule", new uint[] { 960, 699 } },
            { "Upper La Noscea", new uint[] { 139, 19 } },
            { "Western La Noscea", new uint[] { 138, 18 } },
            { "Western Thanalan", new uint[] { 140, 20 } },
            { "Wolves' Den Pier", new uint[] { 250, 51 } },
            { "Yanxia", new uint[] { 614, 354 } },
            { "Zadnor", new uint[] { 975, 665 } }
        };

        public VendorPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            string? assemblyLocation = Assembly.GetExecutingAssembly().Location;
            PluginUi = new PluginUI(Configuration);

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            XivCommon = new XivCommonBase(Hooks.ContextMenu | Hooks.Tooltips);
            XivCommon.Functions.Tooltips.OnItemTooltip += OnItemTooltipOverride;
            XivCommon.Functions.ContextMenu.OpenInventoryContextMenu += OpenInventoryContextMenuOverride;
            XivCommon.Functions.ContextMenu.OpenContextMenu += OpenContextMenuOverride;
        }

        private void OnItemTooltipOverride(ItemTooltip itemTooltip, ulong itemId)
        {
            //HQ items don't have recipes, only NQ items
            if (itemId > 1000000)
            {
                itemId -= 1000000;
            }

            if (IsItemSoldByGilVendor((uint)itemId))
            {
                return;
            }
            List<Lumina.Excel.GeneratedSheets.GCScripShopItem> items = new(DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GCScripShopItem>()!.Where(i => i.Item.Row == itemId));
            // This code assumes all GC shops sell items for the same seal cost, which should be a safe assumption
            if (items.Count > 0)
            {
                itemTooltip[ItemTooltipString.ShopSellingPrice] = $"Shop Selling Price: {items[0].CostGCSeals} GC Seals";
                return;
            }

            if (IsItemSoldBySpecialVendor((uint)itemId))
            {
                itemTooltip[ItemTooltipString.ShopSellingPrice] = "Shop Selling Price: Special Vendor";
                return;
            }
        }

        private static bool IsItemSoldByAnyVendor(Lumina.Excel.GeneratedSheets.Item item)
        {
            return item.Name != null && item.Name != "" && (IsItemSoldByGilVendor(item) || IsItemSoldByGCVendor(item) || IsItemSoldBySpecialVendor(item));
        }

        private static bool IsItemSoldByGilVendor(Lumina.Excel.GeneratedSheets.Item item)
        {
            return IsItemSoldByGilVendor(item.RowId);
        }

        private static bool IsItemSoldByGilVendor(uint itemId)
        {
            return DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GilShopItem>()!.Any(i => i.Item.Row == itemId);
        }

        private static bool IsItemSoldByGCVendor(Lumina.Excel.GeneratedSheets.Item item)
        {
            return IsItemSoldByGCVendor(item.RowId);
        }

        private static bool IsItemSoldByGCVendor(uint itemId)
        {
            return DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GCScripShopItem>()!.Any(i => i.Item.Row == itemId);
        }

        private static bool IsItemSoldBySpecialVendor(Lumina.Excel.GeneratedSheets.Item item)
        {
            return IsItemSoldBySpecialVendor(item.RowId);
        }

        private static bool IsItemSoldBySpecialVendor(uint itemId)
        {
            return DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.SpecialShop>()!.Any(i =>
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

        private void OpenContextMenuOverride(XivCommon.Functions.ContextMenu.ContextMenuOpenArgs args)
        {
            // I think players have a world and items never will??
            // Hopefully this removes the vendor menu for players in the chat log
            if (args.ObjectWorld != 0)
            {
                return;
            }

            switch (args.ParentAddonName)
            {
                case "ChatLog":
                case "DailyQuestSupply":
                case "ItemSearch":
                case "RecipeNote":
                case "ShopExchangeItem":
                case "ShopExchangeItemDialog":
                case "Journal":
                    uint item_id = (uint)GameGui.HoveredItem;
                    selectedItem = DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()!.GetRow(item_id)!;
                    if (IsItemSoldByAnyVendor(selectedItem))
                    {
                        args.Items.Add(new XivCommon.Functions.ContextMenu.NormalContextMenuItem("Vendor Location", selectedArgs =>
                        {
                            HandleItem(selectedItem);
                        }));
                    }
                    return;
                default:
                    break;
            }
        }

        private void OpenInventoryContextMenuOverride(XivCommon.Functions.ContextMenu.Inventory.InventoryContextMenuOpenArgs args)
        {
            selectedItem = DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()!.GetRow(args.ItemId)!;
            if (IsItemSoldByAnyVendor(selectedItem))
            {
                args.Items.Add(new XivCommon.Functions.ContextMenu.Inventory.InventoryContextMenuItem("Vendor Location", selectedArgs =>
                {
                    HandleItem(selectedItem);
                }));
            }
        }
        
        private static ulong FindGarlondToolsItemId(Lumina.Excel.GeneratedSheets.Item item)
        {
            string itemName = item.Name;
            List<GarlandToolsWrapper.Models.ItemSearchResult> results = GarlandToolsWrapper.WebRequests.ItemSearch(itemName);
            GarlandToolsWrapper.Models.ItemSearchResult exactMatch = null!;
            if (results.Count > 1)
            {
                // search for exact match
                exactMatch = results.Find(i => string.Equals(i.obj.n, itemName, StringComparison.OrdinalIgnoreCase))!;
                if (exactMatch == null)
                {
                    throw new Exception("Could not find an exact match with garlond tools");
                }
            }
            else
            {
                exactMatch = results[0];
            }
            return (ulong)exactMatch!.obj.i;
        }

        public static List<Models.Vendor> GetVendors(ulong itemId)
        {
            //get preliminary data
            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(itemId);
            List<Models.Vendor> vendorResults = new();

            // gil vendor
            if (itemDetails.item.vendors != null)
            {
                foreach (ulong vendorId in itemDetails.item.vendors)
                {
                    GarlandToolsWrapper.Models.Partial? vendor = itemDetails.partials.Find(i => (ulong)i.obj.i == vendorId);

                    if (vendor != null)
                    {
                        string name = vendor.obj.n;
                        ulong cost = itemDetails.item.price;
                        string currency = "Gil";

                        if (vendor.obj.l == null)
                        {
                            vendorResults.Add(new Models.Vendor(name, null!, "No Location", cost, currency));
                            break;
                        }

                        string location = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[vendor.obj.l.ToString()].name;
                        uint[] internalLocationIndex = CommonLocationNameToInternalCoords[location];
                        MapLinkPayload? mapLink = null;
                        if (vendor.obj.CIsValid())
                        {
                            mapLink = new(internalLocationIndex[0], internalLocationIndex[1], (float)vendor.obj.c[0], (float)vendor.obj.c[1]);
                        }
                        else
                        {
                            // For now, we'll just set 0,0 as the coords for those vendors that Garland Tools doesn't have actual coords for
                            mapLink = new(internalLocationIndex[0], internalLocationIndex[1], 0f, 0f);
                        }

                        vendorResults.Add(new Models.Vendor(name, mapLink, location, cost, currency));
                    }
                }
            }
            // special currency vendor
            else if (itemDetails.item.tradeShops != null)
            {
                List<GarlandToolsWrapper.Models.TradeShop> tradeShops = itemDetails.item.tradeShops;

                foreach (GarlandToolsWrapper.Models.TradeShop tradeShop in tradeShops)
                {
                    if (tradeShop.npcs.Count > 0)
                    {
                        foreach (ulong npcId in tradeShop.npcs)
                        {
                            GarlandToolsWrapper.Models.Partial? tradeShopNpc = itemDetails.partials.Find(i => (ulong)i.obj.i == npcId);
                            if (tradeShopNpc != null)
                            {
                                string name = tradeShopNpc.obj.n;
                                ulong cost = tradeShop.listings[0].currency[0].amount;
                                string currency = itemDetails.partials.Find(i => i.id == tradeShop.listings[0].currency[0].id && i.type == "item")!.obj.n;

                                if (tradeShopNpc.obj.l == null)
                                {
                                    vendorResults.Add(new Models.Vendor(name, null, "No Location", cost, currency));
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

                                vendorResults.Add(new Models.Vendor(name, mapLink, location, cost, currency));
                            }
                        }
                    }
                    else
                    {
                        string name = tradeShop.shop;
                        ulong cost = tradeShop.listings[0].currency[0].amount;
                        string currency = itemDetails.partials.Find(i => i.id == tradeShop.listings[0].currency[0].id && i.type == "item")!.obj.n;

                        vendorResults.Add(new Models.Vendor(name, null!, "Unknown", cost, currency));
                    }
                }
            }

            return vendorResults;
        }

        private static void DisplayOneVendor(Lumina.Excel.GeneratedSheets.Item item, Models.Vendor vendor)
        {
            //_ = new TextPayload($"{item.Name} can be bought from {vendor.name}");
            //string firstVendorLocationName = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[vendor.obj.l.ToString()].name;
            //uint[] internalLocationIndex = CommonLocationNameToInternalCoords[firstVendorLocationName];
            if (vendor.mapLink != null)
            {
                _ = GameGui.OpenMapWithMapLink(vendor.mapLink);
            }
            SeString payload = new();
            _ = payload.Append(SeString.CreateItemLink(item, false));
            List<Payload> payloadList = new()
            {
                new TextPayload($" can be bought from {vendor.name} at ")
            };
            _ = payload.Append(new SeString(payloadList));
            if (vendor.mapLink != null)
            {
                _ = payload.Append(SeString.CreateMapLink(vendor.mapLink.TerritoryType.RowId, vendor.mapLink.Map.RowId, vendor.mapLink.XCoord, vendor.mapLink.YCoord));
            }
            Chat.PrintChat(new XivChatEntry
            {
                Message = payload
            });
        }

        private void DisplayAllVendors(Lumina.Excel.GeneratedSheets.Item item, List<Models.Vendor> vendors)
        {
            PluginUi.Vendors = vendors;
            PluginUi.ItemName = item.Name;
            PluginUi.VendorResultsVisible = true;
        }

        private void HandleItem(Lumina.Excel.GeneratedSheets.Item item)
        {
            Task.Run(() => {
                ulong garlondToolsId = FindGarlondToolsItemId(item);
                List<Models.Vendor> vendors = GetVendors(garlondToolsId);
                if (Configuration.ShowAllVendorsBool)
                {
                    DisplayOneVendor(item, vendors.Last());
                }
                else
                {
                    DisplayAllVendors(item, vendors);
                }
            });
        }

        public void Dispose()
        {
            PluginUi.Dispose();
            XivCommon.Functions.Tooltips.OnItemTooltip -= OnItemTooltipOverride;
            XivCommon.Functions.ContextMenu.OpenInventoryContextMenu -= OpenInventoryContextMenuOverride;
            XivCommon.Functions.ContextMenu.OpenContextMenu -= OpenContextMenuOverride;
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            PluginUi.SettingsVisible = true;
        }
    }
}
