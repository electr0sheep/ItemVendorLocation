using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Logging;
using ItemVendorLocation.Models;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace ItemVendorLocation
{
    // https://github.com/xivapi/SaintCoinach/blob/36e9d613f4bcc45b173959eed3f7b5549fd6f540/SaintCoinach/Xiv/Collections/ENpcCollection.cs
    internal class LookupItems
    {
        private readonly ExcelSheet<CustomTalk> customTalks;
        private readonly ExcelSheet<ENpcBase> eNpcBases;
        private readonly ExcelSheet<ENpcResident> eNpcResidents;
        private readonly ExcelSheet<FateShopCustom> fateShops;
        private readonly ExcelSheet<GilShopItem> gilShopItems;
        private readonly ExcelSheet<GilShop> gilShops;
        private readonly ExcelSheet<SpecialShopCustom> specialShops;
        private readonly ExcelSheet<TerritoryType> territoryType;

        private readonly ExcelSheet<GCShop> gcShops;
        private readonly ExcelSheet<GCScripShopItem> gcScripShopItems;
        private readonly ExcelSheet<GCScripShopCategory> gcScripShopCategories;

        private readonly Item gil;
        private readonly List<Item> gcSeal;

        private readonly Dictionary<uint, ItemInfo> itemDataMap = new();
        private readonly Dictionary<uint, NpcLocation> npcLocations = new();

        private bool isDataReady;

        public LookupItems()
        {
            eNpcBases = Service.DataManager.GetExcelSheet<ENpcBase>();
            eNpcResidents = Service.DataManager.GetExcelSheet<ENpcResident>();
            gilShopItems = Service.DataManager.GetExcelSheet<GilShopItem>();
            gilShops = Service.DataManager.GetExcelSheet<GilShop>();
            specialShops = Service.DataManager.GetExcelSheet<SpecialShopCustom>();
            customTalks = Service.DataManager.GetExcelSheet<CustomTalk>();
            fateShops = Service.DataManager.GetExcelSheet<FateShopCustom>();
            territoryType = Service.DataManager.GetExcelSheet<TerritoryType>();

            gcScripShopItems = Service.DataManager.GetExcelSheet<GCScripShopItem>();
            gcShops = Service.DataManager.GetExcelSheet<GCShop>();
            gcScripShopCategories = Service.DataManager.GetExcelSheet<GCScripShopCategory>();


            var items = Service.DataManager.GetExcelSheet<Item>();
            gil = items.GetRow(1);

            gcSeal = items.Where(i => i.RowId is >= 20 and <= 22).Select(i => i).ToList();

            Task.Run(async () =>
            {
                while (!Service.DataManager.IsDataReady)
                {
                    await Task.Delay(500);
                }

                BuildNpcLocation();
                BuildVendorInfo();
                isDataReady = true;
                PluginLog.Debug("Data is ready");
            });
        }

        // https://discord.com/channels/581875019861328007/653504487352303619/860865002721247261
        // https://github.com/SapphireServer/Sapphire/blob/a5c15f321f7e795ed7362ae15edaada99ca7d9be/src/world/Manager/EventMgr.cpp#L14
        private static bool MatchEventHandlerType(uint data, EventHandlerType type)
        {
            return ((data >> 16) & (uint)type) == (uint)type;
        }

        private void BuildVendorInfo()
        {
            var firstSpecialShopId = specialShops.First().RowId;
            var lastSpecialShopId = specialShops.Last().RowId;

            foreach (var npcBase in eNpcBases)
            {
                if (npcBase == null)
                {
                    continue;
                }

                var resident = eNpcResidents.GetRow(npcBase.RowId);

                var fateShop = fateShops.GetRow(npcBase.RowId);
                if (fateShop != null)
                {
                    foreach (var specialShop in fateShop.SpecialShop)
                    {
                        if (specialShop.Value == null)
                        {
                            continue;
                        }

                        var specialShopCustom = specialShops.GetRow(specialShop.Row);
                        AddSpecialItem(specialShopCustom, npcBase, resident);
                    }

                    continue;
                }

                foreach (var npcData in npcBase.ENpcData)
                {
                    if (npcData == 0)
                    {
                        continue;
                    }

                    if (MatchEventHandlerType(npcData, EventHandlerType.SpecialShop))
                    {
                        var specialShop = specialShops.GetRow(npcData);
                        AddSpecialItem(specialShop, npcBase, resident);
                        continue;
                    }

                    if (MatchEventHandlerType(npcData, EventHandlerType.GilShop))
                    {
                        var gilShop = gilShops.GetRow(npcData);
                        AddGilShopItem(gilShop, npcBase, resident);
                    }

                    if (MatchEventHandlerType(npcData, EventHandlerType.CustomTalk))
                    {
                        var customTalk = customTalks.GetRow(npcData);
                        if (customTalk == null)
                        {
                            continue;
                        }

                        foreach (var arg in customTalk.ScriptArg)
                        {
                            if (MatchEventHandlerType(arg, EventHandlerType.GilShop))
                            {
                                var gilShop = gilShops.GetRow(arg);
                                AddGilShopItem(gilShop, npcBase, resident);
                                continue;
                            }

                            // some script args contains have special shop stuff, we dont wanna miss that
                            if (arg < firstSpecialShopId || arg > lastSpecialShopId)
                            {
                                continue;
                            }

                            var specialShop = specialShops.GetRow(arg);
                            AddSpecialItem(specialShop, npcBase, resident);
                        }

                        continue;
                    }

                    if (MatchEventHandlerType(npcData, EventHandlerType.GcShop))
                    {
                        AddGcShopItem(npcData, npcBase, resident);
                    }
                }
            }
        }

        private void AddSpecialItem(SpecialShopCustom specialShop, ENpcBase npcBase, ENpcResident resident)
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

                    if (result.Item.Value?.Name == string.Empty)
                    {
                        continue;
                    }

                    if (!npcLocations.ContainsKey(npcBase.RowId))
                    {
                        continue;
                    }

                    var costs = entry.Cost.TakeWhile(cost => cost.Item.Value.Name != string.Empty).Select(cost => new Tuple<uint, string>(cost.Count, cost.Item.Value.Name)).ToList();

                    AddItem(result.Item.Value.RowId, result.Item.Value.Name, npcBase.RowId, resident.Singular, costs, npcLocations[npcBase.RowId], ItemType.SpecialShop);
                }
            }
        }

        private void AddGilShopItem(GilShop shop, ENpcBase npcBase, ENpcResident resident)
        {
            if (shop == null)
            {
                return;
            }

            for (var i = 0u;; i++)
            {
                try
                {
                    var item = gilShopItems.GetRow(shop.RowId, i);

                    if (item?.Item.Value == null)
                    {
                        break;
                    }

                    if (!npcLocations.ContainsKey(npcBase.RowId))
                    {
                        continue;
                    }

                    AddItem(item.Item.Value.RowId, item.Item.Value.Name, npcBase.RowId, resident.Singular, new List<Tuple<uint, string>> { new(item.Item.Value.PriceMid, gil.Name) },
                        npcLocations[npcBase.RowId], ItemType.GilShop);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void AddGcShopItem(uint data, ENpcBase npcBase, ENpcResident resident)
        {
            // Get GrandCompany id first
            var gcId = gcShops.GetRow(data);
            if (gcId == null)
            {
                return;
            }

            // Get shop category based on GrandCompany id
            var categories = gcScripShopCategories.Where(i => i.GrandCompany.Row == gcId.GrandCompany.Row).ToList();
            if (categories.Count == 0)
            {
                return;
            }

            // Get seal name
            var seal = gcSeal.Find(i => i.Name.RawString.Contains(gcId.GrandCompany.Value.Name));
            if (seal == null)
            {
                return;
            }

            foreach (var category in categories)
            {
                for (var i = 0u;; i++)
                {
                    try
                    {
                        var item = gcScripShopItems.GetRow(category.RowId, i);
                        if (item == null)
                        {
                            break;
                        }

                        if (item.SortKey == 0)
                        {
                            break;
                        }

                        // Should nenver happen but just in case
                        if (!npcLocations.ContainsKey(npcBase.RowId))
                        {
                            continue;
                        }

                        AddItem(item.Item.Value.RowId, item.Item.Value.Name, npcBase.RowId, resident.Singular, new List<Tuple<uint, string>> { new(item.CostGCSeals, seal.Name) },
                            npcLocations[npcBase.RowId], ItemType.GcShop);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }

        private void AddItem(uint itemId, string itemName, uint npcId, string npcName, List<Tuple<uint, string>> costs, NpcLocation npcLocation, ItemType type)
        {
            if (!itemDataMap.ContainsKey(itemId))
            {
                itemDataMap.Add(itemId, new ItemInfo
                {
                    Id = npcId,
                    Name = itemName,
                    Costs = costs,
                    NpcInfos = new List<NpcInfo> { new() { Id = npcId, Location = npcLocation, Name = npcName } },
                    Type = type,
                });
                return;
            }

            if (!itemDataMap.TryGetValue(itemId, out var itemInfo))
            {
                itemDataMap.TryAdd(itemId, itemInfo = new ItemInfo
                {
                    Id = npcId,
                    Name = itemName,
                    Costs = costs,
                    NpcInfos = new List<NpcInfo> { new() { Id = npcId, Location = npcLocation, Name = npcName } },
                    Type = type,
                });
            }

            var npcs = itemInfo.NpcInfos;
            if (npcs.Find(j => j.Id == npcId) == null)
            {
                npcs.Add(new NpcInfo { Id = npcId, Location = npcLocation, Name = npcName });
            }

            itemInfo.NpcInfos = npcs;
        }

        // https://github.com/ufx/GarlandTools/blob/3b3475bca6f95c800d2454f2c09a3f1eea0a8e4e/Garland.Data/Modules/Territories.cs
        private void BuildNpcLocation()
        {
            foreach (var sTerritoryType in territoryType)
            {
                var bg = sTerritoryType.Bg.ToString();
                if (string.IsNullOrEmpty(bg))
                {
                    continue;
                }

                var lgbFileName = "bg/" + bg[..(bg.IndexOf("/level/", StringComparison.Ordinal) + 1)] + "level/planevent.lgb";
                var sLgbFile = Service.DataManager.GetFile<LgbFile>(lgbFileName);
                if (sLgbFile == null)
                {
                    continue;
                }

                foreach (var sLgbGroup in sLgbFile.Layers)
                {
                    foreach (var instanceObject in sLgbGroup.InstanceObjects)
                    {
                        if (instanceObject.AssetType != LayerEntryType.EventNPC)
                        {
                            continue;
                        }

                        var eventNpc = (LayerCommon.ENPCInstanceObject)instanceObject.Object;
                        var npcRowId = eventNpc.ParentData.ParentData.BaseId;
                        if (npcRowId == 0)
                        {
                            continue;
                        }

                        if (npcLocations.ContainsKey(npcRowId))
                        {
                            continue;
                        }

                        npcLocations.Add(npcRowId, new NpcLocation(instanceObject.Transform.Translation.X, instanceObject.Transform.Translation.Z, sTerritoryType));
                    }
                }
            }

            var levels = Service.DataManager.GetExcelSheet<Level>();
            foreach (var level in levels)
            {
                // NPC
                if (level.Type != 8)
                {
                    continue;
                }

                // NPC Id
                if (npcLocations.ContainsKey(level.Object))
                {
                    continue;
                }

                if (level.Territory.Value == null)
                {
                    continue;
                }

                npcLocations.Add(level.Object, new NpcLocation(level.X, level.Z, level.Territory.Value));
            }

            // https://github.com/ufx/GarlandTools/blob/7b38def8cf0ab553a2c3679aec86480c0e4e9481/Garland.Data/Modules/NPCs.cs#L59-L66
            var corrected = territoryType.GetRow(698);
            npcLocations[1004418].TerritoryExcel = corrected;
            npcLocations[1006747].TerritoryExcel = corrected;
            npcLocations[1002299].TerritoryExcel = corrected;
            npcLocations[1002281].TerritoryExcel = corrected;
            npcLocations[1001766].TerritoryExcel = corrected;
            npcLocations[1001945].TerritoryExcel = corrected;
            npcLocations[1001821].TerritoryExcel = corrected;

            // some are missing from my test, so im gotta hardcode them
            npcLocations.TryAdd(1006004, new NpcLocation(5.355835f, 155.22998f, territoryType.GetRow(128)));
            npcLocations.TryAdd(1017613, new NpcLocation(2.822865f, 153.521f, territoryType.GetRow(128)));

            npcLocations.TryAdd(1008145, new NpcLocation(-31.265808f, -245.38031f, territoryType.GetRow(133)));
            npcLocations.TryAdd(1006005, new NpcLocation(-61.234497f, -141.31384f, territoryType.GetRow(133)));
            npcLocations.TryAdd(1017614, new NpcLocation(-58.79309f, -142.1073f, territoryType.GetRow(133)));
        }

        public ItemInfo? GetItemInfo(uint itemId)
        {
            if (!isDataReady)
            {
                return null;
            }

            return itemDataMap.TryGetValue(itemId, out var itemInfo) ? itemInfo : null;
        }

        // https://github.com/SapphireServer/Sapphire/blob/a5c15f321f7e795ed7362ae15edaada99ca7d9be/src/world/Event/EventHandler.h#L48-L83
        internal enum EventHandlerType : uint
        {
            GilShop = 0x0004,
            CustomTalk = 0x000B,
            GcShop = 0x0016,
            SpecialShop = 0x001B,
        }
    }
}