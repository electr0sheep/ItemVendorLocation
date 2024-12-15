using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemVendorLocation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ItemVendorLocation;
internal class HighlightMenus : IDisposable
{
    private NpcInfo[] _npcInfo = [];
    private string _itemName = string.Empty;

    public HighlightMenus()
    {
        Service.Framework.Update += Framework_OnUpdate;
    }

    private unsafe void Framework_OnUpdate(IFramework framework)
    {
        if (!Service.Configuration.HighlightMenuSelections || _npcInfo == null)
        {
            return;
        }

        HighlightShopAddon();
        HighlightSelectIconStringAddon();
        HighlightSelectStringAddon();
        HighlightInclusionShopAddon();
        HighlightShopExchangeCurrencyAddon();
        HighlightShopExchangeItem();
    }

    private unsafe void HighlightShopAddon()
    {
        var shopAddonPtr = Service.GameGui.GetAddonByName("Shop");
        if (shopAddonPtr == nint.Zero)
        {
            return;
        }

        var shopAddon = (AtkUnitBase*)shopAddonPtr;

        var itemList = (AtkComponentList*)shopAddon->GetComponentByNodeId(16);

        foreach (uint index in Enumerable.Range(0, itemList->ListLength))
        {
            var listItemRenderer = itemList->ItemRendererList[index].AtkComponentListItemRenderer;

            if (listItemRenderer == null)
            {
                continue;
            }
            var text = (AtkTextNode*)listItemRenderer->GetTextNodeById(3);
            if (text == null)
            {
                continue;
            }
            var itemName = SeString.Parse(text->GetText()).TextValue;
            if (itemName == _itemName)
            {
                text->TextColor = Dalamud.Utility.Numerics.VectorExtensions.ToByteColor(Service.Configuration.ShopHighlightColor);
                // strangely, it doesn't seem like the list gets its color updated until we set the text below
                text->SetText(SeString.Parse(text->GetText()).TextValue);
            }
        }
    }

    private unsafe void HighlightSelectIconStringAddon()
    {
        var selectIconStringAddonPtr = Service.GameGui.GetAddonByName("SelectIconString");

        if (selectIconStringAddonPtr == nint.Zero)
        {
            return;
        }

        var selectIconStringAddon = (AtkUnitBase*)selectIconStringAddonPtr;

        var componentList = selectIconStringAddon->GetComponentListById(3);

        if (componentList == null)
        {
            return;
        }

        foreach (uint index in Enumerable.Range(0, componentList->ListLength))
        {
            var listItemRenderer = componentList->ItemRendererList[index].AtkComponentListItemRenderer;
            if (listItemRenderer == null)
            {
                continue;
            }
            var text = (AtkTextNode*)listItemRenderer->GetTextNodeById(2);
            if (text == null)
            {
                continue;
            }
            try
            {
                if (_npcInfo.Any(n => n.ShopName.Contains(SeString.Parse(text->GetText()).TextValue)))
                {
                    text->TextColor = Dalamud.Utility.Numerics.VectorExtensions.ToByteColor(Service.Configuration.ShopHighlightColor);
                }
            }
            catch (NullReferenceException)
            {
                continue;
            }
        }
    }

    private unsafe void HighlightSelectStringAddon()
    {
        var selectIconStringAddonPtr = Service.GameGui.GetAddonByName("SelectString");

        if (selectIconStringAddonPtr == nint.Zero)
        {
            return;
        }

        var selectIconStringAddon = (AtkUnitBase*)selectIconStringAddonPtr;

        var componentList = selectIconStringAddon->GetComponentListById(3);

        if (componentList == null)
        {
            return;
        }

        foreach (uint index in Enumerable.Range(0, componentList->ListLength))
        {
            var listItemRenderer = componentList->ItemRendererList[index].AtkComponentListItemRenderer;
            if (listItemRenderer == null)
            {
                continue;
            }
            var text = (AtkTextNode*)listItemRenderer->GetTextNodeById(2);
            if (text == null)
            {
                continue;
            }
            try
            {
                if (_npcInfo.Any(n => n.ShopName.Contains(SeString.Parse(text->GetText()).TextValue)))
                {
                    text->TextColor = Dalamud.Utility.Numerics.VectorExtensions.ToByteColor(Service.Configuration.ShopHighlightColor);
                }
            }
            catch (NullReferenceException)
            {
                continue;
            }
        }
    }

