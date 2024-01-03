using System.Linq;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ItemVendorLocation.Models;
using System.Numerics;

namespace ItemVendorLocation;

public class PluginWindow : Window
{
    private ItemInfo _itemToDisplay;

    public PluginWindow() : base("Item Vendor Location")
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    private void DrawTableRow(NpcInfo npcInfo, string shopName, NpcLocation location, string costStr)
    {
        ImGui.TableNextRow();
        _ = ImGui.TableNextColumn();
#if DEBUG
        ImGui.Text(npcInfo.Id.ToString());
        _ = ImGui.TableNextColumn();
#endif
        ImGui.Text(npcInfo.Name);
        _ = ImGui.TableNextColumn();
        if (Service.Configuration.ShowShopName && _itemToDisplay.HasShopNames())
        {
            ImGui.Text(shopName ?? "");
            _ = ImGui.TableNextColumn();
        }
        if (location != null)
        {
            if (location.TerritoryType == 282)
            {
                ImGui.Text("Player Housing");
            }
            else
            {
                // The <i>Endeavor</i> fix
                string placeString = location.TerritoryExcel.PlaceName.Value.Name;
                placeString = placeString.Replace("\u0002", "");
                placeString = placeString.Replace("\u001a", "");
                placeString = placeString.Replace("\u0003", "");
                placeString = placeString.Replace("\u0001", "");

                // need to use an ID here, the armorer/blacksmith vendors have the same location, resulting in a problem otherwise
                if (ImGui.Button($"{placeString} ({location.MapX:F1}, {location.MapY:F1})###{npcInfo.Id}"))
                {
                    _ = Service.GameGui.OpenMapWithMapLink(new MapLinkPayload(location.TerritoryType, location.MapId, location.MapX, location.MapY, 0f));
                }
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
        var columnCount = 3;
#if DEBUG
        columnCount++;
#endif
        if (_itemToDisplay.Type == ItemType.Achievement)
        {
            columnCount++;
        }
        if (Service.Configuration.ShowShopName && _itemToDisplay.HasShopNames())
        {
            columnCount++;
        }
        if (ImGui.BeginTable("Vendors", columnCount, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp, new Vector2(-1, -1)))
        {
#if DEBUG
            ImGui.TableSetupColumn("NPC ID");
#endif
            ImGui.TableSetupColumn("NPC Name");
            if (Service.Configuration.ShowShopName && _itemToDisplay.HasShopNames())
            {
                ImGui.TableSetupColumn("Shop Name");
            }
            ImGui.TableSetupColumn("Location");
            if (_itemToDisplay.Type == ItemType.CollectableExchange)
            {
                ImGui.TableSetupColumn("Exchange Rate");
            }
            else
            {
                ImGui.TableSetupColumn("Cost");
            }
            if (_itemToDisplay.Type == ItemType.Achievement)
            {
                ImGui.TableSetupColumn("Obtain Requirement");
            }

            ImGui.TableHeadersRow();

            foreach (var npcInfo in _itemToDisplay.NpcInfos)
            {
                string costStr;
                if (_itemToDisplay.Type == ItemType.CollectableExchange)
                {
                    costStr = npcInfo.Costs.Aggregate("", (current, cost) => current + $"{cost.Item2} will yield {cost.Item1}\n");
                }
                else
                {
                    costStr = npcInfo.Costs.Aggregate("", (current, cost) => current + $"{cost.Item2} x{cost.Item1}, ");
                    costStr = costStr.Length > 0 ? costStr[..^2] : "";
                }

                DrawTableRow(npcInfo, npcInfo.ShopName, npcInfo.Location, costStr);
            }
        }

        ImGui.EndTable();
    }

    public void SetItemToDisplay(ItemInfo item)
    {
        _itemToDisplay = item;
    }

}