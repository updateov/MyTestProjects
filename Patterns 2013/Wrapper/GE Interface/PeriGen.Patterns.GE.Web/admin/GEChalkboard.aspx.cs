using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Globalization;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Web
{
	public partial class GEChalkboard : System.Web.UI.Page
	{
		/// <summary>
		/// Static initialisation
		/// </summary>
		static GEChalkboard()
		{
			SettingsManager.AutomaticReloadSettings = true;
		}

		/// <summary>
		/// On page load, check if it is a request to close an episode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			//no postback
			if (!IsPostBack)
			{
				Response.Buffer = true;
				Response.CacheControl = "no-cache";
				Response.AddHeader("Pragma", "no-cache");
				Response.Expires = -1441;
				Response.Cache.SetCacheability(HttpCacheability.NoCache);
				Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
				Response.Cache.SetNoStore();

				if (!Request.IsLocal && !SettingsManager.GetBoolean("DisableAdminAPILocalIPValidation"))
				{
					Response.Write("<center><span style=\"font-weight: bold;font-size: 22px;font-family: 'Lucida Sans Unicode', 'Lucida Grande', Sans-Serif;color: #FF0000;\">You are not authorized to see this page.</span></center>");
					return;
				}

				//check query string
				if (Request.QueryString.Count > 0 && Request.QueryString.HasKeys())
				{
					//get Id to delete
					string value = Request.QueryString["id"];
					if (!string.IsNullOrEmpty(value))
					{
						try
						{
							//create request
							var url = string.Format(CultureInfo.InvariantCulture, SettingsManager.GetValue("urlCloseEpisode"), Environment.MachineName, value);
							var request = WebRequest.Create(url);
							request.Method = "POST";
							request.ContentLength = 0;
							request.ContentType = "application/x-www-form-urlencoded";

							//do method
							using (var response = (HttpWebResponse)request.GetResponse())
							{
								//catch error
								if (response.StatusCode != HttpStatusCode.OK)
								{
									string message = String.Format("POST failed. Received HTTP {0}", response.StatusCode);
									throw new ApplicationException(message);
								}
							}
						}
						catch (Exception ex)
						{
							//display error
							Response.Write("<span style=\"color: #ff0000;\">Error performing requested action. Details: " + ex.Message + "</span><br/><br/><a href=\"" + ResolveUrl("GEChalkboard.aspx") + "\">Click here to reload the page </a>");
							return;
						}
					}
					//refresh page
					Response.Redirect("GEChalkboard.aspx");
				}
				else
				{
					this.PlaceHolderChart.Controls.Add(new LiteralControl(GetChalkboard()));
				}
			}
		}

		/// <summary>
		/// Convert an attribute of type UTC time to local time
		/// </summary>
		/// <param name="attribute"></param>
		static void ConvertToLocalTime(XAttribute attribute)
		{
			if ((attribute != null) && (!string.IsNullOrEmpty(attribute.Value)))
			{
				DateTime date;
				if (DateTime.TryParseExact(attribute.Value, "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
				{
					attribute.Value = date.ToLocalTime().ToString("s");
				}
			}
		}

		/// <summary>
		/// Retrieve the chalkboard content
		/// </summary>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public string GetChalkboard()
		{
			try
			{
				// Create the web request  
				var url = string.Format(CultureInfo.InvariantCulture, SettingsManager.GetValue("urlChalkboard"), Environment.MachineName);
				var request = WebRequest.Create(url);

				XElement xmlPatient = null;

				// Get response  
				using (var response = request.GetResponse() as HttpWebResponse)
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					// load the patients (chalkboard)
					xmlPatient = XElement.Parse(reader.ReadToEnd());
				}

				if (xmlPatient == null)
				{
					return "No data available!";
				}

				// From UTC to local time conversion
				ConvertToLocalTime(xmlPatient.Attribute("LastRefreshed"));
				foreach (var episode in xmlPatient.DescendantsAndSelf("Episode"))
				{
					ConvertToLocalTime(episode.Attribute("Created"));
					ConvertToLocalTime(episode.Attribute("LastMonitored"));
				}

				// Create settings for page.
				var sb = new StringBuilder();
				using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto, CloseOutput = true }))
				{
					// Load the style sheet.
					XslCompiledTransform xslt = new XslCompiledTransform();
					using (var streamReader = new StreamReader(Server.MapPath("GEChalkboard.xslt")))
					using (var xmlReader = XmlReader.Create(streamReader))
					{
						xslt.Load(xmlReader);
					}

					// Execute the transform and output the results to a writer.
					xslt.Transform(xmlPatient.CreateNavigator(), writer);
					writer.Flush();
				}
				return sb.ToString();
			}
			catch (Exception ex)
			{
				return string.Format("Exception while constructing the chalkboard:\n{0}", ex);
			}
		}
	}
}
