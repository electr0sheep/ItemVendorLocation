using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ItemVendorLocation
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private readonly Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool vendorLocationsVisable = false;

        private void RetrieveGarlandToolsInfo(ulong itemId)
        {
            //get preliminary data
            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(itemId);

            ItemName = itemDetails.item.name;

            // get vendor data
            List<ulong> vendorIds = itemDetails.item.vendors;
            List<GarlandToolsWrapper.Models.Partial> allVendors = itemDetails.partials.Where(i => vendorIds.Contains(i.obj.i)).ToList();

            // further filter vendors that don't have a location
            // these typically seem to be housing npcs
            List<GarlandToolsWrapper.Models.Partial> vendorsWithLocation = allVendors.Where(i => i.obj.c is not null).ToList();

            Vendors = new();
            foreach (GarlandToolsWrapper.Models.Partial vendor in vendorsWithLocation)
            {
                string vendorLocationName = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[vendor.obj.l.ToString()].name;
                uint[] internalLocationIndex = VendorPlugin.CommonLocationNameToInternalCoords[vendorLocationName];

                Vendors.Add(new Models.Vendor(
                    vendor.obj.n,
                    new MapLinkPayload(internalLocationIndex[0], internalLocationIndex[1], (float)vendor.obj.c[0], (float)vendor.obj.c[1]),
                    vendorLocationName,
                    new List<Models.Currency> { new Models.Currency("Gil", itemDetails.item.price) })
                );
            }
        }


        public ulong GarlandToolsItemId
        {
            set => RetrieveGarlandToolsInfo(value);
        }

        public bool VendorResultsVisible
        {
            get => vendorLocationsVisable;
            set => vendorLocationsVisable = value;
        }

        public bool SettingsVisible { get; set; } = false;

        public string ItemName { get; set; } = "";

        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public List<Models.Vendor> Vendors { get; set; } = new();

        public void Dispose()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawVendorLocationWindow();
        }

        public void DrawVendorLocationWindow()
        {
            if (!vendorLocationsVisable || Vendors.Count == 0)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 200), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 200), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin($"{ItemName} Vendors###Item Vendor Location", ref vendorLocationsVisable))
            {
                if (ImGui.BeginTable("Vendors", 4, ImGuiTableFlags.Resizable))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Location");
                    ImGui.TableSetupColumn("Cost");
                    ImGui.TableSetupColumn("Currency/Item");
                    ImGui.TableHeadersRow();
                    for (int i = 0; i < Vendors.Count; i++) // need i for unique maplink button ids, though I life foreach more
                    {
                        Models.Vendor vendor = Vendors[i];
                        ImGui.TableNextRow();
                        _ = ImGui.TableNextColumn();
                        ImGui.Text(vendor.name);
                        _ = ImGui.TableNextColumn();
                        if (vendor.mapLink != null && vendor.mapLink.CoordinateString != "( 0.0  , 0.0 )")
                        {
                            if (ImGui.Button($"{vendor.location} {vendor.mapLink.CoordinateString}###{vendor.name}--{i}"))
                            {
                                _ = VendorPlugin.GameGui.OpenMapWithMapLink(vendor.mapLink);
                            }
                        }
                        else if (vendor.mapLink != null)
                        {
                            if (ImGui.Button($"{vendor.location} (No Coords from Garland Tools)###{vendor.name}--{i}"))
                            {
                                _ = VendorPlugin.GameGui.OpenMapWithMapLink(vendor.mapLink);
                            }
                        }
                        else
                        {
                            ImGui.Text("No Location");
                        }
                        _ = ImGui.TableNextColumn();
                        ImGui.Text(string.Join("\n", vendor.currencies.Select(i => i.cost.ToString())));
                        _ = ImGui.TableNextColumn();
                        ImGui.Text(string.Join("\n", vendor.currencies.Select(i => i.name)));
                    }
                }
                ImGui.EndTable();
            }
            ImGui.End();
        }
    }
}
