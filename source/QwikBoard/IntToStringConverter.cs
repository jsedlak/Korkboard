using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Korkboard
{
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            if (targetType == typeof(string))
                return value.ToString();

            return null;
        }

        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            if (targetType == typeof(int))
                return int.Parse(value.ToString());

            return 0;
        }
    }
}
