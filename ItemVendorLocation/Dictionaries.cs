using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemVendorLocation;
public static class Dictionaries
{
    public static readonly Dictionary<uint, uint> Currencies = new()
        {
            { 1, 28 },
            { 2, 33913 },
            { 4, 33914 },
            { 6, 41784 },
            { 7, 41785 },
        };
}
