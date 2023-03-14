using CheapLoc;
using Dalamud;
using Newtonsoft.Json;

namespace ItemVendorLocation
{
    internal class Localization
    {
        /// <summary>
        /// I have no professional training
        /// </summary>
        public static void SetupLocalization(ClientLanguage language)
        {
            string localizationJson = language switch
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
                ClientLanguage.English or _ => /*lang=json,strict*/ """
                {
                    "ContextMenuItem": {
                        "message": "Vendor Location"
                    }
                }
                """,
            };
            Loc.Setup(localizationJson);
        }
    }
}
