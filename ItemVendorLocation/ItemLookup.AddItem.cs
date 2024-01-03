using ItemVendorLocation.Models;
using Lumina.Excel.GeneratedSheets;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemVendorLocation;
#if DEBUG
public partial class ItemLookup
#else
internal partial class ItemLookup
#endif
{
    private void AddSpecialItem(SpecialShopCustom specialShop, ENpcBase npcBase, ENpcResident resident, ItemType type = ItemType.SpecialShop, string shop = null)
    {
        if (specialShop == null)
        {
            return;
        }

        foreach (var entry in specialShop.Entries)
        {
            if (entry.Result == null || entry.Cost == null)
            {
                continue;
            }

            foreach (var result in entry.Result)
            {
                if (result.Item.Value == null)
                {
                    continue;
                }

                if (result.Item.Value.Name == string.Empty)
                {
                    continue;
                }

                var costs =
                    (from e in entry.Cost where e.Item != null && e.Item.Value.Name != string.Empty select new Tuple<uint, string>(e.Count, e.Item.Value.Name)).ToList();

                var achievementDescription = "";
                if (type == ItemType.Achievement)
                {
                    achievementDescription = _achievements.Where(i => i.Item.Value == result.Item.Value).Select(i => i.Description).First();
                }

                AddItem_Internal(result.Item.Value.RowId, result.Item.Value.Name, npcBase.RowId, resident.Singular, shop,
                                 costs, _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null, type, achievementDescription);
            }
        }
    }

    private void AddGilShopItem(GilShop gilShop, ENpcBase npcBase, ENpcResident resident, string shop = null)
    {
        if (gilShop == null)
        {
            return;
        }

        for (var i = 0u;; i++)
        {
            try
            {
                var item = _gilShopItems.GetRow(gilShop.RowId, i);

                if (item?.Item.Value == null)
                {
                    break;
                }

                AddItem_Internal(item.Item.Value.RowId, item.Item.Value.Name, npcBase.RowId, resident.Singular,
                                 shop != null ? $"{shop}\n{gilShop.Name}" : gilShop.Name,
                                 new List<Tuple<uint, string>> { new(item.Item.Value.PriceMid, _gil.Name) },
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
        if (gcId == null)
        {
            return;
        }

        var seal = _gcSeal.Find(i => i.Description.RawString.EndsWith($"{gcId.GrandCompany.Value.Name.RawString}."));
        if (seal == null)
        {
            return;
        }

        foreach (var category in _gcScripShopCategories.Where(i => i.GrandCompany.Row == gcId.GrandCompany.Row))
        {
            for (var i = 0u;; i++)
            {
                try
                {
                    var item = _gcScripShopItems.GetRow(category.RowId, i);
                    if (item == null)
                    {
                        break;
                    }

                    if (item.SortKey == 0)
                    {
                        break;
                    }

                    AddItem_Internal(item.Item.Value.RowId, item.Item.Value.Name, npcBase.RowId, resident.Singular, null,
                                     new List<Tuple<uint, string>> { new(item.CostGCSeals, seal.Name) },
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
        if (inclusionShop == null)
        {
            return;
        }

        foreach (var category in inclusionShop.Category)
        {
            if (category.Value.RowId == 0)
            {
                continue;
            }

            for (uint i = 0;; i++)
            {
                try
                {
                    var series = _inclusionShopSeries.GetRow(category.Value.InclusionShopSeries.Row, i);
                    if (series == null)
                    {
                        break;
                    }

                    var specialShop = series.SpecialShopCustoms.Value;
                    AddSpecialItem(specialShop, npcBase, resident, shop: $"{category.Value.Name}\n{specialShop?.Name}");
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
        if (shop == null)
        {
            return;
        }

        for (var i = 0; i < shop.Item.Length; i++)
        {
            var item = _items.GetRow(shop.Item[i]);
            if (item == null || item.Name == string.Empty)
            {
                continue;
            }

            var cost = shop.Cost[i];

            AddItem_Internal(item.RowId, item.Name, npcBase.RowId, resident.Singular, null, new List<Tuple<uint, string>> { new(cost, _fccName.Text) },
                             _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null, ItemType.FcShop);
        }
    }

    private void AddItemsInPrehandler(PreHandler preHandler, ENpcBase npcBase, ENpcResident resident)
    {
        if (preHandler == null)
        {
            return;
        }

        var target = preHandler.Target;
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

        var inclusionShop = _inclusionShops.GetRow(target);
        AddInclusionShop(inclusionShop, npcBase, resident);
    }

    private void AddItemsInTopicSelect(TopicSelect topicSelect, ENpcBase npcBase, ENpcResident resident)
    {
        if (topicSelect == null)
        {
            return;
        }

        foreach (var data in topicSelect.Shop)
        {
            if (data == 0)
            {
                continue;
            }

            if (MatchEventHandlerType(data, EventHandlerType.SpecialShop))
            {
                var specialShop = _specialShops.GetRow(data);
                AddSpecialItem(specialShop, npcBase, resident, shop: topicSelect.Name);

                continue;
            }

            if (MatchEventHandlerType(data, EventHandlerType.GilShop))
            {
                var gilShop = _gilShops.GetRow(data);
                AddGilShopItem(gilShop, npcBase, resident, shop: topicSelect.Name);
                continue;
            }

            var preHandler = _preHandlers.GetRow(data);
            AddItemsInPrehandler(preHandler, npcBase, resident);
        }
    }

    private void AddCollectablesShop(CollectablesShop shop, ENpcBase npcBase, ENpcResident resident)
    {
        if (shop == null)
        {
            return;
        }

        // skip rows without name
        if (shop.Name.RawString == string.Empty)
        {
            return;
        }

        for (uint i = 0; i < shop.ShopItems.Length; i++)
        {
            var row = shop.ShopItems[i].Value.RowId;

            // 0 is unspecified, we dont need that
            if (row == 0)
            {
                continue;
            }

            // 100 should be enough.. unless SE add more subrows in the future
            for (uint subRow = 0; subRow < 100; subRow++)
            {
                try
                {
                    var exchangeItem = _collectablesShopItems.GetRow(row, subRow);
                    var rewardItem = _collectablesShopRewardItems.GetRow(exchangeItem.CollectablesShopRewardScrip.Row);
                    var refine = _collectablesShopRefines.GetRow(exchangeItem.CollectablesShopRefine.Row);
                    // filter out junk data
                    if (exchangeItem.Item.Value == null || exchangeItem.Item.Row <= 1000)
                    {
                        continue;
                    }

                    // This may be confusing, because some things are incorrectly labeled in Lumina
                    // CollectableShopRewardScrip is column 8, which is actually the ID in CollectableShopRewardItem
                    // The reward item is the item that you exchange the collectalbe for.


                    AddItem_Internal(rewardItem.Item.Value.RowId, rewardItem.Item.Value.Name.RawString, npcBase.RowId,
                                     resident.Singular.RawString, exchangeItem.CollectablesShopItemGroup?.Value?.Name,
                                     new List<Tuple<uint, string>>()
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
        if (questReward == null)
        {
            return;
        }

        if (questReward.ClassJobCategory.Row == 0)
        {
            return;
        }

        if (cost == null)
        {
            cost = new List<Tuple<uint, string>>();

            // Build the cost first
            for (uint i = 0; i < questReward.RequiredItem.Length; i++)
            {
                var requireItem = questReward.RequiredItem[i];
                if (requireItem.Row == 0)
                {
                    break;
                }

                cost.Add(new Tuple<uint, string>(questReward.RequiredAmount[i], requireItem.Value.Name));
            }
        }

        // Add the reward items
        for (uint i = 0; i < questReward.RewardItem.Length; i++)
        {
            var rewardItem = questReward.RewardItem[i];
            if (rewardItem.Row == 0)
            {
                break;
            }

            AddItem_Internal(rewardItem.Row, rewardItem.Value.Name, npcBase.RowId, resident.Singular.RawString, "",
                             cost, _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null,
                             ItemType.QuestReward);
        }
    }

    private void AddQuestRewardCost(QuestClassJobReward questReward, ENpcBase npcBase, List<Tuple<uint, string>> cost)
    {
        if (questReward == null || cost == null)
        {
            return;
        }

        if (questReward.ClassJobCategory.Row == 0)
        {
            return;
        }

        for (uint i = 0; i < questReward.RewardItem.Length; i++)
        {
            var rewardItem = questReward.RewardItem[i];
            if (rewardItem.Row == 0)
            {
                break;
            }

            AddItemCost(rewardItem.Row, npcBase.RowId, cost);
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

    private void AddItem_Internal(uint itemId, string itemName, uint npcId, string npcName, string shopName, List<Tuple<uint, string>> cost, NpcLocation npcLocation,
                                  ItemType type,
                                  string achievementDesc = "")
    {
        if (itemId == 0)
        {
            return;
        }

        if (!_itemDataMap.ContainsKey(itemId))
        {
            _itemDataMap.Add(itemId, new ItemInfo
            {
                Id = itemId,
                Name = itemName,
                NpcInfos = new List<NpcInfo> { new() { Id = npcId, Location = npcLocation, Costs = cost, Name = npcName, ShopName = shopName } },
                Type = type,
                AchievementDescription = achievementDesc,
            });
            return;
        }

        if (!_itemDataMap.TryGetValue(itemId, out var itemInfo))
        {
            _ = _itemDataMap.TryAdd(itemId, itemInfo = new ItemInfo
            {
                Id = itemId,
                Name = itemName,
                NpcInfos = new List<NpcInfo> { new() { Id = npcId, Location = npcLocation, Costs = cost, Name = npcName, ShopName = shopName } },
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
            itemInfo.NpcInfos.Add(new NpcInfo { Id = npcId, Location = npcLocation, Name = npcName, Costs = cost, ShopName = shopName });
        }
    }
}