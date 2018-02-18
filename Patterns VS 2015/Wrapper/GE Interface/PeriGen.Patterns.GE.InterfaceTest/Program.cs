using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace PeriGen.Patterns.GEInterfaceTest
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Create the windows event log source entry
			try
			{
				if (!EventLog.SourceExists("PeriGen Patterns Interface"))
				{
					EventLog.CreateEventSource("PeriGen Patterns Interface", "Application");
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(String.Format("Warning, unable to create the log source.\n{0}", e));
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new frmMain());
		}
	}
}
