using System;
using System.Collections.Generic;
using Dalamud.Plugin.Ipc;

namespace ItemVendorLocation.IPC;

public class ItemVendorLocationIpc : IDisposable
{
    private readonly ICallGateProvider<uint, bool, HashSet<(uint npcId, uint territory, (float x, float y))>?> _getItemInfoProvider;
    private readonly ICallGateProvider<uint, object?> _openUiWithItemId;

    public ItemVendorLocationIpc()
    {
        _getItemInfoProvider = Service.Interface.GetIpcProvider<uint, bool, HashSet<(uint npcId, uint territory, (float x, float y))>?>("ItemVendorLocation.GetItemVendors");
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

    /// <summary>
    /// Allows other plugins to open the IVL results window for a specific item.
    /// </summary>
    /// <param name="itemId">Item ID the window will show results for.</param>
    /// <returns>null</returns>
    private object? OpenVendorResult(uint itemId)
    {
        var itemInfo = Service.Plugin.ItemLookup.GetItemInfo(itemId);
        if (itemInfo == null)
            return null;

        Service.VendorResultsUi.SetItemToDisplay(itemInfo);
        Service.VendorResultsUi.IsOpen = true;

        return null;
    }

    /// <summary>
    /// Allows other plugins to request vendor locations for items.
    /// </summary>
    /// <param name="itemId">Item ID to get vendor locations for.</param>
    /// <param name="filterNoLocation">If true, will not return vendors that don't have a location.</param>
    /// <returns>HashSet where each row contains an npc ID, the territory ID of where that npc is, and the x, y coordinates where the npc can be found.</returns>
    private static HashSet<(uint npcId, uint territory, (float x, float y))>? GetItemVendors(uint itemId, bool filterNoLocation)
    {
        var itemInfo = Service.Plugin.ItemLookup.GetItemInfo(itemId);
        if (itemInfo == null)
            return null;

        var vendors = new HashSet<(uint npcId, uint territory, (float x, float y))>();

        foreach (var npcInfo in itemInfo.NpcInfos)
        {
            if (npcInfo.Location != null)
            {
                var location = npcInfo.Location;
                vendors.Add((npcInfo.Id, location.TerritoryType, (location.MapX, location.MapX)));
            }
            else if(!filterNoLocation)
            {
                vendors.Add((npcInfo.Id, 0, (0, 0)));
            }
        }

        return vendors;
    }
}
