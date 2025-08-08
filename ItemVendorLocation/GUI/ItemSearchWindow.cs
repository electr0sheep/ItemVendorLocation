using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ItemVendorLocation.GUI;
public class ItemSearchWindow : Window
{
    private string searchName = "";
    private int selectedItem;

    public ItemSearchWindow() : base("Item Vendor Search")
    {
        RespectCloseHotkey = true;

        SizeCondition = ImGuiCond.FirstUseEver;
        Size = new Vector2(740, 400);
    }

    public override void Draw()
    {
        var filteredItems = Service.Plugin.ItemLookup.GetItems().Where(i => i.Value.Name.Contains(searchName, StringComparison.CurrentCultureIgnoreCase));
        ImGui.Text("Search:");
        ImGui.SameLine();
        _ = ImGui.InputText("##ItemNameSearchFilter", ref searchName, 60);
        if (ImGui.ListBox("##ItemSearchList", ref selectedItem, filteredItems.Select(i => i.Value.Name).ToArray(), filteredItems.ToArray().Length))
        {
            var item = filteredItems.ElementAt(selectedItem).Value;
            Service.VendorResultsUi.SetItemToDisplay(item);
            Service.VendorResultsUi.IsOpen = true;
            Service.VendorResultsUi.Collapsed = false;
            Service.VendorResultsUi.CollapsedCondition = ImGuiCond.Once;
        }
    }
}
