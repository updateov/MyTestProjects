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
    public class CRIStatusToTooltipBackgroundConverter : IValueConverter
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
                    return (ControlTemplate)Application.Current.FindResource("IconPositiveCurrent");

                case CRIState.Negative:
                    return (ControlTemplate)Application.Current.FindResource("IconNegative");

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
