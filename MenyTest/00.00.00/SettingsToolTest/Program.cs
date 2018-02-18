using CommonLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SettingsToolTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.AddDebugTrace("Debuglog", SettingsToolTestSettings.Instance.LoggingLevelDebug);
            Logger.AddConsoleTrace("Console", SettingsToolTestSettings.Instance.LoggingLevelConsole);
            Logger.AddEventLogTrace("Interface", SettingsToolTestSettings.Instance.LoggingLevelEventLog, SettingsToolTestSettings.Instance.LoggingEventLogName);

            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "Program", "Service starting");

        }
    }
}
