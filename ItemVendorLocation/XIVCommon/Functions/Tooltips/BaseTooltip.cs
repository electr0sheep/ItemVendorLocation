using System;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
using XIVCommon;

namespace ItemVendorLocation.XIVCommon.Functions.Tooltips;

/// <summary>
/// The base class for tooltips
/// </summary>
public abstract unsafe class BaseTooltip
{
    /// <summary>
    /// A pointer to the StringArrayData class for this tooltip.
    /// </summary>
    private readonly StringArrayData* _stringArrayData; // this is StringArrayData* when ClientStructs is updated

    /// <summary>
    /// A pointer to the NumberArrayData class for this tooltip.
    /// </summary>
    protected readonly NumberArrayData* NumberArrayData;

    internal BaseTooltip(StringArrayData* stringArrayData, NumberArrayData* numberArrayData)
    {
        _stringArrayData = stringArrayData;
        NumberArrayData = numberArrayData;
    }

    /// <summary>
    /// <para>
    /// Gets the SeString at the given index for this tooltip.
    /// </para>
    /// <para>
    /// Implementors should provide an enum accessor for this.
    /// </para>
    /// </summary>
    /// <param name="index">string index to retrieve</param>
    protected SeString this[int index]
    {
        get
        {
            var stringAddress = new IntPtr(_stringArrayData->StringArray[index]);
            return Util.ReadSeString(stringAddress);
        }
        set
        {
            var encoded = value.Encode().ToList();
            encoded.Add(0);
            _stringArrayData->SetValue(index, encoded.ToArray(), false, true, false);
        }
    }
}
