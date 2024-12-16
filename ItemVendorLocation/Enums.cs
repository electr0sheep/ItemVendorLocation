using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemVendorLocation;

public enum ResultsViewType : byte
{
    Single = 1,
    Multiple = 2,
}

public enum CollectablesShopIconIndex : uint
{
    Carpenter, Carpentry = 3,
    Blacksmith, Blacksmithing = 4,
    Armoer, Armoring = 5,
    Goldsmith, Goldsmithing = 6,
    Leatherworker, Leatherworking = 7,
    Weaver, Clothcrafting = 8,
    Alchemist, Alchemy = 9,
    Culinarian, Cooking = 10,
    Miner = 11,
    Botanist = 12,
    Fisher = 13,
}