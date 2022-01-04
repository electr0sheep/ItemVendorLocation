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

namespace ItemVendor
{
    public sealed class Plugin : IDalamudPlugin
    {
        /// <summary>
        /// XivCommon library instance.
        /// </summary>
        public XivCommonBase XivCommon = null!;

        public string Name => "Item Vendor";

        private const string commandName = "/pmycommand";

        private readonly XivCommon.Functions.ContextMenu.Inventory.InventoryContextMenuItem inventoryContextMenuItem;
        private readonly XivCommon.Functions.ContextMenu.NormalContextMenuItem contextMenuItem;

        private Lumina.Excel.GeneratedSheets.Item selectedItem;

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

        private static readonly Dictionary<string, uint[]> commonLocationNameToInternalCoords = new Dictionary<string, uint[]>
        {
            { "Ul'dah - Steps of Thal - Merchant Strip", new uint[] {131, 14 } },
            { "Central Thanalan", new uint[] {141, 21} },
            { "South Shroud", new uint[] {153, 6} },
            { "The Pillars", new uint[] {419, 219} },
            { "Southern Thanalan", new uint[] {146, 23} },
            { "The Crystarium", new uint[] {819, 497} },
            { "Lily Hills Apartment Lobby", new uint[] {574, 321} },
            { "Mor Dhona", new uint[] {156, 25} },
            { "Limsa Lominsa Lower Decks", new uint[] {129, 12} },
            { "The Ruby Sea", new uint[] {613, 371} },
            { "Limsa Lominsa Upper Decks", new uint[] {128, 11} },
            { "Eulmore - The Buttress", new uint[] {820, 498} },
            { "North Shroud", new uint[] {154, 7} },
            { "The Churning Mists", new uint[] {400, 214} },
            { "Ul'dah - Steps of Thal - Hustings Strip", new uint[] {131, 14} },
            { "Eureka Anemos", new uint[] {732, 414} },
            { "Central Shroud", new uint[] {148, 4} },
            { "New Gridania", new uint[] {132, 2} },
            { "Idyllshire", new uint[] {478, 257} },
            { "The Gold Saucer", new uint[] {144, 196} },
            { "The Tempest", new uint[] {818, 496} },
            { "The Azim Steppe", new uint[] {622, 372} },
            { "The Waking Sands", new uint[] {212, 80} },
            { "The Mists", new uint[] {339, 72} },
            { "The Goblet", new uint[] {341, 83} },
            { "The Lavender Beds", new uint[] {340, 82} },
            { "Kugane", new uint[] {638, 370} },
            { "Upper La Noscea", new uint[] {139, 19} },
            { "Coerthas Western Highlands", new uint[] {397, 211} },
            { "Rhalgr's Reach", new uint[] {635, 366} },
            { "Eastern Thanalan", new uint[] {142, 22} },
            { "Matoya's Cave", new uint[] {463, 253} },
            { "The Peaks", new uint[] {620, 368} },
            { "Yanxia", new uint[] {614, 354} },
            { "The Doman Enclave", new uint[] {759, 463} },
            { "Outer La Noscea", new uint[] {180, 30} },
            { "Kholusia", new uint[] {814, 492} },
            { "The Rak'tika Greatwood", new uint[] {817, 495} },
            { "The Sea of Clouds", new uint[] {401, 215} },
            { "Amh Araeng", new uint[] {815, 493} },
            { "Coerthas Central Highlands", new uint[] {155, 53} },
            { "East Shroud", new uint[] {152, 5} },
            { "Eastern La Noscea", new uint[] {137, 17} },
            { "Middle La Noscea", new uint[] {134, 15} },
            { "Western La Noscea", new uint[] {138, 18} },
            { "Western Thanalan", new uint[] {140, 20} },
            { "Il Mheg", new uint[] {816, 494} },
            { "Azys Lla", new uint[] {402, 216} },
            { "Old Gridania", new uint[] {133, 3} },
            { "The Dravanian Forelands", new uint[] {398, 212 } }
        };

        public Plugin(
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

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.inventoryContextMenuItem = new XivCommon.Functions.ContextMenu.Inventory.InventoryContextMenuItem("Vendor Location", this.InventoryContextItemChanged);
            this.contextMenuItem = new XivCommon.Functions.ContextMenu.NormalContextMenuItem("Vendor Location", this.ContextItemChanged);

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            this.XivCommon = new XivCommonBase(Hooks.ContextMenu);
            this.XivCommon.Functions.ContextMenu.OpenInventoryContextMenu += this.OnOpenInventoryContextMenu;
            this.XivCommon.Functions.ContextMenu.OpenContextMenu += this.OnOpenContextMenu;
        }

