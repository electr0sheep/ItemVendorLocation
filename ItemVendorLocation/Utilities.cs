using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;


namespace ItemVendorLocation
{
    internal class Utilities
    {
        internal static void OutputChatLine(SeString message)
        {
            SeStringBuilder sb = new();
            _ = sb.AddUiForeground("[Item Vendor Location] ", 45);
            _ = sb.Append(message);
            Service.ChatGui.Print(new XivChatEntry
            {
                Message = sb.BuiltString
            });
        }
    }
}
