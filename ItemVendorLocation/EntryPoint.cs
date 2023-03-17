using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheapLoc;
using Dalamud.ContextMenu;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ItemVendorLocation.Models;
using Lumina.Excel.GeneratedSheets;
using XivCommon;
using XivCommon.Functions.Tooltips;

namespace ItemVendorLocation
{
    public class EntryPoint : IDalamudPlugin
    {
        private static readonly List<string> GameAddonWhitelist = new()
        {
             "ChatLog",
             "ContentsInfoDetail",
             "DailyQuestSupply",
             "HousingGoods",
             "ItemSearch",
             "Journal",
             "RecipeMaterialList",
             "RecipeNote",
             "RecipeTree",
             "ShopExchangeItem",
             "ShopExchangeItemDialog",
             "SubmarinePartsMenu",
        };

        public readonly Dictionary<byte, uint> GcVendorIdMap = new()
        {
            { 1, 1002387 },
            { 2, 1002393 },
            { 3, 1002390 },
        };

        private readonly string ButtonName = "";
#if DEBUG
        public readonly ItemLookup _itemLookup;
#else
        private readonly ItemLookup _itemLookup;
#endif
        private readonly LegacyStuff _legacyStuff;
        private readonly WindowSystem _windowSystem;
        private readonly SettingsWindow _configWindow;
        private readonly XivCommonBase _xivCommon;

        public EntryPoint([RequiredVersion("1.0")] DalamudPluginInterface pi)
        {
            _ = pi.Create<Service>();

            Localization.SetupLocalization(Service.ClientState.ClientLanguage);
            ButtonName = Loc.Localize("ContextMenuItem", "Vendor location");
            Service.Plugin = this;
            Service.Configuration = pi.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
            Service.ContextMenu = new DalamudContextMenu();
            _xivCommon = new(Hooks.Tooltips);
            _itemLookup = new();
            _legacyStuff = new();

            // Initialize the UI
            _windowSystem = new WindowSystem(typeof(EntryPoint).AssemblyQualifiedName);
            _configWindow = new();
            Service.PluginUi = new PluginWindow();

            _windowSystem.AddWindow(Service.PluginUi);
            _windowSystem.AddWindow(_configWindow = new());

            _xivCommon.Functions.Tooltips.OnItemTooltip += Tooltips_OnOnItemTooltip;
            Service.ContextMenu.OnOpenInventoryContextMenu += ContextMenu_OnOpenInventoryContextMenu;
            Service.ContextMenu.OnOpenGameObjectContextMenu += ContextMenu_OnOpenGameObjectContextMenu;
            Service.Interface.UiBuilder.Draw += _windowSystem.Draw;
            Service.Interface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
        }

        private void OnOpenConfigUi()
        {
            _configWindow.IsOpen = true;
        }

        public string Name => "ItemVendorLocation";

        private static uint CorrectitemId(uint itemId)
        {
            return itemId switch
            {
                > 1000000 => itemId - 1000000, // hq
                /*
                 > 500000 and < 1000000 => itemId - 500000, // collectible
                 */
                _ => itemId
            };
        }

        private void NewOnOpenGameObjectContextMenu(GameObjectContextMenuOpenArgs args, uint itemId)
        {
            ItemInfo itemInfo = _itemLookup.GetItemInfo(itemId);
            if (itemInfo == null)
            {
                return;
            }
            args.AddCustomItem(new GameObjectContextMenuItem(ButtonName, _ => { NewContextMenuCallback(itemInfo); }, true));
            return;
        }

        private void LegacyOnOpenGameObjectContextMenu(GameObjectContextMenuOpenArgs args, uint itemId)
        {
            if (_legacyStuff.IsItemSoldByAnyVendor(itemId))
            {
                args.AddCustomItem(new GameObjectContextMenuItem(ButtonName, _ => { LegacyContextMenuCallback(itemId); }, true));
            }
        }

        /// <summary>
        /// Function called when a user right-clicks various things in game UI.
        /// </summary>
        /// <remarks>
        /// This function needs to be very quick, as the user will experience a delay
        /// in right-clicking an item and seeing the context menu if we do expensive things
        /// here.
        /// </remarks>
        /// <param name="args"></param>
        private void ContextMenu_OnOpenGameObjectContextMenu(GameObjectContextMenuOpenArgs args)
        {
            // I think players have a world and items never will??
            // Hopefully this removes the vendor menu for players in the chat log
            if (args.ObjectWorld != 0)
            {
                return;
            }

            if (!GameAddonWhitelist.Contains(args.ParentAddonName))
            {
                return;
            }

            uint itemId;
            if (args.ParentAddonName == "RecipeNote")
            {
                unsafe
                {
                    // thank you ottermandias
                    nint recipeNoteAgen = Service.GameGui.FindAgentInterface(args.ParentAddonName);
                    itemId = *(uint*)(recipeNoteAgen + 0x398);
                }
            }
            else
            {
                itemId = CorrectitemId((uint)Service.GameGui.HoveredItem);
            }
            switch (Service.Configuration.DataSource)
            {
                case DataSource.Internal:
                    NewOnOpenGameObjectContextMenu(args, itemId);
                    return;
                case DataSource.GarlandTools:
                    LegacyOnOpenGameObjectContextMenu(args, itemId);
                    return;
            }
        }

