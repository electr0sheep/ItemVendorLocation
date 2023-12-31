using Dalamud.Game.Text.SeStringHandling;

namespace ItemVendorLocation
{
    internal class Utilities
    {
        internal static void OutputChatLine(SeString message)
        {
            SeStringBuilder sb = new();
            _ = sb.AddUiForeground("[Item Vendor Location] ", 45);
            _ = sb.Append(message);
            Service.ChatGui.Print(sb.BuiltString);
        }
    }
}
