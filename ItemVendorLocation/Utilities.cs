using System.Collections.Generic;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI;
using ItemInfo = ItemVendorLocation.Models.ItemInfo;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ItemVendorLocation.Models;
using ItemVendorLocation.XIVCommon.Functions.Tooltips;
using System.Linq;
using Lumina.Excel.Sheets;
using Lumina.Excel;

namespace ItemVendorLocation;

internal class Utilities
{
    private static readonly HashSet<string> GameAddonWhitelist = new()
    {
        "CharacterInspect",
        "ChatLog",
        "ColorantColoring",
        "ContentsInfoDetail",
        "DailyQuestSupply",
        "FreeCompanyCreditShop",
        "GrandCompanyExchange",
        "HousingCatalogPreview",
        "HousingGoods",
        "InclusionShop",
        "ItemSearch",
        "Journal",
        "MateriaAttach",
        "MiragePrismPrismBoxCrystallize",
        "RecipeMaterialList",
        "RecipeNote",
        "RecipeTree",
        "ShopExchangeItem",
        "ShopExchangeItemDialog",
        "ShopExchangeCurrency",
        "SubmarinePartsMenu",
        "Tryon",
        "Shop",
    };

    internal static void OutputChatLine(SeString message)
    {
        SeStringBuilder sb = new();
        _ = sb.AddUiForeground("[IVL] ", 45);
        _ = sb.Append(message);
        Service.ChatGui.Print(sb.BuiltString);
    }

    internal static uint CorrectItemId(uint itemId)
    {
        return itemId switch
               {
                   > 1000000 => itemId - 1000000, // hq
                   > 500000 and < 1000000 => itemId - 500000, // collectible, doesnt seem to work
                   _ => itemId,
               };
    }

    internal static unsafe List<(ItemInfo, bool)> GetItemInfoFromContextMenu(IMenuOpenedArgs args)
    {
        var results = new List<(ItemInfo, bool)>();

        if (args.MenuType == ContextMenuType.Inventory)
        {
            var inventoryTarget = (MenuTargetInventory)args.Target;
            if (!inventoryTarget.TargetItem.HasValue)
            {
                return results;
            }

            var itemInfo = Service.Plugin.ItemLookup.GetItemInfo(CorrectItemId(inventoryTarget.TargetItem.Value.ItemId));
            if (itemInfo != null)
                results.Add((itemInfo, false));

            itemInfo = Service.Plugin.ItemLookup.GetItemInfo(CorrectItemId(inventoryTarget.TargetItem.Value.GlamourId));
            if (itemInfo != null)
                results.Add((itemInfo, true));

            return results;
        }

        var addonName = args.AddonName;

        if (string.IsNullOrEmpty(addonName))
        {
            return results;
        }

        if (!GameAddonWhitelist.Contains(addonName))
        {
            return results;
        }

        var defaultTarget = (MenuTargetDefault)args.Target;

        if (defaultTarget.TargetContentId != 0)
        {
            return results;
        }

        uint itemId = 0;
        uint glamorItemId = 0;

        switch (addonName)
        {
            case "RecipeNote":
            {
                var recipeNoteAgent = Service.GameGui.FindAgentInterface(addonName);
                // sig: 89 91 ? ? ? ? 48 8B D9 E8 ? ? ? ? 48 8B C8 48 8B F8 E8 ? ? ? ? 40 F6 C6 (offset is still the same in dt benchmark)
                itemId = *(uint*)(recipeNoteAgent + 0x398);
                break;
            }
            case "RecipeTree" or "RecipeMaterialList":
            {
                var uiModule = (UIModule*)Service.GameGui.GetUIModule().Address;
                var agents = uiModule->GetAgentModule();
                var agent = agents->GetAgentByInternalId(AgentId.RecipeItemContext);
                // sig: 89 51 ? 48 8B D9 48 8B 49 ? 4D 8B F9 (offset is still the same in dt benchmark)
                itemId = *(uint*)((nint)agent + 0x28);
                break;
            }
            case "ColorantColoring":
            {
                var colorantColoringAgent = Service.GameGui.FindAgentInterface(addonName);
                itemId = *(uint*)(colorantColoringAgent + 0x3C);
                break;
            }
            case "GrandCompanyExchange":
            case "ShopExchangeItem":
            {
                var agent = Service.GameGui.FindAgentInterface(addonName);
                // base sig:
                //     dt benchmark: 48 8D 4F ? C6 44 24 ? ? 41 83 CF
                //     6.58: 48 8D 4E ? 44 0F B6 4D
                // offset sig: 89 73 ?? 44 88 63 (offset is still the same in dt benchmark)
                itemId = *(uint*)(agent + 0x54);
                break;
            }
            case "ChatLog":
            {
                    var agent = Service.GameGui.FindAgentInterface(addonName);
                    // 6.58 sig: 89 83 ? ? ? ? E8 ? ? ? ? 66 89 83 ? ? ? ? 66 85 C0
                    // DT benchmark sig: 41 89 86 ? ? ? ? E8 ? ? ? ? 66 41 89 86 ? ? ? ? 66 85 C0 (offset changes in dt benchmark)
                    itemId = *(uint*)(agent + 0x958);
                    break;
            }
            case "ContentsInfoDetail":
            {
                var agent = Service.GameGui.FindAgentInterface("ContentsInfo");
                // sig: 8B 97 ? ? ? ? 48 8B C8 E8 ? ? ? ? E9 ? ? ? ? 48 83 FB ? 75 ? 8B 91 (offset is still the same in dt benchmark)
                itemId = *(uint*)(agent + 0x17CC);
                break;
            }
            case "ItemSearch":
            {
                itemId = CorrectItemId((uint)AgentContext.Instance()->UpdateCheckerParam);
                break;
            }
            case "CharacterInspect":
            {
                var container = InventoryManager.Instance()->GetInventoryContainer(InventoryType.Examine);
                var agent = Service.GameGui.FindAgentInterface(addonName);

                // signature: 89 AB ? ? ? ? E8 ? ? ? ? 48 8B C8 48 8B F8 (offset changes in dt benchmark)
                var selectedSlot = *(int*)(agent + 0x44C);

                var item = container->GetInventorySlot(selectedSlot);
                itemId = CorrectItemId(item->GetItemId());
                glamorItemId = CorrectItemId(item->GlamourId);
                break;
            }
            case "MiragePrismPrismBoxCrystallize":
                {
                    var uiModule = (UIModule*)Service.GameGui.GetUIModule().Address;
                    var agents = uiModule->GetAgentModule();
                    var agent = (AgentMiragePrismPrismBox*)agents->GetAgentByInternalId(AgentId.MiragePrismPrismBox);
                    itemId = CorrectItemId(agent->Data->TempContextItem.ItemId);
                    break;
                }
            // TODO: Find itemId offset in AgentInterface, HoveredItem is inaccurate sometimes (maybe?)
            default:
            {
                itemId = CorrectItemId((uint)Service.GameGui.HoveredItem);
                break;
            }
        }

        var info = Service.Plugin.ItemLookup.GetItemInfo(itemId);
        if (info != null)
        {
            results.Add((info, false));
        }

        info = Service.Plugin.ItemLookup.GetItemInfo(glamorItemId);
        if (info != null)
        {
            results.Add((info, true));
        }

        return results;
    }
    
