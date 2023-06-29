using System;
using System.Collections.Generic;
using System.Linq;
using CheapLoc;
using Dalamud.ContextMenu;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemVendorLocation.Models;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XivCommon;
using XivCommon.Functions.Tooltips;
using GrandCompany = FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany;
using AgentInterface = FFXIVClientStructs.FFXIV.Component.GUI.AgentInterface;
using Dalamud.Logging;

namespace ItemVendorLocation
{
    public class EntryPoint : IDalamudPlugin
    {
        private static readonly List<string> GameAddonWhitelist = new()
        {
             "ChatLog",
             "ColorantColoring",
             "ContentsInfoDetail",
             "DailyQuestSupply",
             "HousingCatalogPreview",
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

        public readonly Dictionary<GrandCompany, uint> OicVendorIdMap = new()
        {
            { GrandCompany.Maelstrom, 1002389 },
            { GrandCompany.TwinAdder, 1000165 },
            { GrandCompany.ImmortalFlames, 1003925 },
        };

        private readonly string ButtonName = "";
#if DEBUG
        public readonly ItemLookup _itemLookup;
#else
        private readonly ItemLookup _itemLookup;
#endif
        private readonly WindowSystem _windowSystem;
        private readonly SettingsWindow _configWindow;
        private readonly XivCommonBase _xivCommon;

        private readonly ExcelSheet<Item> _items;

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

            // Initialize the UI
            _windowSystem = new WindowSystem(typeof(EntryPoint).AssemblyQualifiedName);
            _configWindow = new();
            Service.PluginUi = new PluginWindow();

            _items = Service.DataManager.GetExcelSheet<Item>();

            _windowSystem.AddWindow(Service.PluginUi);
            _windowSystem.AddWindow(_configWindow = new());

            _xivCommon.Functions.Tooltips.OnItemTooltip += Tooltips_OnOnItemTooltip;
            Service.ContextMenu.OnOpenInventoryContextMenu += ContextMenu_OnOpenInventoryContextMenu;
            Service.ContextMenu.OnOpenGameObjectContextMenu += ContextMenu_OnOpenGameObjectContextMenu;
            Service.Interface.UiBuilder.Draw += _windowSystem.Draw;
            Service.Interface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
            _ = Service.CommandManager.AddHandler(Service.Configuration.CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Displays the Item Vendor Location config window",
            });
        }

