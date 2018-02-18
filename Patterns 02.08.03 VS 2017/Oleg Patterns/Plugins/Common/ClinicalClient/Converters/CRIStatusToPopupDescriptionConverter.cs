using CRIEntities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PatternsCRIClient.Converters
{
    class CRIStatusToPopupDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                case CRIState.PositiveReviewed:
                    return (string)Application.Current.FindResource("POPUP_PositiveCurrent");

                case CRIState.PositivePastNotYetReviewed:
                    return (string)Application.Current.FindResource("POPUP_PositivePastNotYetReviewed");

                case CRIState.Negative:
                    return (string)Application.Current.FindResource("POPUP_Negative");

                case CRIState.UnknownGAOrSingletonMissing:
                    return (string)Application.Current.FindResource("POPUP_UnknownGAOrSingletonMissing");

                case CRIState.UnknownGAOrSingletonNotMet:
                    return (string)Application.Current.FindResource("POPUP_UnknownGAOrSingletonNotMet");

                case CRIState.UnknownNotEnoughTime:
                    return (string)Application.Current.FindResource("POPUP_UnknownNotEnoughTime");

                case CRIState.Off:
                    return (string)Application.Current.FindResource("POPUP_Inactive");

                default:
                    return String.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
