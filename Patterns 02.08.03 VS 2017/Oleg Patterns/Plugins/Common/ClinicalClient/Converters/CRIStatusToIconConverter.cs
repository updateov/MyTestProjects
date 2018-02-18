using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using CRIEntities;
using System.Windows.Controls;
using System.Windows;

namespace PatternsCRIClient.Converters
{
    class CRIStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                    return (ControlTemplate)Application.Current.FindResource("IconPositiveNotReviewed");

                case CRIState.PositivePastNotYetReviewed:
                    return (ControlTemplate)Application.Current.FindResource("IconPositivePast");

                case CRIState.PositiveReviewed:
                    return DependencyProperty.UnsetValue;//(ControlTemplate)Application.Current.FindResource("IconPositiveCurrent");

                case CRIState.Negative:
                   return DependencyProperty.UnsetValue;//(ControlTemplate)Application.Current.FindResource("IconNegative");

                case CRIState.UnknownGAOrSingletonMissing:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownMissingGA");

                case CRIState.UnknownGAOrSingletonNotMet:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownCriteriaNotMet");

                case CRIState.UnknownNotEnoughTime:
                    return (ControlTemplate)Application.Current.FindResource("IconUnknownLessData");

                case CRIState.Off:
                    return (ControlTemplate)Application.Current.FindResource("IconCRI_OFF");

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
