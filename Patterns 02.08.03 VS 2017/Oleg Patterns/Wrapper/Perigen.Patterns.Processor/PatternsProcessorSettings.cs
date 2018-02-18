using PeriGenLogger;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Perigen.Patterns.Processor
{
    public class PatternsProcessorSettings : ServiceSettings
    {
        #region Properties & Members

        public int PatternsProcessorTerminateTimeout { get; private set; }

        #endregion

        internal static TraceSource Source = new TraceSource("PatternsProcessor");

        int m_ProcessId = 0;
        public int ProcessId {
            get
            {
                if (m_ProcessId == 0)
                    m_ProcessId = Process.GetCurrentProcess().Id;
                return m_ProcessId;
            }
        }

        private string m_ProcessGUID = string.Empty;
        public string ProcessGUID
        {
            get { return m_ProcessGUID; }
            set { m_ProcessGUID = value; }
        }

        private static Object s_lockObject = new Object();
        private static PatternsProcessorSettings s_instance = null;
        public static PatternsProcessorSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new PatternsProcessorSettings();
                    }
                }

                return s_instance;
            }
        }

        private PatternsProcessorSettings() : base(true)
        { }

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;
            try
            {
                PatternsProcessorTerminateTimeout = GetSettingsIntValue(NameOf<PatternsProcessorSettings>.Property(e => e.PatternsProcessorTerminateTimeout));

                //check that everything succeeded to load
                if (PatternsProcessorTerminateTimeout != -1)
                {
                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "PatternsServiceSettings", "Failed to Read\\Update settings", ex);
            }

            return bRes;
        }
    }

    public static class NameOf<T>
    {
        public static string Property<TProp>(Expression<Func<T, TProp>> expression)
        {
            var body = expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("'expression' should be a member expression");

            return body.Member.Name;
        }
    }
}
