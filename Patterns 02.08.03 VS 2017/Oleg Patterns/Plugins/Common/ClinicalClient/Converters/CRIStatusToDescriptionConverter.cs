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
    class CRIStatusToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                case CRIState.PositiveReviewed:
                    return (string)Application.Current.FindResource("FRAME_PositiveCurrent");

                case CRIState.PositivePastNotYetReviewed:
                    return (string)Application.Current.FindResource("FRAME_PositivePastNotYetReviewed");

                case CRIState.Negative:
                    return String.Empty;

                case CRIState.UnknownGAOrSingletonMissing:
                    return (string)Application.Current.FindResource("FRAME_UnknownGAOrSingletonMissing");

                case CRIState.UnknownGAOrSingletonNotMet:
                    return (string)Application.Current.FindResource("FRAME_UnknownGAOrSingletonNotMet");

                case CRIState.UnknownNotEnoughTime:
                    return (string)Application.Current.FindResource("FRAME_UnknownNotEnoughTime");

                case CRIState.Off:
                    return (string)Application.Current.FindResource("FRAME_Inactive");

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
