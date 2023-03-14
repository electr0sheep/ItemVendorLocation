using Dalamud.ContextMenu;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace ItemVendorLocation
{
    internal class Service
    {
        internal static EntryPoint Plugin { get; set; } = null!;
        internal static PluginWindow PluginUi { get; set; } = null!;
        internal static SettingsWindow SettingsUi { get; set; } = null!;
        internal static PluginConfiguration Configuration { get; set; } = null!;
        internal static DalamudContextMenu ContextMenu { get; set; } = null!;

        [PluginService] internal static ChatGui ChatGui { get; private set; } = null!;
        [PluginService] internal static ClientState ClientState { get; private set; } = null!;
        [PluginService] internal static DataManager DataManager { get; private set; } = null!;
        [PluginService] internal static GameGui GameGui { get; private set; } = null!;
        [PluginService] internal static DalamudPluginInterface Interface { get; private set; } = null!;
    }
}
