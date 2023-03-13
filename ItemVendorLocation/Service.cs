using Dalamud.IoC;
using Dalamud.Plugin;

namespace ItemVendorLocation
{
    internal class Service
    {
        internal static EntryPoint Plugin { get; set; } = null!;
        internal static PluginConfiguration Configuration { get; set; } = null!;

        [PluginService]
        internal static DalamudPluginInterface Interface { get; private set; } = null!;
    }
}
