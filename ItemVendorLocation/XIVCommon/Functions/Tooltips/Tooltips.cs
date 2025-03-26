using System;
using Dalamud.Game.Gui;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemVendorLocation.XIVCommon.Functions.Tooltips;

// Credit for this obviously goes to Anna, who initially created XIVCommon
// Also, CriticalImpact for 7.1 updates https://github.com/Critical-Impact/CriticalCommonLib/blob/9f018daf2bf2214facc74ed94298a756b7754fa1/Services/TooltipService.cs

/// <summary>
/// The class containing tooltip functionality
/// </summary>
public class Tooltips : IDisposable
{
    private static class Signatures
    {
        internal const string AgentItemDetailUpdateTooltip = "48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 50 48 8B 42 28";
    }

    private unsafe delegate void* ItemUpdateTooltipDelegate(AtkUnitBase* agent, NumberArrayData* numberArrayData, StringArrayData* stringArrayData);

    private Hook<ItemUpdateTooltipDelegate>? ItemUpdateTooltipHook { get; }

    /// <summary>
    /// The delegate for item tooltip events.
    /// </summary>
    public delegate void ItemTooltipEventDelegate(ItemTooltip itemTooltip, ulong itemId);

    /// <summary>
    /// <para>
    /// The event that is fired when an item tooltip is being generated for display.
    /// </para>
    /// <para>
    /// Requires the <see cref="Hooks.Tooltips"/> hook to be enabled.
    /// </para>
    /// </summary>
    public event ItemTooltipEventDelegate? OnItemTooltip;


    private ItemTooltip? ItemTooltip { get; set; }

    internal Tooltips()
    {
        if (Service.SigScanner.TryScanText(Signatures.AgentItemDetailUpdateTooltip, out var updateItemPtr))
        {
            unsafe
            {
                ItemUpdateTooltipHook = Service.GameInteropProvider.HookFromAddress<ItemUpdateTooltipDelegate>(updateItemPtr, ItemUpdateTooltipDetour);
            }

            ItemUpdateTooltipHook.Enable();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ItemUpdateTooltipHook?.Dispose();
    }

    private unsafe void* ItemUpdateTooltipDetour(AtkUnitBase* agent, NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        try
        {
            ItemUpdateTooltipDetourInner(numberArrayData, stringArrayData);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Exception in item tooltip detour");
        }

        return ItemUpdateTooltipHook!.Original(agent, numberArrayData, stringArrayData);
    }

    private unsafe void ItemUpdateTooltipDetourInner(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        ItemTooltip = new ItemTooltip(stringArrayData, numberArrayData);

        try
        {
            OnItemTooltip?.Invoke(ItemTooltip, Service.GameGui.HoveredItem);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Exception in OnItemTooltip event");
        }
    }
}
