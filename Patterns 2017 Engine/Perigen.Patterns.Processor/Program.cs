using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Perigen.Patterns.Processor
{
    static class Program
    {
        static PatternsApplicationContext m_ApplicationContext = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            PatternsProcessorSettings settings = PatternsProcessorSettings.Instance;

            PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 100, "Patterns Processor (PID={0}): instance started", PatternsProcessorSettings.Instance.ProcessId);

            if (args == null || args.Length < 1)
            {
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Error, 105, "Patterns Processor (PID={0}): missing command line argument(s)", PatternsProcessorSettings.Instance.ProcessId);
                return;
            }

            String guid = String.Empty;
            if (!args[0].Equals(String.Empty))
                guid = args[0];

            if (string.IsNullOrEmpty(guid))
            {
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Error, 110, "Patterns Processor (PID={0}): missing GUID argument", PatternsProcessorSettings.Instance.ProcessId);
                return;
            }

            PatternsProcessorSettings.Instance.ProcessGUID = guid;

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            m_ApplicationContext = new PatternsApplicationContext(guid);
            Application.Run(m_ApplicationContext);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Error, 115, "Patterns Processor (PID={0}, GUID={1}): UnhandledException. \r\n {2}",
                    PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, ex);
            }
            catch (Exception ex)
            {
                try
                {
                    PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Error, 120, "Patterns Processor (PID={0}, GUID={1}): UnhandledException2. \r\n {2}",
                        PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, ex);
                }
                catch(Exception)
                {
                }
            }
            finally
            {
                m_ApplicationContext.TerminateProcess();
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Error, 125, "Patterns Processor (PID={0}, GUID={1}): Thread exception. \r\n {2}",
                    PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, e.Exception);
            }
            catch(Exception)
            {
            }
            finally
            {
                m_ApplicationContext.TerminateProcess();
            }
        }
    }
}
