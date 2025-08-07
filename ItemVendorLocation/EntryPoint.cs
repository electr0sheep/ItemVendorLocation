using System;
using System.Collections.Generic;
using System.Linq;
//using CheapLoc;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ItemVendorLocation.Models;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using ItemVendorLocation.XIVCommon;
using ItemVendorLocation.XIVCommon.Functions.Tooltips;
using System.Threading.Tasks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text;
using Dalamud.Bindings.ImGui;
using ItemInfo = ItemVendorLocation.Models.ItemInfo;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace ItemVendorLocation;

public class EntryPoint : IDalamudPlugin
{
    private readonly string _buttonName;

    public ItemLookup ItemLookup;
    public static string Name => "ItemVendorLocation";

    public const string _commandName = "/pvendor";

    private readonly WindowSystem _windowSystem;
    private readonly XivCommonBase _xivCommon;
    private readonly ExcelSheet<Item> _items;

    public EntryPoint(IDalamudPluginInterface pi)
    {
        _ = pi.Create<Service>();

        //Localization.SetupLocalization(Service.ClientState.ClientLanguage);
        _buttonName = "Vendor location";
        ItemLookup = new();
        Service.Plugin = this;
        Service.Configuration = pi.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        Service.ChatTwoIpc = new(pi);
        Service.ChatTwoIpc.Enable();
        Service.ItemVendorLocationIpc = new();

        _xivCommon = new();
        Service.HighlightObject = new();
        Service.HighlightMenus = new();

        // Initialize the UI
        _windowSystem = new(typeof(EntryPoint).AssemblyQualifiedName);
        Service.SettingsUi = new();
        Service.VendorResultsUi = new();
        Service.ItemSearchUi = new();

        _items = Service.DataManager.GetExcelSheet<Item>()!;

        _windowSystem.AddWindow(Service.VendorResultsUi);
        _windowSystem.AddWindow(Service.SettingsUi);
        _windowSystem.AddWindow(Service.ItemSearchUi);

        Service.ChatTwoIpc.OnOpenChatTwoItemContextMenu += OnOpenChatTwoItemContextMenu;
        _xivCommon.Functions.Tooltips.OnItemTooltip += Tooltips_OnOnItemTooltip;
        Service.ContextMenu.OnMenuOpened += ContextMenu_OnMenuOpened;
        Service.Interface.UiBuilder.Draw += _windowSystem.Draw;
        Service.Interface.UiBuilder.OpenConfigUi += OnOpenConfigUi;

        Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "MiragePrismPrismItemDetail", OnMiragePrismPrismItemDetailPreDraw);

