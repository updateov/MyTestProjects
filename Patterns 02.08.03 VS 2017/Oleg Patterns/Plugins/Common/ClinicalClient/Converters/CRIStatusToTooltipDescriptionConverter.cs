using CRIEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PatternsCRIClient.Converters
{
    class CRIStatusToTooltipDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                    return (string)Application.Current.FindResource("TOOLTIP_PositiveCurrentNotReviewed");
                
                case CRIState.PositiveReviewed:
                    return (string)Application.Current.FindResource("TOOLTIP_PositiveCurrent");

                case CRIState.PositivePastNotYetReviewed:
                    return (string)Application.Current.FindResource("TOOLTIP_PositivePastNotYetReviewed");

                case CRIState.Negative:
                    return (string)Application.Current.FindResource("TOOLTIP_Negative");

                case CRIState.UnknownGAOrSingletonMissing:
                    return (string)Application.Current.FindResource("TOOLTIP_UnknownGAOrSingletonMissing");

                case CRIState.UnknownGAOrSingletonNotMet:
                    return (string)Application.Current.FindResource("TOOLTIP_UnknownGAOrSingletonNotMet");

                case CRIState.UnknownNotEnoughTime:
                    return (string)Application.Current.FindResource("TOOLTIP_UnknownNotEnoughTime");

                case CRIState.Off:
                    return (string)Application.Current.FindResource("TOOLTIP_Inactive");

                default:
                    return String.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
