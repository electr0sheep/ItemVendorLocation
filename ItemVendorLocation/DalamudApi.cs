using Dalamud.ContextMenu;
using Dalamud.Data;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace ItemVendorLocation
{
    internal class DalamudApi
    {
        [PluginService] internal static DalamudPluginInterface Interface { get; private set; } = null!;

        [PluginService] internal static ChatGui ChatGui { get; private set; } = null!;

        [PluginService] internal static DataManager DataManager { get; private set; } = null!;

        [PluginService] internal static GameGui GameGui { get; private set; } = null!;

        internal static DalamudContextMenu ContextMenu { get; set; } = null!;
    }
}