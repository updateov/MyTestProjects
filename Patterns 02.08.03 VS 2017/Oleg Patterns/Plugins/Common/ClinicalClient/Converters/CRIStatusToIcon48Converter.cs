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
    class CRIStatusToIcon48Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                    return (ControlTemplate)Application.Current.FindResource("IconPositiveCurrent48");

                case CRIState.PositivePastNotYetReviewed:
                    return (ControlTemplate)Application.Current.FindResource("IconPositivePast48");

                case CRIState.PositiveReviewed:
                    return (ControlTemplate)Application.Current.FindResource("IconPositiveCurrent48");

                case CRIState.Negative:
                    return DependencyProperty.UnsetValue;

                case CRIState.UnknownGAOrSingletonMissing:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownMissingGA48");

                case CRIState.UnknownGAOrSingletonNotMet:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownCriteriaNotMet48");

                case CRIState.UnknownNotEnoughTime:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownLessData48");

                case CRIState.Off:
                    return (ControlTemplate)Application.Current.FindResource("IconCRI_OFF48");

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
