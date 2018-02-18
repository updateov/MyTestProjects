using System;
using System.Collections.Generic;
using System.Linq;
using PeriGen.Patterns.Helper;
using System.ServiceProcess;
using System.Globalization;
using System.Diagnostics;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Service
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>		
		static void Main(string[] args)
		{
			if (Environment.UserInteractive && args.Length > 0)
			{
				string parameter;

				// /help
				parameter = (from h in args where h.ToUpperInvariant() == "/HELP" select h).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					DisplayHelp();
					return;
				}

				// /certificate
				parameter = (from c in args where c.ToUpperInvariant() == "/CERTIFICATE" select c).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					try
					{
						// Validate and Install certificate if it is necessary
						SettingsManager.ValidatePeriGenCertificateInstalled();

						// Log certificate status
						Console.WriteLine("PeriGen Settings Certificate validated/installed.");
					}
					catch (Exception ex)
					{
						Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error.\nDetails: {0}", ex.ToString()));
						return;
					}
				}

				// /counter
				parameter = (from c in args where c.ToUpperInvariant() == "/COUNTER" select c).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					PeriGen.Patterns.GE.Service.PerformanceCounterHelper.InstallPerformanceCounters();
					PeriGen.Patterns.Engine.PerformanceCounterHelper.InstallPerformanceCounters();

					// Log certificate status
					Console.WriteLine("Performance counters installed.");
				}

				// /counter
				parameter = (from c in args where c.ToUpperInvariant() == "/EVENTLOG" select c).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					EventLog.DeleteEventSource("PeriGen Patterns Service");
					EventLog.CreateEventSource("PeriGen Patterns Service", "Application");
					EventLog.DeleteEventSource("PeriGen Patterns Interface");
					EventLog.CreateEventSource("PeriGen Patterns Interface", "Application");
					EventLog.DeleteEventSource("PeriGen Patterns Engine");
					EventLog.CreateEventSource("PeriGen Patterns Engine", "Application");

					// Log certificate status
					Console.WriteLine("EventLog source recreated.");
				}

				// /console
				parameter = (from c in args where c.ToUpperInvariant() == "/CONSOLE" select c).FirstOrDefault();
				if (!string.IsNullOrEmpty(parameter))
				{
					using (var service = new PatternsService())
					{
						try
						{
							Console.WriteLine("Starting service...");
							service.StartTask();
						}
						catch (Exception ex)
						{
							Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error. Service '{0}' cannot be started.\nDetails: {1}", PatternsService.PatternsServiceName, ex.ToString()));
							return;
						}

						Console.WriteLine("Service started!");
						Console.WriteLine("Press <ENTER> to stop the service...");
						Console.Read();
						try
						{
							Console.WriteLine("Stopping service...");
							service.StopTask();
							Console.WriteLine("Service Stopped!");
						}
						catch (Exception ex)
						{
							Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error. Service '{0}' cannot be stopped.\nDetails: {1}", PatternsService.PatternsServiceName, ex.ToString()));
						}
					}
					return;
				}
			}
			else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] { new PatternsService() };
				ServiceBase.Run(ServicesToRun);
			}
		}

		/// <summary>
		/// Display help for command line use of the service
		/// </summary>
		private static void DisplayHelp()
		{
			Console.WriteLine("Syntax: ");
			Console.WriteLine("");

			Console.WriteLine("PeriGen.Patterns.GE.Service.exe {[/console]|[/help][/certificate][/counter][/eventlog]}");
			Console.WriteLine("");
			Console.WriteLine("/console     run service in debug mode");
			Console.WriteLine("/help        display this help");
			Console.WriteLine("/certificate validate/install certificate");
			Console.WriteLine("/counter     re-create the performance counters");
			Console.WriteLine("/eventlog    re-register the eventlog source");

			Console.WriteLine("");
		}
	}
}
