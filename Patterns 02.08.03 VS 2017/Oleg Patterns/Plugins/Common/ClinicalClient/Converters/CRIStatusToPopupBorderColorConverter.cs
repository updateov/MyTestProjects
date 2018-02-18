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
    class CRIStatusToPopupBorderColorConverter : IValueConverter
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
                    color = (Color)ColorConverter.ConvertFromString("#CC5615");
                    return new SolidColorBrush(color);

                case CRIState.Negative:
                case CRIState.UnknownGAOrSingletonMissing:
                case CRIState.UnknownGAOrSingletonNotMet:
                case CRIState.UnknownNotEnoughTime:
                case CRIState.Off:
                    color = (Color)ColorConverter.ConvertFromString("#4E4C4C");
                    return new SolidColorBrush(color);

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
