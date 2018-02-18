using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PeriGen.Patterns.DecisionSupportAPI.TestTool
{
	/// <summary>
	/// Data holder for a visit
	/// </summary>
	public class VisitDataModel
	{
		public string VisitKey { get; private set; }
		public string ContractionsContext { get; private set; }

		public System.Data.DataTable ContractionsTable { get; private set; }

		/// <summary>
		/// Ctr
		/// </summary>
		public VisitDataModel()
		{
			this.ContractionsTable = new System.Data.DataTable("Contractions");

			this.ContractionsTable.Columns.Add("Time", typeof(DateTime));
			this.ContractionsTable.Columns.Add("TimeLocal", typeof(DateTime));
			this.ContractionsTable.Columns.Add("Count", typeof(int));
			this.ContractionsTable.Columns.Add("Updated", typeof(bool));

			this.ContractionsTable.PrimaryKey = new DataColumn[] { this.ContractionsTable.Columns["Time"] };

			this.SetKey(string.Empty);
		}

		/// <summary>
		/// Set the key of the visit
		/// </summary>
		/// <param name="visitKey"></param>
		public void SetKey(string visitKey)
		{
			this.VisitKey = visitKey;
			this.Clear();
		}

		/// <summary>
		/// Reset the currently loaded contractions/context if any
		/// </summary>
		public void Clear()
		{
			this.ContractionsContext = string.Empty;
			this.ContractionsTable.Rows.Clear();
		}

		/// <summary>
		/// Create and Send request to server using REST
		/// </summary>
		/// <param name="uri">url</param>
		/// <param name="body">data to send</param>
		/// <returns>data from server</returns>
		static string SendRequest(string uri, string body)
		{
			string responseBody = null;

			// Create WebRequest			
			var req = (HttpWebRequest)HttpWebRequest.Create(uri);

			// increase speed avoiding checking proxy....
			req.Proxy = null;

			// Config
			req.Method = "POST";
			req.Accept = "application/xml";
			req.ContentType = "application/xml; charset=utf-8";

			if (body != null)
			{
				byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
				req.GetRequestStream().Write(bodyBytes, 0, bodyBytes.Length);
				req.GetRequestStream().Close();
			}

			///Try to get Response
			var resp = (HttpWebResponse)req.GetResponse();

			Stream respStream = resp.GetResponseStream();
			if (respStream != null)
			{
				responseBody = new StreamReader(respStream).ReadToEnd();
			}
			return responseBody;
		}

		/// <summary>
		/// Get the data from the given address and settings
		/// </summary>
		/// <param name="address"></param>
		public void Query(string address, int blockDuration, int minimumUP)
		{
			if (string.IsNullOrEmpty(this.ContractionsContext))
			{
				this.ContractionsContext = new XElement("ctr_cnt", new XAttribute("block_duration", blockDuration), new XAttribute("block_minimum_up", minimumUP)).ToString(SaveOptions.DisableFormatting);
			}

			// Build the query
			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto }))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("request");

				writer.WriteStartElement("visit");
				writer.WriteAttributeString("key", this.VisitKey ?? string.Empty);
				writer.WriteRaw(this.ContractionsContext);
				writer.WriteEndElement();

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			// Execute the request
			var result = VisitDataModel.SendRequest(address, sb.ToString());
			if (string.IsNullOrEmpty(result))
				return;

			var patternsData = XElement.Parse(result);

			// Read the answer
			if (patternsData == null)
				throw new Exception("Empty response from the Patterns service");

			var response = patternsData.Element("visit");
			if (response == null)
				throw new Exception("No response for that visit from the Patterns service");

			// Reset the updated flag
			foreach (DataRow c in this.ContractionsTable.Rows)
			{
				c["Updated"] = false;
			}

			bool firstTime = this.ContractionsTable.Rows.Count == 0;

			var ctr_cnt = response.Element("ctr_cnt");
			if (ctr_cnt != null)
			{
				foreach (var ctr_block in ctr_cnt.Elements("ctr_block"))
				{
					var time = DateTime.ParseExact(ctr_block.Attribute("time").Value, "s", CultureInfo.InvariantCulture);
					var count = int.Parse(ctr_block.Attribute("value").Value, CultureInfo.InvariantCulture);

					var existing = firstTime ? null : this.ContractionsTable.Rows.Find(time);
					if (existing == null)
					{
						existing = this.ContractionsTable.NewRow();
						existing["Time"] = time;
						existing["TimeLocal"] = time.ToLocalTime();
						existing["Count"] = count;
						existing["Updated"] = true;
						this.ContractionsTable.Rows.Add(existing);
					}
					else
					{
						existing["Count"] = count;
						existing["Updated"] = true;
					}
				}

				ctr_cnt.RemoveNodes();
				this.ContractionsContext = ctr_cnt.ToString(SaveOptions.DisableFormatting);
			}
		}
	}
}