    internal static unsafe SeString GetToolTipString(uint itemId)
    {
        var itemInfo = Service.Plugin.ItemLookup.GetItemInfo(Utilities.CorrectItemId(itemId));

        if (itemInfo == null)
        {
            return "Shop Selling Price: None";
        }

        switch (itemInfo.Type)
        {
            case ItemType.GilShop:
                var costStr = itemInfo.NpcInfos[0].Costs[0].Item1.ToString();
                return $"Shop Selling Price: {costStr}";
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

                costStr = $"{info.Costs[0].Item2} x{info.Costs[0].Item1}";

                return $"Shop Selling Price: {costStr}";
            case ItemType.SpecialShop:
                return "Shop Selling Price: Special Shop";
            case ItemType.FcShop:
                info = itemInfo.NpcInfos.First();
                costStr = $"FC Credits x{info.Costs[0].Item1}";
                return $"Shop Selling Price: {costStr}";
            case ItemType.CollectableExchange:
                return $"Shop Selling Price: Collectables Exchange Reward";
            default:
                return "Shop Selling Price: None";
        }

    }

    internal static Item ConvertCurrency(uint itemId, SpecialShop specialShop)
    {
        var tomestonesItemSheet = Service.DataManager.GetExcelSheet<TomestonesItem>();
        var itemSheet = Service.DataManager.GetExcelSheet<Item>();
        return itemId is >= 8 or 0
            ? itemSheet.GetRow(itemId)
            : specialShop.UseCurrencyType switch
            {
                16 => itemSheet.GetRow((uint)Dictionaries.Currencies[itemId]),
                8 => itemSheet.GetRow(1),
                4 => itemSheet.GetRow(tomestonesItemSheet.First(i => i.Tomestones.Value.RowId == itemId).Item.RowId),
                _ => itemSheet.GetRow(itemId),
            };
    }
}
