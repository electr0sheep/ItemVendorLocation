using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using XivCommon;
using Dalamud.DrunkenToad;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;
using System.Linq;
using ImGuiNET;

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
        [PluginService] public static GameGui gameGui { get; private set; } = null!;
        [PluginService] public static Dalamud.Data.DataManager dataManager { get; private set; } = null!;

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
            { "Yanxia", new uint[] { 614, 354 } },
            { "Zadnor", new uint[] { 975, 665 } }
        };

        public VendorPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            this.PluginUi = new PluginUI(this.Configuration);

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            this.XivCommon = new XivCommonBase(Hooks.ContextMenu);
            this.XivCommon.Functions.ContextMenu.OpenInventoryContextMenu += this.OnOpenInventoryContextMenu;
            this.XivCommon.Functions.ContextMenu.OpenContextMenu += this.OnOpenContextMenu;
        }

        private bool isItemSoldByVendor(Lumina.Excel.GeneratedSheets.Item item)
        {
            return dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GilShopItem>()!.Any(i => i.Item.Row == item.RowId);
        }

        private void OnOpenContextMenu(XivCommon.Functions.ContextMenu.ContextMenuOpenArgs args)
        {
            switch (args.ParentAddonName)
            {
                case "ChatLog":
                case "DailyQuestSupply":
                case "ItemSearch":
                case "RecipeNote":
                    var item_id = (uint)gameGui.HoveredItem;
                    this.selectedItem = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()!.GetRow(item_id)!;
                    if (isItemSoldByVendor(this.selectedItem))
                    {
                        args.Items.Add(new XivCommon.Functions.ContextMenu.NormalContextMenuItem("Vendor Location", selectedArgs =>
                        {
                            HandleItem(this.selectedItem);
                        }));
                    }
                    return;
            }
        }

        private void OnOpenInventoryContextMenu(XivCommon.Functions.ContextMenu.Inventory.InventoryContextMenuOpenArgs args)
        {
            this.selectedItem = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()!.GetRow(args.ItemId)!;
            if (isItemSoldByVendor(this.selectedItem))
            {
                args.Items.Add(new XivCommon.Functions.ContextMenu.Inventory.InventoryContextMenuItem("Vendor Location", selectedArgs =>
                {
                    HandleItem(this.selectedItem);
                }));
            }
        }

        private ulong FindGarlondToolsItemId(Lumina.Excel.GeneratedSheets.Item item)
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
            return exactMatch!.obj.i;
        }

        private void DisplayOneVendor(ulong garlondToolsId, Lumina.Excel.GeneratedSheets.Item item)
        {
            TextPayload textPayload;

            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(garlondToolsId);
            ulong firstVendorId = itemDetails.item.vendors[0];
            GarlandToolsWrapper.Models.Partial firstVendor = itemDetails.partials.Find(i => i.obj.i == firstVendorId)!;
            textPayload = new TextPayload($"{itemDetails.item.name} can be bought from {firstVendor.obj.n}");
            string firstVendorLocationName = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[firstVendor.obj.l.ToString()].name;
            uint[] internalLocationIndex = CommonLocationNameToInternalCoords[firstVendorLocationName];
            MapLinkPayload vendorLocation = new(internalLocationIndex[0], internalLocationIndex[1], (float)firstVendor.obj.c[0], (float)firstVendor.obj.c[1]);
            _ = gameGui.OpenMapWithMapLink(vendorLocation);
            SeString payload = new();
            _ = payload.Append(SeString.CreateItemLink(item, false));
            List<Payload> payloadList = new()
            {
                new TextPayload($" can be bought from {firstVendor.obj.n} at ")
            };
            _ = payload.Append(new SeString(payloadList));
            _ = payload.Append(SeString.CreateMapLink(internalLocationIndex[0], internalLocationIndex[1], (float)firstVendor.obj.c[0], (float)firstVendor.obj.c[1]));
            Chat.PrintChat(new XivChatEntry
            {
                Message = payload
            });
        }

        private void DisplayAllVendors(ulong garlondToolsId)
        {
            PluginUi.GarlondToolsItemId = garlondToolsId;
            PluginUi.VendorResultsVisible = true;
        }

        private void HandleItem(Lumina.Excel.GeneratedSheets.Item item)
        {
            try
            {
                ulong garlondToolsId = FindGarlondToolsItemId(item);
                if (Configuration.ShowAllVendorsBool)
                {
                    DisplayOneVendor(garlondToolsId, item);
                }
                else
                {
                    DisplayAllVendors(garlondToolsId);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.XivCommon.Functions.ContextMenu.OpenInventoryContextMenu -= this.OnOpenInventoryContextMenu;
            this.XivCommon.Functions.ContextMenu.OpenContextMenu -= this.OnOpenContextMenu;
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}
