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
    class CRIStatusToHoverBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;
            Color color;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                case CRIState.PositiveReviewed:
                    color = (Color)ColorConverter.ConvertFromString("#E8651E");
                    return new SolidColorBrush(color);

                case CRIState.PositivePastNotYetReviewed:
                case CRIState.Negative:
                case CRIState.UnknownGAOrSingletonMissing:
                case CRIState.UnknownGAOrSingletonNotMet:
                case CRIState.UnknownNotEnoughTime:
                case CRIState.Off:
                    color = (Color)ColorConverter.ConvertFromString("#F2F2F2");
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