    private unsafe void HighlightInclusionShopAddon()
    {
        var inclusionShopAddonPtr = Service.GameGui.GetAddonByName("InclusionShop");

        if (inclusionShopAddonPtr == nint.Zero)
        {
            return;
        }

        var inclusionShopAddon = (AtkUnitBase*)inclusionShopAddonPtr;

        var category = (AtkComponentDropDownList*)inclusionShopAddon->GetComponentByNodeId(7);
        var subcategory = (AtkComponentDropDownList*)inclusionShopAddon->GetComponentByNodeId(9);
        var itemList = (AtkComponentTreeList*)inclusionShopAddon->GetComponentByNodeId(19);

        if (category == null || subcategory == null)
        {
            return;
        }

        foreach (uint index in Enumerable.Range(0, category->List->ListLength))
        {
            var listItemRenderer = category->List->ItemRendererList[index].AtkComponentListItemRenderer;
            if (listItemRenderer == null)
            {
                continue;
            }
            var text = (AtkTextNode*)listItemRenderer->GetTextNodeById(4);
            if (text == null)
            {
                continue;
            }
            var textValue = SeString.Parse(text->GetText()).TextValue;
            try
            {
                if (!string.IsNullOrEmpty(textValue) && _npcInfo.Any(n => n.ShopName.Contains(textValue)))
                {
                    text->TextColor = Dalamud.Utility.Numerics.VectorExtensions.ToByteColor(Service.Configuration.ShopHighlightColor);
                }
            }
            catch (NullReferenceException)
            {
                continue;
            }
        }
        foreach (uint index in Enumerable.Range(0, subcategory->List->ListLength))
        {
            var listItemRenderer = subcategory->List->ItemRendererList[index].AtkComponentListItemRenderer;
            if (listItemRenderer == null)
            {
                continue;
            }
            var text = (AtkTextNode*)listItemRenderer->GetTextNodeById(4);
            if (text == null)
            {
                continue;
            }
            var textValue = SeString.Parse(text->GetText()).TextValue;
            try
            {
                if (!string.IsNullOrEmpty(textValue) && _npcInfo.Any(n => n.ShopName.Contains(textValue)))
                {
                    text->TextColor = Dalamud.Utility.Numerics.VectorExtensions.ToByteColor(Service.Configuration.ShopHighlightColor);
                }
            }
            catch (NullReferenceException)
            {
                continue;
            }
        }

        if (itemList == null)
        {
            return;
        }

        foreach (var item in itemList->Items)
        {
            var listItemRenderer = item.Value->Renderer;
            if (listItemRenderer == null)
            {
                continue;
            }
            var text = (AtkTextNode*)listItemRenderer->GetTextNodeById(5);
            if (text == null)
            {
                continue;
            }
            var itemName = SeString.Parse(text->GetText()).TextValue;
            if (itemName == _itemName)
            {
                text->TextColor = Dalamud.Utility.Numerics.VectorExtensions.ToByteColor(Service.Configuration.ShopHighlightColor);
                // strangely, it doesn't seem like the list gets its color updated until we set the text below
                text->SetText(SeString.Parse(text->GetText()).TextValue);
            }
        }
    }

    private unsafe void HighlightShopExchangeCurrencyAddon()
    {
        var shopExchangeCurrencyAddonPtr = Service.GameGui.GetAddonByName("ShopExchangeCurrency");

        if (shopExchangeCurrencyAddonPtr == nint.Zero)
        {
            return;
        }

        var shopExchangeCurrencyAddon = (AtkUnitBase*)shopExchangeCurrencyAddonPtr;

        var itemList = (AtkComponentTreeList*)shopExchangeCurrencyAddon->GetComponentByNodeId(19);

        if (itemList == null)
        {
            return;
        }

        foreach (var item in itemList->Items)
        {
            var listItemRenderer = item.Value->Renderer;
            if (listItemRenderer == null)
            {
                continue;
            }
            var text = (AtkTextNode*)listItemRenderer->GetTextNodeById(3);
            if (text == null)
            {
                text = (AtkTextNode*)listItemRenderer->GetTextNodeById(8);
            }
            if (text == null)
            {
                continue;
            }
            var itemName = SeString.Parse(text->GetText()).TextValue;
            if (itemName == _itemName)
            {
                text->TextColor = Dalamud.Utility.Numerics.VectorExtensions.ToByteColor(Service.Configuration.ShopHighlightColor);
                // strangely, it doesn't seem like the list gets its color updated until we set the text below
                text->SetText(SeString.Parse(text->GetText()).TextValue);
            }
        }
    }

    private unsafe void HighlightShopExchangeItem()
    {
        var shopExchangeItemAddonPtr = Service.GameGui.GetAddonByName("ShopExchangeItem");

        if (shopExchangeItemAddonPtr == nint.Zero)
        {
            return;
        }

        var shopExchangeItemAddon = (AtkUnitBase*)shopExchangeItemAddonPtr;

        var itemList = (AtkComponentTreeList*)shopExchangeItemAddon->GetComponentByNodeId(19);

        if (itemList == null)
        {
            return;
        }

        foreach (var item in itemList->Items)
        {
            var listItemRenderer = item.Value->Renderer;
            if (listItemRenderer == null)
            {
                continue;
            }
            var text = (AtkTextNode*)listItemRenderer->GetTextNodeById(7);
            if (text == null)
            {
                continue;
            }
            var itemName = SeString.Parse(text->GetText()).TextValue;
            if (itemName == _itemName)
            {
                text->TextColor = Dalamud.Utility.Numerics.VectorExtensions.ToByteColor(Service.Configuration.ShopHighlightColor);
                // strangely, it doesn't seem like the list gets its color updated until we set the text below
                text->SetText(SeString.Parse(text->GetText()).TextValue);
            }
        }
    }

    public void SetNpcInfo(NpcInfo[] npcInfos)
    {
        _npcInfo = npcInfos;
    }

    public void SetItemName(string itemName)
    {
        _itemName = itemName;
    }

    public void Dispose()
    {
        Service.Framework.Update -= Framework_OnUpdate;
    }
}