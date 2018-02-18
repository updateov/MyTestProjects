using PeriGenLogger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WindowsServiceMain
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(String[] args)
        {
            Logger.AddDebugTrace("Debuglog", System.Diagnostics.SourceLevels.All);
            Logger.AddConsoleTrace("Console", System.Diagnostics.SourceLevels.All);
            Logger.AddEventLogTrace("Interface", System.Diagnostics.SourceLevels.All, "Application");

            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "Program", "Service starting");

            if (Environment.UserInteractive && args.Length > 0)
            {
                var parameter = (from h in args where h.ToUpperInvariant() == "/CONSOLE" select h).FirstOrDefault();
                if (string.IsNullOrEmpty(parameter))
                {
                    return;
                }

                using (var service = new OlegMainService())
                {
                    try
                    {
                        Console.WriteLine("Starting service...");
                        service.StartManager();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error. Service cannot be started.\nDetails: {0}", ex));
                        return;
                    };

                    Console.WriteLine("Service started!");
                    Console.WriteLine("Press <ENTER> to stop the service...");
                    Console.ReadLine();
                    try
                    {
                        Console.WriteLine("Stopping service...");
                        service.StopManager();
                        Console.WriteLine("Service Stopped!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error. Service cannot be stopped.\nDetails: {0}", ex));
                    }
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                        new OlegMainService()
                };

                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
