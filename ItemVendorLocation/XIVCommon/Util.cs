using System;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;

namespace XIVCommon;

internal static class Util
{
    internal static byte[] Terminate(this byte[] array)
    {
        var terminated = new byte[array.Length + 1];
        Array.Copy(array, terminated, array.Length);
        terminated[^1] = 0;

        return terminated;
    }

    internal static unsafe byte[] ReadTerminated(IntPtr memory)
    {
        if (memory == IntPtr.Zero)
        {
            return [];
        }

        var buf = new List<byte>();

        var ptr = (byte*)memory;
        while (*ptr != 0)
        {
            buf.Add(*ptr);
            ptr += 1;
        }

        return [.. buf];
    }

    internal static SeString ReadSeString(IntPtr memory)
    {
        var terminated = ReadTerminated(memory);
        return SeString.Parse(terminated);
    }
}
