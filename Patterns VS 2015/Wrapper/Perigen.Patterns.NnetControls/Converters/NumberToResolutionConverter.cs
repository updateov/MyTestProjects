using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Perigen.Patterns.NnetControls.Converters
{
    public class NumberToResolutionConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //////////////////////////////////////////////////////////
            //Point absoluteScreenPos = System.Windows.Input.Mouse.GetPosition(Application.Current.MainWindow);

            // MaximumWindowTrackWidth = primary + virtual monitors !!!

            //if (System.Windows.SystemParameters.VirtualScreenLeft == System.Windows.SystemParameters.WorkArea.Left)
            //{
            //    //Primary Monitor is on the Left
            //    if (absoluteScreenPos.X <= System.Windows.SystemParameters.PrimaryScreenWidth)
            //    {
            //        //Primary monitor
            //        double MaxHeight = System.Windows.SystemParameters.WorkArea.Height;
            //    }
            //    else
            //    {
            //        //Secondary monitor
            //        double MaxHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            //    }
            //}

            //if (System.Windows.SystemParameters.VirtualScreenLeft < 0)
            //{
            //    //Primary Monitor is on the Right
            //    if (absoluteScreenPos.X > 0)
            //    {
            //        //Primary monitor
            //        double MaxHeight = System.Windows.SystemParameters.WorkArea.Height;
            //    }
            //    else
            //    {
            //        //Secondary monitor
            //        double MaxHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            //    }
            //}
            ///////////////////////////////////////////////////////////

            try
            {
                int iVal = Int32.Parse(parameter.ToString());

                double coef = SystemParameters.PrimaryScreenHeight / 1080.0;// / PatternsUIManager.Instance.DpiFactor;
                double res = iVal * coef;

                return Math.Floor(res);
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
