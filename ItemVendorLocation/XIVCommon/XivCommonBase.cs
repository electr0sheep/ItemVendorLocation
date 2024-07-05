using System;

namespace ItemVendorLocation.XIVCommon;

/// <summary>
/// A base class for accessing XivCommon functionality.
/// </summary>
public class XivCommonBase : IDisposable
{
    /// <summary>
    /// Game functions and events
    /// </summary>
    public GameFunctions Functions { get; }

    /// <summary>
    /// <para>
    /// Construct a new XivCommon base.
    /// </para>
    /// <para>
    /// This will automatically enable hooks based on the hooks parameter.
    /// </para>
    /// </summary>
    /// <param name="hooks">Flags indicating which hooks to enable</param>
    public XivCommonBase()
    {
        Functions = new GameFunctions();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Functions.Dispose();
    }
}