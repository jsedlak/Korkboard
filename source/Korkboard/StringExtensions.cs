using System;
using System.Globalization;

namespace Korkboard
{
    public static class StringExtensions
    {
        public static bool CompareEx(this string str, string strB)
        {
            return string.Compare(str, strB, true, CultureInfo.InvariantCulture) == 0;
        }
    }
}
