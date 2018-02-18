using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CurveChartControl
{

	public class DateTimeFromOADateConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return string.Empty;
			try
			{
				return DateTime.FromOADate((double)value);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return string.Empty;
			}

		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class DateStringFromOADateConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return string.Empty;
			try
			{
				return DateTime.FromOADate((double)value).ToLocalTime().ToShortDateString();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return string.Empty;
			}

		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class TimeStringFromOADateConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return string.Empty;
			try
			{
				return DateTime.FromOADate((double)value).ToLocalTime().ToShortTimeString();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return string.Empty;
			}

		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class DateTimeToStringConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return string.Empty;
			try
			{
				var time = ((DateTime)value).ToLocalTime();
				return time.ToShortDateString() + " " + time.ToShortTimeString();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return string.Empty;
			}

		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}


    public class DateOrTimeToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
             
            //no value nor parameter
            if (value == null || parameter == null) return string.Empty;

            try
            {

                var p = parameter.ToString().Trim().ToUpperInvariant();

                if (p == "D")
                {
                    var time = ((DateTime)value).ToLocalTime();
                    return time.ToShortDateString(); ;
                }
                else if (p == "T")
                {
                    var time = ((DateTime)value).ToLocalTime();
                    return time.ToShortTimeString();
                }
                else 
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return string.Empty;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }




	public class BoolToYESNOUnknown : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				if (value != null)
					return ((bool)value) ? "YES" : "NO";
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

    public class FirstExamConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            Control control = parameter as Control;
            if (control == null)
                return null;

            try
            {
                Exam exam = (Exam)value;

                if (exam.IsFirstExam)
                    return control.Resources["ColumnCellFormatSelected"] as Style;

                if (exam.Status == Exam.CurveCalculationStatuses.Ignored)
                    return control.Resources["ColumnCellFormatNoSelected"] as Style;

                if (exam.Status == Exam.CurveCalculationStatuses.Info)
                    return control.Resources["ColumnCellFormatNoSelected"] as Style;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return control.Resources["ColumnCellFormat"] as Style;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

	public class BooleanAsVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			try
			{
				return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class ErrorAsVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			try
			{
				return (Exam.CurveCalculationStatuses)Enum.Parse(typeof(Exam.CurveCalculationStatuses), value.ToString()) == Exam.CurveCalculationStatuses.Error ? Visibility.Visible : Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class InfoAsVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			try
			{
				return (Exam.CurveCalculationStatuses)Enum.Parse(typeof(Exam.CurveCalculationStatuses), value.ToString()) == Exam.CurveCalculationStatuses.Info ? Visibility.Visible : Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class VisitStatusAsVisibility : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			try
			{
				if (Enum.IsDefined(typeof(CurveModel.VisitStatusEnum), value))
				{
					if ((CurveModel.VisitStatusEnum)value != CurveModel.VisitStatusEnum.Invalid && (CurveModel.VisitStatusEnum)value != CurveModel.VisitStatusEnum.Error)
					{
						return Visibility.Collapsed;
					}
				}
				return Visibility.Visible;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class TimeConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{

			double labelValue = ((C1.WPF.C1Chart.AxisPoint)value).Value; //value that would be shown if no template - default value
			if ((labelValue < 0) || (double.IsNaN(labelValue)))
				return string.Empty;

			try
			{
				var dt = DateTime.FromOADate(labelValue).ToLocalTime();
				var ret = new StringBuilder();
				if ((dt.Hour % 12) == 0)
				{
					ret.AppendLine(dt.ToString("HH:mm"));
					ret.Append(dt.ToShortDateString());
				}
				else if ((dt.Hour % 3) == 0) 
				{
					ret.Append(dt.ToString("HH:mm"));
				}

				return ret.ToString();
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class StringDictionaryConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				Dictionary<string, string> dic = (Dictionary<string, string>)value;
				if (dic != null)
				{
					return dic[parameter as string];
				}
				return string.Empty;

			}
			catch (Exception ex)
			{
				Debug.WriteLine("StringDictionaryConverter: " + ex.Message);
				return string.Empty;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public class IntAsVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;
			try
			{
				return ((int)value > 0) ? Visibility.Visible : Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
