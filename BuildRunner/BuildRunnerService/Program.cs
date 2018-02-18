using PeriGenLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace BuildRunnerService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(String[] args)
        {
            Logger.AddDebugTrace("BuildRunner", SourceLevels.Critical);
            Logger.AddEventLogTrace("BuildRunner", SourceLevels.Critical, "Application");

            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "BuildRunner Service, Service startup", "Service starting");
            if (Environment.UserInteractive && args.Length > 0)
            {
                var parameter = (from h in args where h.ToUpperInvariant() == "/CONSOLE" select h).FirstOrDefault();
                if (string.IsNullOrEmpty(parameter))
                {
                    return;
                }

                using (var service = new BuildService())
                {
                    try
                    {
                        Console.WriteLine("Starting service...");
                        service.StartBuilder();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Error. Service cannot be started.\nDetails: {0}", ex));
                        return;
                    }

                    Console.WriteLine("Service started!");
                    Console.WriteLine("Press <ENTER> to stop the service...");

                    Console.Read();
                    try
                    {
                        Console.WriteLine("Stopping service...");
                        service.StopBuilder();
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
                    new BuildService() 
                };
             
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
