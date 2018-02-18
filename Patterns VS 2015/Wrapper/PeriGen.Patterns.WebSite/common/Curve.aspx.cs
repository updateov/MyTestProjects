using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;

namespace PeriGen.Patterns.WebSite.common
{
	public partial class Curve : System.Web.UI.Page
	{
		/// <summary>
		/// Static initialisation
		/// </summary>
		static Curve()
		{
			//SettingsManager.AutomaticReloadSettings = true;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Page.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			if (IsPostBack)
				return;

			if (!PatternsWebSiteSettings.Instance.CurveEnabled)
			{
				Response.Write("<div style=\"border: 2px solid #000000; padding: 2px 10px 10px 10px; margin: 10px; background: #FFFFFF;\"><h1>PeriCALM® Curve™</h1><br/><h2><span style='color:red'>PeriCALM® Curve™ is not enabled.</span></h2></div>");
				Response.End();
				return;
			}

			var parameters = Page.ClientQueryString;

			// check if the parameters are send directly on the query (use case from the Chalkboard page) and if not, grab them from the POST (use case from the CALM client)
			if (string.IsNullOrEmpty(parameters))
			{
				// Get the parameters from the POST data
				var inputData = string.Empty;
				using (var reader = new StreamReader(Page.Request.InputStream))
				{
					inputData = reader.ReadToEnd();
				}
				if (!string.IsNullOrEmpty(inputData))
				{
					// Decrypt it using appropriate password
					inputData = DecryptCALMPost.Decrypt(inputData, PatternsWebSiteSettings.Instance.URLPassword);
				}

				var node = XElement.Parse(inputData);
				foreach (var attribute in node.Attributes())
				{
					if (parameters.Length > 0)
						parameters += "&";
					parameters += attribute.Name;
					parameters += "=";
					parameters += HttpUtility.UrlEncode(attribute.Value);
				}
			}

			// update parameters
			var builder = new StringBuilder(parameters);
            builder.Append("&service=");                    builder.Append(HttpUtility.UrlEncode(string.Format(PatternsWebSiteSettings.Instance.DataFeedURL, Environment.MachineName)));
			builder.Append("&banner=");						builder.Append(PatternsWebSiteSettings.Instance.Banner);
			builder.Append("&curve_client_refresh=");		builder.Append(PatternsWebSiteSettings.Instance.CurveClientRefresh);
			builder.Append("&curve_review_mode_enabled=");	builder.Append(PatternsWebSiteSettings.Instance.CurveReviewModeEnabled);
			builder.Append("&PODI_send_epidural=");			builder.Append(PatternsWebSiteSettings.Instance.PODISendEpidural);
			builder.Append("&PODI_send_vbac=");				builder.Append(PatternsWebSiteSettings.Instance.PODISendVBAC);
			builder.Append("&PODI_send_vaginaldelivery=");	builder.Append(PatternsWebSiteSettings.Instance.PODISendVaginalDelivery);

			Response.Redirect("CurveLoader.html?parameter=" + HttpUtility.UrlEncode(Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(builder.ToString()))));
		}
	}
}