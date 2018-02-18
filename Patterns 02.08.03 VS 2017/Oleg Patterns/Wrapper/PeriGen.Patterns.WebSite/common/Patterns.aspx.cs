using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Globalization;
using System.Configuration;
using System.IO;
using PeriGen.Patterns.WebSite.common;

namespace PeriGen.Patterns.WebSite
{
	public partial class Patterns : System.Web.UI.Page
	{
		/// <summary>
		/// Static initialisation
		/// </summary>
		static Patterns()
		{
			//SettingsManager.AutomaticReloadSettings = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		protected void Page_Load(object sender, EventArgs e)
		{
			Page.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			if (IsPostBack)
				return;

			if (!PatternsWebSiteSettings.Instance.PatternsEnabled)
			{
				//Validate curve enabled
				Response.Write("<div style=\"border: 2px solid #000000; padding: 2px 10px 10px 10px; margin: 10px; background: #FFFFFF;\"><h1>" + PatternsWebSiteSettings.Instance.BrandName + (PatternsWebSiteSettings.Instance.BrandMarkTM ? "™" : "®") + " " + PatternsWebSiteSettings.Instance.ApplicationName + "™</h1><br/><h2><span style='color:red'>" + PatternsWebSiteSettings.Instance.BrandName + " " + PatternsWebSiteSettings.Instance.ApplicationName + " is not enabled.</span></h2></div>");
				Response.End();
				return;
			}

			// Get the inputData from the POST data
			string inputData = string.Empty;
			using (var reader = new StreamReader(Page.Request.InputStream))
			{
				inputData = reader.ReadToEnd();
			}

			string visit_key, user_id, user_name, can_modify;

			// If there is not post
			if (String.IsNullOrEmpty(inputData))
			{
				// Security. Check for localhost only
				if (!Request.IsLocal && !PatternsWebSiteSettings.Instance.DisableAdminAPILocalIPValidation)
				{
					Response.Write("<div style=\"border: 2px solid #000000; padding: 2px 10px 10px 10px; margin: 10px; background: #FFFFFF;\"><h1>" + PatternsWebSiteSettings.Instance.BrandName + (PatternsWebSiteSettings.Instance.BrandMarkTM ? "™" : "®") + " " + PatternsWebSiteSettings.Instance.ApplicationName + "™</h1><br/><h2><span style='color:red'>You are not authorized to see this page.</span></h2></div>");
					Response.End();
					return;
				}

				// When parameters are just in the url (when used from the chalkboard)
                visit_key = Request.QueryString["visit_key"];
				user_id = Request.QueryString["user_id"];
				user_name = Request.QueryString["user_name"];
				can_modify = Request.QueryString["can_modify"]; 

				if (string.IsNullOrEmpty(visit_key) || (string.IsNullOrEmpty(user_id) || string.IsNullOrEmpty(user_name) || string.IsNullOrEmpty(can_modify)))
				{
					Response.Write("<div style=\"border: 2px solid #000000; padding: 2px 10px 10px 10px; margin: 10px; background: #FFFFFF;\"><h1>" + PatternsWebSiteSettings.Instance.BrandName + (PatternsWebSiteSettings.Instance.BrandMarkTM ? "™" : "®") + " " + " " + PatternsWebSiteSettings.Instance.ApplicationName + "™</h1><br/><h2><span style='color:red'>Invalid parameters.</span></h2></div>");
					Response.End();
					return;
				}
			}
			else 
			{
				// When parameters are in a POST (when used from within CALM)

				// Decrypt it using appropriate password
				inputData = DecryptCALMPost.Decrypt(inputData, PatternsWebSiteSettings.Instance.URLPassword);

				var parameters = XDocument.Parse(inputData).Element("parameters").Attributes().ToDictionary(v => v.Name, v => v.Value);

				if ((!parameters.TryGetValue("visit_key", out visit_key))		|| (string.IsNullOrEmpty(visit_key))
					|| (!parameters.TryGetValue("user_id", out user_id))		|| (string.IsNullOrEmpty(user_id)
					|| (!parameters.TryGetValue("user_name", out user_name))	|| (string.IsNullOrEmpty(user_name)))
					|| (!parameters.TryGetValue("can_modify", out can_modify))	|| (string.IsNullOrEmpty(can_modify)))
				{
					Response.Write("<div style=\"border: 2px solid #000000; padding: 2px 10px 10px 10px; margin: 10px; background: #FFFFFF;\"><h1>" + PatternsWebSiteSettings.Instance.BrandName + (PatternsWebSiteSettings.Instance.BrandMarkTM ? "™" : "®") + " " + PatternsWebSiteSettings.Instance.ApplicationName + "™</h1><br/><h2><span style='color:red'>Invalid parameters.</span></h2></div>");
					Response.End();
					return;
				}
			}

            var xml =
                new XElement("configuration",
                    new XAttribute("server_url", string.Format(CultureInfo.InvariantCulture, PatternsWebSiteSettings.Instance.DataFeedURL, Environment.MachineName)),
                    new XAttribute("patient_id", visit_key),
                    new XAttribute("user_id", user_id),
                    new XAttribute("user_name", user_name),
                    new XAttribute("permissions", (string.CompareOrdinal(can_modify.ToUpperInvariant(), "TRUE") == 0 ? "fullaccess" : "readonly")),
                    new XAttribute("refresh", PatternsWebSiteSettings.Instance.PatternsClientRefresh),
                    new XAttribute("cr_limit", PatternsWebSiteSettings.Instance.CR_limit),
                    new XAttribute("cr_window", PatternsWebSiteSettings.Instance.CR_window),
                    new XAttribute("cr_stage1", PatternsWebSiteSettings.Instance.CR_stage1),
                    new XAttribute("cr_stage2", PatternsWebSiteSettings.Instance.CR_stage2),
                    new XAttribute("banner", PatternsWebSiteSettings.Instance.Banner),
                    new XAttribute("timeout_dlg", PatternsWebSiteSettings.Instance.PatternsClientTimeout));                   

			var arguments = HttpUtility.HtmlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(xml.ToString())));

			HtmlGenericControl objectTag = new HtmlGenericControl("object");
			objectTag.Attributes["name"] = "PatChart";
            objectTag.Attributes["classid"] = PatternsWebSiteSettings.Instance.EnableCheckList ? "CLSID:822D5CDC-3062-4C71-8FA5-7156E38B2F1C" : "CLSID:00172661-82A6-4C68-8585-D54E46A07CCF";
			objectTag.Attributes["height"] = "100%";
			//objectTag.Attributes["codebase"] = "./PeriCALMPatternsOEMChart.cab#version=" + SettingsManager.GetValue("VersionPatterns").Replace('.', ',');
            objectTag.Attributes["codebase"] = PatternsWebSiteSettings.Instance.ActiveXClient + "#version=" + PatternsWebSiteSettings.Instance.VersionPatterns.Replace('.', ',');
			objectTag.Attributes["width"] = "100%";
			objectTag.Attributes["border"] = "0";

			this.PlaceHolderChart.Controls.Add(objectTag);

			// Insert the javascript function that sets the active x parameter upon page load
			HtmlGenericControl patFunctions = new HtmlGenericControl("script");
			patFunctions.Attributes.Add("type", "text/vbscript");
			patFunctions.Attributes.Add("language", "VBScript");

			patFunctions.InnerHtml = "\r\n<!--\r\nSub Window_onLoad()\r\nPatChart.ConnectionData = \"" + arguments + "\"\r\nPatChart.focus()\r\nEnd Sub\r\n-->\r\n";
			this.Page.Controls.Add(patFunctions);
		}
	}
}
