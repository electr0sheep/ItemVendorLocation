# ItemVendorLocation
Adds a context menu to find vendor locations for items that can be purchased from a vendor.

This plugin uses [Garland Tools](https://garlandtools.org/) as it's source of information.

The plugin provides the following indicators to know if an item is sold by a vendor:
1. The game indicates the shop selling price without any plugins

![Alt text](/Images/GilVendor.png?raw=true "Item Sold for Gil")

2. If the plugin determines an item is sold by a GC vendor, the item's popup will show the price in GC Seals

![Alt text](/Images/GCVendor.png?raw=true "Item Sold for GC Seals")

3. If the plugin determines an item is sold by other vendors, the item's popup will show that it is sold by Special Vendors

![Alt text](/Images//SpecialVendor.png?raw=true "Item Sold for other currency")

4. If the item is not sold by any vendors, the shop selling price will show None

![Alt text](/Images/NoVendors.png?raw=true "Item not sold for any currency")

If the item is sold by a vendor, a new option will be added to the item's context menu

![Alt text](/Images/ContextMenu.png?raw=true "Item Context Menu")

The plugin will show a list of all possible vendors in a plugin GUI window

![Alt text](/Images/VendorLocations.png?raw=true "Vendor Locations")

2. It can simply find the first vendor and link the coordinates in the in-game chat

![Alt text](/Images/ChatVendorLocation.png?raw=true "Vendor Location")