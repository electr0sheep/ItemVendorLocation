using Dalamud.Data;
using Dalamud.Game.Gui;
using Dalamud.IoC;

namespace ItemVendorLocation;

internal class Service
{
    [PluginService] internal static DataManager DataManager { get; private set; } = null!;
    [PluginService] internal static ChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static GameGui GameGui { get; private set; } = null!;
}