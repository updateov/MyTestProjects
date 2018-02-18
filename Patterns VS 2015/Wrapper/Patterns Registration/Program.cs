using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace PeriGen.Patterns.Engine.Registration
{
	static class Program
	{
		/// <summary>
		/// Check if the current user is a local administrator
		/// </summary>
		static bool CheckCurrentUserLocalAdministrator()
		{
			var identity = WindowsIdentity.GetCurrent();
			if (identity != null)
			{
				var principal = new WindowsPrincipal(identity);
				if (principal.IsInRole(WindowsBuiltInRole.Administrator))
					return true;
			}

			MessageBox.Show("You must belong to the local Administrators group in order to use that application.", "Security", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			if (!CheckCurrentUserLocalAdministrator())
				return;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new RegistrationForm());
		}
	}
}
