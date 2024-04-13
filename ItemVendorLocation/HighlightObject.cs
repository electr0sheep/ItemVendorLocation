using System;
using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ItemVendorLocation.Models;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace ItemVendorLocation;

internal class HighlightObject : IDisposable
{
    private NpcInfo _npcInfo;
    private uint _targetNpcObjectId;
    private DateTime _lastUpdateTime = DateTime.Now;

    public HighlightObject()
    {
        Service.ClientState.TerritoryChanged += ClientState_OnTerritoryChanged;
        Service.Framework.Update += Framework_OnUpdate;
    }

    private unsafe void Framework_OnUpdate(IFramework framework)
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

        Dalamud.Game.ClientState.Objects.Types.GameObject gameObject;

        if (_targetNpcObjectId == 0)
        {
            gameObject = Service.ObjectTable.FirstOrDefault(i =>
            {
                if (!i.IsValid())
                    return false;
                var obj = (GameObject*)i.Address;
                var found = obj->DataID == _npcInfo.Id;
                return found;
            });

            if (gameObject == null)
            {
                return;
            }

            _targetNpcObjectId = gameObject.ObjectId;

            ((GameObject*)gameObject.Address)->Highlight(ObjectHighlightColor.Red);
            return;
        }

        gameObject = Service.ObjectTable.SearchById(_targetNpcObjectId);
        if (gameObject == null || !gameObject.IsValid())
        {
            return;
        }

        ((GameObject*)gameObject.Address)->Highlight(ObjectHighlightColor.Red);
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
        _targetNpcObjectId = 0;
    }

    public unsafe void SetNpcInfo(NpcInfo npcInfo)
    {
        _ = Service.Framework.Run(() =>
        {
            // before we update, we want to know if the previous npc object is still valid
            if (_targetNpcObjectId > 0)
            {
                var gameObject = Service.ObjectTable.SearchById(_targetNpcObjectId);
                if (gameObject != null && gameObject.IsValid())
                {
                    var gameObjectRaw = (GameObject*)gameObject.Address;
                    // disable highlight
                    gameObjectRaw->Highlight(ObjectHighlightColor.None);
                }

                _targetNpcObjectId = 0;
            }

            Service.PluginLog.Debug($"Setting npc info for HighlightObject. {npcInfo.Id} / {npcInfo.Name}");
            _npcInfo = npcInfo;
        });
    }

    public void Dispose()
    {
        Service.ClientState.TerritoryChanged -= ClientState_OnTerritoryChanged;
        Service.Framework.Update -= Framework_OnUpdate;
    }
}