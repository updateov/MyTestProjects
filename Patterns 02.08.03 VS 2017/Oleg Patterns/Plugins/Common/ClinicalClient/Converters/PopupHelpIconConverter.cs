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
    class PopupHelpIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CRIState status = (CRIState)value;

            switch (status)
            {
                case CRIState.PositiveCurrent:
                case CRIState.PositivePastNotYetReviewed:
                case CRIState.PositiveReviewed:
                case CRIState.UnknownGAOrSingletonMissing:
                case CRIState.UnknownGAOrSingletonNotMet:
                case CRIState.UnknownNotEnoughTime:
                case CRIState.Off:
                    return (ControlTemplate)Application.Current.FindResource("IconHelpWhite");

                case CRIState.Negative:
                    return (ControlTemplate)Application.Current.FindResource("IconHelpGrey");

                default:
                    return (ControlTemplate)Application.Current.FindResource("IconHelpGrey");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
