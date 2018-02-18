﻿using PatternsCRIClient.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace PatternsCRIClient.Converters
{
    class VisitKeyToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PatientData patient = (PatientData)value;

            if (App.ClientManager.Settings.IsCentralMode == false)
            {
                PatientData currentPatient = App.ClientManager.CurrentPatient;

                if (patient != null && currentPatient != null && String.IsNullOrEmpty(currentPatient.Key) == false && patient.Key != currentPatient.Key)
                {
                    return 42;
                }
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }  
    }
}
