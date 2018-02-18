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
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Web
{
	public partial class DefaultPage : System.Web.UI.Page
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		protected void Page_Load(object sender, EventArgs e)
		{
			if(!IsPostBack)
			{
				int value;

				var server_url = Request.QueryString["server_url"];
				if(string.IsNullOrEmpty(server_url))
					throw new Exception("Invalid parameters (Server URL missing)");

				var patient_id = Request.QueryString["patient_id"];
				if(string.IsNullOrEmpty(patient_id))
					throw new Exception("Invalid parameters (Patient ID missing)");

				var user_id = Request.QueryString["user_id"];
				if(string.IsNullOrEmpty(user_id))
					throw new Exception("Invalid parameters (User ID missing)");

				var user_name = Request.QueryString["user_name"];
				if(string.IsNullOrEmpty(user_name))
					throw new Exception("Invalid parameters (User name missing)");

				var permissions = Request.QueryString["permissions"];
				if(string.IsNullOrEmpty(permissions))
					permissions = "readonly";

				var refresh = Request.QueryString["refresh"];
				if((string.IsNullOrEmpty(refresh)) || (!int.TryParse(refresh, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					refresh = "5";

				var cr_limit = Request.QueryString["cr_limit"];
				if((string.IsNullOrEmpty(cr_limit)) || (!int.TryParse(cr_limit, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					cr_limit = "10";

				var cr_window = Request.QueryString["cr_window"];
				if((string.IsNullOrEmpty(cr_window)) || (!int.TryParse(cr_window, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					cr_window = "10";

				var cr_stage1 = Request.QueryString["cr_stage1"];
				if((string.IsNullOrEmpty(cr_stage1)) || (!int.TryParse(cr_stage1, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					cr_stage1 = "0";

				var cr_stage2 = Request.QueryString["cr_stage2"];
				if((string.IsNullOrEmpty(cr_stage2)) || (!int.TryParse(cr_stage2, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					cr_stage2 = "0";

				var timeout_dlg = Request.QueryString["timeout_dlg"];
				if ((string.IsNullOrEmpty(timeout_dlg)) || (!int.TryParse(timeout_dlg, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					timeout_dlg = "1";

				// not the compact form so create it from dedicated arguments
				var xml =
					new XElement("configuration",
						new XAttribute("server_url", server_url),
						new XAttribute("patient_id", patient_id),
						new XAttribute("user_id", user_id),
						new XAttribute("user_name", user_name),
						new XAttribute("permissions", permissions),
						new XAttribute("refresh", refresh),
						new XAttribute("cr_limit", cr_limit),
						new XAttribute("cr_window", cr_window),
						new XAttribute("cr_stage1", cr_stage1),
						new XAttribute("cr_stage2", cr_stage2),
						new XAttribute("banner", 2),
						new XAttribute("timeout_dlg", timeout_dlg));

				var arguments = HttpUtility.HtmlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(xml.ToString())));

				HtmlGenericControl objectTag = new HtmlGenericControl("object");
				objectTag.Attributes["name"] = "PatChart";
				objectTag.Attributes["classid"] = "CLSID:00172661-82A6-4C68-8585-D54E46A07CCF";
				objectTag.Attributes["height"] = "100%";
				objectTag.Attributes["codebase"] = "./PeriCALMPatternsOEMChart.cab#version=" + SettingsManager.GetValue("versionActiveX").Replace('.', ',');
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
}
