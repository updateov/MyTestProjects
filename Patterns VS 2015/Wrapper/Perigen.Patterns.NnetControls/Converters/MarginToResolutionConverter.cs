using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Perigen.Patterns.NnetControls.Converters
{
    public class MarginToResolutionConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string[] sVal = parameter.ToString().Split(',');
                
                if (sVal.Count() == 0)
                    return null;

                double coef = SystemParameters.PrimaryScreenHeight / 1080.0;// / PatternsUIManager.Instance.DpiFactor;

                if (sVal.Count() == 1)
                {
                    double dVal = Double.Parse(sVal[0]);

                    return new Thickness((int)Math.Floor(dVal * coef));
                }

                int left = (int)Math.Floor(Double.Parse(sVal[0]) * coef);
                int top = (int)Math.Floor(Double.Parse(sVal[1]) * coef);
                int right = (int)Math.Floor(Double.Parse(sVal[2]) * coef);
                int bottom = (int)Math.Floor(Double.Parse(sVal[3]) * coef);

                return new Thickness(left, top, right, bottom);
            }
            catch
            {
                return parameter;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
