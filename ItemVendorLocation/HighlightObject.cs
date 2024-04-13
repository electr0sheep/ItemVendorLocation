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
    private NpcInfo _npcInfo;
    private uint _targetNpcDataId;
    private DateTime _lastUpdateTime = DateTime.Now;

    public HighlightObject()
    {
        Service.ClientState.TerritoryChanged += ClientState_OnTerritoryChanged;
        Service.Framework.Update += Framework_OnUpdate;
    }

    private void Framework_OnUpdate(IFramework framework)
    {
        // we want to update every 300 ms
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

    private void ClientState_OnTerritoryChanged(ushort territoryId)
    {
        if (_npcInfo == null)
        {
            return;
        }

        if (_npcInfo.Location.TerritoryType == territoryId)
        {
            return;
        }

        _npcInfo = null;
        _targetNpcDataId = 0;
    }

    public void SetNpcInfo(NpcInfo npcInfo)
    {
        _ = Service.Framework.Run(() =>
        {
            // before we update, we want to know if the previous npc object is still valid
            if (_targetNpcDataId > 0)
            {
                ToggleHighlight(false);
            }

            Service.PluginLog.Debug($"Setting npc info for HighlightObject. {npcInfo.Id} / {npcInfo.Name}");
            _targetNpcDataId = npcInfo.Id;
            _npcInfo = npcInfo;
        });
    }

    public unsafe void ToggleHighlight(bool on)
    {
        if (_targetNpcDataId == 0)
        {
            return;
        }

        var gameObject = Service.ObjectTable.FirstOrDefault(i =>
        {
            if (!i.IsValid())
                return false;
            var obj = (GameObject*)i.Address;
            var found = obj->DataID == _targetNpcDataId;
            return found;
        });

        if (gameObject == null)
        {
            return;
        }

        Service.PluginLog.Debug("Setting highlight color");

        ((GameObject*)gameObject.Address)->Highlight(on ? ObjectHighlightColor.Red : ObjectHighlightColor.None);
    }

    public void Dispose()
    {
        Service.ClientState.TerritoryChanged -= ClientState_OnTerritoryChanged;
        Service.Framework.Update -= Framework_OnUpdate;
    }
}