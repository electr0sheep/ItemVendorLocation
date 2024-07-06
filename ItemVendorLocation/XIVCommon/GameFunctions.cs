using System;
using ItemVendorLocation.XIVCommon.Functions.Tooltips;

namespace ItemVendorLocation.XIVCommon;

/// <summary>
/// A class containing game functions
/// </summary>
public class GameFunctions : IDisposable
{
    /// <summary>
    /// Tooltip events
    /// </summary>
    public Tooltips Tooltips { get; }

    internal GameFunctions()
    {
        Tooltips = new Tooltips();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Tooltips.Dispose();
    }
}
