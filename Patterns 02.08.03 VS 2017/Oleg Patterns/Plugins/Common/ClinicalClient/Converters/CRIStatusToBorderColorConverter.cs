using CRIEntities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace PatternsCRIClient.Converters
{
    class CRIStatusToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;
            Color color;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                case CRIState.PositivePastNotYetReviewed:
                case CRIState.PositiveReviewed:
                    color = Color.FromArgb(100, 204, 86, 21);
                    return new SolidColorBrush(color);

                case CRIState.Negative:
                    color = Color.FromArgb(100, 78, 76, 76);
                    return new SolidColorBrush(color);

                case CRIState.UnknownGAOrSingletonMissing:
                case CRIState.UnknownGAOrSingletonNotMet:
                case CRIState.UnknownNotEnoughTime:
                case CRIState.Off:
                    color = Color.FromArgb(100, 78, 76, 76);
                    return new SolidColorBrush(color);

                default:
                    return Brushes.WhiteSmoke;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
