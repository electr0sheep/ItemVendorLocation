using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemVendorLocation.Models;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace ItemVendorLocation;
#if DEBUG
public partial class ItemLookup
#else
    internal partial class ItemLookup
#endif
{
    private readonly ExcelSheet<Achievement> _achievements;
    private readonly ExcelSheet<CustomTalk> _customTalks;
    private readonly ExcelSheet<CustomTalkNestHandlers> _customTalkNestHandlers;
    private readonly ExcelSheet<CollectablesShop> _collectablesShops;
    private readonly ExcelSheet<CollectablesShopItem> _collectablesShopItems;
    private readonly ExcelSheet<CollectablesShopRefine> _collectablesShopRefines;
    private readonly ExcelSheet<CollectablesShopRewardItem> _collectablesShopRewardItems;
    private readonly ExcelSheet<ENpcBase> _eNpcBases;
    private readonly ExcelSheet<ENpcResident> _eNpcResidents;
    private readonly ExcelSheet<FateShopCustom> _fateShops;
    private readonly ExcelSheet<FccShop> _fccShops;
    private readonly ExcelSheet<GCShop> _gcShops;
    private readonly ExcelSheet<GCScripShopCategory> _gcScripShopCategories;
    private readonly ExcelSheet<GCScripShopItem> _gcScripShopItems;
    private readonly ExcelSheet<GilShop> _gilShops;
    private readonly ExcelSheet<GilShopItem> _gilShopItems;
    private readonly ExcelSheet<InclusionShop> _inclusionShops;
    private readonly ExcelSheet<InclusionShopSeriesCustom> _inclusionShopSeries;
    private readonly ExcelSheet<Item> _items;
    private readonly ExcelSheet<Map> _maps;
    private readonly ExcelSheet<PreHandler> _preHandlers;
    private readonly ExcelSheet<QuestClassJobReward> _questClassJobRewards;
    private readonly ExcelSheet<SpecialShopCustom> _specialShops;
    private readonly ExcelSheet<TerritoryType> _territoryType;
    private readonly ExcelSheet<TopicSelect> _topicSelects;

    private readonly Item _gil;
    private readonly List<Item> _gcSeal;
    private readonly Addon _fccName;

    private readonly Dictionary<uint, ItemInfo> _itemDataMap = new();
    private readonly Dictionary<uint, NpcLocation> _npcLocations = new();

    private bool _isDataReady;

    private readonly EventHandlerType[] _eventHandlerTypes;

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
        _eventHandlerTypes = Enum.GetValues<EventHandlerType>();

        _achievements = Service.DataManager.GetExcelSheet<Achievement>();
        _customTalks = Service.DataManager.GetExcelSheet<CustomTalk>();
        _customTalkNestHandlers = Service.DataManager.GetExcelSheet<CustomTalkNestHandlers>();
        _collectablesShops = Service.DataManager.GetExcelSheet<CollectablesShop>();
        _collectablesShopItems = Service.DataManager.GetExcelSheet<CollectablesShopItem>();
        _collectablesShopRefines = Service.DataManager.GetExcelSheet<CollectablesShopRefine>();
        _collectablesShopRewardItems = Service.DataManager.GetExcelSheet<CollectablesShopRewardItem>();
        _eNpcBases = Service.DataManager.GetExcelSheet<ENpcBase>();
        _eNpcResidents = Service.DataManager.GetExcelSheet<ENpcResident>();
        _fateShops = Service.DataManager.GetExcelSheet<FateShopCustom>();
        _fccShops = Service.DataManager.GetExcelSheet<FccShop>();
        _gcShops = Service.DataManager.GetExcelSheet<GCShop>();
        _gcScripShopCategories = Service.DataManager.GetExcelSheet<GCScripShopCategory>();
        _gcScripShopItems = Service.DataManager.GetExcelSheet<GCScripShopItem>();
        _gilShops = Service.DataManager.GetExcelSheet<GilShop>();
        _gilShopItems = Service.DataManager.GetExcelSheet<GilShopItem>();
        _inclusionShops = Service.DataManager.GetExcelSheet<InclusionShop>();
        _inclusionShopSeries = Service.DataManager.GetExcelSheet<InclusionShopSeriesCustom>();
        _items = Service.DataManager.GetExcelSheet<Item>();
        _maps = Service.DataManager.GetExcelSheet<Map>();
        _preHandlers = Service.DataManager.GetExcelSheet<PreHandler>();
        _questClassJobRewards = Service.DataManager.GetExcelSheet<QuestClassJobReward>();
        _specialShops = Service.DataManager.GetExcelSheet<SpecialShopCustom>();
        _territoryType = Service.DataManager.GetExcelSheet<TerritoryType>();
        _topicSelects = Service.DataManager.GetExcelSheet<TopicSelect>();

        _fccName = Service.DataManager.GetExcelSheet<Addon>().GetRow(102233);
        _gil = _items.GetRow(1);
        _gcSeal = _items.Where(i => i.RowId is >= 20 and <= 22).Select(i => i).ToList();

        _ = Task.Run(() =>
        {
            BuildNpcLocation();
            ApplyNpcLocationCorrections();
            BuildVendors();
            AddAchievementItem();
            FixJapaneseShopName();
            _isDataReady = true;
#if DEBUG
            Dictionary<string, uint> noLocationNpcs = new();
            foreach (var items in _itemDataMap)
            {
                foreach (var npc in items.Value.NpcInfos)
                {
                    if (npc.Location != null)
                    {
                        continue;
                    }

                    if (noLocationNpcs.TryAdd(npc.Name, 1))
                    {
                        continue;
                    }

                    noLocationNpcs[npc.Name]++;
                }
            }

            Service.PluginLog.Debug("Data is ready");
            Service.PluginLog.Debug($"Items sold by NPCs with no location: {noLocationNpcs.Values.Aggregate((sum, i) => sum += i)}");
            Service.PluginLog.Debug("Named NPCs:");

            foreach (var npc in noLocationNpcs.Where(npc => char.IsUpper(npc.Key.First())))
            {
                Service.PluginLog.Debug($"{npc.Key} sells {npc.Value} items");
            }

            Service.PluginLog.Debug("Unnamed NPCs:");
            foreach (var npc in noLocationNpcs.Where(npc => !char.IsUpper(npc.Key.First())))
            {
                Service.PluginLog.Debug($"{npc.Key} sells {npc.Value} items");
            }
#endif
            return Task.CompletedTask;
        });
    }

