using System;
using System.Globalization;
using System.Xml.Linq;

namespace PeriGen.Patterns.Service
{
	/// <summary>
	/// Allow to perform a data request on one patient, contains id of the patient (either Id or Key) and
	/// the last Id already known of curve snapshot
	/// </summary>
	public class CurveRequestHeader
	{
		/// <summary>
		/// That current instance of server unique ID
		/// </summary>
		static string ServerInstanceUniqueID = ((int)DateTime.UtcNow.Ticks).ToString("x", CultureInfo.InvariantCulture);

		/// <summary>
		/// Patient Key
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Patient Id
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Snapshot id (to retrieve a specific snapshot)
		/// </summary>
		public string Snapshot { get; set; }

		/// <summary>
		/// Id of last snapshot sent by the service
		/// </summary>
		public string LastSnapshot { get; set; }
	  
		/// <summary>
		/// Default constructor
		/// </summary>
		public CurveRequestHeader()
		{
			this.Reset();
		}

		/// <summary>
		/// Reset pointers
		/// </summary>
		public void Reset()
		{
			this.Id = null;
			this.Snapshot = null;
			this.LastSnapshot = null;
		}

		/// <summary>
		/// Constructor from an xml element
		/// </summary>
		/// <param name="element"></param>
		public CurveRequestHeader(XElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			if (element.Attribute("key") != null)
				this.Key = element.Attribute("key").Value;

			if (element.Attribute("id") != null)
				this.Id = element.Attribute("id").Value;

			if (element.Attribute("snapshot") != null)
				this.Snapshot = element.Attribute("snapshot").Value;

			if (element.Attribute("last") != null)
				this.LastSnapshot = element.Attribute("last").Value;

			// Make sure the server unique ID is the proper one!
			if ((element.Attribute("serveruid") != null)
				&& (string.CompareOrdinal(element.Attribute("serveruid").Value??string.Empty, ServerInstanceUniqueID) != 0))
			{
				this.Reset();
			}
		}

		/// <summary>
		/// Serialize as an XElement
		/// </summary>
		/// <returns></returns>
		public XElement SerializeXml()
		{
			return new XElement("request",
				new XAttribute("key", this.Key),
				new XAttribute("id", this.Id ?? "-1"),
				new XAttribute("snapshot", this.Snapshot ?? string.Empty),
				new XAttribute("last", this.LastSnapshot ?? "-1"),
				new XAttribute("serveruid", ServerInstanceUniqueID));
		}
	}
}
