using System;
using Dalamud.Game.Gui;
using Dalamud.Hooking;

namespace ItemVendorLocation.XIVCommon.Functions.Tooltips;

/// <summary>
/// The class containing tooltip functionality
/// </summary>
public class Tooltips : IDisposable
{
    private static class Signatures
    {
        internal const string AgentItemDetailUpdateTooltip = "E8 ?? ?? ?? ?? 48 8B 6C 24 ?? 48 8B 74 24 ?? 4C 89 B7";
        internal const string AgentActionDetailUpdateTooltip = "E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? FF 50 40";
    }

    // Last checked: 6.0
    // E8 ?? ?? ?? ?? EB 68 FF 50 40
    private const int AgentActionDetailUpdateFlagOffset = 0x58;

    private unsafe delegate ulong ItemUpdateTooltipDelegate(IntPtr agent, int** numberArrayData, byte*** stringArrayData, float a4);

    private unsafe delegate void ActionUpdateTooltipDelegate(IntPtr agent, int** numberArrayData, byte*** stringArrayData);

    private Hook<ItemUpdateTooltipDelegate>? ItemUpdateTooltipHook { get; }
    private Hook<ActionUpdateTooltipDelegate>? ActionGenerateTooltipHook { get; }

    /// <summary>
    /// The delegate for item tooltip events.
    /// </summary>
    public delegate void ItemTooltipEventDelegate(ItemTooltip itemTooltip, ulong itemId);

    /// <summary>
    /// The tooltip for action tooltip events.
    /// </summary>
    public delegate void ActionTooltipEventDelegate(ActionTooltip actionTooltip, HoveredAction action);

    /// <summary>
    /// <para>
    /// The event that is fired when an item tooltip is being generated for display.
    /// </para>
    /// <para>
    /// Requires the <see cref="Hooks.Tooltips"/> hook to be enabled.
    /// </para>
    /// </summary>
    public event ItemTooltipEventDelegate? OnItemTooltip;

    /// <summary>
    /// <para>
    /// The event that is fired when an action tooltip is being generated for display.
    /// </para>
    /// <para>
    /// Requires the <see cref="Hooks.Tooltips"/> hook to be enabled.
    /// </para>
    /// </summary>
    public event ActionTooltipEventDelegate? OnActionTooltip;

    private ItemTooltip? ItemTooltip { get; set; }
    private ActionTooltip? ActionTooltip { get; set; }

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

        if (Service.SigScanner.TryScanText(Signatures.AgentActionDetailUpdateTooltip, out var updateActionPtr))
        {
            unsafe
            {
                ActionGenerateTooltipHook = Service.GameInteropProvider.HookFromAddress<ActionUpdateTooltipDelegate>(updateActionPtr, ActionUpdateTooltipDetour);
            }

            ActionGenerateTooltipHook.Enable();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ActionGenerateTooltipHook?.Dispose();
        ItemUpdateTooltipHook?.Dispose();
    }

    private unsafe ulong ItemUpdateTooltipDetour(IntPtr agent, int** numberArrayData, byte*** stringArrayData, float a4)
    {
        var ret = ItemUpdateTooltipHook!.Original(agent, numberArrayData, stringArrayData, a4);

        if (ret > 0)
        {
            try
            {
                ItemUpdateTooltipDetourInner(numberArrayData, stringArrayData);
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error(ex, "Exception in item tooltip detour");
            }
        }

        return ret;
    }

    private unsafe void ItemUpdateTooltipDetourInner(int** numberArrayData, byte*** stringArrayData)
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

    private unsafe void ActionUpdateTooltipDetour(IntPtr agent, int** numberArrayData, byte*** stringArrayData)
    {
        var flag = *(byte*)(agent + AgentActionDetailUpdateFlagOffset);
        ActionGenerateTooltipHook!.Original(agent, numberArrayData, stringArrayData);

        if (flag == 0)
        {
            return;
        }

        try
        {
            ActionUpdateTooltipDetourInner(numberArrayData, stringArrayData);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Exception in action tooltip detour");
        }
    }

    private unsafe void ActionUpdateTooltipDetourInner(int** numberArrayData, byte*** stringArrayData)
    {
        ActionTooltip = new ActionTooltip(stringArrayData, numberArrayData);

        try
        {
            OnActionTooltip?.Invoke(ActionTooltip, Service.GameGui.HoveredAction);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Exception in OnActionTooltip event");
        }
    }
}
