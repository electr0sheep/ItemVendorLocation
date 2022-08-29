using System.Collections.Generic;
using System.Linq;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace ItemVendorLocation.Models
{
    // https://github.com/Caraxi/ItemSearchPlugin/blob/3f28e10ea3cd54832af7c6650f44647e5034ba6f/ItemSearchPlugin/SpecialShopCustom.cs
    // https://github.com/xivapi/SaintCoinach/blob/fabeedb29921358fe3025c65e72a3de14a3c3070/SaintCoinach/Xiv/SpecialShopListing.cs

    [Sheet("SpecialShop")]
    public class SpecialShopCustom : SpecialShop
    {
        private readonly Dictionary<int, uint> currencies = new()
        {
            { 1, 28 },
            { 2, 25199 },
            { 4, 25200 },
            { 6, 33913 },
            { 7, 33914 },
        };

        private readonly Dictionary<int, int> Tomestones = new();

        public Entry[] Entries;

        public override void PopulateData(RowParser parser, GameData lumina, Language language)
        {
            base.PopulateData(parser, lumina, language);

            Entries = new Entry[60];

            var tomestonesItems = Service.DataManager.GetExcelSheet<TomestonesItem>().Where(i => i.Tomestones.Row > 0).OrderBy(i => i.Tomestones.Row).ToArray();

            for (var i = 0; i < tomestonesItems.Length; i++)
            {
                Tomestones[i + 1] = (int)tomestonesItems[i].Item.Row;
            }

            for (var i = 0; i < Entries.Length; i++)
            {
                Entries[i] = new Entry
                {
                    Result = new[]
                    {
                        new ResultEntry
                        {
                            Item = new LazyRow<Item>(lumina, parser.ReadColumn<int>(1 + i), language),
                            Count = parser.ReadColumn<uint>(61 + i),
                            SpecialShopItemCategory = new LazyRow<SpecialShopItemCategory>(lumina, parser.ReadColumn<int>(121 + i), language),
                            Hq = parser.ReadColumn<bool>(181 + i),
                        },
                        new ResultEntry
                        {
                            Item = new LazyRow<Item>(lumina, parser.ReadColumn<int>(241 + i), language),
                            Count = parser.ReadColumn<uint>(301 + i),
                            SpecialShopItemCategory = new LazyRow<SpecialShopItemCategory>(lumina, parser.ReadColumn<int>(361 + i), language),
                            Hq = parser.ReadColumn<bool>(421 + i),
                        },
                    },
                    Cost = new[]
                    {
                        new CostEntry
                        {
                            Item = new LazyRow<Item>(lumina, ConvertCurrency(parser.ReadColumn<int>(481 + i), UseCurrencyType), language),
                            Count = parser.ReadColumn<uint>(541 + i),
                            Hq = parser.ReadColumn<bool>(601 + i),
                            Collectability = parser.ReadColumn<ushort>(661 + i),
                        },
                        new CostEntry
                        {
                            Item = new LazyRow<Item>(lumina, ConvertCurrency(parser.ReadColumn<int>(721 + i), UseCurrencyType), language),
                            Count = parser.ReadColumn<uint>(781 + i),
                            Hq = parser.ReadColumn<bool>(841 + i),
                            Collectability = parser.ReadColumn<ushort>(901 + i),
                        },
                        new CostEntry
                        {
                            Item = new LazyRow<Item>(lumina, ConvertCurrency(parser.ReadColumn<int>(961 + i), UseCurrencyType), language),
                            Count = parser.ReadColumn<uint>(1021 + i),
                            Hq = parser.ReadColumn<bool>(1081 + i),
                            Collectability = parser.ReadColumn<ushort>(1141 + i),
                        },
                    },
                };
            }
        }

        private int ConvertCurrency(int itemId, ushort useCurrecntType)
        {
            if (itemId is >= 8 or 0)
            {
                return itemId;
            }

            return useCurrecntType switch
            {
                16 => (int)currencies[itemId],
                8 => 1,
                4 => Tomestones[itemId],
                _ => itemId,
            };
        }

        public struct Entry
        {
            public ResultEntry[] Result;
            public CostEntry[] Cost;
        }

        public struct ResultEntry
        {
            public LazyRow<Item> Item;
            public uint Count;
            public LazyRow<SpecialShopItemCategory> SpecialShopItemCategory;
            public bool Hq;
        }

        public struct CostEntry
        {
            public LazyRow<Item> Item;
            public uint Count;
            public bool Hq;
            public ushort Collectability;
        }
    }
}