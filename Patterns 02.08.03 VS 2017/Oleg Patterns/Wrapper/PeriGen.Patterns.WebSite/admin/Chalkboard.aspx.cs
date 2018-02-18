using System;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Net;
using System.Text;
using System.Globalization;

namespace PeriGen.Patterns.WebSite.admin
{
	public partial class Chalkboard : System.Web.UI.Page
	{
		/// <summary>
		/// Static initialisation
		/// </summary>
		static Chalkboard()
		{
			//SettingsManager.AutomaticReloadSettings = true;
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

				if (!Request.IsLocal && !PatternsWebSiteSettings.Instance.DisableAdminAPILocalIPValidation)
				{
                    String str666 = String.Format("<center><span style=\"font-weight: bold;font-size: 22px;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;color: #FEFFFF;\">PeriGen {0}{1} {2}™ Chalkboard</span><br /><br /><br /><br /><span style=\"font-weight: bold;font-size: 22px;font-family: 'Lucida Sans Unicode', 'Lucida Grande', Sans-Serif;color: #FF0000;\">You are not authorized to see this page.</span></center>", PatternsWebSiteSettings.Instance.BrandName, PatternsWebSiteSettings.Instance.BrandMarkTM ? "™" : "®", PatternsWebSiteSettings.Instance.ApplicationName);
					Response.Write(str666);
					return;
				}

				//check query string
				if (Request.QueryString.Count > 0 && Request.QueryString.HasKeys())
				{
					//check action
					string action = Request.QueryString["action"];
					if (!string.IsNullOrEmpty(action))
					{
						if (action.ToLowerInvariant() == "close")
						{
							string value = Request.QueryString["id"];
							if (!string.IsNullOrEmpty(value))
							{
								try
								{
									// Create the web request
                                    var url = new Uri(new Uri(string.Format(CultureInfo.InvariantCulture, PatternsWebSiteSettings.Instance.DataFeedURL, Environment.MachineName)),
												        string.Format(CultureInfo.InvariantCulture, "close/{0}", value));

									var request = WebRequest.Create(url);
									request.Method = "POST";
									request.ContentLength = 0;
									request.ContentType = "application/x-www-form-urlencoded";

									// Get response
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
									DisplayError(ex);
									return;
								}
							}
						}
						else if (action.ToUpperInvariant() == "CURVE")
						{
							string id = Request.QueryString["id"];
							if (!string.IsNullOrEmpty(id))
							{
								try
								{
									// Add parameters
									var builder = new StringBuilder();

									builder.Append(HttpUtility.UrlPathEncode(PatternsWebSiteSettings.Instance.UrlOpenCurve));
									builder.Append("visit_key="); builder.Append(HttpUtility.UrlEncode(id));
									builder.Append("&user_id="); builder.Append(HttpUtility.UrlEncode("PeriGen"));
									builder.Append("&user_name="); builder.Append(HttpUtility.UrlEncode("PeriGen Support"));
									builder.Append("&can_modify="); builder.Append("False");
									builder.Append("&can_print="); builder.Append("False");

									var target = ResolveUrl(builder.ToString());
									Response.Redirect(target);
									return;
								}
								catch (Exception ex)
								{
									//display error
									DisplayError(ex);
									return;
								}
							}
						}
						else if (action.ToUpperInvariant() == "PATTERNS")
						{
							string id = Request.QueryString["id"];
							if (!string.IsNullOrEmpty(id))
							{
								try
								{
									// Add parameters
									var builder = new StringBuilder();

									builder.Append(HttpUtility.UrlPathEncode(PatternsWebSiteSettings.Instance.UrlOpenPatterns));
									builder.Append("visit_key="); builder.Append(HttpUtility.UrlEncode(id));
									builder.Append("&user_id="); builder.Append(HttpUtility.UrlEncode("PeriGen"));
									builder.Append("&user_name="); builder.Append(HttpUtility.UrlEncode("PeriGen Support"));
									builder.Append("&can_modify="); builder.Append("False");
									builder.Append("&can_print="); builder.Append("False");

									var target = ResolveUrl(builder.ToString());
									Response.Redirect(target);
									return;
								}
								catch (Exception ex)
								{
									//display error
									DisplayError(ex);
									return;
								}
							}

						}
					}
					//refresh page
					Response.Redirect("Chalkboard.aspx");
				}
				else
				{
					try
					{
						this.PlaceHolderChart.Controls.Add(new LiteralControl(GetChalkboard()));
					}
					catch (Exception ex)
					{
						DisplayError(ex);
						return;
					}
				}
			}
			else
			{
				try
				{
					this.PlaceHolderChart.Controls.Add(new LiteralControl(GetChalkboard()));
				}
				catch (Exception ex)
				{
					DisplayError(ex);
					return;
				}
			}
		}

		private void DisplayError(Exception ex)
		{
            String errorStr = "<center><span style=\"font-weight: bold;font-size: 22px;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;color: #FEFFFF;\">PeriGen " + PatternsWebSiteSettings.Instance.BrandName + (PatternsWebSiteSettings.Instance.BrandMarkTM ? "™" : "®") + " " + PatternsWebSiteSettings.Instance.ApplicationName + "™ Chalkboard</span><br/><br /><br /><br /><span style=\"font-weight: bold;font-size: 12px;font-family: 'Lucida Sans Unicode', 'Lucida Grande', Sans-Serif;color: #FF0000;\">Error performing requested action. Details: " + ex.ToString() + "</span><br/><br/><a href=\"" + ResolveUrl("Chalkboard.aspx") + "\">Click here to reload the page </a></center>";
			Response.Write(errorStr);
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
                var url = new Uri(new Uri(string.Format(CultureInfo.InvariantCulture, PatternsWebSiteSettings.Instance.DataFeedURL, Environment.MachineName)), "patients");

				var request = WebRequest.Create(url);

				XElement xmlPatient = null;

				// Get response
				using (var response = request.GetResponse() as HttpWebResponse)
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					// load the patients (chalkboard)
					xmlPatient = XElement.Parse(reader.ReadToEnd());
					reader.Close();
					response.Close();
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

				//Add validations for apps
				xmlPatient.SetAttributeValue("CurveEnabled", PatternsWebSiteSettings.Instance.CurveEnabled.ToString().ToLowerInvariant());
				xmlPatient.SetAttributeValue("PatternsEnabled", PatternsWebSiteSettings.Instance.PatternsEnabled.ToString().ToLowerInvariant());

				// Create settings for page.
				var sb = new StringBuilder();
				using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto, CloseOutput = true }))
				{
					// Load the style sheet.
					XslCompiledTransform xslt = new XslCompiledTransform();
					using (var stream = new StreamReader(Server.MapPath("Chalkboard.xslt")))
					using (var reader = XmlReader.Create(stream))
					{
						xslt.Load(reader);
					}

					// Execute the transform and output the results to a writer.
					xslt.Transform(xmlPatient.CreateNavigator(), writer);
					writer.Flush();
				}
				return sb.ToString();
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Exception while constructing the chalkboard", ex);
			}
		}
	}
}
