using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.Diagnostics;
using System.Xml.Linq;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Interface.Demographics
{
	public class Patient
	{
		#region Settings

		static string Settings_ChartDateFormat = SettingsManager.GetValue("OBLinkChartColumnDateFormat");
		static string Settings_EDDDateFormat = SettingsManager.GetValue("OBLinkEDDDateFormat");
		static bool Settings_TraceExtendedInformationWithPHIData = SettingsManager.GetBoolean("TraceExtendedInformationWithPHIData");

		#endregion

		/// <summary>
		/// Ctor
		/// </summary>
		public Patient()
		{
			this.TrailList = new PatientTrailItemList();
		}

		/// <summary>
		/// MRN
		/// </summary>
		public string MRN { get; set; }

		/// <summary>
		/// MRN crc to validate that the MRN retrieved from the OBLink matches the current one
		/// </summary>
		[XmlIgnore]
		public string MRNCRC { get; set; }

		/// <summary>
		/// Bed id
		/// </summary>
		public String BedId { get; set; }

		/// <summary>
		/// Bed name
		/// </summary>
		public String BedName { get; set; }

		/// <summary>
		/// Unit name
		/// </summary>
		public String UnitName { get; set; }

		/// <summary>
		/// Number of fetuses
		/// </summary>
		public int? Fetuses { get; set; }

		/// <summary>
		/// Number of fetuses as a string to use it when getting the data from OBLink using the SetProperty...
		/// </summary>
		[XmlIgnore]
		public string FetusesAsString
		{
			get
			{
				return this.Fetuses.HasValue ? this.Fetuses.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
			}
			set
			{
				int fetusCount;

				if (!string.IsNullOrEmpty(value))
				{
					// Check values before try parsing. GE Server can provide additional information like this:
					// 1|Data stored by CPN on behalf of user
					// We need to remove everything after the '|' character
					var index = value.IndexOf('|');
					if (index > -1)
					{
						value = value.Substring(0, index);
					}
					if ((!string.IsNullOrEmpty(value)) && (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out fetusCount)))
					{
						this.Fetuses = fetusCount;
					}
					else
					{
						this.Fetuses = null;
					}
				}
				else
				{
					this.Fetuses = null;
				}
			}
		}

		/// <summary>
		/// The EDD (estimated date of delivery) as a string, directly from GE
		/// </summary>
		public DateTime? EDD { get; set; }

		/// <summary>
		/// The EDD as a string to use it when getting the data from OBLink using the SetProperty...
		/// </summary>
		[XmlIgnore]
		public string EDDAsString
		{
			get
			{
				return this.EDD.HasValue ? this.EDD.Value.ToString(Settings_EDDDateFormat, CultureInfo.InvariantCulture) : string.Empty;
			}
			set
			{
				DateTime time;
				if (!string.IsNullOrEmpty(value))
				{
					// Check values before try parsing. GE Server can provide additional information like this:
					// 1|Data stored by CPN on behalf of user
					// We need to remove everything after the '|' character
					var index = value.IndexOf('|');
					if (index > -1)
					{
						value = value.Substring(0, index);
					}
					if ((!string.IsNullOrEmpty(value)) && (DateTime.TryParseExact(value, Settings_EDDDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out time)))
					{
						this.EDD = time;
					}
					else
					{
						this.EDD = null;
					}
				}
				else
				{
					this.EDD = null;
				}
			}
		}

		string _ADTLog;

		/// <summary>
		/// The ADT log for that patient
		/// </summary>
		public string ADTLog
		{
			get
			{
				return this._ADTLog;
			}

			set
			{
				this._ADTLog = value;

				// Count the number of merge
				int index = -1;
				int mergeStarted = 0;
				while ((index = value.IndexOf("MERGE:", index + 1, StringComparison.OrdinalIgnoreCase)) != -1)
				{
					++mergeStarted;
				}

				// Count the number of merge completed
				index = -1; 
				int mergeCompleted = 0;
				while ((index = value.IndexOf("Async merge complete.", index + 1, StringComparison.OrdinalIgnoreCase)) != -1)
				{
					++mergeCompleted;
				}

				this.IsMergeInProgress = mergeStarted > mergeCompleted * 2;
			}
		}

		/// <summary>
		/// Check the ADT log to see if a merge operation is in progress
		/// </summary>
		public bool IsMergeInProgress {get; set;}

		/// <summary>
		/// Name of the patient
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Historic of ADT log (at X time, the patient was in bed Y, unit Z)
		/// </summary>
		public PatientTrailItemList TrailList { get; set; }

		/// <summary>
		/// Propulates trail data for the patient
		/// </summary>
		/// <param name="strDoc"></param>
		public bool PopulateTrailList(String strDoc)
		{
			// Clear the list
			this.TrailList.Clear();

			// Fix table
			strDoc = HttpUtility.HtmlDecode(strDoc).Replace("\r\n", string.Empty);

			// Extract table
			int start = strDoc.IndexOf("<table border=\"1\"", StringComparison.OrdinalIgnoreCase);
			int end = strDoc.IndexOf("</table>", StringComparison.OrdinalIgnoreCase) + 8;

			if (start <= -1 || end <= -1)
			{
				return strDoc.IndexOf("Required CPN Chart items contain no data.", StringComparison.OrdinalIgnoreCase) != -1;
			}

			var doc = new XmlDocument();
			doc.LoadXml(strDoc.Substring(start, end - start));

			var tableNode = doc.DocumentElement;
			if (!tableNode.HasChildNodes)
			{
				// No data
				return false;
			}

			var firstRow = tableNode.ChildNodes[0];

			// Scan td and fix them
			int index = 0;
			while (index < firstRow.ChildNodes.Count)
			{
				XmlNode td = firstRow.ChildNodes[index];

				// Fix RowSpan
				if (td.Attributes["rowspan"] != null
					&& !String.IsNullOrEmpty(td.Attributes["rowspan"].Value)
					&& (Convert.ToInt32(td.Attributes["rowspan"].Value, CultureInfo.InvariantCulture) > 1))
				{
					int value = Convert.ToInt32(td.Attributes["rowspan"].Value, CultureInfo.InvariantCulture);
					int row = 0;
					td.Attributes["rowspan"].Value = "1";
					XmlNode tdToInsert = td.CloneNode(true);
					tdToInsert.Attributes.RemoveNamedItem("rowspan");
					while (value > 1)
					{
						++row;
						XmlNode RowtoFix = tableNode.ChildNodes[row];
						RowtoFix.InsertBefore(tdToInsert.CloneNode(true), RowtoFix.ChildNodes[0]);
						--value;
					}
				}

				// Fix ColumnSpan
				if (td.Attributes["colspan"] != null
					&& !String.IsNullOrEmpty(td.Attributes["colspan"].Value)
					&& (Convert.ToInt32(td.Attributes["colspan"].Value, CultureInfo.InvariantCulture) > 1))
				{
					int value = Convert.ToInt32(td.Attributes["colspan"].Value, CultureInfo.InvariantCulture);
					int col = index;
					td.Attributes["colspan"].Value = "1";
					XmlNode tdToInsert = td.CloneNode(true);
					while (value > 1)
					{
						firstRow.InsertAfter(doc.ImportNode(tdToInsert, (true)).CloneNode(true), firstRow.ChildNodes[col]);
						--value;
						++col;
					}
				}

				index++;
			}

			// Map captions and indexes
			var Indexes = new Dictionary<string, int>();
			var rows = tableNode.ChildNodes.Count;
			for (int i = 2; i < rows; i++)
			{
				Indexes.Add(tableNode.ChildNodes[i].ChildNodes[0].InnerText, i);
			}

			var columns = tableNode.ChildNodes[0].ChildNodes.Count;
			for (int i = 1; i < columns; i++)
			{
				// Verify if there is a date
				if (!string.IsNullOrEmpty(tableNode.ChildNodes[0].ChildNodes[i].InnerText.Trim()))
				{
					string text = (tableNode.ChildNodes[0].ChildNodes[i].InnerText).Trim() + " " + tableNode.ChildNodes[1].ChildNodes[i].InnerText.Trim();

					DateTime date;
					if (DateTime.TryParseExact(text, Settings_ChartDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
					{
						PatientTrailItem item = new PatientTrailItem { Time = date };

						foreach (var key in Indexes.Keys)
						{
							var idx = Indexes[key];
							var prop = FieldsMapping.GetMappingValue(key);
							if (!string.IsNullOrEmpty(prop))
							{
								item.SetPropertyValue(prop, tableNode.ChildNodes[idx].ChildNodes[i].InnerText);
							}
						}
						this.TrailList.Add(item);
					}
					else
					{
						Common.Source.TraceEvent(TraceEventType.Warning, 6001, "Chalkboard: Unable to parse the time '{0}' for the audit trail with the configure time format '{1}'.\nPatient: {2}", text, Settings_ChartDateFormat, this);
					}
				}
			}

			// Make sure we properly decoded the values returned by OBLink
			if ((this.TrailList.Count == 0) || (this.TrailList.Any(a => (a.BedName == null) || (a.UnitName == null))))
			{
				Common.Source.TraceEvent(TraceEventType.Warning, 6002, "Chalkboard: Some errors happened while parsing the OBLink TrailLog and we may have an incomplete picture of the data.\nPatient: {0}", this);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Populates standard data for the patient
		/// </summary>
		/// <param name="strDoc"></param>
		public bool PopulateData(String strDoc)
		{
			//clean html
			strDoc = HttpUtility.HtmlDecode(strDoc).Replace("\r\n", string.Empty).Replace("\n", string.Empty);

			//extract table
			int start = strDoc.IndexOf("<table", StringComparison.OrdinalIgnoreCase);
			int end = strDoc.IndexOf("</table>", StringComparison.OrdinalIgnoreCase) + 8;

			if (start <= -1 || end <= -1)
			{
				return false;
			}

			//table
			strDoc = strDoc.Substring(start, end - start);
			start = strDoc.IndexOf("<tr", StringComparison.OrdinalIgnoreCase);
			end = strDoc.IndexOf("</tr>", StringComparison.OrdinalIgnoreCase) + 5;
			while (start > -1 && end > -1)
			{
				//Row with Data
				string row = strDoc.Substring(start, end - start);

				//find key
				int startKey = row.IndexOf("<b>", StringComparison.OrdinalIgnoreCase) + 3;
				int endKey = row.IndexOf("</b>", StringComparison.OrdinalIgnoreCase);

				//check for data
				if (startKey > -1 && endKey > -1)
				{
					//Ok
					string key = row.Substring(startKey, endKey - startKey);

					//find value
					int startValue = row.IndexOf("value=", StringComparison.OrdinalIgnoreCase) + 7;
					int endValue = row.IndexOf("</td>", startValue, StringComparison.OrdinalIgnoreCase) - 2;
					string value = row.Substring(startValue, endValue - startValue).Trim();

					//Set property value                                                
					var prop = FieldsMapping.GetMappingValue(key);
					if (!string.IsNullOrEmpty(prop))
					{
						this.SetPropertyValue(prop, value);
					}
				}

				//Remove oldData
				strDoc = strDoc.Substring(end);

				//Find Another Row
				start = strDoc.IndexOf("<tr", StringComparison.OrdinalIgnoreCase);
				end = strDoc.IndexOf("</tr>", StringComparison.OrdinalIgnoreCase) + 5;
			}

			// Validation of proper retrieval of fields
			if ((string.IsNullOrEmpty(this.BedName)) || (string.IsNullOrEmpty(this.UnitName)))
			{
				Common.Source.TraceEvent(TraceEventType.Warning, 6003, "Chalkboard: Some errors happened while parsing the OBLink Data chart and we may have an incomplete picture of the data.\nPatient: {0}", this);
				return false;
			}

			// Make sure we properly decoded the values returned by OBLink
			if (string.IsNullOrEmpty(this.MRNCRC) || (string.CompareOrdinal(this.MRN, this.MRNCRC) != 0))
			{
				Common.Source.TraceEvent(TraceEventType.Warning, 6004, "Chalkboard: Invalid MRN returned by the query on OBLink, make sure there is no other user on that OBLink server!\nWe may have an incomplete picture of the data.\nPatient: {0}", this);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Populates ADTLog data for the patient
		/// </summary>
		/// <param name="strDoc"></param>
		/// <returns></returns>
		public bool PopulateADTLogList(String strDoc)
		{
			var list = new List<PatientADTLogItem>();

			// Fix table
			strDoc = HttpUtility.HtmlDecode(strDoc).Replace("\r\n", string.Empty);

			// Extract table
			int start = strDoc.IndexOf("<table border=\"1\"", StringComparison.OrdinalIgnoreCase);
			int end = strDoc.IndexOf("</table>", StringComparison.OrdinalIgnoreCase) + 8;

			if (start <= -1 || end <= -1)
			{
				return strDoc.IndexOf("Required CPN Chart items contain no data.", StringComparison.OrdinalIgnoreCase) != -1;
			}

			var doc = new XmlDocument();
			doc.LoadXml(strDoc.Substring(start, end - start));

			var tableNode = doc.DocumentElement;
			if (!tableNode.HasChildNodes)
			{
				// No data
				return false;
			}

			var firstRow = tableNode.ChildNodes[0];

			// Scan td and fix them
			int index = 0;
			while (index < firstRow.ChildNodes.Count)
			{
				// Fix RowSpan
				XmlNode td = firstRow.ChildNodes[index];

				if (td.Attributes["rowspan"] != null
					&& !String.IsNullOrEmpty(td.Attributes["rowspan"].Value)
					&& (Convert.ToInt32(td.Attributes["rowspan"].Value, CultureInfo.InvariantCulture) > 1))
				{
					int value = Convert.ToInt32(td.Attributes["rowspan"].Value, CultureInfo.InvariantCulture);
					int row = 0;
					td.Attributes["rowspan"].Value = "1";
					XmlNode tdToInsert = td.CloneNode(true);
					tdToInsert.Attributes.RemoveNamedItem("rowspan");
					while (value > 1)
					{
						++row;
						XmlNode RowtoFix = tableNode.ChildNodes[row];
						RowtoFix.InsertBefore(tdToInsert.CloneNode(true), RowtoFix.ChildNodes[0]);
						--value;
					}
				}

				// Fix ColumnSpan
				if (td.Attributes["colspan"] != null
					&& !String.IsNullOrEmpty(td.Attributes["colspan"].Value)
					&& (Convert.ToInt32(td.Attributes["colspan"].Value, CultureInfo.InvariantCulture) > 1))
				{
					int value = Convert.ToInt32(td.Attributes["colspan"].Value, CultureInfo.InvariantCulture);
					int col = index;
					td.Attributes["colspan"].Value = "1";
					XmlNode tdToInsert = td.CloneNode(true);
					while (value > 1)
					{
						firstRow.InsertAfter(doc.ImportNode(tdToInsert, (true)).CloneNode(true), firstRow.ChildNodes[col]);
						--value;
						++col;
					}
				}

				index++;
			}

			// Map captions and indexes
			var Indexes = new Dictionary<string, int>();
			var rows = tableNode.ChildNodes.Count;
			for (int i = 2; i < rows; i++)
			{
				Indexes.Add(tableNode.ChildNodes[i].ChildNodes[0].InnerText, i);
			}

			var columns = tableNode.ChildNodes[0].ChildNodes.Count;
			for (int i = 1; i < columns; i++)
			{
				// Verify if there is a date
				if (!string.IsNullOrEmpty(tableNode.ChildNodes[0].ChildNodes[i].InnerText.Trim()))
				{
					string text = tableNode.ChildNodes[0].ChildNodes[i].InnerText.Trim() + " " + tableNode.ChildNodes[1].ChildNodes[i].InnerText.Trim();

					DateTime date;
					if (DateTime.TryParseExact(text, Settings_ChartDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
					{
						PatientADTLogItem item = new PatientADTLogItem { Time = date };

						foreach (var key in Indexes.Keys)
						{
							var idx = Indexes[key];
							var prop = FieldsMapping.GetMappingValue(key);
							if (prop.Length > 0) item.SetPropertyValue(prop, tableNode.ChildNodes[idx].ChildNodes[i].InnerText);
						}
						list.Add(item);
					}
					else
					{
						Common.Source.TraceEvent(TraceEventType.Warning, 6005, "Chalkboard: Unable to parse the time '{0}' for the ADT log with the configure time format '{1}'.\nPatient: {2}", text, Settings_ChartDateFormat, this);
					}
				}
			}

			// What we really care about is the last column value
			this.ADTLog = list.Count == 0 ? string.Empty : list.Last().ADTLog;

			return true;
		}

		/// <summary>
		/// For nice traces
		/// </summary>
		/// <returns></returns>
		public string FullDescription
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, "mrn='{0}' name='{1}' bed_id='{2}' bed='{3}-{4}' edd='{5}' fetus='{6}'", Settings_TraceExtendedInformationWithPHIData ? this.MRN : "n/a", Settings_TraceExtendedInformationWithPHIData ? this.Name : "n/a", this.BedId, this.UnitName, this.BedName, this.EDDAsString, this.FetusesAsString);
			}
		}
		
		/// <summary>
		/// For nice traces
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "mrn='{0}' bed_id='{1}'", Settings_TraceExtendedInformationWithPHIData ? this.MRN : "n/a", this.BedId);
		}

		#region Custom Serialization

		/// <summary>
		/// Serialize episode in XML string
		/// </summary>
		/// <returns></returns>
		public string WriteToXml()
		{
			var sb = new StringBuilder();
			using (var xw = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto }))
			{
				xw.WriteStartDocument();

				// call writer
				this.WriteToXml(xw);

				///End document
				xw.WriteEndDocument();
				xw.Flush();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Serialize episode in writer
		/// </summary>
		/// <param name="xw"></param>
		public void WriteToXml(XmlWriter xw)
		{
			// Start patient element
			xw.WriteStartElement("Patient");

			// Attributes
			xw.WriteAttributeString("MRN", this.MRN);
			xw.WriteAttributeString("BedId", this.BedId);
			xw.WriteAttributeString("BedName", this.BedName);
			xw.WriteAttributeString("UnitName", this.UnitName);
			xw.WriteAttributeString("Name", this.Name);
			if (this.Fetuses.HasValue) xw.WriteAttributeString("Fetuses", this.Fetuses.Value.ToString(CultureInfo.InvariantCulture));
			if (this.EDD.HasValue) xw.WriteAttributeString("EDD", this.EDD.Value.ToString("s", CultureInfo.InvariantCulture));

			// TrailList
			xw.WriteStartElement("TrailList");
			foreach (var item in this.TrailList)
			{
				xw.WriteStartElement("Item");
				xw.WriteAttributeString("Time", item.Time.ToString("s", CultureInfo.InvariantCulture));
				xw.WriteAttributeString("BedName", item.BedName);
				xw.WriteAttributeString("UnitName", item.UnitName);
				xw.WriteEndElement();
			}
			xw.WriteEndElement();

			// End Patient element
			xw.WriteEndElement();
			xw.Flush();

			// Merge in progress info and ADTLog info are not needed in database
		}

		/// <summary>
		/// initialize episode from XElement
		/// </summary>
		/// <param name="element"></param>
		public void ReadFromXml(XElement element)
		{
			// Attributes
			this.MRN = element.Attribute("MRN").Value;
			this.BedId = element.Attribute("BedId").Value;
			this.BedName = element.Attribute("BedName").Value;
			this.UnitName = element.Attribute("UnitName").Value;
			this.Name = element.Attribute("Name").Value;
			this.Fetuses = element.Attribute("Fetuses") == null ? null : (int?)(int.Parse(element.Attribute("Fetuses").Value, CultureInfo.InvariantCulture));
			this.EDD = element.Attribute("EDD") == null ? null : (DateTime?)(DateTime.ParseExact(element.Attribute("EDD").Value, "s", CultureInfo.InvariantCulture));

			// Trails
			this.TrailList.Clear();
			if (element.Element("TrailList") != null)
			{
				var trails = element.Element("TrailList").Elements("Item");
				if (trails != null)
				{
					this.TrailList.AddRange(trails.Select(item =>
							new PatientTrailItem
							{
								Time = DateTime.ParseExact(item.Attribute("Time").Value, "s", CultureInfo.InvariantCulture),
								BedName = item.Attribute("BedName").Value,
								UnitName = item.Attribute("UnitName").Value
							}));
				}
			}
		}

		/// <summary>
		/// Initialize episode from xmlstring
		/// </summary>
		/// <param name="data"></param>
		public void ReadFromXml(string data)
		{
			this.ReadFromXml(XElement.Parse(data));
		}

		#endregion
	}
}
