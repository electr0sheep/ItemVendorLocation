using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.ContextMenu;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ItemVendorLocation.Models;
using XivCommon;
using XivCommon.Functions.Tooltips;

namespace ItemVendorLocation
{
    public class EntryPoint : IDalamudPlugin
    {
        private const string ButtonName = "Vendor location";
        private readonly ItemLookup _itemLookup;
        private readonly WindowSystem _windowSystem;
        private readonly SettingsWindow _configWindow;
        private readonly XivCommonBase _xivCommon;

        public EntryPoint([RequiredVersion("1.0")] DalamudPluginInterface pi)
        {
            _ = pi.Create<Service>();
            _ = pi.Create<DalamudApi>();
            _ = pi.Create<Plugin>();

            Service.Plugin = this;
            Service.Configuration = pi.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
            DalamudApi.ContextMenu = new DalamudContextMenu();
            _xivCommon = new XivCommonBase(Hooks.Tooltips);
            _itemLookup = new ItemLookup();

            // Initialize the UI
            _windowSystem = new WindowSystem(typeof(EntryPoint).AssemblyQualifiedName);
            _configWindow = new();
            Plugin.PluginUi = new PluginWindow();

            _windowSystem.AddWindow(Plugin.PluginUi);
            _windowSystem.AddWindow(_configWindow = new());

            _xivCommon.Functions.Tooltips.OnItemTooltip += Tooltips_OnOnItemTooltip;
            DalamudApi.ContextMenu.OnOpenInventoryContextMenu += ContextMenu_OnOpenInventoryContextMenu;
            DalamudApi.ContextMenu.OnOpenGameObjectContextMenu += ContextMenu_OnOpenGameObjectContextMenu;
            DalamudApi.Interface.UiBuilder.Draw += _windowSystem.Draw;
            DalamudApi.Interface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
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

        private void ContextMenu_OnOpenGameObjectContextMenu(GameObjectContextMenuOpenArgs args)
        {
            // I think players have a world and items never will??
            // Hopefully this removes the vendor menu for players in the chat log
            if (args.ObjectWorld != 0)
            {
                return;
            }

            uint itemId;
            ItemInfo itemInfo;
            switch (args.ParentAddonName)
            {
                case "RecipeNote":
                    unsafe
                    {
                        // thank you ottermandias
                        nint recipeNoteAgen = DalamudApi.GameGui.FindAgentInterface(args.ParentAddonName);
                        itemId = *(uint*)(recipeNoteAgen + 0x398);
                        itemInfo = _itemLookup.GetItemInfo(CorrectitemId(itemId));
                        if (itemInfo == null)
                        {
                            return;
                        }

                        args.AddCustomItem(new GameObjectContextMenuItem(ButtonName, _ => { ContextMenuCallback(itemInfo); }, true));
                        return;
                    }
                case "ChatLog":
                case "DailyQuestSupply":
                case "ItemSearch":
                case "ShopExchangeItem":
                case "ShopExchangeItemDialog":
                case "Journal":
                case "SubmarinePartsMenu":
                case "HousingGoods":
                    itemId = CorrectitemId((uint)DalamudApi.GameGui.HoveredItem);
                    itemInfo = _itemLookup.GetItemInfo(CorrectitemId(itemId));
                    if (itemInfo == null)
                    {
                        return;
                    }

                    args.AddCustomItem(new GameObjectContextMenuItem(ButtonName, _ => { ContextMenuCallback(itemInfo); }, true));
                    return;
            }
        }

        private void ContextMenu_OnOpenInventoryContextMenu(InventoryContextMenuOpenArgs args)
        {
            uint itemId = CorrectitemId(args.ItemId);
            ItemInfo itemInfo = _itemLookup.GetItemInfo(itemId);
            if (itemInfo == null)
            {
                return;
            }

            args.AddCustomItem(new InventoryContextMenuItem(ButtonName, _ => { ContextMenuCallback(itemInfo); }, true));
        }

        private void Tooltips_OnOnItemTooltip(ItemTooltip itemtooltip, ulong itemid)
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
                            switch (gc)
                            {
                                case 1:
                                    info = npcInfos.Find(i => i.Id == 1002387);
                                    break;
                                case 2:
                                    // Order of the Twin Adder
                                    info = npcInfos.Find(i => i.Id == 1002393);
                                    break;
                                case 3:
                                    // Immortal Flames
                                    info = npcInfos.Find(i => i.Id == 1002390);
                                    break;
                            }
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
                    itemtooltip[ItemTooltipString.ShopSellingPrice] = string.Concat(origStr.TextValue.AsSpan(0, origStr.TextValue.IndexOfAny(new[] { '：', ':' })), "：Special vendor");
                    return;
            }
        }

        private static void ContextMenuCallback(ItemInfo item)
        {
            switch (Service.Configuration.ResultsViewType)
            {
                case ResultsViewType.Multiple:
                    ShowMultipleVendors(item);
                    return;
                case ResultsViewType.Single:
                    ShowSingleVendor(item);
                    return;
            }
        }

        private static void ShowMultipleVendors(ItemInfo item)
        {
            Plugin.PluginUi.SetItemToDisplay(item);
            Plugin.PluginUi.IsOpen = true;
        }

        private static void ShowSingleVendor(ItemInfo item)
        {
            NpcInfo vendor = item.NpcInfos[0];
            SeStringBuilder sb = new();

            sb.AddUiForeground(45);
            sb.AddText("[Item Vendor Location]");
            sb.AddUiForegroundOff();
            sb.Append(SeString.CreateItemLink(item.Id, false));
            sb.AddText(" can be purchased from ");
            sb.AddUiForeground(62);
            sb.AddText(vendor.Name);
            sb.AddUiForegroundOff();
            sb.AddText(" at ");
            sb.Append(SeString.CreateMapLink(vendor.Location.TerritoryType, vendor.Location.MapId, vendor.Location.MapX, vendor.Location.MapY));

            DalamudApi.ChatGui.PrintChat(new XivChatEntry
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
            DalamudApi.ContextMenu.OnOpenInventoryContextMenu -= ContextMenu_OnOpenInventoryContextMenu;
            DalamudApi.ContextMenu.OnOpenGameObjectContextMenu -= ContextMenu_OnOpenGameObjectContextMenu;
            DalamudApi.ContextMenu.Dispose();

            DalamudApi.Interface.UiBuilder.Draw -= _windowSystem.Draw;
            DalamudApi.Interface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
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