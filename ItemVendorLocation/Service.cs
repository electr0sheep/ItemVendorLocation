using Dalamud.ContextMenu;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ItemVendorLocation
{
    internal class Service
    {
        internal static EntryPoint Plugin { get; set; } = null!;
        internal static PluginWindow PluginUi { get; set; } = null!;
        internal static SettingsWindow SettingsUi { get; set; } = null!;
        internal static PluginConfiguration Configuration { get; set; } = null!;
        internal static DalamudContextMenu ContextMenu { get; set; } = null!;

        [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
        [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
        [PluginService] internal static DalamudPluginInterface Interface { get; private set; } = null!;
        [PluginService] internal static SigScanner SigScanner { get; private set; } = null!;
        [PluginService] internal static IKeyState KeyState { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; set; } = null!;
    }
}
