using CRIEntities;
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
    class CRIStatusToIcon32Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                    return (ControlTemplate)Application.Current.FindResource("IconPositiveCurrent32");

                case CRIState.PositivePastNotYetReviewed:
                    return (ControlTemplate)Application.Current.FindResource("IconPositivePast32");

                case CRIState.PositiveReviewed:
                    return (ControlTemplate)Application.Current.FindResource("IconPositiveCurrent32");

                case CRIState.Negative:
                    return (ControlTemplate)Application.Current.FindResource("IconNegative32");

                case CRIState.UnknownGAOrSingletonMissing:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownMissingGAWhite32");

                case CRIState.UnknownGAOrSingletonNotMet:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownCriteriaNotMetWhite32");

                case CRIState.UnknownNotEnoughTime:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownLessDataWhite32");

                case CRIState.Off:
                    return (ControlTemplate)Application.Current.FindResource("IconCRI_OFF_White32");

                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
