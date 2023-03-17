using Lumina.Excel.GeneratedSheets;
using System;

namespace ItemVendorLocation.Models
{
    public class NpcLocation
    {
        private readonly float x;

        private readonly float y;

        public NpcLocation(float x, float y, TerritoryType territoryType)
        {
            this.x = x;
            this.y = y;
            TerritoryExcel = territoryType;
        }

        public TerritoryType TerritoryExcel { get; set; }

        public float MapX => ToMapCoordinate(x, TerritoryExcel.Map.Value.SizeFactor, TerritoryExcel.Map.Value.OffsetX);
        public float MapY => ToMapCoordinate(y, TerritoryExcel.Map.Value.SizeFactor, TerritoryExcel.Map.Value.OffsetY);
        // Garland Tools already sends over "map coordinates", so another option would be to invert ToMapCoordinate(),
        // or use a constructor that doesn't assume the coordinates aren't already converted or something.
        // Going with this for now
        public float LegacyMapX => x;
        public float LegacyMapY => y;
        public uint TerritoryType => TerritoryExcel.RowId;
        public uint MapId => TerritoryExcel.Map.Row;

        private static float ToMapCoordinate(float val, float scale, short offset)
        {
            float c = scale / 100.0f;

            val = (val + offset) * c;

            return (41.0f / c * ((val + 1024.0f) / 2048.0f)) + 1;
        }
    }
}