        private void NewOnOpenInventoryContextMenu(InventoryContextMenuOpenArgs args)
        {
            uint itemId = CorrectitemId(args.ItemId);
            ItemInfo itemInfo = _itemLookup.GetItemInfo(itemId);
            if (itemInfo == null)
            {
                return;
            }

            args.AddCustomItem(new InventoryContextMenuItem(ButtonName, _ => { NewContextMenuCallback(itemInfo); }, true));
        }

        private void LegacyOnOpenInventoryContextMenu(InventoryContextMenuOpenArgs args)
        {
            if (_legacyStuff.IsItemSoldByAnyVendor(args.ItemId))
            {
                args.AddCustomItem(new InventoryContextMenuItem(ButtonName, _ => { LegacyContextMenuCallback(args.ItemId); }, true));
            }
        }

        /// <summary>
        /// Function called when user right-clicks an inventory item.
        /// here.
        /// </summary>
        /// <remarks>
        /// This function needs to be very quick, as the user will experience a delay
        /// in right-clicking an item and seeing the context menu if we do expensive things
        /// here.
        /// </remarks>
        private void ContextMenu_OnOpenInventoryContextMenu(InventoryContextMenuOpenArgs args)
        {
            switch (Service.Configuration.DataSource)
            {
                case DataSource.Internal:
                    NewOnOpenInventoryContextMenu(args);
                    return;
                case DataSource.GarlandTools:
                    LegacyOnOpenInventoryContextMenu(args);
                    return;
            }
        }

        private void NewOnItemTooltip(ItemTooltip itemtooltip, ulong itemid)
        {
            ItemInfo itemInfo = _itemLookup.GetItemInfo(CorrectitemId((uint)itemid));
            if (itemInfo == null)
            {
                return;
            }

            SeString origStr = itemtooltip[ItemTooltipString.ShopSellingPrice];

            switch (itemInfo.Type)
            {
                case ItemType.GcShop:
                    List<NpcInfo> npcInfos = itemInfo.NpcInfos;
                    NpcInfo info = new();
                    unsafe
                    {
                        // Only do this if the item can be found among the companies
                        if (npcInfos.Count > 1)
                        {
                            byte gc = UIState.Instance()->PlayerState.GrandCompany;
                            info = gc switch
                            {
                                1 => npcInfos.Find(i => i.Id == 1002387),
                                2 => npcInfos.Find(i => i.Id == 1002393),
                                3 => npcInfos.Find(i => i.Id == 1002390),
                            };
                        }
                        else
                        {
                            info = npcInfos.First();
                        }
                    }

                    string costStr = $"{info.Costs[0].Item2} x{info.Costs[0].Item1}";

                    itemtooltip[ItemTooltipString.ShopSellingPrice] = string.Concat(origStr.TextValue.AsSpan(0, origStr.TextValue.IndexOfAny(new[] { '：', ':' })), "：", costStr);
                    return;
                case ItemType.SpecialShop:
                    itemtooltip[ItemTooltipString.ShopSellingPrice] = string.Concat(origStr.TextValue.AsSpan(0, origStr.TextValue.IndexOfAny(new[] { '：', ':' })), "：Special Vendor");
                    return;
            }
        }

        private void LegacyOnItemTooltip(ItemTooltip itemTooltip, ulong itemId)
        {
            //HQ items don't have recipes, only NQ items
            if (itemId > 1000000)
            {
                itemId -= 1000000;
            }

            if (_legacyStuff.IsItemSoldByGilVendor((uint)itemId))
            {
                return;
            }
            List<GCScripShopItem> items = new(Service.DataManager.GetExcelSheet<GCScripShopItem>()!.Where(i => i.Item.Row == itemId));
            // This code assumes all GC shops sell items for the same seal cost, which should be a safe assumption
            if (items.Count > 0)
            {
                itemTooltip[ItemTooltipString.ShopSellingPrice] = $"Shop Selling Price: {items[0].CostGCSeals} GC Seals";
                return;
            }

            if (_legacyStuff.IsItemSoldBySpecialVendor((uint)itemId))
            {
                itemTooltip[ItemTooltipString.ShopSellingPrice] = "Shop Selling Price: Special Vendor";
                return;
            }
        }

