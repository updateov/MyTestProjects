using System.Windows;
using System.Windows.Threading;

namespace PeriCALM.Patterns.Curve.UI.Chart
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

        protected override void OnStartup(StartupEventArgs e)
		{
			//hook exceptions
			Dispatcher.UnhandledException += DispatcherUnhandledException;

			// defer other startup processing to base class
			base.OnStartup(e);

		}

		new void DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{

			if (e.Exception is System.ComponentModel.Win32Exception)
			{
				//stop error, do nothing\
				e.Handled = true;
			}
			else 
			{
				//string query=string.Empty;
				//if (ApplicationDeployment.IsNetworkDeployed)
				//{//uri
				//    Uri launchUri = ApplicationDeployment.CurrentDeployment.ActivationUri;
				//    if (!string.IsNullOrEmpty(launchUri.Query)) query = launchUri.PathAndQuery;
				//}	

#if DEBUG
				MessageBox.Show("Error loading the application.\r\nDetails:\r\n" + e.Exception.ToString());
#else
				MessageBox.Show("Error loading the application.\r\nDetails:\r\n" + e.Exception.Message);
#endif
			}
		}
		
	  
	}
}
