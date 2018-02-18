using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using CRIEntities;
using System.Windows.Media;

namespace PatternsCRIClient.Converters
{
    class CRIStatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                case CRIState.PositiveReviewed:
                    Color color = (Color)ColorConverter.ConvertFromString("#CC5615");
                    return new SolidColorBrush(color);

                case CRIState.PositivePastNotYetReviewed:
                case CRIState.Negative:
                case CRIState.UnknownGAOrSingletonMissing:
                case CRIState.UnknownGAOrSingletonNotMet:
                case CRIState.UnknownNotEnoughTime:
                case CRIState.Off:
                    return Brushes.White;

                default:
                    return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
