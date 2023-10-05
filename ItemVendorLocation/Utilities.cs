using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;


namespace ItemVendorLocation
{
    internal class Utilities
    {
        internal static void OutputChatLine(SeString message)
        {
            SeStringBuilder sb = new();
            _ = sb.AddUiForeground("[Item Vendor Location] ", 45).Append(message);

            Service.ChatGui.Print(new XivChatEntry { Message = sb.BuiltString });
        }
    }
}
