using System;
using System.Globalization;

namespace Korkboard
{
    public static class StringArrayExtensions
    {
        public static bool Contains(this string[] array, string value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (string.Compare(array[i], value, true, CultureInfo.InvariantCulture) == 0) return true;
            }

            return false;
        }
    }
}
