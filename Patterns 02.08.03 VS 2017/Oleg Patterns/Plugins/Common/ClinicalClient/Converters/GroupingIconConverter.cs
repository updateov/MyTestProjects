using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PatternsCRIClient.Converters
{
    class GroupingIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isExpanded = (bool)value;

            if (isExpanded == true)
            {
                return (ControlTemplate)Application.Current.FindResource("IconButtonMinus");
            }
            else
            {
                return (ControlTemplate)Application.Current.FindResource("IconButtonPlus");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }   
    }
}
