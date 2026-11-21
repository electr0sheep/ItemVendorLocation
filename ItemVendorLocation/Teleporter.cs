using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ItemVendorLocation.Models;
using Lumina.Excel.Sheets;

namespace ItemVendorLocation;

internal class Teleporter : IDisposable
{
    private const uint TeleportActionId = 5;
    private const byte AetheryteMarkerType = 3;

    private readonly Dictionary<uint, (Aetheryte Aetheryte, Vector2 Position)[]> _aetherytesByTerritory;
    private readonly Dictionary<uint, DalamudLinkPayload> _chatLinks = new();

    public Teleporter()
    {
        var sheet = Service.DataManager.GetExcelSheet<Aetheryte>();
        var aetherytes = new List<(Aetheryte Aetheryte, Vector2 Position)>();
        foreach (var aetheryte in sheet.Where(a => a.IsAetheryte && a.Territory.RowId > 1 && a.Map.IsValid))
        {
            if (FindWorldPosition(aetheryte) is { } position)
            {
                aetherytes.Add((aetheryte, position));
            }
        }

        _aetherytesByTerritory = aetherytes.GroupBy(a => a.Aetheryte.Territory.RowId).ToDictionary(g => g.Key, g => g.ToArray());

        // Zones with only aethernet shards (e.g. Limsa Upper Decks) are reached through their city's aetheryte
        foreach (var shard in sheet.Where(a => !a.IsAetheryte && a.AethernetGroup != 0))
        {
            _ = _aetherytesByTerritory.TryAdd(shard.Territory.RowId, [.. aetherytes.Where(a => a.Aetheryte.AethernetGroup == shard.AethernetGroup)]);
        }
    }

    internal unsafe Aetheryte? FindNearestAetheryte(NpcLocation location)
    {
        if (!_aetherytesByTerritory.TryGetValue(location.TerritoryType, out var aetherytes))
        {
            return null;
        }

        var npcPosition = new Vector2(location.X, location.Y);
        Aetheryte? nearest = null;
        var nearestDistance = float.MaxValue;
        foreach (var (aetheryte, position) in aetherytes)
        {
            if (!UIState.Instance()->IsAetheryteUnlocked(aetheryte.RowId))
            {
                continue;
            }

            var distance = Vector2.DistanceSquared(position, npcPosition);
            if (distance < nearestDistance)
            {
                nearest = aetheryte;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    internal void TeleportNear(NpcLocation location)
    {
        if (FindNearestAetheryte(location) is not { } aetheryte)
        {
            return;
        }

        _ = Service.GameGui.OpenMapWithMapLink(new(location.TerritoryType, location.MapId, location.MapX, location.MapY, 0f));
        Teleport(aetheryte);
    }

    internal SeString CreateChatLink(NpcInfo npc)
    {
        lock (_chatLinks)
        {
            if (!_chatLinks.TryGetValue(npc.Id, out var link))
            {
                link = Service.ChatGui.AddChatLinkHandler(npc.Id, OnChatLinkClicked);
                _chatLinks.Add(npc.Id, link);
            }

            return new SeStringBuilder()
                   .Add(link)
                   .Append(SeString.TextArrowPayloads)
                   .AddText("Teleport")
                   .Add(RawPayload.LinkTerminator)
                   .BuiltString;
        }
    }

    public void Dispose()
    {
        Service.ChatGui.RemoveChatLinkHandler();
    }

    private void OnChatLinkClicked(uint npcId, SeString _)
    {
        if (Service.Plugin.ItemLookup.GetNpcLocation(npcId) is { } location)
        {
            TeleportNear(location);
        }
    }

    private static Vector2? FindWorldPosition(Aetheryte aetheryte)
    {
        var map = aetheryte.Map.Value;
        if (Service.DataManager.GetSubrowExcelSheet<MapMarker>().GetRowOrDefault(map.MapMarkerRange) is not { } markers)
        {
            return null;
        }

        var scale = map.SizeFactor / 100f;
        foreach (var marker in markers)
        {
            if (marker.DataType == AetheryteMarkerType && marker.DataKey.RowId == aetheryte.RowId)
            {
                return new((marker.X - 1024f) / scale - map.OffsetX, (marker.Y - 1024f) / scale - map.OffsetY);
            }
        }

        return null;
    }

    private static unsafe void Teleport(Aetheryte aetheryte)
    {
        if (Service.ObjectTable.LocalPlayer == null)
        {
            return;
        }

        var status = ActionManager.Instance()->GetActionStatus(ActionType.Action, TeleportActionId);
        if (status != 0)
        {
            var reason = Service.DataManager.GetExcelSheet<LogMessage>().GetRowOrDefault(status)?.Text.ExtractText();
            Utilities.OutputChatLine(string.IsNullOrEmpty(reason) ? "Unable to teleport right now." : reason);
            return;
        }

        var telepo = Telepo.Instance();
        telepo->UpdateAetheryteList();
        _ = telepo->Teleport(aetheryte.RowId, 0);
    }
}
