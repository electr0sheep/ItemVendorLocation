using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface.Components;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Keys;

namespace ItemVendorLocation
{
    public class SettingsWindow : Window
    {
        public SettingsWindow() : base("Item Vendor Location Settings")
        {
            RespectCloseHotkey = true;

            SizeCondition = ImGuiCond.FirstUseEver;
            Size = new Vector2(740, 200);
        }

        public override void Draw()
        {
#if DEBUG
            ImGui.SetNextItemWidth(200f);
            int num = Service.Configuration.BuildDebugVendorInfo;
            if (ImGui.InputInt("NPC ID", ref num))
            {
                Service.Configuration.BuildDebugVendorInfo = num;
                Service.Configuration.Save();
            }
            if (ImGui.Button("Build Debug Vendor Info"))
            {
                Service.Plugin._itemLookup.BuildDebugVendorInfo((uint)num);
            }
            ImGui.SameLine();
            if (ImGui.Button("Build NPC location"))
            {
                Service.Plugin._itemLookup.BuildDebugNpcLocation((uint)num);
            }
#endif
            bool filterDuplicates = Service.Configuration.FilterDuplicates;
            if (ImGui.Checkbox("Filter Duplicates", ref filterDuplicates))
            {
                Service.Configuration.FilterDuplicates = filterDuplicates;
                Service.Configuration.Save();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"If checked, will filter duplicate vendors by location");

            bool filterGCResults = Service.Configuration.FilterGCResults;
            if (ImGui.Checkbox("Filter GC Results", ref filterGCResults))
            {
                Service.Configuration.FilterGCResults = filterGCResults;
                Service.Configuration.Save();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"If checked, will only show your own GC vendor");

            bool filterNPCsWithNoLocation = Service.Configuration.FilterNPCsWithNoLocation;
            if (ImGui.Checkbox("Filter Results With No Location", ref filterNPCsWithNoLocation))
            {
                Service.Configuration.FilterNPCsWithNoLocation = filterNPCsWithNoLocation;
                Service.Configuration.Save();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"If checked, will only show npcs with a location");

            bool showShopName = Service.Configuration.ShowShopName;
            if (ImGui.Checkbox("Show Shop Info", ref showShopName))
            {
                Service.Configuration.ShowShopName = showShopName;
                Service.Configuration.Save();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"If checked, will show shop name info e.g. 'Purchase Disciple of Magic Gear - Purchase Gear (Lv. 20-29)'");

            ImGui.SetNextItemWidth(200f);
            int maxSearchResults = Service.Configuration.MaxSearchResults;
            if (ImGui.InputInt("Max Search Results", ref maxSearchResults))
            {
                if (maxSearchResults is <= 50 and >= 1)
                {
                    Service.Configuration.MaxSearchResults = (ushort)maxSearchResults;
                    Service.Configuration.Save();
                }
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"The max number of search results when using the text command to prevent chat spam.

Max allowable is 50.");

            string[] resultsViewTypeNames = Enum.GetNames<ResultsViewType>();
            ResultsViewType[] resultsViewTypeValues = Enum.GetValues<ResultsViewType>();
            int selectedResultsViewType = Array.IndexOf(resultsViewTypeValues, Service.Configuration.ResultsViewType);
            ImGui.SetNextItemWidth(200f);
            if (ImGui.Combo("Results View Type", ref selectedResultsViewType, resultsViewTypeNames, resultsViewTypeNames.Length))
            {
                Service.Configuration.ResultsViewType = resultsViewTypeValues[selectedResultsViewType];
                Service.Configuration.Save();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"How the plugin displays vendor location results.

Single will pick the first result and print it to your chat window.

Multiple will display the results in a popup window. If you leave it as this the plugin will function as it did before with no changes.");

            List<UIColor> uiColors = Service.DataManager.GetExcelSheet<UIColor>().DistinctBy(i => i.UIForeground).ToList();
            int npcNameChatColor = Service.Configuration.NPCNameChatColor;
            ImGui.SetNextItemWidth(200f);
            // my lame way to allow selection of colors as defined in the UIColor sheet
            if (ImGui.BeginCombo("NPC Name Text Color", ""))
            {
                foreach (UIColor color in uiColors)
                {
                    bool isChecked = Service.Configuration.NPCNameChatColor == color.RowId;
                    Vector4 reversedColors = ImGui.ColorConvertU32ToFloat4(color.UIForeground);
                    // Seems like the above function reverses the order of the bytes
                    // There's got to be a better way to do this, but brain no working :P
                    Vector4 correctColors = new()
                    {
                        X = reversedColors.W,
                        Y = reversedColors.Z,
                        Z = reversedColors.Y,
                        W = reversedColors.X,
                    };
                    if (ImGui.Checkbox($"###{color.RowId}", ref isChecked))
                    {
                        Service.Configuration.NPCNameChatColor = (ushort)uiColors.Find(i => i.UIForeground == ImGui.ColorConvertFloat4ToU32(reversedColors)).RowId;
                        Service.Configuration.Save();
                    }
                    ImGui.SameLine();
                    _ = ImGui.ColorEdit4($"", ref correctColors, ImGuiColorEditFlags.None | ImGuiColorEditFlags.NoInputs);
                }
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"The chat text highlight color of the NPC name.");

            string[] keyNames = Service.KeyState.GetValidVirtualKeys().Select(i => i.GetFancyName()).ToArray();
            keyNames = keyNames.Prepend("None").ToArray();
            VirtualKey[] keyValues = Service.KeyState.GetValidVirtualKeys();
            keyValues = keyValues.Prepend(VirtualKey.NO_KEY).ToArray();
            int selectedKey = Array.IndexOf(keyValues, Service.Configuration.SearchDisplayModifier);
            ImGui.SetNextItemWidth(200f);
            if (ImGui.Combo("Results View Type Modifier", ref selectedKey, keyNames, keyNames.Length))
            {
                Service.Configuration.SearchDisplayModifier = keyValues[selectedKey];
                Service.Configuration.Save();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"Changes the Results View Type when held.");
        }
    }
}