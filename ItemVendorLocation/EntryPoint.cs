using System;
using System.Linq;
using Dalamud.ContextMenu;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ItemVendorLocation.Models;
using XivCommon;
using XivCommon.Functions.Tooltips;

namespace ItemVendorLocation;

public class EntryPoint : IDalamudPlugin
{
    private const string ButtonName = "Vendor location";
    private readonly ItemLookup _itemLookup;
    private readonly WindowSystem _windowSystem;
    private readonly XivCommonBase _xivCommon;

    public EntryPoint([RequiredVersion("1.0")] DalamudPluginInterface pi)
    {
        pi.Create<DalamudApi>();
        pi.Create<Plugin>();
        DalamudApi.ContextMenu = new DalamudContextMenu();
        _xivCommon = new XivCommonBase(Hooks.Tooltips);
        _itemLookup = new ItemLookup();

        // Initialize the UI
        _windowSystem = new WindowSystem(typeof(EntryPoint).AssemblyQualifiedName);
        Plugin.PluginUi = new PluginWindow();

        _windowSystem.AddWindow(Plugin.PluginUi);

        _xivCommon.Functions.Tooltips.OnItemTooltip += Tooltips_OnOnItemTooltip;
        DalamudApi.ContextMenu.OnOpenInventoryContextMenu += ContextMenu_OnOpenInventoryContextMenu;
        DalamudApi.ContextMenu.OnOpenGameObjectContextMenu += ContextMenu_OnOpenGameObjectContextMenu;
        DalamudApi.Interface.UiBuilder.Draw += _windowSystem.Draw;
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
        if (args.ObjectWorld != 0) return;

        uint itemId;
        ItemInfo itemInfo;
        switch (args.ParentAddonName)
        {
            case "RecipeNote":
                unsafe
                {
                    // thank you ottermandias
                    var recipeNoteAgen = DalamudApi.GameGui.FindAgentInterface(args.ParentAddonName);
                    itemId = *(uint*)(recipeNoteAgen + 0x398);
                    itemInfo = _itemLookup.GetItemInfo(CorrectitemId(itemId));
                    if (itemInfo == null)
                        return;

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
                    return;

                args.AddCustomItem(new GameObjectContextMenuItem(ButtonName, _ => { ContextMenuCallback(itemInfo); }, true));
                return;
        }
    }

    private void ContextMenu_OnOpenInventoryContextMenu(InventoryContextMenuOpenArgs args)
    {
        var itemId = CorrectitemId(args.ItemId);
        var itemInfo = _itemLookup.GetItemInfo(itemId);
        if (itemInfo == null)
            return;

        args.AddCustomItem(new InventoryContextMenuItem(ButtonName, _ => { ContextMenuCallback(itemInfo); }, true));
    }

    private void Tooltips_OnOnItemTooltip(ItemTooltip itemtooltip, ulong itemid)
    {
        var itemInfo = _itemLookup.GetItemInfo(CorrectitemId((uint)itemid));
        if (itemInfo == null)
            return;

        var origStr = itemtooltip[ItemTooltipString.ShopSellingPrice];

        switch (itemInfo.Type)
        {
            case ItemType.GcShop:
                var npcInfos = itemInfo.NpcInfos;
                var info = new NpcInfo();
                unsafe
                {
                    // Only do this if the item can be found among the companies
                    if (npcInfos.Count > 1)
                    {
                        var gc = UIState.Instance()->PlayerState.GrandCompany;
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

                var costStr = $"{info.Costs[0].Item2} x{info.Costs[0].Item1}";

                itemtooltip[ItemTooltipString.ShopSellingPrice] = origStr.TextValue.Substring(0, origStr.TextValue.IndexOfAny(new[] { '：', ':' })) + "：" + costStr;
                return;
            case ItemType.SpecialShop:
                itemtooltip[ItemTooltipString.ShopSellingPrice] = origStr.TextValue.Substring(0, origStr.TextValue.IndexOfAny(new[] { '：', ':' })) + "：Special vendor";
                return;
        }
    }

    private void ContextMenuCallback(ItemInfo item)
    {
        Plugin.PluginUi.SetItemToDisplay(item);
        Plugin.PluginUi.IsOpen = true;
    }

#region IDisposable Support
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        _xivCommon.Functions.Tooltips.OnItemTooltip -= Tooltips_OnOnItemTooltip;
        DalamudApi.ContextMenu.OnOpenInventoryContextMenu -= ContextMenu_OnOpenInventoryContextMenu;
        DalamudApi.ContextMenu.OnOpenGameObjectContextMenu -= ContextMenu_OnOpenGameObjectContextMenu;
        DalamudApi.ContextMenu.Dispose();

        DalamudApi.Interface.UiBuilder.Draw -= _windowSystem.Draw;
        _windowSystem.RemoveAllWindows();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
#endregion
}