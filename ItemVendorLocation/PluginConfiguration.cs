using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;
using System.Numerics;

namespace ItemVendorLocation;

[Serializable]
public class PluginConfiguration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public ResultsViewType ResultsViewType { get; set; } = ResultsViewType.Multiple;
    public ushort NPCNameChatColor { get; set; } = 67;
    public bool FilterGCResults { get; set; } = false;
    public bool FilterNPCsWithNoLocation { get; set; } = false;
    public bool FilterDuplicates { get; set; } = true;
    public bool ShowShopName { get; set; } = false;
    public ushort MaxSearchResults { get; set; } = 5;
    public bool HighlightSelectedNpc { get; set; } = true;
    public ObjectHighlightColor HighlightColor { get; set; } = ObjectHighlightColor.Red;
    public bool HighlightMenuSelections { get; set; } = true;
    public Vector4 ShopHighlightColor { get; set; } = ImGuiColors.DalamudRed;
    public VirtualKey SearchDisplayModifier { get; set; } = VirtualKey.NO_KEY;
#if DEBUG
    public int BuildDebugVendorInfo { get; set; } = 0;
#endif
    public void Save()
    {
        Service.Interface.SavePluginConfig(this);
    }
}