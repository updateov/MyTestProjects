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
    class CRIStatusToFrameBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                case CRIState.PositivePastNotYetReviewed:
                case CRIState.PositiveReviewed:
                    Color color = (Color)ColorConverter.ConvertFromString("#CC5615");
                    return new SolidColorBrush(color);

                case CRIState.Negative:
                case CRIState.UnknownGAOrSingletonMissing:
                case CRIState.UnknownGAOrSingletonNotMet:
                case CRIState.UnknownNotEnoughTime:
                case CRIState.Off:
                    return Brushes.Transparent;

                default:
                    return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
