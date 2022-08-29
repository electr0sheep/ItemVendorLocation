using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;
using ItemVendorLocation.Models;

namespace ItemVendorLocation
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private readonly Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool vendorLocationsVisable;

        public ItemInfo ItemToDisplay;

        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public bool VendorResultsVisible
        {
            get => vendorLocationsVisable;
            set => vendorLocationsVisable = value;
        }

        public bool SettingsVisible { get; set; } = false;

        public void Dispose()
        {
        }

        public void Draw()
        {
            DrawVendorLocationWindow();
        }

        public void DrawVendorLocationWindow()
        {
            if (!vendorLocationsVisable)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 200), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 200), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin($"{ItemToDisplay.Name} Vendors###Item Vendor Location", ref vendorLocationsVisable))
            {
                if (ImGui.BeginTable("Vendors", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp, new Vector2(-1, -1)))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Location");
                    ImGui.TableSetupColumn("Cost");
                    ImGui.TableHeadersRow();

                    var costInfo = ItemToDisplay.Costs;

                    foreach (var npcInfo in ItemToDisplay.NpcInfos)
                    {
                        ImGui.TableNextRow();
                        _ = ImGui.TableNextColumn();
                        ImGui.Text(npcInfo.Name);
                        _ = ImGui.TableNextColumn();

                        var location = npcInfo.Location;

                        if (location != null)
                        {
                            if (ImGui.Button($"{location.TerritoryExcel.PlaceName.Value.Name} ({location.MapX:F1}, {location.MapY:F1})"))
                            {
                                _ = Service.GameGui.OpenMapWithMapLink(new MapLinkPayload(location.TerritoryType, location.MapId, location.MapX, location.MapY));
                            }
                        }
                        else
                        {
                            ImGui.Text("No Location");
                        }

                        _ = ImGui.TableNextColumn();

                        var cost = costInfo.Aggregate("", (current, info) => current + $"{info.Item2} x{info.Item1}, ");
                        cost = cost[..^2];

                        ImGui.Text(cost);
                    }
                }

                ImGui.EndTable();
            }

            ImGui.End();
        }
    }
}