using ImGuiScene;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace UIDev
{
    internal class UITest : IPluginUIMock
    {
        public static void Main()
        {
            UIBootstrap.Inititalize(new UITest());
        }

        private SimpleImGuiScene? scene;

        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here

            // eg, to load an image resource for use with ImGui 
            scene.OnBuildUI += Draw;

            MainWindowVisible = true;
            ResultsWindowVisible = false;

            // saving thi22 only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            this.scene = scene;
        }

        public void Dispose()
        {
        }

        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
        // don't want to imply that is easy or the best way to go usually, so it's not done here either
        private void Draw()
        {
            DrawMainWindow();
            DrawResultsWindow();

            if (!MainWindowVisible)
            {
                scene!.ShouldQuit = true;
            }
        }

        #region Nearly a copy/paste of PluginUI
        private bool mainWindowVisible = false;
        private bool resultsWindowVisible = false;
        private string itemName = "Versatile Lure";
        private List<ItemVendorLocation.Models.Vendor>? vendorResults = null;
        public bool MainWindowVisible
        {
            get => mainWindowVisible;
            set => mainWindowVisible = value;
        }

        public bool ResultsWindowVisible
        {
            get => resultsWindowVisible;
            set => resultsWindowVisible = value;
        }

        // this is where you'd have to start mocking objects if you really want to match
        // but for simple UI creation purposes, just hardcoding values works        
        public void DrawMainWindow()
        {
            if (!MainWindowVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Test", ref mainWindowVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse))
            {
                _ = ImGui.InputText("Item Name", ref itemName, 50);
                if (ImGui.Button("Search for Item"))
                {
                    List<GarlandToolsWrapper.Models.ItemSearchResult> items = GarlandToolsWrapper.WebRequests.ItemSearch(itemName);

                    GarlandToolsWrapper.Models.ItemSearchResult item = items.Find(i => string.Equals(i.obj.n, itemName, StringComparison.OrdinalIgnoreCase))!;

                    vendorResults = new();
                    vendorResults = GetVendors((ulong)item.obj.i);
                    
                    ResultsWindowVisible = true;
                }

                ImGui.End();
            }
        }

        public void DrawResultsWindow()
        {
            if (!ResultsWindowVisible || vendorResults == null)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 200), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 200), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin($"{itemName} Vendors", ref resultsWindowVisible))
            {
                if (ImGui.BeginTable("Vendors", 4, ImGuiTableFlags.Resizable))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Location");
                    ImGui.TableSetupColumn("Cost");
                    ImGui.TableSetupColumn("Currency/Item");
                    ImGui.TableHeadersRow();
                    foreach (ItemVendorLocation.Models.Vendor vendor in vendorResults)
                    {
                        ImGui.TableNextRow();
                        _ = ImGui.TableNextColumn();
                        ImGui.Text(vendor.name);
                        _ = ImGui.TableNextColumn();
                        ImGui.Text(vendor.location);
                        _ = ImGui.TableNextColumn();
                        ImGui.Text(vendor.cost.ToString());
                        _ = ImGui.TableNextColumn();
                        ImGui.Text(vendor.currency);
                    }
                }
                ImGui.EndTable();
            }
            ImGui.End();
        }

        public static List<ItemVendorLocation.Models.Vendor> GetVendors(ulong itemId)
        {
            //get preliminary data
            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(itemId);
            List<ItemVendorLocation.Models.Vendor> vendorResults = new();

            // gil vendor
            if (itemDetails.item.vendors != null)
            {
                foreach (ulong vendorId in itemDetails.item.vendors)
                {
                    GarlandToolsWrapper.Models.Partial? vendor = itemDetails.partials.Find(i => (ulong)i.obj.i == vendorId);

                    if (vendor != null)
                    {
                        // get rid of any vendor that doesn't have a location
                        // typically man/maid servants
                        string name = vendor.obj.n;
                        ulong cost = itemDetails.item.price;
                        string currency = "Gil";
                        if (vendor.obj.l == null)
                        {
                            vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, "No Location", cost, currency));
                            break;
                        }

                        string location = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[vendor.obj.l.ToString()].name;
                        uint[] internalLocationIndex = ItemVendorLocation.VendorPlugin.CommonLocationNameToInternalCoords[location];

                        vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, location, cost, currency));
                    }
                }
            }
            // special currency vendor
            else if (itemDetails.item.tradeShops != null)
            {
                List<GarlandToolsWrapper.Models.TradeShop> tradeShops = itemDetails.item.tradeShops;

                foreach (GarlandToolsWrapper.Models.TradeShop tradeShop in tradeShops)
                {
                    if (tradeShop.npcs.Count > 0)
                    {
                        foreach (ulong npcId in tradeShop.npcs)
                        {
                            GarlandToolsWrapper.Models.Partial? tradeShopNpc = itemDetails.partials.Find(i => (ulong)i.obj.i == npcId);
                            if (tradeShopNpc != null)
                            {
                                string name = tradeShopNpc.obj.n;
                                ulong cost = tradeShop.listings[0].currency[0].amount;
                                string currency = itemDetails.partials.Find(i => i.id == tradeShop.listings[0].currency[0].id && i.type == "item")!.obj.n;

                                if (tradeShopNpc.obj.l == null)
                                {
                                    vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, "No Location", cost, currency));
                                    break;
                                }

                                string location = GarlandToolsWrapper.WebRequests.DataObject.locationIndex[tradeShopNpc.obj.l.ToString()].name;
                                uint[] internalLocationIndex = ItemVendorLocation.VendorPlugin.CommonLocationNameToInternalCoords[location];

                                vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, location, cost, currency));
                            }
                        }
                    }
                    else
                    {
                        string name = tradeShop.shop;
                        ulong cost = tradeShop.listings[0].currency[0].amount;
                        string currency = itemDetails.partials.Find(i => i.id == tradeShop.listings[0].currency[0].id && i.type == "item")!.obj.n;

                        vendorResults.Add(new ItemVendorLocation.Models.Vendor(name, null!, "Unknown", cost, currency));
                    }
                }
            }

            return vendorResults;
        }



        #endregion
    }
}
