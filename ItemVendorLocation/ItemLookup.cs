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

namespace ItemVendorLocation;

internal class ItemLookup
{
    private readonly ExcelSheet<CustomTalk> _customTalks;
    private readonly ExcelSheet<Achievement> _achievements;

    private readonly ExcelSheet<ENpcBase> _eNpcBases;
    private readonly ExcelSheet<ENpcResident> _eNpcResidents;

    private readonly ExcelSheet<FateShopCustom> _fateShops;
    private readonly ExcelSheet<GilShopItem> _gilShopItems;
    private readonly ExcelSheet<GilShop> _gilShops;
    private readonly ExcelSheet<SpecialShopCustom> _specialShops;
    private readonly ExcelSheet<GCShop> _gcShops;
    private readonly ExcelSheet<GCScripShopItem> _gcScripShopItems;
    private readonly ExcelSheet<GCScripShopCategory> _gcScripShopCategories;
    private readonly ExcelSheet<InclusionShop> _inclusionShops;
    private readonly ExcelSheet<InclusionShopSeriesCustom> _inclusionShopSeries;
    private readonly ExcelSheet<FccShop> _fccShops;
    private readonly ExcelSheet<PreHandler> _preHandlers;
    private readonly ExcelSheet<TopicSelect> _topicSelects;

    private readonly ExcelSheet<TerritoryType> _territoryType;

    private readonly Item _gil;
    private readonly ExcelSheet<Item> _items;
    private readonly List<Item> _gcSeal;
    private readonly Addon _fccName;

    private readonly Dictionary<uint, ItemInfo> _itemDataMap = new();
    private readonly Dictionary<uint, NpcLocation> _npcLocations = new();

    private bool _isDataReady;

    private readonly Dictionary<uint, uint> _shbFateShopNpc = new()
    {
        { 1027998, 1769957 },
        { 1027538, 1769958 },
        { 1027385, 1769959 },
        { 1027497, 1769960 },
        { 1027892, 1769961 },
        { 1027665, 1769962 },
        { 1027709, 1769963 },
        { 1027766, 1769964 },
    };

    public ItemLookup()
    {
        _eNpcBases = DalamudApi.DataManager.GetExcelSheet<ENpcBase>();
        _eNpcResidents = DalamudApi.DataManager.GetExcelSheet<ENpcResident>();
        _gilShopItems = DalamudApi.DataManager.GetExcelSheet<GilShopItem>();
        _gilShops = DalamudApi.DataManager.GetExcelSheet<GilShop>();
        _specialShops = DalamudApi.DataManager.GetExcelSheet<SpecialShopCustom>();
        _customTalks = DalamudApi.DataManager.GetExcelSheet<CustomTalk>();
        _fateShops = DalamudApi.DataManager.GetExcelSheet<FateShopCustom>();
        _territoryType = DalamudApi.DataManager.GetExcelSheet<TerritoryType>();

        _gcScripShopItems = DalamudApi.DataManager.GetExcelSheet<GCScripShopItem>();
        _gcShops = DalamudApi.DataManager.GetExcelSheet<GCShop>();
        _gcScripShopCategories = DalamudApi.DataManager.GetExcelSheet<GCScripShopCategory>();

        _inclusionShops = DalamudApi.DataManager.GetExcelSheet<InclusionShop>();
        _inclusionShopSeries = DalamudApi.DataManager.GetExcelSheet<InclusionShopSeriesCustom>();
        _fccShops = DalamudApi.DataManager.GetExcelSheet<FccShop>();
        _preHandlers = DalamudApi.DataManager.GetExcelSheet<PreHandler>();
        _topicSelects = DalamudApi.DataManager.GetExcelSheet<TopicSelect>();

        _achievements = DalamudApi.DataManager.GetExcelSheet<Achievement>();

        _fccName = DalamudApi.DataManager.GetExcelSheet<Addon>().GetRow(102233);

        _items = DalamudApi.DataManager.GetExcelSheet<Item>();
        _gil = _items.GetRow(1);

        _gcSeal = _items.Where(i => i.RowId is >= 20 and <= 22).Select(i => i).ToList();

        Task.Run(async () =>
        {
            while (!DalamudApi.DataManager.IsDataReady)
            {
                await Task.Delay(500);
            }

            BuildNpcLocation();
            BuildVendorInfo();
            AddAchievementItem();
            _isDataReady = true;
            PluginLog.Debug("Data is ready");
        });
    }


    // https://discord.com/channels/581875019861328007/653504487352303619/860865002721247261
    // https://github.com/SapphireServer/Sapphire/blob/a5c15f321f7e795ed7362ae15edaada99ca7d9be/src/world/Manager/EventMgr.cpp#L14
    private static bool MatchEventHandlerType(uint data, EventHandlerType type) => ((data >> 16) & (uint)type) == (uint)type;

