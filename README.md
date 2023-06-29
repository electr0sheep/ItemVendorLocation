# ItemVendorLocation
Adds a context menu to find vendor locations for items that can be purchased from a vendor.
Also adds a chat command that can be used to search for items
`xlvendor` opens the configuration window
`xlvendor ceruleum tank` searches for an item with "ceruleum tank" in the name. This is not case-sensitive and results are limited
to a maximum of 50.

The plugin provides the following indicators to know if an item is sold by a vendor:
1. The game indicates the shop selling price without any plugins

![Alt text](/Images/GilVendor.png?raw=true "Item Sold for Gil")

2. If the plugin determines an item is sold by a GC vendor, the item's popup will show the price in your GC's currency

![Alt text](/Images/GCVendor.png?raw=true "Item Sold for GC Seals")

3. If the plugin determines an item is sold by other vendors, the item's popup will show that it is sold by Special Vendors

![Alt text](/Images//SpecialVendor.png?raw=true "Item Sold for other currency")

4. If the item is not sold by any vendors, the shop selling price will show None

![Alt text](/Images/NoVendors.png?raw=true "Item not sold for any currency")

If the item is sold by a vendor, a new option will be added to the item's context menu. When clicked, this option can display
results in one of two possible formats.

![Alt text](/Images/ContextMenu.png?raw=true "Item Context Menu")

1. The plugin will show a list of all possible vendors in a plugin GUI window

![Alt text](/Images/VendorLocations.png?raw=true "Vendor Locations")

2. It can simply find the first vendor and link the coordinates in the in-game chat

![Alt text](/Images/ChatVendorLocation.png?raw=true "Vendor Location")

In addition, there is a chat command that can be used i.e. `/xlvendor bronze chaser hammer`

# Contributors
I appreciate everyone who has contributed, visible over there to the right.

It would be remiss of me to not give special mention to [Nuko](https://github.com/NukoOoOoOoO) for a near complete rewrite.
They reworked the plugin to completly remove Garland Tools as the data source. This must have taken
a ton of work, and is much appreciated!