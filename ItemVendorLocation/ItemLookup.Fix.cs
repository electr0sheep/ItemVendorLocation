using ItemVendorLocation.Models;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud;

namespace ItemVendorLocation;
#if DEBUG
public partial class ItemLookup
#else
public partial class ItemLookup
#endif
{
    private void ApplyNpcLocationCorrections()
    {
#pragma warning disable format
        // Fix Kugane npcs location
        var kugane = _territoryType.GetRow(641);
        _npcLocations[1019100] = new(-85.03851f, 117.05188f, kugane);
        _npcLocations[1022846] = new(-83.93994f, 115.31238f, kugane);
        _npcLocations[1019106] = new(-99.22949f, 105.6687f, kugane);
        _npcLocations[1019107] = new(-100.26703f, 107.43872f, kugane);
        _npcLocations[1019104] = new(-67.582275f, 59.739014f, kugane);
        _npcLocations[1019102] = new(-59.617065f, 33.524048f, kugane);
        _npcLocations[1019103] = new(-52.35376f, 76.58496f, kugane);
        _npcLocations[1019101] = new(-36.484375f, 49.240845f, kugane);

        // random NPC fixes
        _ = _npcLocations[1004418] = new(-114.0307f, 118.30322f, _territoryType.GetRow(131), 73);

        // some are missing from my test, so we gotta hardcode them
        _ = _npcLocations.TryAdd(1006004, new(5.355835f, 155.22998f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1017613, new(2.822865f, 153.521f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1003633, new(-259.32715f, 37.491333f, _territoryType.GetRow(129)));

        _ = _npcLocations.TryAdd(1008145, new(-31.265808f, -245.38031f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1006005, new(-61.234497f, -141.31384f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1017614, new(-58.79309f, -142.1073f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1003077, new(145.83044f, -106.767456f, _territoryType.GetRow(133)));

        // more locations missing
        _ = _npcLocations.TryAdd(1000215, new(155.35205f, -70.26782f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000996, new(-28.152893f, 196.70398f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1000999, new(-29.465149f, 197.92468f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1000217, new(170.30591f, -73.16705f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000597, new(-163.07324f, -78.62976f, _territoryType.GetRow(153)));
        _ = _npcLocations.TryAdd(1000185, new(-8.590881f, -2.2125854f, _territoryType.GetRow(132)));
        _ = _npcLocations.TryAdd(1000392, new(-17.746277f, 43.35083f, _territoryType.GetRow(132)));
        _ = _npcLocations.TryAdd(1000391, new(66.819214f, -143.45007f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000232, new(164.72107f, -133.68433f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000301, new(-87.174866f, -173.51044f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1000267, new(103.89868f, -213.03125f, _territoryType.GetRow(133)));
        _ = _npcLocations.TryAdd(1003252, new(-139.57434f, 31.967651f, _territoryType.GetRow(129)));
        _ = _npcLocations.TryAdd(1001016, new(-42.679565f, 119.920654f, _territoryType.GetRow(128)));
        _ = _npcLocations.TryAdd(1005422, new(-397.6349f, 80.979614f, _territoryType.GetRow(129)));
        _ = _npcLocations.TryAdd(1000244, new(423.17834f, -119.95117f, _territoryType.GetRow(154)));
        _ = _npcLocations.TryAdd(1000234, new(423.69714f, -122.08746f, _territoryType.GetRow(154)));
        _ = _npcLocations.TryAdd(1000230, new(421.46936f, -125.993774f, _territoryType.GetRow(154)));

        // merchant & mender
        // East Shroud
        _ = _npcLocations.TryAdd(1000222, new(-213.94684f, 300.4348f, _territoryType.GetRow(152)));
        _ = _npcLocations.TryAdd(1000535, new(-579.4003f, 104.32593f, _territoryType.GetRow(152)));
        _ = _npcLocations.TryAdd(1002371, new(-480.91858f, 201.9226f, _territoryType.GetRow(152)));
        // Central Shroud
        _ = _npcLocations.TryAdd(1000396, new(82.597046f, -103.349365f, _territoryType.GetRow(148)));
        _ = _npcLocations.TryAdd(1000220, new(16.189758f, -15.640564f, _territoryType.GetRow(148)));
        _ = _npcLocations.TryAdd(1000717, new(175.61597f, 319.32544f, _territoryType.GetRow(148)));
        // North Shroud
        _ = _npcLocations.TryAdd(1000718, new(332.23462f, 332.47876f, _territoryType.GetRow(154)));
        _ = _npcLocations.TryAdd(1002376, new(10.635498f, 220.20288f, _territoryType.GetRow(154)));

        // arms supplier
        _ = _npcLocations.TryAdd(1002374, new(204.39453f, -65.75122f, _territoryType.GetRow(153)));

        // encampment clothier & tailor
        _ = _npcLocations.TryAdd(1000579, new(16.03717f, 220.50806f, _territoryType.GetRow(152)));

        // encampment clothier
        _ = _npcLocations.TryAdd(1002377, new(11.062683f, 221.57617f, _territoryType.GetRow(154)));

        // traveling armorer
        _ = _npcLocations.TryAdd(1002375, new(203.75366f, -64.560974f, _territoryType.GetRow(153)));

        // OIC Quartermaster hax, only Maelstrom missing
        _ = _npcLocations.TryAdd(1002389, new(95.8114f, 67.61267f, _territoryType.GetRow(128)));
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

                //var rawExcel = Service.DataManager.GameData.Excel.GetRawSheet("custom/007/CtsMjiSpecialShop_00789");

                // can't figure out how to read data from custom/007/CtsMjiSpecialShop_00789, so I'm just gonna recreate it!
                Dictionary<string, string> mjiSpecialShopNames = new()
                {
                    { "0",  "TEXT_CTSMJISPECIALSHOP_00789_TALK_ACTOR" },
                    { "1",  "TEXT_CTSMJISPECIALSHOP_00789_SYSTEM_000_000" },
                    { "2",  "TEXT_CTSMJISPECIALSHOP_00789_SYSTEM_000_005" },
                    { "3",  "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_000" },
                    { "4",  "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_005" },
                    { "5",  "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_010" },
                    { "6",  "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_015" },
                    { "7",  "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_020" },
                    { "8",  "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_025" },
                    { "9",  "TEXT_CTSMJISPECIALSHOP_00789_Q1_000_030" },
                    { "10", "TEXT_CTSMJISPECIALSHOP_00789_Q2_000_000" },
                    { "11", "TEXT_CTSMJISPECIALSHOP_00789_Q2_000_005" },
                    { "12", "TEXT_CTSMJISPECIALSHOP_00789_Q2_000_010" },
                    { "13", "TEXT_CTSMJISPECIALSHOP_00789_Q2_000_015" },
                    { "14", "TEXT_CTSMJISPECIALSHOP_00789_OMISE_100_000" },
                    { "15", "TEXT_CTSMJISPECIALSHOP_00789_SYSTEM_100_000" },
                    { "16", "TEXT_CTSMJISPECIALSHOP_00789_OMISE_200_000" },
                };

                //foreach (var parser in rawExcel.GetRowParsers())
                //{
                //    var key = parser.ReadColumn<string>(0);
                //    var name = parser.ReadColumn<string>(1);
                //    mjiSpecialShopNames[key] = name;
                //}

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
            // add quest rewards, like relic weapons, to item list
            // but this needs to upadte every time when a new patch drops
            // hopefully someone can find a better way to handle this -- nuko
            case 1035012: // Emeny
                // 14, 15, 19 -- SkySteel tool
                for (ushort i = 0; i <= 10; i++)
                {
                    var questClassJobReward = _questClassJobRewards.GetSubrow(14, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    questClassJobReward = _questClassJobRewards.GetSubrow(15, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    questClassJobReward = _questClassJobRewards.GetSubrow(19, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                }

                return true;
            case 1016135: // Ardashir

                List<Tuple<uint, string>>? GetCost(uint i)
                {
                    return i switch
                    {
                        3 => new()
                        {
                            new(1, _items.GetRow(13575).Name.ExtractText()), new(1, _items.GetRow(13576).Name.ExtractText()),
                        },
                        5 => new()
                        {
                            new(1, _items.GetRow(13577).Name.ExtractText()), new(1, _items.GetRow(13578).Name.ExtractText()), new(1, _items.GetRow(13579).Name.ExtractText()),
                            new(1, _items.GetRow(13580).Name.ExtractText()),
                        },
                        6 => new()
                        {
                            new(5, _items.GetRow(14899).Name.ExtractText()),
                        },
                        7 => new()
                        {
                            // The amounts are uncertain, so will use the maximum amount
                            new(60, _items.GetRow(15840).Name.ExtractText()), new(60, _items.GetRow(15841).Name.ExtractText()),
                        },
                        8 => new()
                        {
                            new(50, _items.GetRow(16064).Name.ExtractText()),
                        },
                        9 => new()
                        {
                            new(1, _items.GetRow(16932).Name.ExtractText()),
                        },
                        10 => new()
                        {
                            new(1, _items.GetRow(16934).Name.ExtractText()),
                        },
                        _ => null
                    };
                }

                // 3 ~ 10 Anima Weapons
                for (uint i = 3; i <= 10; i++)
                {
                    for (ushort j = 0; j <= 12; j++)
                    {
                        var questClassJobReward = _questClassJobRewards.GetSubrow(i, j);
                        AddQuestReward(questClassJobReward, npcBase, resident);
                        AddQuestRewardCost(questClassJobReward, npcBase, GetCost(i));
                    }
                }

                return true;
            case 1032903: // gerolt Resistance Weapons
                // Build the cost/required items manually, they dont exist in the sheet
                for (ushort i = 0; i <= 16; i++)
                {
                    var questClassJobReward = _questClassJobRewards.GetSubrow(12, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, new()
                    {
                        new(4, _items.GetRow(30273).Name.ExtractText()),
                    });
                }

                return true;

            case 1032905: // Zlatan
                // Build the cost/required items manually, they dont exist in the sheet
                // IL 485
                for (ushort i = 0; i <= 16; i++)
                {
                    var questClassJobReward = _questClassJobRewards.GetSubrow(13, i);
                    AddQuestReward(questClassJobReward, npcBase, resident, new()
                    {
                        new(4, _items.GetRow(30273).Name.ExtractText()),
                    });
                }

                // build reward items first, then we manually add cost/required items
                // code is messy, this could be more optimized and readable, but leave it as it is for now -- nuko
                for (ushort i = 0; i <= 16; i++)
                {
                    // IL 500
                    var questClassJobReward = _questClassJobRewards.GetSubrow(17, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(20, _items.GetRow(31573).Name.ExtractText()),
                        new(20, _items.GetRow(31574).Name.ExtractText()),
                        new(20, _items.GetRow(31575).Name.ExtractText()),
                    });

                    // IL 500 #2
                    questClassJobReward = _questClassJobRewards.GetSubrow(18, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(6, _items.GetRow(31576).Name.ExtractText())
                    });

                    // IL 510
                    questClassJobReward = _questClassJobRewards.GetSubrow(20, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(15, _items.GetRow(32956).Name.ExtractText())
                    });

                    // IL 515
                    questClassJobReward = _questClassJobRewards.GetSubrow(21, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(15, _items.GetRow(32959).Name.ExtractText())
                    });

                    // IL 535
                    questClassJobReward = _questClassJobRewards.GetSubrow(22, i);
                    AddQuestReward(questClassJobReward, npcBase, resident);
                    AddQuestRewardCost(questClassJobReward, npcBase, new()
                    {
                        new(15, _items.GetRow(33767).Name.ExtractText())
                    });
                }

                return true;
            default:
                if (!Dictionaries.ShbFateShopNpc.TryGetValue(npcBase.RowId, out var value))
                {
                    return false;
                }

                AddSpecialItem(_specialShops.GetRow(value), npcBase, resident);
                return true;
        }
    }
}