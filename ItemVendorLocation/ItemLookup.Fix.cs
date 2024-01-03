using ItemVendorLocation.Models;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud;

namespace ItemVendorLocation;
#if DEBUG
public partial class ItemLookup
#else
    internal partial class ItemLookup
#endif
{
    private void FixJapaneseShopName()
    {
        // This fix is for non-japanese client
        // SE is just being lazy on this, hence we have this bug lol
        if (Service.ClientState.ClientLanguage == ClientLanguage.Japanese)
        {
            return;
        }

        // Look for items that can be purchased from this npc
        foreach (var item in _itemDataMap)
        {
            foreach (var npcInfo in item.Value.NpcInfos.Where(npcInfo => npcInfo.ShopName == "アイテムの購入"))
            {
                Service.PluginLog.Debug($"{_items.GetRow(item.Key).Name} has ShopName \"アイテムの購入\", correcting to correct one.");
                // This correction is for Aenc Ose, who sells "Sheep Equipment Materials", for example.
                // A shop is the sub-menu presented at some vendors. Aenc Ose has no such sub-menu, so we simply remove the shop.
                npcInfo.ShopName = null;
            }
        }
    }

    private void ApplyNpcLocationCorrections()
    {
#pragma warning disable format
        // Fix Kugane npcs location
        var kugane = _territoryType.GetRow(641);
        _npcLocations[1019100] = new NpcLocation(-85.03851f, 117.05188f, kugane);
        _npcLocations[1022846] = new NpcLocation(-83.93994f, 115.31238f, kugane);
        _npcLocations[1019106] = new NpcLocation(-99.22949f, 105.6687f, kugane);
        _npcLocations[1019107] = new NpcLocation(-100.26703f, 107.43872f, kugane);
        _npcLocations[1019104] = new NpcLocation(-67.582275f, 59.739014f, kugane);
        _npcLocations[1019102] = new NpcLocation(-59.617065f, 33.524048f, kugane);
        _npcLocations[1019103] = new NpcLocation(-52.35376f, 76.58496f, kugane);
        _npcLocations[1019101] = new NpcLocation(-36.484375f, 49.240845f, kugane);

        // random NPC fixes
        _ = _npcLocations[1004418] = new NpcLocation(-114.0307f, 118.30322f, _territoryType.GetRow(131), 73);

        // some are missing from my test, so we gotta hardcode them
        _ = _npcLocations.TryAdd(1006004, new NpcLocation(5.355835f, 155.22998f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1017613, new NpcLocation(2.822865f, 153.521f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1003077, new NpcLocation(-259.32715f, 37.491333f, _territoryType.GetRow(129)));

        _ = _npcLocations.TryAdd(1008145, new NpcLocation(-31.265808f, -245.38031f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1006005, new NpcLocation(-61.234497f, -141.31384f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1017614, new NpcLocation(-58.79309f, -142.1073f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1003633, new NpcLocation(145.83044f, -106.767456f, _territoryType.GetRow(133)));

        // more locations missing
        _ = _npcLocations.TryAdd(1000215, new NpcLocation(155.35205f, -70.26782f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000996, new NpcLocation(-28.152893f, 196.70398f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1000999, new NpcLocation(-29.465149f, 197.92468f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1000217, new NpcLocation(170.30591f, -73.16705f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000597, new NpcLocation(-163.07324f, -78.62976f, _territoryType.GetRow(153)));
        _ = _npcLocations.TryAdd(1000185, new NpcLocation(-8.590881f, -2.2125854f, _territoryType.GetRow(132)));
        _ = _npcLocations.TryAdd(1000392, new NpcLocation(-17.746277f, 43.35083f, _territoryType.GetRow(132)));
        _ = _npcLocations.TryAdd(1000391, new NpcLocation(66.819214f, -143.45007f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000232, new NpcLocation(164.72107f, -133.68433f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000301, new NpcLocation(-87.174866f, -173.51044f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000267, new NpcLocation(103.89868f, -213.03125f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1003252, new NpcLocation(-139.57434f, 31.967651f, _territoryType.GetRow(129)));
        _ = _npcLocations.TryAdd(1001016, new NpcLocation(-42.679565f, 119.920654f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1005422, new NpcLocation(-397.6349f, 80.979614f, _territoryType.GetRow(129)));
        _ = _npcLocations.TryAdd(1000244, new NpcLocation(423.17834f, -119.95117f, _territoryType.GetRow(154)));
        _ = _npcLocations.TryAdd(1000234, new NpcLocation(423.69714f, -122.08746f, _territoryType.GetRow(154)));
        _ = _npcLocations.TryAdd(1000230, new NpcLocation(421.46936f, -125.993774f, _territoryType.GetRow(154)));

        // merchant & mender
        // East Shroud
        _ = _npcLocations.TryAdd(1000222, new NpcLocation(-213.94684f, 300.4348f, _territoryType.GetRow(152)));
        _ = _npcLocations.TryAdd(1000535, new NpcLocation(-579.4003f, 104.32593f, _territoryType.GetRow(152)));
        _ = _npcLocations.TryAdd(1002371, new NpcLocation(-480.91858f, 201.9226f, _territoryType.GetRow(152)));
        // Central Shroud
        _ = _npcLocations.TryAdd(1000396, new NpcLocation(82.597046f, -103.349365f, _territoryType.GetRow(148)));
        _ = _npcLocations.TryAdd(1000220, new NpcLocation(16.189758f, -15.640564f, _territoryType.GetRow(148)));
        _ = _npcLocations.TryAdd(1000717, new NpcLocation(175.61597f, 319.32544f, _territoryType.GetRow(148)));
        // North Shroud
        _ = _npcLocations.TryAdd(1000718, new NpcLocation(332.23462f, 332.47876f, _territoryType.GetRow(154)));
        _ = _npcLocations.TryAdd(1002376, new NpcLocation(10.635498f, 220.20288f, _territoryType.GetRow(154)));

        // arms supplier
        _ = _npcLocations.TryAdd(1002374, new NpcLocation(204.39453f, -65.75122f, _territoryType.GetRow(153)));

        // encampment clothier & tailor
        _ = _npcLocations.TryAdd(1000579, new NpcLocation(16.03717f, 220.50806f, _territoryType.GetRow(152)));

        // encampment clothier
        _ = _npcLocations.TryAdd(1002377, new NpcLocation(11.062683f, 221.57617f, _territoryType.GetRow(154)));

        // traveling armorer
        _ = _npcLocations.TryAdd(1002375, new NpcLocation(203.75366f, -64.560974f, _territoryType.GetRow(153)));

        // OIC Quartermaster hax, only Maelstrom missing
        _ = _npcLocations.TryAdd(1002389, new NpcLocation(95.8114f, 67.61267f, _territoryType.GetRow(128)));
#pragma warning restore format
    }

    private bool FixNpcVendorInfo(ENpcBase npcBase, ENpcResident resident)
    {
        switch (npcBase.RowId)
        {
            case 1043463: // horrendous hoarder
                // very ugly code and i dont like it, because se inserted new data between rows in patch 6.5
                // see here (https://github.com/xivapi/ffxiv-datamining/commit/fd1e8189682d52ee239b9037815a54d54b17a7bc#diff-983b68d9961598b3f0a8cecfc05d0f76f93afd0fd31b6a0cfea188ec12a729a1)
                // who knows if they will do it again when they add new stuffs to sanctuary -- nuko

                var rawExcel = Service.DataManager.GameData.Excel.GetSheetRaw("custom/007/CtsMjiSpecialShop_00789");
                Dictionary<string, string> mjiSpecialShopNames = new();

                foreach (var parser in rawExcel.GetRowParsers())
                {
                    var key = parser.ReadColumn<string>(0);
                    var name = parser.ReadColumn<string>(1);
                    mjiSpecialShopNames[key] = name;
                }

                AddSpecialItem(_specialShops.GetRow(1770601), npcBase, resident, ItemType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")}\n{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_000")}");
                AddSpecialItem(_specialShops.GetRow(1770659), npcBase, resident, ItemType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")} \n {GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_005")}");
                AddSpecialItem(_specialShops.GetRow(1770660), npcBase, resident, ItemType.SpecialShop,
                               $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000")}\n{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q2_000_010")}");
                AddSpecialItem(_specialShops.GetRow(1770602), npcBase, resident, ItemType.SpecialShop, $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_005")}");
                AddSpecialItem(_specialShops.GetRow(1770603), npcBase, resident, ItemType.SpecialShop, $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_010")}");
                AddSpecialItem(_specialShops.GetRow(1770723), npcBase, resident, ItemType.SpecialShop, $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_025")}");
                AddSpecialItem(_specialShops.GetRow(1770734), npcBase, resident, ItemType.SpecialShop, $"{GetNameFromKey("TEXT_CTSMJISPECIALSHOP_00789_Q1_000_030")}");
                return true;

                string GetNameFromKey(string key)
                {
                    return mjiSpecialShopNames.TryGetValue(key, out var str) ? str : string.Empty;
                }
            case 1018655: // disreputable priest
                AddSpecialItem(_specialShops.GetRow(1769743), npcBase, resident);
                AddSpecialItem(_specialShops.GetRow(1769744), npcBase, resident);
                AddSpecialItem(_specialShops.GetRow(1770537), npcBase, resident);
                return true;
            case 1016289: // syndony
                AddSpecialItem(_specialShops.GetRow(1769635), npcBase, resident);
                return true;
            case 1025047: // gerolt but in eureka
                for (uint i = 1769820; i <= 1769834; i++)
                {
                    var specialShop = _specialShops.GetRow(i);
                    AddSpecialItem(specialShop, npcBase, resident);
                }

                return true;
            case 1025763: // doman junkmonger
                AddGilShopItem(_gilShops.GetRow(262919), npcBase, resident);
                return true;
            case 1027123: // eureka expedition artisan
                AddSpecialItem(_specialShops.GetRow(1769934), npcBase, resident);
                AddSpecialItem(_specialShops.GetRow(1769935), npcBase, resident);
                return true;
            case 1027124: // eureka expedition scholar
                AddSpecialItem(_specialShops.GetRow(1769937), npcBase, resident);
                return true;
            case 1033921: // faux
                AddSpecialItem(_specialShops.GetRow(1770282), npcBase, resident);
                return true;
            case 1027566: // Limbeth, Resplendent Tool Exchange
                // we only need the first three npc data (the last one is CustomTalk, we dont need it here)
                // the first one is from CollectablesShopItem and the last one is from SpecialShop
                AddCollectablesShop(_collectablesShops.GetRow(npcBase.ENpcData[0]), npcBase, resident);
                // the second one is obsolete materials, even though they are not craftable, still add them anyway
                AddCollectablesShop(_collectablesShops.GetRow(npcBase.ENpcData[1]), npcBase, resident);
                // after adding all the items, build the cost
                AddSpecialItem(_specialShops.GetRow(npcBase.ENpcData[2]), npcBase, resident);
                return true;
            case 1035014: // Spanner, Skysteel Tool Exchange (but it doesnt seem to do anything???)
                // NPCData:
                // 0 - Story
                // 1 - Default talk
                // 2 - CollectableShop
                // 3 ~ 5 - PreHandler
                AddCollectablesShop(_collectablesShops.GetRow(npcBase.ENpcData[2]), npcBase, resident);
                for (var i = 3; i <= 5; i++)
                {
                    var preHandler = _preHandlers.GetRow(npcBase.ENpcData[i]);
                    AddItemsInPrehandler(preHandler, npcBase, resident);
                }

                return true;
            case 1032900:
                // NPCData:
                // 0 - Story id
                // 1 - SwitchTalk
                // 2 ~ 3 SpecialShop 
                // 4 - CollectableShop
                // 5 - SpecialShop
                // 6 - GilShop
                // 7 - 8 PreHandler (Replica)

                AddCollectablesShop(_collectablesShops.GetRow(npcBase.ENpcData[4]), npcBase, resident);
                AddSpecialItem(_specialShops.GetRow(npcBase.ENpcData[2]), npcBase, resident);
                AddSpecialItem(_specialShops.GetRow(npcBase.ENpcData[3]), npcBase, resident);
                AddSpecialItem(_specialShops.GetRow(npcBase.ENpcData[5]), npcBase, resident);
                AddGilShopItem(_gilShops.GetRow(npcBase.ENpcData[6]), npcBase, resident);
                AddItemsInPrehandler(_preHandlers.GetRow(npcBase.ENpcData[7]), npcBase, resident);
                AddItemsInPrehandler(_preHandlers.GetRow(npcBase.ENpcData[8]), npcBase, resident);
                return true;

            // add quest rewards, like relic weapons, to item list
            // but this needs to upadte every time when a new patch drops
            // hopefully someone can find a better way to handle this -- nuko
            case 1035012: // Emeny
                // 14, 15, 19 -- SkySteel tool
                for (uint i = 0; i <= 10; i++)
                {
                    var questClassJobReward = _questClassJobRewards.GetRow(14, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    questClassJobReward = _questClassJobRewards.GetRow(15, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    questClassJobReward = _questClassJobRewards.GetRow(19, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                }

                return true;
            case 1016135: // Ardashir

                List<Tuple<uint, string>> GetCost(uint i) =>
                    i switch
                    {
                        3 => new()
                        {
                            new(1, _items.GetRow(13575).Name), new(1, _items.GetRow(13576).Name),
                        },
                        5 => new()
                        {
                            new(1, _items.GetRow(13577).Name), new(1, _items.GetRow(13578).Name), new(1, _items.GetRow(13579).Name),
                            new(1, _items.GetRow(13580).Name),
                        },
                        6 => new()
                        {
                            new(5, _items.GetRow(14899).Name),
                        },
                        7 => new()
                        {
                            // The amounts are uncertain, so will use the maximum amount
                            new(60, _items.GetRow(15840).Name), new(60, _items.GetRow(15841).Name),
                        },
                        8 => new()
                        {
                            new(50, _items.GetRow(16064).Name),
                        },
                        9 => new()
                        {
                            new(1, _items.GetRow(16932).Name),
                        },
                        10 => new()
                        {
                            new(1, _items.GetRow(16934).Name),
                        },
                        _ => null
                    };

                // 3 ~ 10 Anima Weapons
                for (uint i = 3; i <= 10; i++)
                {
                    for (uint j = 0; j <= 12; j++)
                    {
                        var questClassJobReward = _questClassJobRewards.GetRow(i, j);
                        AddQuestReward(questClassJobReward, npcBase, resident);
                        AddQuestRewardCost(questClassJobReward, npcBase, GetCost(i));
                    }
                }

                return true;
            case 1032903: // gerolt Resistance Weapons
                // Build the cost/required items manually, they dont exist in the sheet
                for (uint i = 0; i <= 16; i++)
                {
                    var questClassJobReward = _questClassJobRewards.GetRow(12, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, new()
                    {
                        new(4, _items.GetRow(30273).Name),
                    });
                }

                return true;

            case 1032905: // Zlatan
                // Build the cost/required items manually, they dont exist in the sheet
                // IL 485
                for (uint i = 0; i <= 16; i++)
                {
                    var questClassJobReward = _questClassJobRewards.GetRow(13, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, new()
                    {
                        new(4, _items.GetRow(30273).Name),
                    });
                }

                // build reward items first, then we manually add cost/required items
                // code is messy, this could be more optimized and readable, but leave it as it is for now -- nuko
                for (uint i = 0; i <= 16; i++)
                {
                    // IL 500
                    var questClassJobReward = _questClassJobRewards.GetRow(17, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(20, _items.GetRow(31573).Name),
                        new(20, _items.GetRow(31574).Name),
                        new(20, _items.GetRow(31575).Name),
                    });

                    // IL 500 #2
                    questClassJobReward = _questClassJobRewards.GetRow(18, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(6, _items.GetRow(31576).Name)
                    });

                    // IL 510
                    questClassJobReward = _questClassJobRewards.GetRow(20, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(15, _items.GetRow(32956).Name)
                    });

                    // IL 515
                    questClassJobReward = _questClassJobRewards.GetRow(21, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(15, _items.GetRow(32959).Name)
                    });

                    // IL 535
                    questClassJobReward = _questClassJobRewards.GetRow(22, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(15, _items.GetRow(33767).Name)
                    });
                }

                return true;
            default:
                if (!_shbFateShopNpc.TryGetValue(npcBase.RowId, out var value))
                {
                    return false;
                }

                AddSpecialItem(_specialShops.GetRow(value), npcBase, resident);
                return true;
        }
    }
}