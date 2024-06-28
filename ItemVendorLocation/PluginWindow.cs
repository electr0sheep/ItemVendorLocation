using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ItemVendorLocation.Models;
using System.Numerics;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiNotification;

namespace ItemVendorLocation;

public class PluginWindow : Window
{
    private ItemInfo _itemToDisplay;

    public PluginWindow() : base("Item Vendor Location")
    {
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new(409, 120),
            MaximumSize = new(-1, -1),
        };
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

                placeString = $"{placeString} ({location.MapX:F1}, {location.MapY:F1})";

                // need to use an ID here, the armorer/blacksmith vendors have the same location, resulting in a problem otherwise
                if (ImGui.Button($"{placeString}###{npcInfo.Id}"))
                {
                    Service.HighlightObject.SetNpcInfo(npcInfo);
                    _ = Service.GameGui.OpenMapWithMapLink(new(location.TerritoryType, location.MapId, location.MapX, location.MapY, 0f));
                }

                bool isHoveringButton = ImGui.IsItemHovered();

                if (isHoveringButton)
                {
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                    {
                        ImGui.SetClipboardText($"{_itemToDisplay.Name} -> {npcInfo.Name}@{placeString}, costs {costStr}");
                        Service.NotificationManager.AddNotification(new()
                        {
                            Content = "Copied vendor info to clipboard",
                            Title = "ItemVendorLocation",
                            Type = NotificationType.Success,
                        });
                    }
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
        ImGuiComponents.HelpMarker("You can right-click the button to copy vendor info to clipboard");

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

        if (ImGui.BeginChild("VendorListChild"))
        {
            if (ImGui.BeginTable("Vendors", columnCount, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp, new(-1, -1)))
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
                ImGui.TableSetupColumn(_itemToDisplay.Type == ItemType.CollectableExchange ? "Exchange Rate" : "Cost");

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

                ImGui.EndTable();
            }
            ImGui.EndChild();
        }
    }

    public void SetItemToDisplay(ItemInfo item)
    {
        _itemToDisplay = item;
    }
}