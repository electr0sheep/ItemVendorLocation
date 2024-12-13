using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ItemVendorLocation.Models;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace ItemVendorLocation;

internal class HighlightObject : IDisposable
{
    private NpcInfo[] _npcInfo = [];
    private uint[] _targetNpcDataId = [];
    private DateTime _lastUpdateTime = DateTime.Now;

    public HighlightObject()
    {
        Service.Framework.Update += Framework_OnUpdate;
    }

    private void Framework_OnUpdate(IFramework framework)
    {
        //we want to update every 300 ms
        if (DateTime.Now - _lastUpdateTime <= TimeSpan.FromMilliseconds(300))
        {
            return;
        }

        _lastUpdateTime = DateTime.Now;

        if (!Service.Configuration.HighlightSelectedNpc || _npcInfo == null)
        {
            return;
        }

        ToggleHighlight(true);
    }

    public void SetNpcInfo(NpcInfo[] npcInfos)
    {
        _ = Service.Framework.Run(() =>
        {
            // before we update, we want to know if the previous npc object is still valid
            if (_targetNpcDataId.Any(n => n > 0))
            {
                ToggleHighlight(false);
            }

            foreach (var npcInfo in npcInfos)
            {
                Service.PluginLog.Debug($"Setting npc info for HighlightObject. {npcInfo.Id} / {npcInfo.Name}");
            }
            _targetNpcDataId = npcInfos.Select(n => n.Id).ToArray();
            _npcInfo = npcInfos;
        });
    }

    public unsafe void ToggleHighlight(bool on)
    {
        if (_targetNpcDataId.All(n => n == 0))
        {
            return;
        }

        var gameObjects = Service.ObjectTable.Where(i =>
        {
            if (!i.IsValid())
                return false;
            var obj = (GameObject*)i.Address;
            return _targetNpcDataId.Contains(obj->BaseId);
        });

        if (!gameObjects.Any())
        {
            return;
        }

        foreach (var obj in gameObjects)
        {
            ((GameObject*)obj.Address)->Highlight(on ? Service.Configuration.HighlightColor : ObjectHighlightColor.None);
        }
    }

    public void Dispose()
    {
        Service.Framework.Update -= Framework_OnUpdate;
    }
}