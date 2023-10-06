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

        [PluginService] public static IChatGui ChatGui { get; set; } = null!;
        [PluginService] public static IClientState ClientState { get; set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; set; } = null!;
        [PluginService] public static IDataManager DataManager { get; set; } = null!;
        [PluginService] public static IGameGui GameGui { get; set; } = null!;
        [PluginService] public static DalamudPluginInterface Interface { get; set; } = null!;
        [PluginService] public static ISigScanner SigScanner { get; set; } = null!;
        [PluginService] public static IKeyState KeyState { get; set; } = null!;
        [PluginService] public static IPluginLog PluginLog { get; set; } = null!;
    }
}
