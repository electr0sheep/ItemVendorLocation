using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace ItemVendorLocation.Models;

[Sheet("InclusionShopSeries")]
public class InclusionShopSeriesCustom : InclusionShop
{
    public LazyRow<SpecialShopCustom> SpecialShopCustoms { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        SpecialShopCustoms = new LazyRow<SpecialShopCustom>(gameData, parser.ReadColumn<uint>(0), language);
    }
}