        /// <summary>
        /// Function called when an in-game tooltip is generated.
        /// </summary>
        /// <remarks>
        /// This function needs to be very quick, as the user will experience a delay
        /// when any tooltip is generated if we do expensive things here.
        /// </remarks>
        private void Tooltips_OnOnItemTooltip(ItemTooltip itemtooltip, ulong itemid)
        {
            switch (Service.Configuration.DataSource)
            {
                case DataSource.Internal:
                    NewOnItemTooltip(itemtooltip, itemid);
                    break;
                case DataSource.GarlandTools:
                    LegacyOnItemTooltip(itemtooltip, itemid);
                    break;
            }
        }

        private void NewContextMenuCallback(ItemInfo itemInfo)
        {
            // filteredResults allows us to apply filters without modifying core data,
            // itemInfo is initialized once upon plugin load, so a filter would not
            // be able to be unchecked otherwise
            ItemInfo filteredResults = new()
            {
                AchievementDescription = itemInfo.AchievementDescription,
                Id = itemInfo.Id,
                Name = itemInfo.Name,
                Type = itemInfo.Type,
                NpcInfos = itemInfo.NpcInfos,
            };

            filteredResults.ApplyFilters();

            switch (Service.Configuration.ResultsViewType)
            {
                case ResultsViewType.Multiple:
                    ShowMultipleVendors(filteredResults);
                    return;
                case ResultsViewType.Single:
                    ShowSingleVendor(filteredResults);
                    return;
            }
        }

        private void LegacyContextMenuCallback(uint itemId)
        {
            // we use threading here so that the game ui is not frozen while expensive
            // operations take place
            _ = Task.Run(() =>
            {
                LegacyStuff _legacyStuff = new();

                ItemInfo itemInfo = _legacyStuff.GetItemInfo(CorrectitemId(itemId));

                itemInfo.ApplyFilters();

                switch (Service.Configuration.ResultsViewType)
                {
                    case ResultsViewType.Multiple:
                        ShowMultipleVendors(itemInfo);
                        return;
                    case ResultsViewType.Single:
                        ShowSingleVendor(itemInfo);
                        return;
                }
            });
        }

        /// <summary>
        /// This function is called when our custom context menu option is clicked.
        /// Therefore, all the heavy lifting needs to be done here. A small delay here
        /// is acceptable, since we know the user is wanting to interact with the plugin
        /// </summary>
        private void ContextMenuCallback(uint itemId, ItemInfo itemInfo)
        {
            switch (Service.Configuration.DataSource)
            {
                case DataSource.Internal:
                    NewContextMenuCallback(itemInfo);
                    return;
                case DataSource.GarlandTools:
                    LegacyContextMenuCallback(itemId);
                    return;
            }
        }

        private static void ShowMultipleVendors(ItemInfo item)
        {
            Service.PluginUi.SetItemToDisplay(item);
            Service.PluginUi.IsOpen = true;
        }

        private static void ShowSingleVendor(ItemInfo item)
        {
            NpcInfo vendor = item.NpcInfos[0];
            SeStringBuilder sb = new();

            _ = sb.AddUiForeground(45);
            _ = sb.AddText("[Item Vendor Location]");
            _ = sb.AddUiForegroundOff();
            _ = sb.Append(SeString.CreateItemLink(item.Id, false));
            _ = sb.AddText(" can be purchased from ");
            _ = sb.AddUiForeground(Service.Configuration.NPCNameChatColor);
            _ = sb.AddText(vendor.Name);
            _ = sb.AddUiForegroundOff();
            _ = sb.AddText(" at ");
            _ = sb.Append(SeString.CreateMapLink(vendor.Location.TerritoryType, vendor.Location.MapId, vendor.Location.MapX, vendor.Location.MapY));

            Service.ChatGui.PrintChat(new XivChatEntry
            {
                Message = sb.BuiltString
            });
        }

        #region IDisposable Support     
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _xivCommon.Functions.Tooltips.OnItemTooltip -= Tooltips_OnOnItemTooltip;
            Service.ContextMenu.OnOpenInventoryContextMenu -= ContextMenu_OnOpenInventoryContextMenu;
            Service.ContextMenu.OnOpenGameObjectContextMenu -= ContextMenu_OnOpenGameObjectContextMenu;
            Service.ContextMenu.Dispose();

            Service.Interface.UiBuilder.Draw -= _windowSystem.Draw;
            Service.Interface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
            _windowSystem.RemoveAllWindows();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}