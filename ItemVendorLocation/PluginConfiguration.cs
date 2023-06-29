using Dalamud.Configuration;
using System;

namespace ItemVendorLocation
{
    [Serializable]
    public class PluginConfiguration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public string CommandName = "/xlvendor";
        public ResultsViewType ResultsViewType { get; set; } = ResultsViewType.Multiple;
        public ushort NPCNameChatColor { get; set; } = 67;
        public bool FilterGCResults { get; set; } = false;
        public bool FilterNPCsWithNoLocation { get; set; } = false;
        public bool FilterDuplicates { get; set; } = true;
        public bool ShowShopName { get; set; } = false;
        public ushort MaxSearchResults { get; set; } = 5;
#if DEBUG
        public int BuildDebugVendorInfo { get; set; } = 0;
#endif
        public void Save()
        {
            Service.Interface.SavePluginConfig(this);
        }
    }
}
