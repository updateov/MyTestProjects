using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.ServiceModel.Web;
using System.ServiceModel;
using PeriGenSettingsManager;
using System.Diagnostics;
using CommonLogger;

namespace PatternsAddOnService
{
    class Program
    {
        static void Main(string[] args)
        {
            SourceLevels lvl = GetLoggerLevel(Settings_PatternsLoggerLevel);
            Logger.AddDebugTrace("PatternsAddOn", lvl);
            Logger.AddEventLogTrace("PatternsAddOn", lvl, "Application");

            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "Patterns Add On Service, Service startup", "Service starting");
            if (Environment.UserInteractive && args.Length > 0)
            {
                String parameter;
                parameter = (from c in args where c.ToUpperInvariant() == "/CONSOLE" select c).FirstOrDefault();
                if (parameter == null)
                {
                    return;
                }

                var task = new AddOnTask();
                task.StartTask();

                Console.WriteLine("The Patterns Service is running... [Press Enter to close]");
                Console.ReadLine();
                Console.WriteLine("Closing service...");
                task.StopTask();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new AddOnWinService() };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static SourceLevels GetLoggerLevel(string Settings_PatternsLoggerLevel)
        {
            if (Settings_PatternsLoggerLevel.ToUpper().Equals("ALL"))
                return SourceLevels.All;
            if (Settings_PatternsLoggerLevel.ToUpper().Equals("VERBOSE"))
                return SourceLevels.Verbose;
            if (Settings_PatternsLoggerLevel.ToUpper().Equals("INFORMATION"))
                return SourceLevels.Information;
            if (Settings_PatternsLoggerLevel.ToUpper().Equals("WARNING"))
                return SourceLevels.Warning;
            if (Settings_PatternsLoggerLevel.ToUpper().Equals("ERROR"))
                return SourceLevels.Error;
            if (Settings_PatternsLoggerLevel.ToUpper().Equals("CRITICAL"))
                return SourceLevels.Critical;

            return SourceLevels.Off;
        }

        static String Settings_PatternsLoggerLevel = AppSettingsMngr.GetSettingsStrValue("PatternsLoggerLevel");
    }
}
