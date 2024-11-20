using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;

//namespace ItemVendorLocation.Models
//{
//    [Sheet("FateShop")]
//    public class FateShopCustom : FateShop
//    {
//        public LazyRow<SpecialShopCustom>[] SpecialShopCustoms { get; set; }

//        public override void PopulateData(RowParser parser, GameData gameData, Language language)
//        {
//            base.PopulateData(parser, gameData, language);

//            SpecialShopCustoms = new LazyRow<SpecialShopCustom>[2];

//            for (var i = 0; i < 2; i++)
//            {
//                SpecialShopCustoms[i] = new(gameData, parser.ReadColumn<uint>(0 + i), language);
//            }
//        }
//    }
//}