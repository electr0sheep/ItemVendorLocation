using System;

namespace ItemVendorLocation.XIVCommon.Functions.Tooltips;

/// <summary>
/// An enum containing the strings used in action tooltips.
/// </summary>
public enum ActionTooltipString
{
    Name = 0,
    Type = 1,
    RangeLabel = 3,
    Range = 4,
    RadiusLabel = 5,
    Radius = 6,
    CostLabel = 7,
    Cost = 8,
    RecastLabel = 9,
    Recast = 10,
    CastLabel = 11,
    Cast = 12,
    Description = 13,
    Acquired = 14,
    Affinity = 15,
}

/// <summary>
/// An enum containing the fields that can be displayed in action tooltips.
/// </summary>
[Flags]
public enum ActionTooltipFields
{
    Range = 1 << 0,
    Radius = 1 << 1,
    Cost = 1 << 2,
    Recast = 1 << 3,
    Cast = 1 << 4,
    Description = 1 << 5,
    Acquired = 1 << 6,
    Affinity = 1 << 7,
    Unknown8 = 1 << 8,
}