        private void OnCommand(string command, string args)
        {
            if (args.IsNullOrEmpty())
            {
                _configWindow.IsOpen = true;
            }
            else
            {
                if (_items.Any(i => i.Name.RawString.ToLower() == args.ToLower()))
                {
                    List<Item> items = _items.Where(i => i.Name.RawString.ToLower() == args.ToLower()).ToList();
                    foreach (Item item in items)
                    {
                        ItemInfo itemDetails = _itemLookup.GetItemInfo(item.RowId);
                        if (itemDetails == null)
                        {
                            continue;
                        }
                        ShowSingleVendor(itemDetails);
                    }
                }
                else
                {
                    List<Item> items = _items.Where(i => i.Name.RawString.ToLower().Contains(args.ToLower())).ToList();
                    if (items.Count == 0)
                    {
                        SeStringBuilder sb = new();

                        _ = sb.AddUiForeground(45);
                        _ = sb.AddText("[Item Vendor Location]");
                        _ = sb.AddUiForegroundOff();
                        _ = sb.AddText($" No vendors found for \"{args}\"");
                        Service.ChatGui.PrintChat(new XivChatEntry
                        {
                            Message = sb.BuiltString
                        });
                    }
                    else
                    {
                        SeStringBuilder sb = new();

                        _ = sb.AddUiForeground(45);
                        _ = sb.AddText("[Item Vendor Location]");
                        _ = sb.AddUiForegroundOff();
                        _ = sb.AddText($" {items.Count} {(items.Count == 1 ? "item" : "items")} found for \"{args}\"");
                        Service.ChatGui.PrintChat(new XivChatEntry
                        {
                            Message = sb.BuiltString
                        });
                        if (items.Count > 20)
                        {
                            sb = new();
                            _ = sb.AddUiForeground(45);
                            _ = sb.AddText("[Item Vendor Location]");
                            _ = sb.AddUiForegroundOff();
                            _ = sb.AddText($" You may want to refine your search");
                            Service.ChatGui.PrintChat(new XivChatEntry
                            {
                                Message = sb.BuiltString
                            });
                        }
                        uint results = 0;
                        foreach (Item item in items)
                        {
                            if (results == Service.Configuration.MaxSearchResults)
                            {
                                break;
                            }
                            ItemInfo itemDetails = _itemLookup.GetItemInfo(item.RowId);
                            if (itemDetails == null)
                            {
                                continue;
                            }
                            results++;
                            ShowSingleVendor(itemDetails);
                        }
                    }
                }
            }
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
                 > 500000 and < 1000000 => itemId - 500000, // collectible, doesnt seem to work
                 */
                _ => itemId
            };
        }

        private void OnOpenGameObjectContextMenu(GameObjectContextMenuOpenArgs args, uint itemId)
        {
            ItemInfo itemInfo = _itemLookup.GetItemInfo(itemId);
            if (itemInfo == null)
            {
                return;
            }
            args.AddCustomItem(new GameObjectContextMenuItem(ButtonName, _ => { ContextMenuCallback(itemInfo); }, true));
            return;
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

            // thank you ottermandias for the offsets
            uint itemId;
            if (args.ParentAddonName == "RecipeNote")
            {
                nint recipeNoteAgent = Service.GameGui.FindAgentInterface(args.ParentAddonName);
                unsafe
                {
                    itemId = *(uint*)(recipeNoteAgent + 0x398);
                }
            }
            else if (args.ParentAddonName is "RecipeTree" or "RecipeMaterialList")
            {
                unsafe
                {
                    UIModule* uiModule = (UIModule*)Service.GameGui.GetUIModule();
                    AgentModule* agents = uiModule->GetAgentModule();
                    AgentInterface* agent = agents->GetAgentByInternalId(AgentId.RecipeItemContext);

                    itemId = *(uint*)((nint)agent + 0x28);
                }
            }
            else if (args.ParentAddonName == "ColorantColoring")
            {
                nint colorantColoringAgent = Service.GameGui.FindAgentInterface(args.ParentAddonName);
                unsafe
                {
                    itemId = *(uint*)(colorantColoringAgent + 0x34);
                }
            }
            else
            {
                itemId = CorrectitemId((uint)Service.GameGui.HoveredItem);
            }
                OnOpenGameObjectContextMenu(args, itemId);
        }

        private void OnOpenInventoryContextMenu(InventoryContextMenuOpenArgs args)
        {
            uint itemId = CorrectitemId(args.ItemId);
            ItemInfo itemInfo = _itemLookup.GetItemInfo(itemId);
            if (itemInfo == null)
            {
                return;
            }

            args.AddCustomItem(new InventoryContextMenuItem(ButtonName, _ => { ContextMenuCallback(itemInfo); }, true));
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
            OnOpenInventoryContextMenu(args);
        }

        private void OnItemTooltip(ItemTooltip itemtooltip, ulong itemid)
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
                case ItemType.FcShop:
                    info = itemInfo.NpcInfos.First();
                    costStr = $"FC Credits x{info.Costs[0].Item1}";
                    itemtooltip[ItemTooltipString.ShopSellingPrice] = string.Concat(origStr.TextValue.AsSpan(0, origStr.TextValue.IndexOfAny(new[] { '：', ':' })), "：", costStr);
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
            OnItemTooltip(itemtooltip, itemid);
        }

        private void ContextMenuCallback(ItemInfo itemInfo)
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

        /// <summary>
        /// This function is called when our custom context menu option is clicked.
        /// Therefore, all the heavy lifting needs to be done here. A small delay here
        /// is acceptable, since we know the user is wanting to interact with the plugin
        /// </summary>
        private void ContextMenuCallback(uint itemId, ItemInfo itemInfo)
        {
            ContextMenuCallback(itemInfo);
        }

        private static void ShowMultipleVendors(ItemInfo item)
        {
            Service.PluginUi.SetItemToDisplay(item);
            Service.PluginUi.IsOpen = true;
        }

        private static void ShowSingleVendor(ItemInfo item)
        {
            NpcInfo vendor = null;
            try
            {
                vendor = item.NpcInfos.First(i => i.Location != null);
            }
            catch (InvalidOperationException) {
                PluginLog.LogError($"NpcInfos does not contain data for {item.Name}");
                return;
            }
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

            _ = Service.CommandManager.RemoveHandler(Service.Configuration.CommandName);
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