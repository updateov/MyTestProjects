using PatternsCRIClient.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PatternsCRIClient.Converters
{
    class CurrentBedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PatientData selectedPatient = (PatientData)value;

            if (App.ClientManager.Settings.IsCentralMode == false)
            {
                PatientData currentPatient = App.ClientManager.CurrentPatient;

                if (selectedPatient != null && currentPatient != null && String.IsNullOrEmpty(currentPatient.Key) == false && selectedPatient.Key != currentPatient.Key)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }  
    }
}
