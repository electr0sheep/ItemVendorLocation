using System.Linq;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ItemVendorLocation.Models;
using System.Numerics;

namespace ItemVendorLocation
{
    public class PluginWindow : Window
    {
        private ItemInfo _itemToDisplay;

        public PluginWindow() : base("Item Vendor Location")
        {
            Flags = ImGuiWindowFlags.AlwaysAutoResize;
        }

        private void DrawTable(string npcName, NpcLocation location, string costStr)
        {
            ImGui.TableNextRow();
            _ = ImGui.TableNextColumn();
            ImGui.Text(npcName);
            _ = ImGui.TableNextColumn();
            if (location != null)
            {
                if (ImGui.Button($"{location.TerritoryExcel.PlaceName.Value.Name} ({location.MapX:F1}, {location.MapY:F1})"))
                {
                    _ = Service.GameGui.OpenMapWithMapLink(new MapLinkPayload(location.TerritoryType, location.MapId, location.MapX, location.MapY));
                }
            }
            else
            {
                ImGui.Text("No location");
            }

            _ = ImGui.TableNextColumn();

            ImGui.Text(costStr);

            if (_itemToDisplay.Type == ItemType.Achievement)
            {
                _ = ImGui.TableNextColumn();
                ImGui.Text(_itemToDisplay.AchievementDescription);
            }
        }

        public override void PreOpenCheck()
        {
            if (_itemToDisplay != null)
            {
                return;
            }

            IsOpen = false;
        }

        public override void Draw()
        {
            ImGui.Text($"{_itemToDisplay.Name} Vendor list:");
            if (ImGui.BeginTable("Vendors", _itemToDisplay.Type == ItemType.Achievement ? 4 : 3, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp, new Vector2(-1, -1)))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Location");
                ImGui.TableSetupColumn("Cost");
                if (_itemToDisplay.Type == ItemType.Achievement)
                {
                    ImGui.TableSetupColumn("Obtain Requirement");
                }

                ImGui.TableHeadersRow();

                foreach (NpcInfo npcInfo in _itemToDisplay.NpcInfos)
                {
                    string costStr = npcInfo.Costs.Aggregate("", (current, cost) => current + $"{cost.Item2} x{cost.Item1}, ");
                    costStr = costStr[..^2];

                    DrawTable(npcInfo.Name, npcInfo.Location, costStr);
                }
            }

            ImGui.EndTable();
        }

        public void SetItemToDisplay(ItemInfo item)
        {
            _itemToDisplay = item;
        }

    }
}