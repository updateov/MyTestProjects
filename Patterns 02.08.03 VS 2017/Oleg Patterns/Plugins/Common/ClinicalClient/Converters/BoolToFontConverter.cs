using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PatternsCRIClient.Converters
{
    class BoolToFontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? (Style)Application.Current.FindResource("BoldFontStyle") : (Style)Application.Current.FindResource("RegularFontStyle");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }    
    }
}
