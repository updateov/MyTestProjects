//Review: 27/04/15
using CommonLogger;
using PatternsCALMMediator;
using PatternsCRIClient.Downloader;
using PatternsCRIClient.Screens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PatternsCRIClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string CRIToolBar = "ToolBar";

        // TODO: this call is redundant
        static public ClientManager ClientManager
        {
            get
            {
                return ClientManager.Instance;
            }
        }

        public App()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;

            string currentStrValue = String.Empty;

            bool bSucc = GetConfigFile();

            if (!bSucc)
            {
                MessageBox.Show("Could not find the configuration file.\nThe application will close", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown();
            }
            
            ClientSettings settings = new ClientSettings();

            Logger.AddDebugTrace("Debuglog", settings.LoggingLevelDebug);
            Logger.AddConsoleTrace("Console", settings.LoggingLevelConsole);
            Logger.AddEventLogTrace("Interface", settings.LoggingLevelEventLog, settings.LoggingEventLogName);
        }

        private bool GetConfigFile()
        {
            bool bRes = false;
            try
            {
                string appLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                string strConfigDir = System.Configuration.ConfigurationManager.AppSettings["ConfigDir"];
                string strMode = System.Configuration.ConfigurationManager.AppSettings["Mode"];
                string strDestFileName = System.Configuration.ConfigurationManager.AppSettings["config"];

                if (strMode != null && strConfigDir != null && strDestFileName != null)
                {
                    string sourceFilePath = String.Format(@"{0}\CheckList_Client_{1}.psf", strConfigDir, strMode);
                    string destFilePath = String.Format(@"{0}\{1}", appLocation, strDestFileName);

                    bool isSourceExists = File.Exists(sourceFilePath);
                    bool isDestExists = File.Exists(destFilePath);

                    if (isSourceExists)
                    {
                        if (isDestExists)
                        {
                            DateTime souceLastModified = File.GetLastWriteTime(sourceFilePath);
                            DateTime destLastModified = File.GetLastWriteTime(destFilePath);
                            if (souceLastModified > destLastModified)
                            {
                                System.IO.File.Copy(sourceFilePath, destFilePath, true);
                            }
                        }
                        else
                        {
                            System.IO.File.Copy(sourceFilePath, destFilePath, true);
                        }
                    }

                    bRes = File.Exists(destFilePath);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to GetConfigFile. error: " + ex.Message);
            }

            return bRes;
        }

        private void OnTaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception as Exception);
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject as Exception);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);

            e.Handled = true;
        }

        private void LogException(Exception ex)
        {
            if (ex != null)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, PatternsCRIClient.Properties.Resources.CRIClient_ModuleName, "Unhandled Exception", ex);
                //PeriGenLogger.Logger.WriteLogEntry(TraceEventType.Critical, PatternsCRIClient.Properties.Resources.CRIClient_ModuleName, "Unhandled Inner Exception", ex.InnerException);

                //string errorMessage = string.Format("An unhandled exception occurred: {0} \n", ex.ToString());
                //errorMessage += string.Format("An unhandled inner exception occurred: {0} \n", ex.InnerException.Message);
                //MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // TODO: make this message readable by nurses, and exit?
                MessageBox.Show("Got null exception", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType().Name == "AutomaticMessageWindow" ||
                    window.GetType().Name == "AddPatientWindow" ||
                    window.GetType().Name == "EditPatientWindow" ||
                    window.GetType().Name == "DischargePatientWindow" ||
                    window.GetType().Name == "LoginWindow" ||
                    window.GetType().Name == "MergePatientWindow" ||
                    window.GetType().Name == "TimeoutWindow")
                {
                    window.Topmost = false;
                }
            }

            base.OnDeactivated(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType().Name == "AutomaticMessageWindow" ||
                    window.GetType().Name == "AddPatientWindow" ||
                    window.GetType().Name == "EditPatientWindow" ||
                    window.GetType().Name == "DischargePatientWindow" ||
                    window.GetType().Name == "LoginWindow" ||
                    window.GetType().Name == "MergePatientWindow" ||
                    window.GetType().Name == "TimeoutWindow")
                {
                    window.Topmost = true;
                }
            }

            base.OnActivated(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StopProcessIfExist();

            EventManager.RegisterClassHandler(typeof(DatePicker), DatePicker.LoadedEvent, new RoutedEventHandler(DatePicker_Loaded));
        }

        public static void StopProcessIfExist()
        {
            try
            {
                Process currentProc = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(currentProc.ProcessName);

                foreach (Process proc in processes)
                {
                    if (currentProc.Id != proc.Id)
                    {
                        proc.Kill();
                        proc.WaitForExit(5000);
                    }
                }

                processes = Process.GetProcessesByName(App.CRIToolBar);

                foreach (Process proc in processes)
                {
                    proc.Kill();
                    proc.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Verbose, PatternsCRIClient.Properties.Resources.CRIClient_ModuleName, ex.Message);
            }
        }

        public static bool IsMainWindowActive()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType().Name == "MainWindow")
                {
                    return window.IsActive;
                }
            }
            return false;
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);

                if (result != null)
                    return result;
            }
            return null;
        }

        void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker;

            if (dp == null)
                return;

            var tb = GetChildOfType<DatePickerTextBox>(dp);

            if (tb == null)
                return;

            var wm = tb.Template.FindName("PART_Watermark", tb) as ContentControl;

            if (wm == null)
                return;

            wm.Content = String.Empty;
        }
    }
}
