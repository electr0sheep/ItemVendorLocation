using CheapLoc;
using Dalamud.Common;

namespace ItemVendorLocation;

internal class Localization
{
    public static void SetupLocalization(ClientLanguage language)
    {
        var localizationJson = language switch
                               {
                                   ClientLanguage.Japanese => /*lang=json,strict*/ """
                                                                                   {
                                                                                       "ContextMenuItem": {
                                                                                           "message": "ベンダーロケーション"
                                                                                       }
                                                                                   }
                                                                                   """,
                                   ClientLanguage.German => /*lang=json,strict*/ """
                                                                                 {
                                                                                     "ContextMenuItem": {
                                                                                         "message": "Standort des Anbieters"
                                                                                     }
                                                                                 }
                                                                                 """,
                                   ClientLanguage.French => /*lang=json,strict*/ """
                                                                                 {
                                                                                     "ContextMenuItem": {
                                                                                         "message": "Emplacement du Vendeur"
                                                                                     }
                                                                                 }
                                                                                 """,
                                   (ClientLanguage)4 => /*lang=json,strict*/ """
                                                                             {
                                                                                 "ContextMenuItem": {
                                                                                     "message": "查找兑换位置"
                                                                                 }
                                                                             }
                                                                             """,
                                   ClientLanguage.English or _ => /*lang=json,strict*/ """
                                                                                       {
                                                                                           "ContextMenuItem": {
                                                                                               "message": "Vendor Location"
                                                                                           }
                                                                                       }
                                                                                       """
                               };

        Loc.Setup(localizationJson);
    }
}