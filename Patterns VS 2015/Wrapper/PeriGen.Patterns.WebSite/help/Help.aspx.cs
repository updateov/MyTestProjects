using System;

namespace PeriGen.Patterns.WebSite
{
	public partial class Help : System.Web.UI.Page
	{
		/// <summary>
		/// Static initialisation
		/// </summary>
		static Help()
		{
			//SettingsManager.AutomaticReloadSettings = true;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!PatternsWebSiteSettings.Instance.PatternsEnabled)
			{
				PatternsDiv.Visible = false;
			}

            if (!PatternsWebSiteSettings.Instance.CurveEnabled)
			{
				CurveDiv.Visible = false;
			}
		}
	}
}