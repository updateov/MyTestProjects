using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace PeriGen.Patterns.Service
{
	/// <summary>
	/// Extension to make code easier to read
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Convert a string returned by PODI to an EpisodeStatuses value
		/// </summary>
		public static PeriGen.Patterns.Service.PatternsEpisode.PODIVisitStatus ToPODIVisitStatus(this string value)
		{
			try
			{
				return (PeriGen.Patterns.Service.PatternsEpisode.PODIVisitStatus)Enum.ToObject(typeof(PeriGen.Patterns.Service.PatternsEpisode.PODIVisitStatus), int.Parse(value, CultureInfo.InvariantCulture));
			}
			catch (Exception)
			{
				Debug.Assert(false);

				Common.Source.TraceEvent(TraceEventType.Verbose, 4100, string.Format(CultureInfo.InvariantCulture, "Unrecognised PODI visit status: {0}", value));
				return PeriGen.Patterns.Service.PatternsEpisode.PODIVisitStatus.Unknown;
			}
		}

		/// <summary>
		/// Helper to convert an attribute to a bool?
		/// </summary>
		/// <param name="node"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool? AttributeToBoolean(this XElement node, string name)
		{
			if (node == null)
				return null;

			var attribute = node.Attribute(name);
			if (attribute == null)
				return null;

			if (string.IsNullOrEmpty(attribute.Value))
				return null;

			bool result;
			if (!bool.TryParse(attribute.Value, out result))
			{
				Debug.Assert(false);
				return null;
			}

			return (bool?)result;
		}

		/// <summary>
		/// Helper to convert an attribute to an int?
		/// </summary>
		/// <param name="node"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static int? AttributeToInteger(this XElement node, string name)
		{
			if (node == null)
				return null;

			var attribute = node.Attribute(name);
			if (attribute == null)
				return null;

			if (string.IsNullOrEmpty(attribute.Value))
				return null;

			int result;
			if (!int.TryParse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
			{
				Debug.Assert(false);
				return null;
			}

			return (int?)result;
		}

		/// <summary>
		/// Helper to convert an attribute to an int?
		/// </summary>
		/// <param name="node"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static DateTime? AttributeToDateTime(this XElement node, string name)
		{
			if (node == null)
				return null;

			var attribute = node.Attribute(name);
			if (attribute == null)
				return null;

			if (string.IsNullOrEmpty(attribute.Value))
				return null;

			DateTime result;
			if (!DateTime.TryParseExact(attribute.Value, "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
			{
				Debug.Assert(false);
				return null;
			}

			return (DateTime?)result;
		}
	}
}
