using ImGuiNET;
using ImGuiScene;
using System.Collections.Generic;
using System.Numerics;
using UIDev.Framework;
using System.Linq;

namespace UIDev
{
    internal class UITest : IPluginUIMock
    {
        private static readonly Dictionary<string, uint[]> commonLocationNameToInternalCoords = new Dictionary<string, uint[]>
        {
            { "Ul'dah - Steps of Thal - Merchant Strip", new uint[] {131, 14 } },
            { "Central Thanalan", new uint[] {141, 21} },
            { "South Shroud", new uint[] {153, 6} },
            { "The Pillars", new uint[] {419, 219} },
            { "Southern Thanalan", new uint[] {146, 23} },
            { "The Crystarium", new uint[] {819, 497} },
            { "Lily Hills Apartment Lobby", new uint[] {574, 321} },
            { "Mor Dhona", new uint[] {156, 25} },
            { "Limsa Lominsa Lower Decks", new uint[] {129, 12} },
            { "The Ruby Sea", new uint[] {613, 371} },
            { "Limsa Lominsa Upper Decks", new uint[] {128, 11} },
            { "Eulmore - The Buttress", new uint[] {820, 498} },
            { "North Shroud", new uint[] {154, 7} },
            { "The Churning Mists", new uint[] {400, 214} },
            { "Ul'dah - Steps of Thal - Hustings Strip", new uint[] {131, 14} },
            { "Eureka Anemos", new uint[] {732, 414} },
            { "Central Shroud", new uint[] {148, 4} },
            { "New Gridania", new uint[] {132, 2} },
            { "Idyllshire", new uint[] {478, 257} },
            { "The Gold Saucer", new uint[] {144, 196} },
            { "The Tempest", new uint[] {818, 496} },
            { "The Azim Steppe", new uint[] {622, 372} },
            { "The Waking Sands", new uint[] {212, 80} },
            { "The Mists", new uint[] {339, 72} },
            { "The Goblet", new uint[] {341, 83} },
            { "The Lavender Beds", new uint[] {340, 82} },
            { "Kugane", new uint[] {638, 370} },
            { "Upper La Noscea", new uint[] {139, 19} },
            { "Coerthas Western Highlands", new uint[] {397, 211} },
            { "Rhalgr's Reach", new uint[] {635, 366} },
            { "Eastern Thanalan", new uint[] {142, 22} },
            { "Matoya's Cave", new uint[] {463, 253} },
            { "The Peaks", new uint[] {620, 368} },
            { "Yanxia", new uint[] {614, 354} },
            { "The Doman Enclave", new uint[] {759, 463} },
            { "Outer La Noscea", new uint[] {180, 30} },
            { "Kholusia", new uint[] {814, 492} },
            { "The Rak'tika Greatwood", new uint[] {817, 495} },
            { "The Sea of Clouds", new uint[] {401, 215} },
            { "Amh Araeng", new uint[] {815, 493} },
            { "Coerthas Central Highlands", new uint[] {155, 53} },
            { "East Shroud", new uint[] {152, 5} },
            { "Eastern La Noscea", new uint[] {137, 17} },
            { "Middle La Noscea", new uint[] {134, 15} },
            { "Western La Noscea", new uint[] {138, 18} },
            { "Western Thanalan", new uint[] {140, 20} },
            { "Il Mheg", new uint[] {816, 494} },
            { "Azys Lla", new uint[] {402, 216} },
            { "Old Gridania", new uint[] {133, 3} },
            { "The Dravanian Forelands", new uint[] {398, 212 } }
        };
        public static void Main()
        {
            UIBootstrap.Inititalize(new UITest());
        }

        private SimpleImGuiScene? scene;

        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here

            scene.OnBuildUI += Draw;

            Visible = true;

            // saving this only so we can kill the test application by closing the window
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
            // test is just going to use flatbread
            DrawVendorLocationWindow(4696);
            //DrawMainWindow();
            DrawSettingsWindow();

            if (!Visible)
            {
                scene!.ShouldQuit = true;
            }
        }

        #region Nearly a copy/paste of PluginUI
        private bool visible = false;
        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        // this is where you'd have to start mocking objects if you really want to match
        // but for simple UI creation purposes, just hardcoding values works
        private bool showAllVendorsBool = true;

        public void DrawVendorLocationWindow(int itemId)
        {
            //get preliminary data
            GarlandToolsWrapper.Models.Data data = GarlandToolsWrapper.WebRequests.GetData();
            GarlandToolsWrapper.Models.ItemDetails itemDetails = GarlandToolsWrapper.WebRequests.GetItemDetails(itemId);

            // get vendor data
            List<ulong> vendorIds = itemDetails.item.vendors;
            List<GarlandToolsWrapper.Models.Partial> allVendors = itemDetails.partials.Where(i => vendorIds.Contains(i.obj.i)).ToList();

            // further filter vendors that don't have a location
            List<GarlandToolsWrapper.Models.Partial> vendorsWithLocation = allVendors.Where(i => i.obj.c is not null).ToList();

            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 200), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 200), new Vector2(float.MaxValue, float.MaxValue));
            //if (ImGui.Begin("Flatbread Vendors", ref visible))
            if (ImGui.Begin($"{itemDetails.item.name} Vendors", ref visible))
            {
                if (ImGui.BeginTable("Vendors", 2, ImGuiTableFlags.Resizable))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Location");
                    ImGui.TableHeadersRow();
                    foreach (GarlandToolsWrapper.Models.Partial vendor in vendorsWithLocation)
                    {
                        string vendorLocationName = data.locationIndex[vendor.obj.l.ToString()].name;
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text(vendor.obj.n);
                        ImGui.TableNextColumn();
                        if (ImGui.Button($"{vendorLocationName} ({vendor.obj.c[0]}, {vendor.obj.c[1]})"))
                        {
                            settingsVisible = true;
                        }
                    }
                }
                ImGui.EndTable();
            }
            ImGui.End();
        }

        private void buttonClick()
        {

        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 200), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 200), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("My Amazing Window", ref visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.Text($"The random config bool is {showAllVendorsBool}");

                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
            if (ImGui.Begin("Item Vendor Location Settings", ref settingsVisible,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Checkbox("Show only one vendor", ref showAllVendorsBool))
                {
                    // nothing to do in a fake ui!
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                    ImGui.TextUnformatted("If this setting is enabled, only the first vendor will be displayed.");
                    ImGui.PopTextWrapPos();
                    ImGui.EndTooltip();
                }
            }
            ImGui.End();
        }
        #endregion
    }
}