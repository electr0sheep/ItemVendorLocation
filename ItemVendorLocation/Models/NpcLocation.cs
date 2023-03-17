using Lumina.Excel.GeneratedSheets;
using System;

namespace ItemVendorLocation.Models
{
    public class NpcLocation
    {
        public NpcLocation(float x, float y, TerritoryType territoryType, uint? map = null)
        {
            X = x;
            Y = y;
            TerritoryExcel = territoryType;
            MapId = map != null ? (uint)map : territoryType.Map.Row;
        }

        public TerritoryType TerritoryExcel { get; set; }

        public float MapX => ToMapCoordinate(X, TerritoryExcel.Map.Value.SizeFactor, TerritoryExcel.Map.Value.OffsetX);
        public float MapY => ToMapCoordinate(Y, TerritoryExcel.Map.Value.SizeFactor, TerritoryExcel.Map.Value.OffsetY);
        // Garland Tools already sends over "map coordinates", so another option would be to invert ToMapCoordinate(),
        // or use a constructor that doesn't assume the coordinates aren't already converted or something.
        // Going with this for now
        public float X { get; }
        public float Y { get; }
        public uint TerritoryType => TerritoryExcel.RowId;
        public uint MapId { get; }

        private static float ToMapCoordinate(float val, float scale, short offset)
        {
            float c = scale / 100.0f;

            val = (val + offset) * c;

            return (41.0f / c * ((val + 1024.0f) / 2048.0f)) + 1;
        }
    }
}