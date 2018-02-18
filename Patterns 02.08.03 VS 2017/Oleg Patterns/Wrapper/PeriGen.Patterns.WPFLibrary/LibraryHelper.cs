using CommonLogger;
using PeriGen.Patterns.WPFLibrary.Screens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace PeriGen.Patterns.WPFLibrary
{
    public class LibraryHelper
    {
        #region Singleton functionality

        private static volatile LibraryHelper s_instance;
        private static object s_lockObject = new Object();

        private LibraryHelper()
        {
        }

        public void InitLogger(SourceLevels loggingLevelDebug, SourceLevels loggingLevelConsole, SourceLevels loggingLevelEventLog, string loggingEventLogName)
        {
            Logger.AddDebugTrace("Debuglog", loggingLevelDebug);
            Logger.AddConsoleTrace("Console", loggingLevelConsole);
            Logger.AddEventLogTrace("Interface", loggingLevelEventLog, loggingEventLogName);
        }

        public static LibraryHelper Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new LibraryHelper();
                    }
                }
                return s_instance;
            }
        }

        #endregion

        public void ShowAboutWindow(string pluginURL, bool isCheckListExists, int companyMode, string udi, bool isCheckListApp)
        {
            bool isPeriGen = companyMode == 0 ? true : false;

            AboutWindow dlg = new AboutWindow(pluginURL, isCheckListExists, isPeriGen, udi, isCheckListApp);
            WindowInteropHelper helper = new WindowInteropHelper(dlg);
            helper.Owner = Process.GetCurrentProcess().MainWindowHandle;
            dlg.ShowDialog();
        }
    }
}