    private void BuildVendorInfo()
    {
        var firstSpecialShopId = _specialShops.First().RowId;
        var lastSpecialShopId = _specialShops.Last().RowId;

        foreach (var npcBase in _eNpcBases)
        {
            if (npcBase == null)
                continue;
            var resident = _eNpcResidents.GetRow(npcBase.RowId);

            if (HackyFix_Npc(npcBase, resident))
                continue;

            var fateShop = _fateShops.GetRow(npcBase.RowId);
            if (fateShop != null)
            {
                foreach (var specialShop in fateShop.SpecialShop)
                {
                    if (specialShop.Value == null)
                        continue;

                    var specialShopCustom = _specialShops.GetRow(specialShop.Row);
                    AddSpecialItem(specialShopCustom, npcBase, resident);
                }

                continue;
            }

            foreach (var npcData in npcBase.ENpcData)
            {
                if (npcData == 0)
                    continue;

                AddInclusionShopItem(npcData, npcBase, resident);
                AddFccShop(npcData, npcBase, resident);
                AddItemsInPrehandler(npcData, npcBase, resident);
                AddItemsInTopicSelect(npcData, npcBase, resident);

                if (MatchEventHandlerType(npcData, EventHandlerType.GcShop))
                {
                    AddGcShopItem(npcData, npcBase, resident);
                    continue;
                }

                if (MatchEventHandlerType(npcData, EventHandlerType.SpecialShop))
                {
                    var specialShop = _specialShops.GetRow(npcData);
                    AddSpecialItem(specialShop, npcBase, resident);
                    continue;
                }

                if (MatchEventHandlerType(npcData, EventHandlerType.GilShop))
                {
                    var gilShop = _gilShops.GetRow(npcData);
                    AddGilShopItem(gilShop, npcBase, resident);
                }

                if (MatchEventHandlerType(npcData, EventHandlerType.CustomTalk))
                {
                    var customTalk = _customTalks.GetRow(npcData);
                    if (customTalk == null)
                        continue;

                    foreach (var arg in customTalk.ScriptArg)
                    {
                        if (MatchEventHandlerType(arg, EventHandlerType.GilShop))
                        {
                            var gilShop = _gilShops.GetRow(arg);
                            AddGilShopItem(gilShop, npcBase, resident);
                            continue;
                        }

                        if (arg < firstSpecialShopId || arg > lastSpecialShopId)
                            continue;
                        var specialShop = _specialShops.GetRow(arg);
                        AddSpecialItem(specialShop, npcBase, resident);
                    }
                }
            }
        }
    }

    private void AddSpecialItem(SpecialShopCustom specialShop, ENpcBase npcBase, ENpcResident resident, ItemType type = ItemType.SpecialShop)
    {
        if (specialShop == null) return;

        foreach (var entry in specialShop.Entries)
        {
            if (entry.Result == null || entry.Cost == null) continue;

            foreach (var result in entry.Result)
            {
                if (result.Item.Value == null)
                    continue;

                if (result.Item.Value.Name == string.Empty)
                    continue;

                var costs = (from e in entry.Cost where e.Item != null && e.Item.Value.Name != string.Empty select new Tuple<uint, string>(e.Count, e.Item.Value.Name)).ToList();

                var achievementDescription = "";
                if (type == ItemType.Achievement)
                    achievementDescription = _achievements.Where(i => i.Item.Value == result.Item.Value).Select(i => i.Description).First();

                AddItem_Internal(result.Item.Value.RowId, result.Item.Value.Name, npcBase.RowId, resident.Singular, costs,
                    _npcLocations.ContainsKey(npcBase.RowId) ? _npcLocations[npcBase.RowId] : null, type, achievementDescription);
            }
        }
    }

    private void AddGilShopItem(GilShop shop, ENpcBase npcBase, ENpcResident resident)
    {
        if (shop == null)
            return;

        for (var i = 0u; ; i++)
        {
            try
            {
                var item = _gilShopItems.GetRow(shop.RowId, i);

                if (item?.Item.Value == null) break;

                AddItem_Internal(item.Item.Value.RowId, item.Item.Value.Name, npcBase.RowId, resident.Singular, new List<Tuple<uint, string>> { new(item.Item.Value.PriceMid, _gil.Name) },
                    _npcLocations.ContainsKey(npcBase.RowId) ? _npcLocations[npcBase.RowId] : null, ItemType.GilShop);
            }
            catch (Exception)
            {
                break;
            }
        }
    }

