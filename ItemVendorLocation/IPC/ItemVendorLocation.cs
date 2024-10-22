using System;
using System.Collections.Generic;
using Dalamud.Plugin.Ipc;

namespace ItemVendorLocation.IPC;

public class ItemVendorLocationIpc : IDisposable
{
    private readonly ICallGateProvider<uint, HashSet<(uint npcId, uint territory, (float x, float y))>?> _getItemInfoProvider;
    private readonly ICallGateProvider<uint, object?> _openUiWithItemId;

    public ItemVendorLocationIpc()
    {
        _getItemInfoProvider = Service.Interface.GetIpcProvider<uint, HashSet<(uint npcId, uint territory, (float x, float y))>?>("ItemVendorLocation.GetItemVendors");
        _openUiWithItemId = Service.Interface.GetIpcProvider<uint, object?>("ItemVendorLocation.OpenVendorResults");

        RegisterFunctions();
    }

    public void Dispose()
    {
        _getItemInfoProvider.UnregisterFunc();
        _openUiWithItemId.UnregisterFunc();
    }

    private void RegisterFunctions()
    {
        _getItemInfoProvider.RegisterFunc(GetItemVendors);
        _openUiWithItemId.RegisterFunc(OpenVendorResult);
    }

    private object? OpenVendorResult(uint itemId)
    {
        var itemInfo = Service.Plugin.ItemLookup.GetItemInfo(itemId);
        if (itemInfo == null)
            return null;

        Service.VendorResultsUi.SetItemToDisplay(itemInfo);
        Service.VendorResultsUi.IsOpen = true;

        return null;
    }

    private static HashSet<(uint npcId, uint territory, (float x, float y))>? GetItemVendors(uint itemId)
    {
        var itemInfo = Service.Plugin.ItemLookup.GetItemInfo(itemId);
        if (itemInfo == null)
            return null;

        var vendors = new HashSet<(uint npcId, uint territory, (float x, float y))>();

        foreach (var npcInfo in itemInfo.NpcInfos)
        {
            var location = npcInfo.Location;
            vendors.Add((npcInfo.Id, location.TerritoryType, (location.MapX, location.MapX)));
        }

        return vendors;
    }
}
