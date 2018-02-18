using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Configuration;
using System.Globalization;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Web
{
	public partial class TestGEService : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				if (!Request.IsLocal && !SettingsManager.GetBoolean("DisableAdminAPILocalIPValidation"))
				{
					Response.Write("<center><span style=\"font-weight: bold;font-size: 22px;font-family: 'Lucida Sans Unicode', 'Lucida Grande', Sans-Serif;color: #FF0000;\">You are not authorized to see this page.</span></center>");
					return;
				}

				txtServerURL.Text = HttpUtility.UrlPathEncode(string.Format(CultureInfo.InvariantCulture, SettingsManager.GetValue("urlServiceFeed"), Environment.MachineName));
				if (Request.QueryString.Count > 0)
				{
					var mrn = Request.QueryString["id"];
					if (!string.IsNullOrEmpty(mrn))
					{
						txtPatientID.Text = mrn;
					}
				}

				int value;

				// Add parameters
				var cr_limit = SettingsManager.GetValue("cr_limit");
				if ((string.IsNullOrEmpty(cr_limit)) || (!int.TryParse(cr_limit, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					cr_limit = "10";

				var cr_window = SettingsManager.GetValue("cr_window");
				if ((string.IsNullOrEmpty(cr_window)) || (!int.TryParse(cr_window, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					cr_window = "10";

				var cr_stage1 = SettingsManager.GetValue("cr_stage1");
				if ((string.IsNullOrEmpty(cr_stage1)) || (!int.TryParse(cr_stage1, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					cr_stage1 = "5";

				var cr_stage2 = SettingsManager.GetValue("cr_stage2");
				if ((string.IsNullOrEmpty(cr_stage2)) || (!int.TryParse(cr_stage2, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					cr_stage2 = "30";

				var timeout_dlg = SettingsManager.GetValue("timeout_dlg");
				if ((string.IsNullOrEmpty(timeout_dlg)) || (!int.TryParse(timeout_dlg, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					timeout_dlg = "1";

				var refresh = SettingsManager.GetValue("refresh");
				if ((string.IsNullOrEmpty(refresh)) || (!int.TryParse(refresh, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)))
					refresh = "5";

				txtWinDuration.Text = cr_window;
				txtGraphMax.Text = cr_limit;
				txtStage1.Text = cr_stage1;
				txtStage2.Text = cr_stage2;
				txtTimeoutDlg.Text = timeout_dlg;
				txtRefresh.Text = refresh;
			}
		}

		string BuildURL()
		{
			var builder = new StringBuilder();

			builder.Append(HttpUtility.UrlPathEncode(SettingsManager.GetValue("urlOpenActiveX")));
			builder.Append("server_url="); builder.Append(HttpUtility.UrlEncode(txtServerURL.Text));
			builder.Append("&patient_id="); builder.Append(HttpUtility.UrlEncode(txtPatientID.Text));
			builder.Append("&user_id="); builder.Append(HttpUtility.UrlEncode(txtUserID.Text));
			builder.Append("&user_name="); builder.Append(HttpUtility.UrlEncode(txtUserName.Text));
			builder.Append("&permissions="); builder.Append(cmbPermissions.SelectedValue);
			builder.Append("&refresh="); builder.Append(txtRefresh.Text);
			builder.Append("&cr_limit="); builder.Append(txtGraphMax.Text);
			builder.Append("&cr_window="); builder.Append(txtWinDuration.Text);
			builder.Append("&cr_stage1="); builder.Append(txtStage1.Text);
			builder.Append("&cr_stage2="); builder.Append(txtStage2.Text);
			builder.Append("&timeout_dlg="); builder.Append(txtTimeoutDlg.Text);

			return builder.ToString();
		}

		protected void btnOpenPage_Click(object sender, EventArgs e)
		{
			lblResult.Text = ResolveUrl(BuildURL());
			Response.Redirect(lblResult.Text, true);
		}
	}
}
