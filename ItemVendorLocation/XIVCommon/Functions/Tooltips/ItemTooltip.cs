using Dalamud.Game.Text.SeStringHandling;

namespace ItemVendorLocation.XIVCommon.Functions.Tooltips;

/// <summary>
/// The class allowing for item tooltip manipulation
/// </summary>
public unsafe class ItemTooltip : BaseTooltip
{
    internal ItemTooltip(byte*** stringArrayData, int** numberArrayData) : base(stringArrayData, numberArrayData)
    {
    }

    /// <summary>
    /// Gets or sets the SeString for the given string enum.
    /// </summary>
    /// <param name="its">the string to retrieve/update</param>
    public SeString this[ItemTooltipString its]
    {
        get => this[(int)its];
        set => this[(int)its] = value;
    }

    /// <summary>
    /// Gets or sets which fields are visible on the tooltip.
    /// </summary>
    public ItemTooltipFields Fields
    {
        get => (ItemTooltipFields)(*(*(NumberArrayData + 4) + 4));
        set => *(*(NumberArrayData + 4) + 4) = (int)value;
    }
}
