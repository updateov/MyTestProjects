using PatternsCRIClient.Data;
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
    class VisitToIconBed32Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCurrentBed = (bool)value;

            if(App.ClientManager.Settings.IsCentralMode == false)
            {
                PatientData patient = App.ClientManager.CurrentPatient;

                if (patient != null && isCurrentBed == true)
                {
                    switch (patient.CRIStatus)
                    {
                        case CRIState.PositiveCurrent:
                        case CRIState.PositiveReviewed:
                            return (ControlTemplate)Application.Current.FindResource("IconBedWhite32");

                        case CRIState.PositivePastNotYetReviewed:
                        case CRIState.Negative:
                        case CRIState.UnknownGAOrSingletonMissing:
                        case CRIState.UnknownGAOrSingletonNotMet:
                        case CRIState.UnknownNotEnoughTime:
                        case CRIState.Off:
                            return (ControlTemplate)Application.Current.FindResource("IconBedGrey32");

                        default:
                            return DependencyProperty.UnsetValue;
                    }
                }
            }
            
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }  
    }
}
