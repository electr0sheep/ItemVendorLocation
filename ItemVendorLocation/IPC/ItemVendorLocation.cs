using System;
using System.Collections.Generic;
using Dalamud.Plugin.Ipc;

namespace ItemVendorLocation.IPC;

public class ItemVendorLocationIpc : IDisposable
{
    private readonly ICallGateProvider<uint, bool, HashSet<(uint npcId, uint territory, (float x, float y))>?> _getItemInfoProvider;
    private readonly ICallGateProvider<uint, object?> _openUiWithItemId;
    private readonly ICallGateProvider<uint, (uint territory, (float x, float y))?> _getVendorLocation;
    //private readonly ICallGateProvider<uint, List<uint>?> _getVendorItems;

    public ItemVendorLocationIpc()
    {
        _getItemInfoProvider = Service.Interface.GetIpcProvider<uint, bool, HashSet<(uint npcId, uint territory, (float x, float y))>?>("ItemVendorLocation.GetItemVendors");
        _openUiWithItemId = Service.Interface.GetIpcProvider<uint, object?>("ItemVendorLocation.OpenVendorResults");
        _getVendorLocation = Service.Interface.GetIpcProvider<uint, (uint territory, (float x, float y))?>("ItemVendorLocation.GetVendorLocation");
        //_getVendorItems = Service.Interface.GetIpcProvider<uint, List<uint>?>("ItemVendorLocation.GetVendorItems");

        RegisterFunctions();
    }

    public void Dispose()
    {
        _getItemInfoProvider.UnregisterFunc();
        _openUiWithItemId.UnregisterFunc();
        _getVendorLocation.UnregisterFunc();
        //_getVendorItems.UnregisterFunc();
    }

    private void RegisterFunctions()
    {
        _getItemInfoProvider.RegisterFunc(GetItemVendors);
        _openUiWithItemId.RegisterFunc(OpenVendorResult);
        _getVendorLocation.RegisterFunc(GetVendorLocation);
        //_getVendorItems.RegisterFunc(GetVendorItems);
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
    /// Allows other plugins to request vendor locations for an item.
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

    /// <summary>
    /// Allows other plugins to get the location of an npc.
    /// </summary>
    /// <param name="npcId">npc ID to get a location for.</param>
    /// <returns>The territory and x, y  coordinates of the NPC if location exists. Null otherwise.</returns>
    private static (uint territory, (float x, float y))? GetVendorLocation(uint npcId)
    {
        var npcLocation = Service.Plugin.ItemLookup.GetNpcLocation(npcId);
        if (npcLocation == null)
            return null;

        return (npcLocation.TerritoryType, (npcLocation.MapX, npcLocation.MapY));
    }

    //private static List<uint>? GetVendorItems(uint npcId)
    //{
    //    var npcInfo = Service.Plugin.ItemLookup.GetVendorInfo(npcId);
    //    if (npcInfo == null)
    //        return null;

    //    // TODO: We don't have an easy way to take an npcId and get a list of items
    //}
}
