using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace CurveChartControl
{
	public static class Extensions
	{
		/// <summary>
		/// Return a value from dictionary based in the key. If key does not exist, it return String.Empty.
		/// This avoid the KeyNotFoundException.
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetValueWithDefault(this Dictionary<string, string> dictionary, string key)
		{
			if (dictionary.ContainsKey(key))
			{
				return dictionary[key];
			}
			return string.Empty;
		}

		/// <summary>
		/// Never return null, a string empty at least
		/// </summary>
		/// <param name="element"></param>
		/// <param name="attributeName"></param>
		/// <returns></returns>
		public static String GetAttributeAsString(this XElement element, String attributeName)
		{
			object value = GetAttributeValue(element, attributeName);
			if (value != null)
			{
				return value.ToString();
			}
			return string.Empty;
		}

		public static Exam.FetalPositionEnum GetAttributeAsFetalPosition(this XElement element, String attributeName)
		{
			var value = GetAttributeAsString(element, attributeName);
			if ((!string.IsNullOrEmpty(value)) && Enum.IsDefined(typeof(Exam.FetalPositionEnum), value))
			{
				return (Exam.FetalPositionEnum)Enum.Parse(typeof(Exam.FetalPositionEnum), value);
			}
			return Exam.FetalPositionEnum.Unknown;
		}

		public static Exam.CurveCalculationStatuses GetAttributeAsCurveCalculationStatuses(this XElement element, String attributeName)
		{
			var value = GetAttributeAsString(element, attributeName);
			if ((!string.IsNullOrEmpty(value)) && Enum.IsDefined(typeof(Exam.CurveCalculationStatuses), value))
			{
				return (Exam.CurveCalculationStatuses)Enum.Parse(typeof(Exam.CurveCalculationStatuses), value);
			}
			return Exam.CurveCalculationStatuses.Error;
		}

		public static Boolean? GetAttributeAsBoolean(this XElement element, String attributeName)
		{
			var value = GetAttributeValue(element, attributeName);
			if (value != null)
			{
				Boolean result;
				if (Boolean.TryParse(value.ToString(), out result))
					return result;
			}
			return null;
		}

		public static Boolean GetAttributeAsBoolean(this XElement element, String attributeName, Boolean defaultValue)
		{
			var value = GetAttributeAsBoolean(element, attributeName);
			return (value.HasValue) ? value.Value : defaultValue;
		}

		public static DateTime? GetAttributeAsDateTime(this XElement element, String attributeName)
		{
			var value = GetAttributeValue(element, attributeName);
			if (value != null)
			{
				DateTime result;
				if (DateTime.TryParseExact(value.ToString(), "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
					return result;
			}
			return null;
		}

		public static DateTime GetAttributeAsDateTime(this XElement element, String attributeName, DateTime defaultValue)
		{
			var value = GetAttributeAsDateTime(element, attributeName);
			return (value.HasValue) ? value.Value : defaultValue;
		}

		public static Double? GetAttributeAsDouble(this XElement element, String attributeName)
		{
			var value = GetAttributeValue(element, attributeName);
			if (value != null)
			{
				Double result;
				if (Double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result))
					return result;
			}
			return null;
		}

		public static Double GetAttributeAsDouble(this XElement element, String attributeName, Double defaultValue)
		{
			var value = GetAttributeAsDouble(element, attributeName);
			return (value.HasValue) ? value.Value : defaultValue;
		}

		public static Int32? GetAttributeAsInt(this XElement element, String attributeName)
		{
			var value = GetAttributeValue(element, attributeName);
			if (value != null)
			{
				Int32 result;
				if (Int32.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
					return result;
			}
			return null;
		}

		public static Int32 GetAttributeAsInt(this XElement element, String attributeName, Int32 defaultValue)
		{
			var value = GetAttributeAsInt(element, attributeName);
			return (value.HasValue) ? value.Value : defaultValue;
		}

		private static object GetAttributeValue(XElement element, String attributeName)
		{
			if (element.Attribute(attributeName) != null)
			{
				return element.Attribute(attributeName).Value;
			}
			return null;
		}
	}
}
