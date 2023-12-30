using ItemVendorLocation.Models;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;

namespace ItemVendorLocation
{
    internal class ManualItemCorrections
    {
        internal static void ApplyCorrections(Dictionary<uint, NpcLocation> _npcLocations)
        {
            ExcelSheet<TerritoryType> _territoryType = Service.DataManager.GetExcelSheet<TerritoryType>();

#pragma warning disable format
            // Fix Kugane npcs location
            TerritoryType kugane = _territoryType.GetRow(641);
            _npcLocations[1019100] = new NpcLocation(-85.03851f,    117.05188f, kugane);
            _npcLocations[1022846] = new NpcLocation(-83.93994f,    115.31238f, kugane);
            _npcLocations[1019106] = new NpcLocation(-99.22949f,    105.6687f,  kugane);
            _npcLocations[1019107] = new NpcLocation(-100.26703f,   107.43872f, kugane);
            _npcLocations[1019104] = new NpcLocation(-67.582275f,   59.739014f, kugane);
            _npcLocations[1019102] = new NpcLocation(-59.617065f,   33.524048f, kugane);
            _npcLocations[1019103] = new NpcLocation(-52.35376f,    76.58496f,  kugane);
            _npcLocations[1019101] = new NpcLocation(-36.484375f,   49.240845f, kugane);

            // random NPC fixes
            _ = _npcLocations[1004418] = new NpcLocation(-114.0307f, 118.30322f, _territoryType.GetRow(131), 73);

            // some are missing from my test, so we gotta hardcode them
            _ = _npcLocations.TryAdd(1006004, new NpcLocation(5.355835f,    155.22998f,     _territoryType.GetRow(128)));
            _ = _npcLocations.TryAdd(1017613, new NpcLocation(2.822865f,    153.521f,       _territoryType.GetRow(128)));
            _ = _npcLocations.TryAdd(1003077, new NpcLocation(-259.32715f,  37.491333f,     _territoryType.GetRow(129)));

            _ = _npcLocations.TryAdd(1008145, new NpcLocation(-31.265808f,  -245.38031f,    _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1006005, new NpcLocation(-61.234497f,  -141.31384f,    _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1017614, new NpcLocation(-58.79309f,   -142.1073f,     _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1003633, new NpcLocation(145.83044f,   -106.767456f,   _territoryType.GetRow(133)));

            // more locations missing
            _ = _npcLocations.TryAdd(1000215, new NpcLocation(155.35205f,   -70.26782f,     _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1000996, new NpcLocation(-28.152893f,  196.70398f,     _territoryType.GetRow(128)));
            _ = _npcLocations.TryAdd(1000999, new NpcLocation(-29.465149f,  197.92468f,     _territoryType.GetRow(128)));
            _ = _npcLocations.TryAdd(1000217, new NpcLocation(170.30591f,   -73.16705f,     _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1000597, new NpcLocation(-163.07324f,  -78.62976f,     _territoryType.GetRow(153)));
            _ = _npcLocations.TryAdd(1000185, new NpcLocation(-8.590881f,   -2.2125854f,    _territoryType.GetRow(132)));
            _ = _npcLocations.TryAdd(1000392, new NpcLocation(-17.746277f,  43.35083f,      _territoryType.GetRow(132)));
            _ = _npcLocations.TryAdd(1000391, new NpcLocation(66.819214f,   -143.45007f,    _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1000232, new NpcLocation(164.72107f,   -133.68433f,    _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1000301, new NpcLocation(-87.174866f,  -173.51044f,    _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1000267, new NpcLocation(103.89868f,   -213.03125f,    _territoryType.GetRow(133)));
            _ = _npcLocations.TryAdd(1003252, new NpcLocation(-139.57434f,  31.967651f,     _territoryType.GetRow(129)));
            _ = _npcLocations.TryAdd(1001016, new NpcLocation(-42.679565f,  119.920654f,    _territoryType.GetRow(128)));
            _ = _npcLocations.TryAdd(1005422, new NpcLocation(-397.6349f,   80.979614f,     _territoryType.GetRow(129)));
            _ = _npcLocations.TryAdd(1000244, new NpcLocation(423.17834f,   -119.95117f,    _territoryType.GetRow(154)));
            _ = _npcLocations.TryAdd(1000234, new NpcLocation(423.69714f,   -122.08746f,    _territoryType.GetRow(154)));
            _ = _npcLocations.TryAdd(1000230, new NpcLocation(421.46936f,   -125.993774f,    _territoryType.GetRow(154)));

            // merchant & mender
            // East Shroud
            _ = _npcLocations.TryAdd(1000222, new NpcLocation(-213.94684f,  300.4348f,      _territoryType.GetRow(152)));
            _ = _npcLocations.TryAdd(1000535, new NpcLocation(-579.4003f,   104.32593f,     _territoryType.GetRow(152)));
            _ = _npcLocations.TryAdd(1002371, new NpcLocation(-480.91858f,  201.9226f,      _territoryType.GetRow(152)));
            // Central Shroud
            _ = _npcLocations.TryAdd(1000396, new NpcLocation(82.597046f,   -103.349365f,   _territoryType.GetRow(148)));
            _ = _npcLocations.TryAdd(1000220, new NpcLocation(16.189758f,   -15.640564f,    _territoryType.GetRow(148)));
            _ = _npcLocations.TryAdd(1000717, new NpcLocation(175.61597f,   319.32544f,     _territoryType.GetRow(148)));
            // North Shroud
            _ = _npcLocations.TryAdd(1000718, new NpcLocation(332.23462f,   332.47876f,     _territoryType.GetRow(154)));
            _ = _npcLocations.TryAdd(1002376, new NpcLocation(10.635498f,   220.20288f,     _territoryType.GetRow(154)));

            // arms supplier
            _ = _npcLocations.TryAdd(1002374, new NpcLocation(204.39453f,   -65.75122f,     _territoryType.GetRow(153)));

            // encampment clothier & tailor
            _ = _npcLocations.TryAdd(1000579, new NpcLocation(16.03717f,    220.50806f,     _territoryType.GetRow(152)));

            // encampment clothier
            _ = _npcLocations.TryAdd(1002377, new NpcLocation(11.062683f,   221.57617f,     _territoryType.GetRow(154)));

            // traveling armorer
            _ = _npcLocations.TryAdd(1002375, new NpcLocation(203.75366f,   -64.560974f,    _territoryType.GetRow(153)));

            // OIC Quartermaster hax, only Maelstrom missing
            _ = _npcLocations.TryAdd(1002389, new NpcLocation(95.8114f, 67.61267f, _territoryType.GetRow(128)));
#pragma warning restore format
        }
    }
}
