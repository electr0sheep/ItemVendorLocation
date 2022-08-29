using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using ItemVendorLocation.Models;
using Lumina.Excel.GeneratedSheets;
using XivCommon;
using XivCommon.Functions.ContextMenu;
using XivCommon.Functions.ContextMenu.Inventory;
using XivCommon.Functions.Tooltips;

namespace ItemVendorLocation
{
    public class VendorPlugin : IDalamudPlugin
    {
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

        /// <summary>
        ///     XivCommon library instance.
        /// </summary>
        private readonly XivCommonBase XivCommon;

        private Item? selectedItem;

        public VendorPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            pluginInterface.Create<Service>();

            PluginInterface = pluginInterface;
            CommandManager = commandManager;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            PluginUi = new PluginUI(Configuration);

            LookupItem = new LookupItems();

            PluginInterface.UiBuilder.Draw += DrawUI;
            XivCommon = new XivCommonBase(Hooks.ContextMenu | Hooks.Tooltips);
            XivCommon.Functions.Tooltips.OnItemTooltip += OnItemTooltipOverride;
            XivCommon.Functions.ContextMenu.OpenInventoryContextMenu += OpenInventoryContextMenuOverride;
            XivCommon.Functions.ContextMenu.OpenContextMenu += OpenContextMenuOverride;
        }

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }
        private LookupItems LookupItem { get; init; }

        public string Name => "Item Vendor Location";

        public void Dispose()
        {
            PluginUi.Dispose();
            XivCommon.Functions.Tooltips.OnItemTooltip -= OnItemTooltipOverride;
            XivCommon.Functions.ContextMenu.OpenInventoryContextMenu -= OpenInventoryContextMenuOverride;
            XivCommon.Functions.ContextMenu.OpenContextMenu -= OpenContextMenuOverride;
        }

        private void OnItemTooltipOverride(ItemTooltip itemTooltip, ulong itemId)
        {
            //HQ items don't have recipes, only NQ items
            if (itemId > 1000000)
            {
                itemId -= 1000000;
            }

            var itemInfo = LookupItem.GetItemInfo((uint)itemId);
            if (itemInfo == null)
            {
                return;
            }

            switch (itemInfo.Type)
            {
                case ItemType.GcShop:
                    itemTooltip[ItemTooltipString.ShopSellingPrice] = $"Shop Selling Price: {itemInfo.Costs[0].Item1} {itemInfo.Costs[0].Item2}";
                    return;

                case ItemType.SpecialShop:
                    itemTooltip[ItemTooltipString.ShopSellingPrice] = "Shop Selling Price: Special vendor";
                    return;
            }
        }

        private void OpenContextMenuOverride(ContextMenuOpenArgs args)
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
                    var itemId = (uint)Service.GameGui.HoveredItem;
                    selectedItem = Service.DataManager.GetExcelSheet<Item>()!.GetRow(itemId)!;
                    args.Items.Add(new NormalContextMenuItem("Vendor Location", _ => { HandleItem(selectedItem.RowId); }));
                    return;
            }
        }

        private void OpenInventoryContextMenuOverride(InventoryContextMenuOpenArgs args)
        {
            selectedItem = Service.DataManager.GetExcelSheet<Item>()!.GetRow(args.ItemId)!;
            args.Items.Add(new InventoryContextMenuItem("Vendor Location", _ => { HandleItem(selectedItem.RowId); }));
        }

        private void HandleItem(uint itemId)
        {
            var itemInfo = LookupItem.GetItemInfo(itemId);
            if (itemInfo == null)
            {
                return;
            }

            PluginUi.ItemToDisplay = itemInfo;
            PluginUi.VendorResultsVisible = true;
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }
    }
}