using CheapLoc;
using Dalamud;

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

            // this signature only exists in chinese client
            if (Service.SigScanner.ScanText("48 8D 15 ?? ?? ?? ?? 33 F6 44 89 4C 24") != nint.Zero)
                localizationJson = /*lang=json,strict*/ """
                {
                    "ContextMenuItem": {
                        "message": "查找兑换位置"
                    }
                }
                """;

            Loc.Setup(localizationJson);
        }
    }
}
