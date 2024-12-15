using ItemVendorLocation.Models;
using Lumina.Excel.Sheets;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud;
using Dalamud.Game;

namespace ItemVendorLocation;
#if DEBUG
public partial class ItemLookup
#else
public partial class ItemLookup
#endif
{
    private void AddSpecialItem(SpecialShop specialShop, ENpcBase npcBase, ENpcResident resident, ItemType type = ItemType.SpecialShop, string? shop = null)
    {
        foreach (var entry in specialShop.Item)
        {
            for (var i = 0; i < entry.ReceiveItems.Count; i++)
            {
                var item = entry.ReceiveItems[i].Item.Value;
                var costs = (from e in entry.ItemCosts where e.ItemCost.IsValid && e.ItemCost.Value.Name != string.Empty select new Tuple<uint, string>(e.CurrencyCost, Utilities.ConvertCurrency(e.ItemCost.Value.RowId, specialShop).Name.ExtractText())).ToList();

                var achievementDescription = "";
                if (type == ItemType.Achievement)
                {
                    achievementDescription = _achievements.Where(i => i.Item.Value.RowId == item.RowId).Select(i => i.Description).First().ExtractText();
                }

                AddItem_Internal(item.RowId, item.Name.ExtractText(), npcBase.RowId, resident.Singular.ExtractText(), shop, costs, _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null, type, achievementDescription);
            }
        }
    }

    private void AddGilShopItem(GilShop gilShop, ENpcBase npcBase, ENpcResident resident, string shop = null)
    {
        for (ushort i = 0; ; i++)
        {
            try
            {
                var item = _gilShopItems.GetSubrowOrDefault(gilShop.RowId, i);

                if (!item.HasValue)
                {
                    break;
                }

                AddItem_Internal(item.Value.Item.Value.RowId, item.Value.Item.Value.Name.ExtractText(), npcBase.RowId, resident.Singular.ExtractText(),
                                 shop != null ? $"{shop}\n{gilShop.Name}" : gilShop.Name.ExtractText(),
                                 new() { new(item.Value.Item.Value.PriceMid, _gil.Name.ExtractText()) },
                                 _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null, ItemType.GilShop);
            }
            catch (Exception)
            {
                break;
            }
        }
    }

    private void AddGcShopItem(GCShop gcId, ENpcBase npcBase, ENpcResident resident)
    {
        // cannot use EndsWith here because the description for each gc seal is different in every language
        // but they all have the grandcompany name in it so Contains is needed
        var seal = _gcSeal.Find(i => i.Description.ExtractText().Contains($"{gcId.GrandCompany.Value.Name.ExtractText()}"));

        foreach (var category in _gcScripShopCategories.Where(i => i.GrandCompany.RowId == gcId.GrandCompany.RowId))
        {
            for (ushort i = 0;; i++)
            {
                try
                {
                    var item = _gcScripShopItems.GetSubrowOrDefault(category.RowId, i);
                    if (item == null)
                    {
                        break;
                    }

                    if (item.Value.SortKey == 0)
                    {
                        break;
                    }

                    AddItem_Internal(item.Value.Item.Value.RowId, item.Value.Item.Value.Name.ExtractText(), npcBase.RowId, resident.Singular.ExtractText(), null,
                                     new() { new(item.Value.CostGCSeals, seal.Name.ExtractText()) },
                                     _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null, ItemType.GcShop);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }

    private void AddInclusionShop(InclusionShop inclusionShop, ENpcBase npcBase, ENpcResident resident)
    {
        foreach (var category in inclusionShop.Category)
        {
            if (category.Value.RowId == 0)
            {
                continue;
            }

            for (ushort i = 0;; i++)
            {
                try
                {
                    var series = _inclusionShopSeries.GetSubrowOrDefault(category.Value.InclusionShopSeries.RowId, i);
                    if (!series.HasValue)
                    {
                        break;
                    }

                    var specialShop = series.Value.SpecialShop.Value;
                    var shop = "";
                    if (!string.IsNullOrEmpty(inclusionShop.Unknown0.ExtractText()))
                    {
                        shop += $"{inclusionShop.Unknown0.ExtractText()}\n";
                    }
                    shop += $"{category.Value.Name}\n{specialShop.Name}";
                    AddSpecialItem(specialShop, npcBase, resident, shop: shop);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }

    private void AddFccShop(FccShop shop, ENpcBase npcBase, ENpcResident resident)
    {
        for (var i = 0; i < shop.ItemData.Count; i++)
        {
            var item = _items.GetRowOrDefault(shop.ItemData[i].Item.RowId);
            if (item == null || item.Value.Name == string.Empty)
            {
                continue;
            }

            var cost = shop.ItemData[i].Cost;

            AddItem_Internal(item.Value.RowId, item.Value.Name.ExtractText(), npcBase.RowId, resident.Singular.ExtractText(), null, new() { new(cost, _fccName.Text.ExtractText()) },
                             _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null, ItemType.FcShop);
        }
    }

    private void AddItemsInPrehandler(PreHandler preHandler, ENpcBase npcBase, ENpcResident resident)
    {
        var target = preHandler.Target.RowId;
        if (target == 0)
        {
            return;
        }

        if (MatchEventHandlerType(target, EventHandlerType.GilShop))
        {
            var gilShop = _gilShops.GetRow(target);
            AddGilShopItem(gilShop, npcBase, resident);
            return;
        }

        if (MatchEventHandlerType(target, EventHandlerType.SpecialShop))
        {
            var specialShop = _specialShops.GetRow(target);
            AddSpecialItem(specialShop, npcBase, resident);
            return;
        }

        if (MatchEventHandlerType(target, EventHandlerType.InclusionShop))
        {
            var inclusionShop = _inclusionShops.GetRow(target);
            AddInclusionShop(inclusionShop, npcBase, resident);
            return;
        }
    }

    private void AddItemsInTopicSelect(TopicSelect topicSelect, ENpcBase npcBase, ENpcResident resident)
    {
        foreach (var data in topicSelect.Shop.Select(x => x.RowId))
        {
            if (data == 0)
            {
                continue;
            }

            if (MatchEventHandlerType(data, EventHandlerType.SpecialShop))
            {
                var specialShop = _specialShops.GetRow(data);
                
                AddSpecialItem(specialShop, npcBase, resident, shop: $"{topicSelect.Name.ExtractText()}\n{specialShop.Name.ExtractText()}");

                continue;
            }

            if (MatchEventHandlerType(data, EventHandlerType.GilShop))
            {
                var gilShop = _gilShops.GetRow(data);
                AddGilShopItem(gilShop, npcBase, resident, shop: topicSelect.Name.ExtractText());
                continue;
            }

            if (MatchEventHandlerType(data, EventHandlerType.PreHandler))
            {
                var preHandler = _preHandlers.GetRow(data);
                AddItemsInPrehandler(preHandler, npcBase, resident);
                continue;
            }
        }
    }

    private void AddCollectablesShop(CollectablesShop shop, ENpcBase npcBase, ENpcResident resident)
    {
        // skip rows without name
        if (shop.Name.ExtractText() == string.Empty)
        {
            return;
        }

        for (int i = 0; i < shop.ShopItems.Count; i++)
        {
            var row = shop.ShopItems[i].Value.RowId;

            // 0 is unspecified, we dont need that
            if (row == 0)
            {
                continue;
            }

            // 100 should be enough.. unless SE add more subrows in the future
            for (ushort subRow = 0; subRow < 100; subRow++)
            {
                try
                {
                    var exchangeItem = _collectablesShopItems.GetSubrow(row, subRow);
                    var rewardItem = _collectablesShopRewardItems.GetRow(exchangeItem.CollectablesShopRewardScrip.RowId);
                    var refine = _collectablesShopRefines.GetRow(exchangeItem.CollectablesShopRefine.RowId);
                    // filter out junk data
                    //if (exchangeItem.Item.Value == null || exchangeItem.Item.Row <= 1000)
                    if (exchangeItem.Item.RowId <= 1000)
                    {
                        continue;
                    }

                    // This may be confusing, because some things are incorrectly labeled in Lumina
                    // CollectableShopRewardScrip is column 8, which is actually the ID in CollectableShopRewardItem
                    // The reward item is the item that you exchange the collectalbe for.


                    AddItem_Internal(rewardItem.Item.Value.RowId, rewardItem.Item.Value.Name.ExtractText(), npcBase.RowId,
                                     resident.Singular.ExtractText(), exchangeItem.CollectablesShopItemGroup.Value.Name.ExtractText(),
                                     new()
                                     {
                                         new(rewardItem.RewardLow, $"{exchangeItem.Item.Value.Name} min collectability of {refine.LowCollectability}"),
                                         new(rewardItem.RewardMid, $"{exchangeItem.Item.Value.Name} min collectability of {refine.MidCollectability}"),
                                         new(rewardItem.RewardHigh, $"{exchangeItem.Item.Value.Name} min collectability of {refine.HighCollectability}"),
                                     }, /* Will build cost later*/
                                     _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null,
                                     ItemType.CollectableExchange /*Yes this is special shop*/);
                }
                catch
                {
                    break;
                }
            }
        }
    }

    private void AddQuestReward(QuestClassJobReward questReward, ENpcBase npcBase, ENpcResident resident, List<Tuple<uint, string>> cost = null)
    {
        if (questReward.ClassJobCategory.RowId == 0)
        {
            return;
        }

        if (cost == null)
        {
            cost = new();

            // Build the cost first
            for (int i = 0; i < questReward.RequiredItem.Count; i++)
            {
                var requireItem = questReward.RequiredItem[i];
                if (requireItem.RowId == 0)
                {
                    break;
                }

                cost.Add(new(questReward.RequiredAmount[i], requireItem.Value.Name.ExtractText()));
            }
        }

        // Add the reward items
        for (int i = 0; i < questReward.RewardItem.Count; i++)
        {
            var rewardItem = questReward.RewardItem[i];
            if (rewardItem.RowId == 0)
            {
                break;
            }

            AddItem_Internal(rewardItem.RowId, rewardItem.Value.Name.ExtractText(), npcBase.RowId, resident.Singular.ExtractText(), "",
                             cost, _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null,
                             ItemType.QuestReward);
        }
    }

    private void AddQuestRewardCost(QuestClassJobReward questReward, ENpcBase npcBase, List<Tuple<uint, string>> cost)
    {
        if (cost == null || questReward.ClassJobCategory.RowId == 0)
        {
            return;
        }

        for (int i = 0; i < questReward.RewardItem.Count; i++)
        {
            var rewardItem = questReward.RewardItem[i];
            if (rewardItem.RowId == 0)
            {
                break;
            }

            AddItemCost(rewardItem.RowId, npcBase.RowId, cost);
        }
    }


    private void AddAchievementItem()
    {
        for (var i = 1006004u; i <= 1006006; i++)
        {
            var npcBase = _eNpcBases.GetRow(i);
            var resident = _eNpcResidents.GetRow(i);

            for (var j = 1769898u; j <= 1769906; j++)
            {
                AddSpecialItem(_specialShops.GetRow(j), npcBase, resident, ItemType.Achievement);
            }
        }
    }

    private void AddItemCost(uint itemId, uint npcId, List<Tuple<uint, string>> cost)
    {
        if (itemId == 0)
        {
            return;
        }

        if (!_itemDataMap.TryGetValue(itemId, out var itemInfo))
        {
            Service.PluginLog.Error($"Failed to get value for ItemId \"{itemId}\" when adding item cost, did you call AddItemCost before the item is added to datamap?");
            return;
        }

        var result = itemInfo.NpcInfos.Find(i => i.Id == npcId);
        if (result == null)
        {
            Service.PluginLog.Error($"Failed to find npcId \"{npcId}\" for ItemId \"{itemId}\" when adding item cost, did you call AddItemCost before the item is added to datamap?");
            return;
        }

        result.Costs.AddRange(cost);
    }

    private void AddItem_Internal(uint itemId, string itemName, uint npcId, string npcName, string? shopName, List<Tuple<uint, string>> cost, NpcLocation npcLocation,
                                  ItemType type,
                                  string achievementDesc = "")
    {
        if (itemId == 0)
        {
            return;
        }
        
        if (Service.ClientState.ClientLanguage != ClientLanguage.Japanese && shopName == "アイテムの購入")
            shopName = string.Empty;

        if (!_itemDataMap.ContainsKey(itemId))
        {
            _itemDataMap.Add(itemId, new()
            {
                Id = itemId,
                Name = itemName,
                NpcInfos = new() { new() { Id = npcId, Location = npcLocation, Costs = cost, Name = npcName, ShopName = shopName } },
                Type = type,
                AchievementDescription = achievementDesc,
            });
            return;
        }

        if (!_itemDataMap.TryGetValue(itemId, out var itemInfo))
        {
            _ = _itemDataMap.TryAdd(itemId, itemInfo = new()
            {
                Id = itemId,
                Name = itemName,
                NpcInfos = new() { new() { Id = npcId, Location = npcLocation, Costs = cost, Name = npcName, ShopName = shopName } },
                Type = type,
                AchievementDescription = achievementDesc,
            });
        }

        if (type == ItemType.Achievement && itemInfo.Type != ItemType.Achievement)
        {
            itemInfo.Type = ItemType.Achievement;
            itemInfo.AchievementDescription = achievementDesc;
        }

        if (itemInfo.NpcInfos.Find(j => j.Id == npcId) == null)
        {
            itemInfo.NpcInfos.Add(new() { Id = npcId, Location = npcLocation, Name = npcName, Costs = cost, ShopName = shopName });
        }
    }
}