        private bool itemSoldByVendor(Lumina.Excel.GeneratedSheets.Item item)
        {
            // TODO: This might be able to be replaced with the code in the inventory plugin that checks if an item is sold by a vendor
            // As it stands, this is pretty inefficient, but will work for testing
            GarlandToolsWrapper.Models.Data data = GarlandToolsWrapper.WebRequests.GetData();
            string itemName = item.Name;
            List<GarlandToolsWrapper.Models.ItemSearchResult> results = GarlandToolsWrapper.WebRequests.ItemSearch(itemName);
            GarlandToolsWrapper.Models.ItemSearchResult exactMatch = null;
            if (results.Count > 1)
            {
                // search for exact match
                exactMatch = results.Find(i => string.Equals(i.obj.n, itemName, StringComparison.OrdinalIgnoreCase))!;
                if (exactMatch != null)
                {
                    Logger.LogDebug("Found exact match");
                }
            }
            else
            {
                exactMatch = results[0];
            }
            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(exactMatch!.obj.i);
            if (itemDetails.item.vendors.Count == 0)
            {
                Logger.LogDebug($"{itemName} doesn't appear to have any vendors!");
                return false;
            }
            ulong firstVendorId = itemDetails.item.vendors[0];
            GarlandToolsWrapper.Models.Partial firstVendor = itemDetails.partials.Find(i => i.obj.i == firstVendorId)!;
            return firstVendor != null;
        }

        private void OnOpenContextMenu(XivCommon.Functions.ContextMenu.ContextMenuOpenArgs args)
        {
            switch (args.ParentAddonName)
            {
                case "ChatLog":
                case "DailyQuestSupply":
                case "ItemSearch":
                    var item_id = (uint)gameGui.HoveredItem;
                    this.selectedItem = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()!.GetRow(item_id)!;
                    if (itemSoldByVendor(this.selectedItem))
                    {
                        args.Items.Add(this.contextMenuItem);
                    }
                    return;
            }
        }

        private void OnOpenInventoryContextMenu(XivCommon.Functions.ContextMenu.Inventory.InventoryContextMenuOpenArgs args)
        {
            this.selectedItem = dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()!.GetRow(args.ItemId)!;
            if (itemSoldByVendor(this.selectedItem))
            {
                Logger.LogDebug("adding inventory context menu");
                args.Items.Add(this.inventoryContextMenuItem);
            }
        }

        private void ContextItemChanged(XivCommon.Functions.ContextMenu.ContextMenuItemSelectedArgs args)
        {
            Logger.LogDebug("Handling context item");
            HandleItem(this.selectedItem!);
        }

        private void InventoryContextItemChanged(XivCommon.Functions.ContextMenu.Inventory.InventoryContextMenuItemSelectedArgs args)
        {
            Logger.LogDebug("Handling inventory item");
            HandleItem(this.selectedItem!);
        }

        private void HandleItem(Lumina.Excel.GeneratedSheets.Item item)
        {

            Logger.LogDebug("Got to handle item");
            TextPayload textPayload;

            // TODO: Crazy inefficient, literally copy pasta from itemSoldByVendor(), but going this route for testing
            GarlandToolsWrapper.Models.Data data = GarlandToolsWrapper.WebRequests.GetData();
            string itemName = item.Name;
            List<GarlandToolsWrapper.Models.ItemSearchResult> results = GarlandToolsWrapper.WebRequests.ItemSearch(itemName);
            GarlandToolsWrapper.Models.ItemSearchResult exactMatch = null;
            if (results.Count > 1)
            {
                // search for exact match
                exactMatch = results.Find(i => string.Equals(i.obj.n, itemName, StringComparison.OrdinalIgnoreCase))!;
                if (exactMatch != null)
                {
                    Logger.LogDebug("Found exact match");
                }
            }
            else
            {
                exactMatch = results[0];
            }
            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(exactMatch!.obj.i);
            ulong firstVendorId = itemDetails.item.vendors[0];
            GarlandToolsWrapper.Models.Partial firstVendor = itemDetails.partials.Find(i => i.obj.i == firstVendorId)!;
            textPayload = new TextPayload($"{item.Name} can be bought from {firstVendor.obj.n}");
            string firstVendorLocationName = data.locationIndex[firstVendor.obj.l.ToString()].name;
            uint[] internalLocationIndex = commonLocationNameToInternalCoords[firstVendorLocationName];
            MapLinkPayload vendorLocation = new MapLinkPayload(internalLocationIndex[0], internalLocationIndex[1], (float)firstVendor.obj.c[0], (float)firstVendor.obj.c[1]);
            gameGui.OpenMapWithMapLink(vendorLocation);
            SeString payload = new SeString();
            payload.Append(SeString.CreateItemLink(item, false));
            List<Payload> payloadList = new List<Payload>
            {
                new TextPayload($" can be bought from {firstVendor.obj.n} at ")
            };
            payload.Append(new SeString(payloadList));
            //SeString maplink = SeString.CreateMapLink(internalLocationIndex[0], internalLocationIndex[1], (float)firstVendor.obj.c[0], (float)firstVendor.obj.c[1]);
            payload.Append(SeString.CreateMapLink(internalLocationIndex[0], internalLocationIndex[1], (float)firstVendor.obj.c[0], (float)firstVendor.obj.c[1]));
            Chat.PrintChat(new XivChatEntry
            {
                Message = payload
            });
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
            this.XivCommon.Functions.ContextMenu.OpenInventoryContextMenu -= this.OnOpenInventoryContextMenu;
            this.XivCommon.Functions.ContextMenu.OpenContextMenu -= this.OnOpenContextMenu;
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;
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
