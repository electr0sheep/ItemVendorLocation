using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface.Components;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using System.Collections.Generic;

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
            string[] dataSourceNames = Enum.GetNames<DataSource>();
            DataSource[] dataSourceValues = Enum.GetValues<DataSource>();
            int selectedDataSource = Array.IndexOf(dataSourceValues, Service.Configuration.DataSource);
            ImGui.SetNextItemWidth(200f);
            if (ImGui.Combo("Data Source", ref selectedDataSource, dataSourceNames, dataSourceNames.Length))
            {
                Service.Configuration.DataSource = dataSourceValues[selectedDataSource];
                Service.Configuration.Save();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(@"The data source the plugin uses to find out where vendors are located.

GarlandTools is the original data source. If you leave it as this the plugin will function as it did before
with no changes.

Internal means the plugin will not make network requests, which means results appear MUCH faster.

NOTE: If you are using the Internal option and notice that an item doesn't have results, but there are
results using GarlandTools as the data source, please let me know by either leaving feedback, or
creating a GitHub issue. Be sure to let me know what the item is. I would like to eventually deprecate
GarlandTools, but I want to give an option in the meantime.
");

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
            ImGuiComponents.HelpMarker(@"The text color to use to highlight the name of the NPC.

Only has an effect if Results View Type is set to Single");
        }
    }
}