using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using ItemVendorLocation.Models;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace ItemVendorLocation;
#if DEBUG
public partial class ItemLookup
#else
public partial class ItemLookup
#endif
{
    private readonly ExcelSheet<Achievement> _achievements;
    private readonly ExcelSheet<CustomTalk> _customTalks;
    private readonly SubrowExcelSheet<CustomTalkNestHandlers> _customTalkNestHandlers;
    private readonly ExcelSheet<CollectablesShop> _collectablesShops;
    private readonly SubrowExcelSheet<CollectablesShopItem> _collectablesShopItems;
    private readonly ExcelSheet<CollectablesShopRefine> _collectablesShopRefines;
    private readonly ExcelSheet<CollectablesShopRewardItem> _collectablesShopRewardItems;
    private readonly ExcelSheet<ENpcBase> _eNpcBases;
    private readonly ExcelSheet<ENpcResident> _eNpcResidents;
    private readonly ExcelSheet<FateShop> _fateShops;
    private readonly ExcelSheet<FccShop> _fccShops;
    private readonly ExcelSheet<GCShop> _gcShops;
    private readonly ExcelSheet<GCScripShopCategory> _gcScripShopCategories;
    private readonly SubrowExcelSheet<GCScripShopItem> _gcScripShopItems;
    private readonly ExcelSheet<GilShop> _gilShops;
    private readonly SubrowExcelSheet<GilShopItem> _gilShopItems;
    private readonly ExcelSheet<InclusionShop> _inclusionShops;
    private readonly SubrowExcelSheet<InclusionShopSeries> _inclusionShopSeries;
    private readonly ExcelSheet<Item> _items;
    private readonly ExcelSheet<Map> _maps;
    private readonly ExcelSheet<PreHandler> _preHandlers;
    private readonly SubrowExcelSheet<QuestClassJobReward> _questClassJobRewards;
    private readonly ExcelSheet<SpecialShop> _specialShops;
    private readonly ExcelSheet<TerritoryType> _territoryType;
    private readonly ExcelSheet<TopicSelect> _topicSelects;
    private readonly ExcelSheet<TomestoneConvert> _tomestoneConvert;

    private readonly Item _gil;
    private readonly List<Item> _gcSeal;
    private readonly Addon _fccName;

    private readonly Dictionary<uint, ItemInfo> _itemDataMap = new();
    private readonly Dictionary<uint, NpcLocation> _npcLocations = new();

    private readonly EventHandlerType[] _eventHandlerTypes;

    public ItemLookup()
    {
        _eventHandlerTypes = Enum.GetValues<EventHandlerType>();

        _achievements = Service.DataManager.GetExcelSheet<Achievement>();
        _customTalks = Service.DataManager.GetExcelSheet<CustomTalk>();
        _customTalkNestHandlers = Service.DataManager.GetSubrowExcelSheet<CustomTalkNestHandlers>();
        _collectablesShops = Service.DataManager.GetExcelSheet<CollectablesShop>();
        _collectablesShopItems = Service.DataManager.GetSubrowExcelSheet<CollectablesShopItem>();
        _collectablesShopRefines = Service.DataManager.GetExcelSheet<CollectablesShopRefine>();
        _collectablesShopRewardItems = Service.DataManager.GetExcelSheet<CollectablesShopRewardItem>();
        _eNpcBases = Service.DataManager.GetExcelSheet<ENpcBase>();
        _eNpcResidents = Service.DataManager.GetExcelSheet<ENpcResident>();
        _fateShops = Service.DataManager.GetExcelSheet<FateShop>();
        _fccShops = Service.DataManager.GetExcelSheet<FccShop>();
        _gcShops = Service.DataManager.GetExcelSheet<GCShop>();
        _gcScripShopCategories = Service.DataManager.GetExcelSheet<GCScripShopCategory>();
        _gcScripShopItems = Service.DataManager.GetSubrowExcelSheet<GCScripShopItem>();
        _gilShops = Service.DataManager.GetExcelSheet<GilShop>();
        _gilShopItems = Service.DataManager.GetSubrowExcelSheet<GilShopItem>();
        _inclusionShops = Service.DataManager.GetExcelSheet<InclusionShop>();
        _inclusionShopSeries = Service.DataManager.GetSubrowExcelSheet<InclusionShopSeries>();
        _items = Service.DataManager.GetExcelSheet<Item>();
        _maps = Service.DataManager.GetExcelSheet<Map>();
        _preHandlers = Service.DataManager.GetExcelSheet<PreHandler>();
        _questClassJobRewards = Service.DataManager.GetSubrowExcelSheet<QuestClassJobReward>();
        _specialShops = Service.DataManager.GetExcelSheet<SpecialShop>();
        _territoryType = Service.DataManager.GetExcelSheet<TerritoryType>();
        _topicSelects = Service.DataManager.GetExcelSheet<TopicSelect>();
        _tomestoneConvert = Service.DataManager.GetExcelSheet<TomestoneConvert>();

        _fccName = Service.DataManager.GetExcelSheet<Addon>().GetRow(102233);
        _gil = _items.GetRow(1);
        _gcSeal = _items.Where(i => i.RowId is >= 20 and <= 22).Select(i => i).ToList();

        BuildNpcLocation();
        ApplyNpcLocationCorrections();
        BuildVendors();
        AddAchievementItem();
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
    }

#if DEBUG
    public void BuildDebugVendorInfo(uint vendorId)
    {
        var npcBase = _eNpcBases.GetRowOrDefault(vendorId);
        if (!npcBase.HasValue)
        {
            return;
        }

        BuildVendorInfo(npcBase.Value);
    }
#endif

    private void BuildVendors()
    {
        foreach (var npcBase in _eNpcBases)
        {
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

        var fateShop = _fateShops.GetRowOrDefault(npcBase.RowId);
        if (fateShop.HasValue)
        {
            foreach (var specialShop in fateShop.Value.SpecialShop)
            {
                var specialShopCustom = _specialShops.GetRowOrDefault(specialShop.RowId);

                if (specialShopCustom == null)
                {
                    continue;
                }

                AddSpecialItem(specialShopCustom.Value, npcBase, resident);
            }

            return;
        }

        foreach (var npcData in npcBase.ENpcData.Select(x => x.RowId))
        {
            if (npcData == 0)
            {
                break;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.CollectablesShop))
            {
                var collectablesShop = _collectablesShops.GetRow(npcData);
                AddCollectablesShop(collectablesShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.InclusionShop))
            {
                var inclusionShop = _inclusionShops.GetRow(npcData);
                AddInclusionShop(inclusionShop, npcBase, resident);
                continue;
            }

            if (MatchEventHandlerType(npcData, EventHandlerType.FcShop))
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
                AddSpecialItem(specialShop, npcBase, resident, shop: specialShop.Name.ExtractText());
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
                var customTalk = _customTalks.GetRowOrDefault(npcData);
                if (!customTalk.HasValue)
                {
                    break;
                }

                var scriptArgs = customTalk.Value.Script.Select(x => x.ScriptArg).ToArray();
                if (npcData == 721068)
                {
                    // scriptArgs[0] -> QuestId
                    // scriptArgs[2] -> ItemId
                    // scriptArgs[3] -> Amount of item
                    // scriptArgs[4] -> Amount of currency
                    AddItem_Internal(scriptArgs[2], _items.GetRow(scriptArgs[2]).Name.ExtractText(), npcBase.RowId, resident.Singular.ExtractText(), customTalk.Value.MainOption.ExtractText(),
                                     new()
                                     {
                                         new(scriptArgs[4], _items.GetRow(28).Name.ExtractText()),
                                     },
                                     _npcLocations.TryGetValue(npcBase.RowId, out var value) ? value : null,
                                     ItemType.SpecialShop);
                    continue;
                }

                if (customTalk.Value.SpecialLinks.RowId != 0)
                {
                    try
                    {
                        for (ushort index = 0; index <= 30; index++)
                        {
                            var customTalkNestHandler = _customTalkNestHandlers.GetSubrowOrDefault(customTalk.Value.SpecialLinks.RowId, index);
                            if (!customTalkNestHandler.HasValue)
                            {
                                break;
                            }

                            if (MatchEventHandlerType(customTalkNestHandler.Value.NestHandler.RowId, EventHandlerType.SpecialShop))
                            {
                                var specialShop = _specialShops.GetRow(customTalkNestHandler.Value.NestHandler.RowId);
                                AddSpecialItem(specialShop, npcBase, resident);
                                continue;
                            }

                            if (MatchEventHandlerType(customTalkNestHandler.Value.NestHandler.RowId, EventHandlerType.GilShop))
                            {
                                var gilShop = _gilShops.GetRow(customTalkNestHandler.Value.NestHandler.RowId);
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
        foreach (var territory in aetheryteSheet!.Where(i => i.Territory.IsValid && i.Territory.RowId != 1).Select(i => i.Territory.Value))
        {
            if (addedAetheryte.Contains(territory.RowId))
            {
                continue;
            }

            ParseLgbFile(GetLgbFileFromBg(territory.Bg.ExtractText()), territory, npcId);
            addedAetheryte.Add(territory.RowId);
        }

        foreach (var territory in _territoryType.Where(type =>
                 {
                     var condition = type.ContentFinderCondition.Value;
                     // eureka, bozja, gathering
                     return condition.ContentType.RowId is 26 or 29 or 16;
                 }))
        {
            ParseLgbFile(GetLgbFileFromBg(territory.Bg.ExtractText()), territory, npcId);
        }

        var levels = Service.DataManager.GetExcelSheet<Level>();
        foreach (var level in levels!.Where(i => i.Type == 8 && i.Territory.ValueNullable != null))
        {
            // NPC Id
            if (level.Object.RowId != npcId)
            {
                continue;
            }

            var npcBase = _eNpcBases.GetRowOrDefault(level.Object.RowId);
            if (npcBase == null)
            {
                continue;
            }

            var match = npcBase.Value.ENpcData.Any(data => _eventHandlerTypes.Any(i => MatchEventHandlerType(data.RowId, i)));

            if (!match)
            {
                continue;
            }

            try
            {
                _npcLocations.Add(level.Object.RowId, new NpcLocation(level.X, level.Z, level.Territory.Value));
            }
            catch (ArgumentException)
            {
                _ = _npcLocations.TryGetValue(level.Object.RowId, out var npcLocation);
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

                var npcBase = _eNpcBases.GetRowOrDefault(npcRowId);
                if (!npcBase.HasValue)
                {
                    continue;
                }

                var resident = _eNpcResidents.GetRowOrDefault(npcRowId);
                if (!resident.HasValue)
                {
                    continue;
                }

                var match = npcBase.Value.ENpcData.Any(data => _eventHandlerTypes.Any(i => MatchEventHandlerType(data.RowId, i)));
                if (!match)
                {
                    continue;
                }


                var mapId = resident.Value.Map;
                try
                {
                    var map = _maps.First(i => i.TerritoryType.RowId == sTerritoryType.RowId && i.MapIndex == mapId);
                    _npcLocations.Add(npcRowId, new(instanceObject.Transform.Translation.X, instanceObject.Transform.Translation.Z, sTerritoryType, map.RowId));
                }
                catch (InvalidOperationException)
                {
                    _npcLocations.Add(npcRowId, new(instanceObject.Transform.Translation.X, instanceObject.Transform.Translation.Z, sTerritoryType));
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
        foreach (var territory in aetheryteSheet!.Where(i => i.Territory.IsValid && i.Territory.RowId != 1).Select(i => i.Territory.Value))
        {
            if (addedAetheryte.Contains(territory.RowId))
            {
                continue;
            }

            var file = GetLgbFileFromBg(territory.Bg.ExtractText());
            if (file == null)
            {
                Service.PluginLog.Debug($"[Aetheryte] LgbFile is null. territory: ({territory.PlaceName.Value.Name}){territory.RowId}, Bg: {territory.Bg}");
                continue;
            }

            ParseLgbFile(file, territory);
            addedAetheryte.Add(territory.RowId);
        }

        foreach (var territory in _territoryType.Where(type =>
                 {
                     var condition = type.ContentFinderCondition.Value;
                     // eureka, bozja, gathering
                     return condition.ContentType.RowId is 26 or 29 or 16;
                 }))
        {
            var file = GetLgbFileFromBg(territory.Bg.ExtractText());
            if (file == null)
            {
                Service.PluginLog.Debug($"[TerritoryType] LgbFile is null. territory: ({territory.PlaceName.Value.Name}){territory.RowId}, Bg: {territory.Bg}");
                continue;
            }

            ParseLgbFile(file, territory);
        }

        var levels = Service.DataManager.GetExcelSheet<Level>();
        foreach (var level in levels!.Where(i => i.Type == 8 && i.Territory.ValueNullable != null))
        {
            if (_npcLocations.ContainsKey(level.Object.RowId))
            {
                continue;
            }

            var npcBase = _eNpcBases.GetRowOrDefault(level.Object.RowId);
            if (!npcBase.HasValue)
            {
                continue;
            }
            var match = npcBase.Value.ENpcData.Any(data => _eventHandlerTypes.Any(i => MatchEventHandlerType(data.RowId, i)));

            if (!match)
            {
                continue;
            }

            _npcLocations.Add(level.Object.RowId, new(level.X, level.Z, level.Territory.Value));
        }

        // housing vendors
        var employmentNpcLists = Service.DataManager.GetSubrowExcelSheet<HousingEmploymentNpcList>();
        var housingTerrotry = _territoryType.GetRow(282);

        foreach (var npc in employmentNpcLists)
        {
            foreach (var id in npc.Select(x => x.MaleENpcBase).Where(i => i.RowId != 0))
            {
                _npcLocations.Add(id.RowId, new(0, 0, housingTerrotry));
            }
        }
    }

    private LgbFile? GetLgbFileFromBg(string bg)
    {
        var lgbFileName = "bg/" + bg[..(bg.IndexOf("/level/", StringComparison.Ordinal) + 1)] + "level/planevent.lgb";
        return Service.DataManager.GetFile<LgbFile>(lgbFileName);
    }

    public ItemInfo? GetItemInfo(uint itemId)
    {
        return _itemDataMap.TryGetValue(itemId, out var itemInfo) ? itemInfo : null;
    }

    public Dictionary<uint, ItemInfo> GetItems()
    {
        return _itemDataMap;
    }

    // https://discord.com/channels/581875019861328007/653504487352303619/860865002721247261
    // https://github.com/SapphireServer/Sapphire/blob/a5c15f321f7e795ed7362ae15edaada99ca7d9be/src/world/Manager/EventMgr.cpp#L14
    private static bool MatchEventHandlerType(uint data, EventHandlerType type)
    {
        return (data >> 16) == (uint)type;
    }

    // https://github.com/SapphireServer/Sapphire/blob/a5c15f321f7e795ed7362ae15edaada99ca7d9be/src/world/Event/EventHandler.h#L48-L83
    internal enum EventHandlerType : uint
    {
        GilShop = 0x0004,
        CustomTalk = 0x000B,
        GcShop = 0x0016,
        SpecialShop = 0x001B,
        FcShop = 0x002A, // not sure how these numbers were obtained by the folks at sapphire. This works for my isolated use case though I guess.
        TopicSelect = 0x0032,
        PreHandler = 0x0036,
        InclusionShop = 0x003a,
        CollectablesShop = 0x003B,
    }
}