using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrandCompany = FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany;

namespace ItemVendorLocation;
public static class Dictionaries
{
    public static readonly Dictionary<uint, uint> Currencies = new()
        {
            { 1, 28 },
            { 2, 33913 },
            { 4, 33914 },
            { 6, 41784 },
            { 7, 41785 },
        };

    public static readonly Dictionary<uint, uint> ShbFateShopNpc = new()
    {
        { 1027998, 1769957 },
        { 1027538, 1769958 },
        { 1027385, 1769959 },
        { 1027497, 1769960 },
        { 1027892, 1769961 },
        { 1027665, 1769962 },
        { 1027709, 1769963 },
        { 1027766, 1769964 },
    };

    public static readonly Dictionary<byte, uint> GcVendorIdMap = new()
    {
        { 0, 0 },
        { 1, 1002387 },
        { 2, 1002393 },
        { 3, 1002390 },
    };

    public static readonly Dictionary<GrandCompany, uint> OicVendorIdMap = new()
    {
        { GrandCompany.Maelstrom, 1002389 },
        { GrandCompany.TwinAdder, 1000165 },
        { GrandCompany.ImmortalFlames, 1003925 },
        { GrandCompany.None, 0 },
    };
}