        _ = Service.CommandManager.AddHandler(_commandName, new(OnCommand)
        {
            HelpMessage = "Displays the Item Vendor Location config window",

        });
    }

    private static unsafe void OnMiragePrismPrismItemDetailPreDraw(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon.Address;
        var componentNode = addon->GetComponentByNodeId(16);
        if (componentNode == null)
        {
            return;
        }

        var textNode = componentNode->GetTextNodeById(2)->GetAsAtkTextNode();
        var text = textNode->NodeText;
        if (text.ToString().Contains("Shop Selling Price"))
        {
            return;
        }

        var uiModule = (UIModule*)Service.GameGui.GetUIModule().Address;
        var agents = uiModule->GetAgentModule();
        var agent = (AgentMiragePrismPrismItemDetail*)agents->GetAgentByInternalId(AgentId.MiragePrismPrismItemDetail);
        var itemId = Utilities.CorrectItemId(agent->ItemId);
        textNode->SetText($"{text}    {Utilities.GetToolTipString(itemId)}");
    }

    private void ContextMenu_OnMenuOpened(IMenuOpenedArgs args)
    {
        var itemInfos = Utilities.GetItemInfoFromContextMenu(args);
        if (itemInfos.Count == 0)
        {
            return;
        }

        foreach (var (itemInfo, isGlamour) in itemInfos)
        {
            var menuItem = new MenuItem
            {
                IsEnabled = true,
                IsReturn = false,
                IsSubmenu = false,
                Prefix = SeIconChar.BoxedLetterV,
                PrefixColor = 518,
            };

            if (isGlamour)
                menuItem.Name = _buttonName + "(Glamour)";
            else
                menuItem.Name = _buttonName;

            menuItem.OnClicked = _ => { ContextMenuCallback(itemInfo); };
            args.AddMenuItem(menuItem);
        }
    }

    private void OnCommand(string command, string args)
    {
        if (args.IsNullOrEmpty())
        {
            //Service.ItemSearchUi.IsOpen = true;
            Service.SettingsUi.IsOpen = true;
            return;
        }

        if (args == "config")
        {
            Service.SettingsUi.IsOpen = true;
            return;
        }

        _ = Task.Run(() =>
        {
            if (_items.Any(i => string.Equals(i.Name.ExtractText(), args, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var itemDetails in _items.Where(i => string.Equals(i.Name.ExtractText(), args, StringComparison.OrdinalIgnoreCase))
                                                  .Select(item => ItemLookup.GetItemInfo(item.RowId)).Where(itemDetails => itemDetails != null))
                {
                    ShowSingleVendor(itemDetails!);
                }

                return;
            }

            var items = _items.Where(i => i.Name.ExtractText().Contains(args, StringComparison.OrdinalIgnoreCase)).ToList();
            switch (items.Count)
            {
                case 0:
                    Utilities.OutputChatLine($" No items found for \"{args}\"");
                    return;
                case > 20:
                    Utilities.OutputChatLine("You may want to refine your search");
                    break;
            }

            uint results = 0;
            foreach (var item in items)
            {
                if (results == Service.Configuration.MaxSearchResults)
                {
                    Utilities.OutputChatLine($"Displayed {Service.Configuration.MaxSearchResults}/{items.Count} matches.");
                    if (items.Count > Service.Configuration.MaxSearchResults)
                    {
                        Utilities.OutputChatLine("You may want to be more specific.");
                    }

                    break;
                }

                var itemDetails = ItemLookup.GetItemInfo(item.RowId);
                if (itemDetails == null)
                {
                    continue;
                }

                results++;
                ShowSingleVendor(itemDetails);
            }

            if (results == 0)
            {
                Utilities.OutputChatLine($"No vendors found for \"{args}\"");
            }
        });
    }

    private void OnOpenConfigUi()
    {
        Service.SettingsUi.IsOpen = true;
    }

    private void OnOpenChatTwoItemContextMenu(uint itemId)
    {
        var itemInfo = ItemLookup.GetItemInfo(itemId);
        if (itemInfo == null)
        {
            return;
        }

        if (ImGui.Selectable(_buttonName))
        {
            ContextMenuCallback(itemInfo);
        }
    }

    private unsafe void Tooltips_OnOnItemTooltip(ItemTooltip itemtooltip, ulong itemid)
    {
        var itemInfo = ItemLookup.GetItemInfo(Utilities.CorrectItemId((uint)itemid));
        if (itemInfo == null)
        {
            return;
        }

        var origStr = itemtooltip[ItemTooltipString.ShopSellingPrice];
        var colonIndex = origStr.TextValue.IndexOfAny(['：', ':']);

        switch (itemInfo.Type)
        {
            case ItemType.GcShop:
                var npcInfos = itemInfo.NpcInfos;
                var playerGC = UIState.Instance()->PlayerState.GrandCompany;
                var otherGcVendorIds = Dictionaries.GcVendorIdMap.Values.Where(i => i != Dictionaries.GcVendorIdMap[playerGC]);
                // Only remove items if doing so doesn't remove all the results
                if (npcInfos.Any(i => !otherGcVendorIds.Contains(i.Id)))
                {
                    _ = npcInfos.RemoveAll(i => otherGcVendorIds.Contains(i.Id));
                }

                var info = npcInfos.First();

                var costStr = $"{info.Costs[0].Item2} x{info.Costs[0].Item1}";

                itemtooltip[ItemTooltipString.ShopSellingPrice] = string.Concat(origStr.TextValue.AsSpan(0, colonIndex), ": ", costStr);
                return;
            case ItemType.SpecialShop:
                // Avoid modification for certain seasonal items with no Shop Selling Price line
                if (colonIndex != -1)
                {
                    itemtooltip[ItemTooltipString.ShopSellingPrice] = string.Concat(origStr.TextValue.AsSpan(0, colonIndex), ": Special Vendor");
                }

                return;
            case ItemType.FcShop:
                info = itemInfo.NpcInfos.First();
                costStr = $"FC Credits x{info.Costs[0].Item1}";
                itemtooltip[ItemTooltipString.ShopSellingPrice] = string.Concat(origStr.TextValue.AsSpan(0, colonIndex), ": ", costStr);
                return;
            case ItemType.CollectableExchange:
                itemtooltip[ItemTooltipString.ShopSellingPrice] =
                    string.Concat(origStr.TextValue.AsSpan(0, colonIndex), ": Collectables Exchange Reward");
                return;
        }
    }

    private static void ContextMenuCallback(ItemInfo itemInfo)
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

        ResultDisplayHandler(filteredResults);
    }

    private static void ShowMultipleVendors(ItemInfo item)
    {
        Service.VendorResultsUi.SetItemToDisplay(item);
        Service.VendorResultsUi.IsOpen = true;
        Service.VendorResultsUi.Collapsed = false;
        Service.VendorResultsUi.CollapsedCondition = ImGuiCond.Once;
        Service.HighlightObject.SetNpcInfo([.. item.NpcInfos]);
        Service.HighlightMenus.SetNpcInfo([.. item.NpcInfos]);
        Service.HighlightMenus.SetItemName(item.Name);
    }

    private static void ShowSingleVendor(ItemInfo item)
    {
        NpcInfo vendor;
        SeStringBuilder sb = new();
        try
        {
            vendor = item.NpcInfos.First(i => i.Location != null);
        }
        catch (InvalidOperationException)
        {
            _ = sb.AddText("No NPCs with a location could be found for ");
            _ = sb.Append(SeString.CreateItemLink(item.Id, false));
            Utilities.OutputChatLine(sb.BuiltString);
            return;
        }

        _ = sb.Append(SeString.CreateItemLink(item.Id, false));
        _ = sb.AddText(" can be purchased from ");
        _ = sb.AddUiForeground(Service.Configuration.NPCNameChatColor);
        _ = sb.AddText(vendor.Name);
        _ = sb.AddUiForegroundOff();
        _ = sb.AddText(" at ");
        _ = sb.Append(SeString.CreateMapLink(vendor.Location.TerritoryType, vendor.Location.MapId, vendor.Location.MapX, vendor.Location.MapY));
        Utilities.OutputChatLine(sb.BuiltString);
        Service.HighlightObject.SetNpcInfo([.. item.NpcInfos]);
        Service.HighlightMenus.SetNpcInfo([.. item.NpcInfos]);
        Service.HighlightMenus.SetItemName(item.Name);
    }

    private static void ResultDisplayHandler(ItemInfo item)
    {
        var viewType = Service.Configuration.ResultsViewType;
        if (Service.Configuration.SearchDisplayModifier != VirtualKey.NO_KEY && Service.KeyState.GetRawValue(Service.Configuration.SearchDisplayModifier) == 1)
        {
            viewType = viewType == ResultsViewType.Single ? ResultsViewType.Multiple : ResultsViewType.Single;
        }

        if (viewType == ResultsViewType.Multiple)
        {
            ShowMultipleVendors(item);
        }
        else
        {
            ShowSingleVendor(item);
        }
    }

    #region IDisposable Support

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "MiragePrismPrismItemDetail", OnMiragePrismPrismItemDetailPreDraw);

        Service.ChatTwoIpc.OnOpenChatTwoItemContextMenu -= OnOpenChatTwoItemContextMenu;
        Service.ChatTwoIpc.Disable();
        Service.ItemVendorLocationIpc.Dispose();
        Service.HighlightObject.Dispose();
        Service.HighlightMenus.Dispose();

        _ = Service.CommandManager.RemoveHandler(_commandName);
        _xivCommon.Functions.Tooltips.OnItemTooltip -= Tooltips_OnOnItemTooltip;
        _xivCommon.Dispose();
        Service.ContextMenu.OnMenuOpened -= ContextMenu_OnMenuOpened;

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