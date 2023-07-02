using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;


namespace ItemVendorLocation
{
    internal class Utilities
    {
        internal static void OutputChatLine(SeString message)
        {
            SeStringBuilder sb = new();
            _ = sb.AddUiForeground(45);
            _ = sb.AddText("[Item Vendor Location] ");
            _ = sb.AddUiForegroundOff();
            _ = sb.Append(message);
            Service.ChatGui.PrintChat(new XivChatEntry
            {
                Message = sb.BuiltString
            });
        }
    }
}