#if DEBUG
    public void BuildDebugVendorInfo(uint vendorId)
    {
        var npcBase = _eNpcBases.GetRow(vendorId);
        if (npcBase == null)
        {
            return;
        }

        BuildVendorInfo(npcBase);
    }
#endif

    private void BuildVendors()
    {
        foreach (var npcBase in _eNpcBases)
        {
            if (npcBase == null)
            {
                continue;
            }

            BuildVendorInfo(npcBase);
        }
    }

    private void BuildVendorInfo(ENpcBase npcBase)
    {
        var resident = _eNpcResidents.GetRow(npcBase.RowId);

        if (FixNpcVendorInfo(npcBase, resident))
        {
            return;
        }

        var fateShop = _fateShops.GetRow(npcBase.RowId);
        if (fateShop != null)
        {
            foreach (var specialShop in fateShop.SpecialShop)
            {
                if (specialShop.Value == null)
                {
                    continue;
                }

                var specialShopCustom = _specialShops.GetRow(specialShop.Row);
                AddSpecialItem(specialShopCustom, npcBase, resident);
            }

            return;
        }

        foreach (var npcData in npcBase.ENpcData)
        {
            if (npcData == 0)
            {
                break;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.InclusionShop))
            {
                var inclusionShop = _inclusionShops.GetRow(npcData);
                AddInclusionShop(inclusionShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.InclusionShop))
            {
                var fccShop = _fccShops.GetRow(npcData);
                AddFccShop(fccShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.PreHandler))
            {
                var preHandler = _preHandlers.GetRow(npcData);
                AddItemsInPrehandler(preHandler, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.TopicSelect))
            {
                var topicSelect = _topicSelects.GetRow(npcData);
                AddItemsInTopicSelect(topicSelect, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.GcShop))
            {
                var gcShop = _gcShops.GetRow(npcData);
                AddGcShopItem(gcShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.SpecialShop))
            {
                var specialShop = _specialShops.GetRow(npcData);
                AddSpecialItem(specialShop, npcBase, resident, shop: specialShop?.Name?.RawString);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.GilShop))
            {
                var gilShop = _gilShops.GetRow(npcData);
                AddGilShopItem(gilShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.CustomTalk))
            {
                var customTalk = _customTalks.GetRow(npcData);
                if (customTalk == null)
                {
                    break;
                }

                var scriptArgs = customTalk.ScriptArg;
                if (npcData == 721068)
                {
                    // scriptArgs[0] -> QuestId
                    // scriptArgs[2] -> ItemId
                    // scriptArgs[3] -> Amount of item
                    // scriptArgs[4] -> Amount of currency
                    AddItem_Internal(scriptArgs[2], _items.GetRow(scriptArgs[2]).Name.RawString, npcBase.RowId, resident.Singular, customTalk.MainOption.RawString,
                                     new List<Tuple<uint, string>>
                                     {
                                         new(scriptArgs[4], _items.GetRow(28).Name.RawString),
                                     },
                                     _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null,
                                     ItemType.SpecialShop);
                    continue;
                }

                if (customTalk.SpecialLinks != 0)
                {
                    try
                    {
                        for (uint index = 0; index <= 30; index++)
                        {
                            var customTalkNestHandler = _customTalkNestHandlers.GetRow(customTalk.SpecialLinks, index);
                            if (customTalkNestHandler == null)
                            {
                                break;
                            }

                            if (MatchEventHandlerType(customTalkNestHandler.NestHandler, EventHandlerType.SpecialShop))
                            {
                                var specialShop = _specialShops.GetRow(customTalkNestHandler.NestHandler);
                                AddSpecialItem(specialShop, npcBase, resident);
                                continue;
                            }

                            if (MatchEventHandlerType(customTalkNestHandler.NestHandler, EventHandlerType.GilShop))
                            {
                                var gilShop = _gilShops.GetRow(customTalkNestHandler.NestHandler);
                                AddGilShopItem(gilShop, npcBase, resident);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                foreach (var arg in scriptArgs)
                {
                    if (arg == 0)
                    {
                        break;
                    }

                    if (MatchEventHandlerType(arg, EventHandlerType.GilShop))
                    {
                        var gilShop = _gilShops.GetRow(arg);
                        AddGilShopItem(gilShop, npcBase, resident);
                        continue;
                    }

                    if (MatchEventHandlerType(arg, EventHandlerType.FcShop))
                    {
                        var shop = _fccShops.GetRow(arg);
                        AddFccShop(shop, npcBase, resident);
                        continue;
                    }

                    if (MatchEventHandlerType(arg, EventHandlerType.SpecialShop))
                    {
                        var specialShop = _specialShops.GetRow(arg);
                        AddSpecialItem(specialShop, npcBase, resident);
                    }
                }
            }
        }
    }

#if DEBUG
    public void BuildDebugNpcLocation(uint npcId)
    {
        HashSet<uint> addedAetheryte = new();
        var aetheryteSheet = Service.DataManager.GetExcelSheet<Aetheryte>();
        foreach (var territory in aetheryteSheet!.Where(i => i.Territory.Value != null && i.Territory.Row != 1).Select(i => i.Territory.Value))
        {
            if (addedAetheryte.Contains(territory.RowId))
            {
                continue;
            }

            ParseLgbFile(GetLgbFileFromBg(territory.Bg), territory, npcId);
            addedAetheryte.Add(territory.RowId);
        }

        foreach (var territory in _territoryType.Where(type =>
                 {
                     var condition = type.ContentFinderCondition.Value;
                     // eureka, bozja, gathering
                     return condition?.ContentType.Row is 26 or 29 or 16;
                 }))
        {
            ParseLgbFile(GetLgbFileFromBg(territory.Bg), territory, npcId);
        }

        var levels = Service.DataManager.GetExcelSheet<Level>();
        foreach (var level in levels!.Where(i => i.Type == 8 && i.Territory.Value != null))
        {
            // NPC Id
            if (level.Object != npcId)
            {
                continue;
            }

            var npcBase = _eNpcBases.GetRow(level.Object);
            if (npcBase == null)
            {
                continue;
            }

            var match = npcBase.ENpcData.Any(data => _eventHandlerTypes.Any(i => MatchEventHandlerType(data, i)));

            if (!match)
            {
                continue;
            }

            try
            {
                _npcLocations.Add(level.Object, new NpcLocation(level.X, level.Z, level.Territory.Value));
            }
            catch (ArgumentException)
            {
                _ = _npcLocations.TryGetValue(level.Object, out var npcLocation);
                Service.PluginLog.Debug($"This npc has this location: Map {npcLocation.MapId} Territory {npcLocation.TerritoryType}");
                // The row should already exist. This is just for debugging.
            }
        }
    }
#endif

    public void ParseLgbFile(LgbFile lgbFile, TerritoryType sTerritoryType, uint? npcId = null)
    {
        foreach (var sLgbGroup in lgbFile.Layers)
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

#if DEBUG
                if (npcId != null && npcRowId != npcId)
                {
                    continue;
                }

                if (npcId == null && _npcLocations.ContainsKey(npcRowId))
                {
                    continue;
                }
#else
                    if (_npcLocations.ContainsKey(npcRowId))
                    {
                        continue;
                    }
#endif

                var npcBase = _eNpcBases.GetRow(npcRowId);
                if (npcBase == null)
                {
                    continue;
                }

                var resident = _eNpcResidents.GetRow(npcRowId);
                if (resident == null)
                {
                    continue;
                }

                var match = npcBase.ENpcData.Any(data => _eventHandlerTypes.Any(i => MatchEventHandlerType(data, i)));
                if (!match)
                {
                    continue;
                }

                var mapId = resident.Map;
                try
                {
                    var map = _maps.First(i => i.TerritoryType.Row == sTerritoryType.RowId && i.MapIndex == mapId);
                    _npcLocations.Add(npcRowId, new NpcLocation(instanceObject.Transform.Translation.X, instanceObject.Transform.Translation.Z, sTerritoryType, map.RowId));
                }
                catch (InvalidOperationException)
                {
                    _npcLocations.Add(npcRowId, new NpcLocation(instanceObject.Transform.Translation.X, instanceObject.Transform.Translation.Z, sTerritoryType));
                }
            }
        }
    }

    // https://github.com/ufx/GarlandTools/blob/3b3475bca6f95c800d2454f2c09a3f1eea0a8e4e/Garland.Data/Modules/Territories.cs
