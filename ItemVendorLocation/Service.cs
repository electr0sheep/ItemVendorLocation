using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ItemVendorLocation.GUI;

namespace ItemVendorLocation;

internal class Service
{
    internal static EntryPoint Plugin { get; set; } = null!;
    internal static VendorResultsWindow VendorResultsUi { get; set; } = null!;
    internal static SettingsWindow SettingsUi { get; set; } = null!;
    internal static ItemSearchWindow ItemSearchUi { get; set; } = null!;
    internal static PluginConfiguration Configuration { get; set; } = null!;
    internal static Ipc Ipc { get; set; } = null!;
    internal static HighlightObject HighlightObject { get; set; } = null!;


    [PluginService] public static IChatGui ChatGui { get; set; } = null!;
    [PluginService] public static IClientState ClientState { get; set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] public static IDataManager DataManager { get; set; } = null!;
    [PluginService] public static IGameGui GameGui { get; set; } = null!;
    [PluginService] public static IDalamudPluginInterface Interface { get; set; } = null!;
    [PluginService] public static IKeyState KeyState { get; set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; set; } = null!;
    [PluginService] public static IContextMenu ContextMenu { get; set; } = null!;
    [PluginService] public static INotificationManager NotificationManager { get; set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
}