    private void AddGcShopItem(uint data, ENpcBase npcBase, ENpcResident resident)
    {
        var gcId = _gcShops.GetRow(data);
        if (gcId == null)
            return;

        var categories = _gcScripShopCategories.Where(i => i.GrandCompany.Row == gcId.GrandCompany.Row).ToList();
        if (categories.Count == 0)
            return;

        var seal = _gcSeal.Find(i => i.Name.RawString.StartsWith(gcId.GrandCompany.Value.Name));
        if (seal == null)
            return;

        foreach (var category in categories)
        {
            for (var i = 0u; ; i++)
            {
                try
                {
                    var item = _gcScripShopItems.GetRow(category.RowId, i);
                    if (item == null)
                        break;

                    if (item.SortKey == 0)
                        break;

                    AddItem_Internal(item.Item.Value.RowId, item.Item.Value.Name, npcBase.RowId, resident.Singular, new List<Tuple<uint, string>> { new(item.CostGCSeals, seal.Name) },
                        _npcLocations.ContainsKey(npcBase.RowId) ? _npcLocations[npcBase.RowId] : null, ItemType.GcShop);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }

    private void AddInclusionShopItem(uint data, ENpcBase npcBase, ENpcResident resident)
    {
        var inclusionShop = _inclusionShops.GetRow(data);
        if (inclusionShop == null) return;

        foreach (var category in inclusionShop.Category)
        {
            if (category.Value.RowId == 0)
                continue;

            for (uint i = 0; ; i++)
            {
                try
                {
                    var series = _inclusionShopSeries.GetRow(category.Value.InclusionShopSeries.Row, i);
                    if (series == null)
                        break;

                    var specialShop = series.SpecialShopCustoms.Value;
                    AddSpecialItem(specialShop, npcBase, resident);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }

    private void AddFccShop(uint data, ENpcBase npcBase, ENpcResident resident)
    {
        var shop = _fccShops.GetRow(data);
        if (shop == null)
            return;

        for (var i = 0; i < shop.Item.Length; i++)
        {
            var item = _items.GetRow((uint)i);
            if (item == null || item.Name == string.Empty)
                continue;

            var cost = shop.Cost[i];

            AddItem_Internal(item.RowId, item.Name, npcBase.RowId, resident.Singular, new List<Tuple<uint, string>> { new(cost, _fccName.Text) },
                _npcLocations.ContainsKey(npcBase.RowId) ? _npcLocations[npcBase.RowId] : null, ItemType.GcShop);
        }
    }

    private void AddItemsInPrehandler(uint data, ENpcBase npcBase, ENpcResident resident)
    {
        var preHandler = _preHandlers.GetRow(data);
        if (preHandler == null)
            return;

        var target = preHandler.Target;
        if (target == 0)
            return;

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

        AddInclusionShopItem(target, npcBase, resident);
    }

    private void AddItemsInTopicSelect(uint npcData, ENpcBase npcBase, ENpcResident resident)
    {
        var topicSelect = _topicSelects.GetRow(npcData);
        if (topicSelect == null)
            return;

        foreach (var data in topicSelect.Shop)
        {
            if (data == 0)
                continue;

            if (MatchEventHandlerType(data, EventHandlerType.SpecialShop))
            {
                var specialShop = _specialShops.GetRow(data);
                AddSpecialItem(specialShop, npcBase, resident);

                continue;
            }

            if (MatchEventHandlerType(data, EventHandlerType.GilShop))
            {
                var gilShop = _gilShops.GetRow(data);
                AddGilShopItem(gilShop, npcBase, resident);
                continue;
            }

            AddItemsInPrehandler(data, npcBase, resident);
        }
    }

    private bool HackyFix_Npc(ENpcBase npcBase, ENpcResident resident)
    {
        switch (npcBase.RowId)
        {
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
                var gilShop = _gilShops.GetRow(262919);
                AddGilShopItem(gilShop, npcBase, resident);
                return true;

            case 1033921: // faux
                var sShop = _specialShops.GetRow(1770282);
                AddSpecialItem(sShop, npcBase, resident);
                return true;

            case 1034007: // bozja
            case 1036895:
                var specShop = _specialShops.GetRow(1770087);
                AddSpecialItem(specShop, npcBase, resident);
                return true;

            default:
                if (_shbFateShopNpc.ContainsKey(npcBase.RowId))
                {
                    var specialShop = _specialShops.GetRow(_shbFateShopNpc[npcBase.RowId]);
                    AddSpecialItem(specialShop, npcBase, resident);
                    return true;
                }

                return false;
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

    private void AddItem_Internal(uint itemId, string itemName, uint npcId, string npcName, List<Tuple<uint, string>> cost, NpcLocation npcLocation, ItemType type,
        string achievementDesc = "")
    {
        if (itemId == 0)
            return;

        if (!_itemDataMap.ContainsKey(itemId))
        {
            _itemDataMap.Add(itemId, new ItemInfo
            {
                Id = itemId,
                Name = itemName,
                NpcInfos = new List<NpcInfo> { new() { Id = npcId, Location = npcLocation, Costs = cost, Name = npcName } },
                Type = type,
                AchievementDescription = achievementDesc,
            });
            return;
        }

        if (!_itemDataMap.TryGetValue(itemId, out var itemInfo))
        {
            _itemDataMap.TryAdd(itemId, itemInfo = new ItemInfo
            {
                Id = itemId,
                Name = itemName,
                NpcInfos = new List<NpcInfo> { new() { Id = npcId, Location = npcLocation, Costs = cost, Name = npcName } },
                Type = type,
                AchievementDescription = achievementDesc,
            });
        }

        if (type == ItemType.Achievement && itemInfo.Type != ItemType.Achievement)
        {
            itemInfo.Type = ItemType.Achievement;
            itemInfo.AchievementDescription = achievementDesc;
        }

        var npcs = itemInfo.NpcInfos;
        if (npcs.Find(j => j.Id == npcId) == null) npcs.Add(new NpcInfo { Id = npcId, Location = npcLocation, Name = npcName, Costs = cost });

        itemInfo.NpcInfos = npcs;
    }

    // https://github.com/ufx/GarlandTools/blob/3b3475bca6f95c800d2454f2c09a3f1eea0a8e4e/Garland.Data/Modules/Territories.cs
    private void BuildNpcLocation()
    {
        foreach (var sTerritoryType in _territoryType)
        {
            var bg = sTerritoryType.Bg.ToString();
            if (string.IsNullOrEmpty(bg))
                continue;

            var lgbFileName = "bg/" + bg[..(bg.IndexOf("/level/", StringComparison.Ordinal) + 1)] + "level/planevent.lgb";
            var sLgbFile = DalamudApi.DataManager.GetFile<LgbFile>(lgbFileName);
            if (sLgbFile == null) continue;

            foreach (var sLgbGroup in sLgbFile.Layers)
            {
                foreach (var instanceObject in sLgbGroup.InstanceObjects)
                {
                    if (instanceObject.AssetType != LayerEntryType.EventNPC)
                        continue;

                    var eventNpc = (LayerCommon.ENPCInstanceObject)instanceObject.Object;
                    var npcRowId = eventNpc.ParentData.ParentData.BaseId;
                    if (npcRowId == 0)
                        continue;

                    if (_npcLocations.ContainsKey(npcRowId))
                        continue;

                    _npcLocations.Add(npcRowId, new NpcLocation(instanceObject.Transform.Translation.X, instanceObject.Transform.Translation.Z, sTerritoryType));
                }
            }
        }

        var levels = DalamudApi.DataManager.GetExcelSheet<Level>();
        foreach (var level in levels)
        {
            // NPC
            if (level.Type != 8)
                continue;

            // NPC Id
            if (_npcLocations.ContainsKey(level.Object))
                continue;

            if (level.Territory.Value == null)
                continue;

            _npcLocations.Add(level.Object, new NpcLocation(level.X, level.Z, level.Territory.Value));
        }

        // https://github.com/ufx/GarlandTools/blob/7b38def8cf0ab553a2c3679aec86480c0e4e9481/Garland.Data/Modules/NPCs.cs#L59-L66
        var corrected = _territoryType.GetRow(698);
        _npcLocations[1004418].TerritoryExcel = corrected;
        _npcLocations[1006747].TerritoryExcel = corrected;
        _npcLocations[1002299].TerritoryExcel = corrected;
        _npcLocations[1002281].TerritoryExcel = corrected;
        _npcLocations[1001766].TerritoryExcel = corrected;
        _npcLocations[1001945].TerritoryExcel = corrected;
        _npcLocations[1001821].TerritoryExcel = corrected;

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

        // some are missing from my test, so we gotta hardcode them
        _npcLocations.TryAdd(1006004, new NpcLocation(5.355835f, 155.22998f, _territoryType.GetRow(128)));
        _npcLocations.TryAdd(1017613, new NpcLocation(2.822865f, 153.521f, _territoryType.GetRow(128)));
        _npcLocations.TryAdd(1003077, new NpcLocation(-259.32715f, 37.491333f, _territoryType.GetRow(129)));

        _npcLocations.TryAdd(1008145, new NpcLocation(-31.265808f, -245.38031f, _territoryType.GetRow(133)));
        _npcLocations.TryAdd(1006005, new NpcLocation(-61.234497f, -141.31384f, _territoryType.GetRow(133)));
        _npcLocations.TryAdd(1017614, new NpcLocation(-58.79309f, -142.1073f, _territoryType.GetRow(133)));
        _npcLocations.TryAdd(1003633, new NpcLocation(145.83044f, -106.767456f, _territoryType.GetRow(133)));
    }

    public ItemInfo GetItemInfo(uint itemId)
    {
        if (!_isDataReady) return null;

        return _itemDataMap.TryGetValue(itemId, out var itemInfo) ? itemInfo : null;
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