#if DEBUG
    public void BuildNpcLocation()
#else
        private void BuildNpcLocation()
#endif
    {
        HashSet<uint> addedAetheryte = new();
        var aetheryteSheet = Service.DataManager.GetExcelSheet<Aetheryte>();
        foreach (var territory in aetheryteSheet!.Where(i => i.Territory.Value != null && i.Territory.Row != 1).Select(i => i.Territory.Value))
        {
            if (addedAetheryte.Contains(territory.RowId))
            {
                continue;
            }

            ParseLgbFile(GetLgbFileFromBg(territory.Bg), territory);
            addedAetheryte.Add(territory.RowId);
        }

        foreach (var territory in _territoryType.Where(type =>
                 {
                     var condition = type.ContentFinderCondition.Value;
                     // eureka, bozja, gathering
                     return condition?.ContentType.Row is 26 or 29 or 16;
                 }))
        {
            ParseLgbFile(GetLgbFileFromBg(territory.Bg), territory);
        }

        var levels = Service.DataManager.GetExcelSheet<Level>();
        foreach (var level in levels!.Where(i => i.Type == 8 && i.Territory.Value != null))
        {
            if (_npcLocations.ContainsKey(level.Object))
            {
                continue;
            }

            var npcBase = _eNpcBases.GetRow(level.Object);
            if (npcBase == null)
            {
                continue;
            }

            var match = npcBase.ENpcData.Any(data => _eventHandlerTypes.Any(i => MatchEventHandlerType(data, i)));

            if (!match)
            {
                continue;
            }

            _npcLocations.Add(level.Object, new NpcLocation(level.X, level.Z, level.Territory.Value));
        }

        // housing vendors
        var employmentNpcLists = Service.DataManager.GetExcelSheet<HousingEmploymentNpcList>();
        var housingTerrotry = _territoryType.GetRow(282);

        foreach (var npc in employmentNpcLists!)
        {
            foreach (var id in npc.ENpcBase.Where(i => i.Row != 0))
            {
                _npcLocations.Add(id.Row, new NpcLocation(0, 0, housingTerrotry));
            }
        }
    }

    private LgbFile GetLgbFileFromBg(string bg)
    {
        var lgbFileName = "bg/" + bg[..(bg.IndexOf("/level/", StringComparison.Ordinal) + 1)] + "level/planevent.lgb";
        return Service.DataManager.GetFile<LgbFile>(lgbFileName);
    }

    public ItemInfo GetItemInfo(uint itemId)
    {
        return !_isDataReady ? null : _itemDataMap.TryGetValue(itemId, out var itemInfo) ? itemInfo : null;
    }

    // https://discord.com/channels/581875019861328007/653504487352303619/860865002721247261
    // https://github.com/SapphireServer/Sapphire/blob/a5c15f321f7e795ed7362ae15edaada99ca7d9be/src/world/Manager/EventMgr.cpp#L14
    private static bool MatchEventHandlerType(uint data, EventHandlerType type)
    {
        return ((data >> 16) & (uint)type) == (uint)type;
    }

    // https://github.com/SapphireServer/Sapphire/blob/a5c15f321f7e795ed7362ae15edaada99ca7d9be/src/world/Event/EventHandler.h#L48-L83
    internal enum EventHandlerType : uint
    {
        GilShop = 0x0004,
        CustomTalk = 0x000B,
        GcShop = 0x0016,
        SpecialShop = 0x001B,
        FcShop = 0x002A, // not sure how these numbers were obtained by the folks at sapphire. This works for my isolated use case though I guess.
        TopicSelect = 0x32,
        PreHandler = 0x36,
        InclusionShop = 0x3a, // 0x38 seems to work too?
    }
}