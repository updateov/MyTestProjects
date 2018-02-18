using CommonLogger;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SettingsToolTest
{
    class SettingsToolTestSettings : ServiceSettings
    {
        #region Properties & Members

        public SourceLevels LoggingLevelDebug { get; private set; }
        public SourceLevels LoggingLevelConsole { get; private set; }
        public SourceLevels LoggingLevelEventLog { get; private set; }
        public String LoggingEventLogName { get; private set; }

        #endregion

        #region Singleton

        private static Object s_lock = new Object();
        private static SettingsToolTestSettings s_instance = null;
        public static SettingsToolTestSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new SettingsToolTestSettings();
                    }
                }

                return s_instance;
            }
        }

        private SettingsToolTestSettings()
        { }

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;
            try
            {
                string loggingLevelDebug = GetSettingsStrValue(NameOf<SettingsToolTestSettings>.Property(e => e.LoggingLevelDebug));
                LoggingLevelDebug = GetSourceLevelFromString(loggingLevelDebug);

                string loggingLevelConsole = GetSettingsStrValue(NameOf<SettingsToolTestSettings>.Property(e => e.LoggingLevelConsole));
                LoggingLevelConsole = GetSourceLevelFromString(loggingLevelConsole);

                string loggingLevelEventLog = GetSettingsStrValue(NameOf<SettingsToolTestSettings>.Property(e => e.LoggingLevelEventLog));
                LoggingLevelEventLog = GetSourceLevelFromString(loggingLevelEventLog);

                LoggingEventLogName = GetSettingsStrValue(NameOf<SettingsToolTestSettings>.Property(e => e.LoggingEventLogName));


                //check that everything succeeded to load
                if (!LoggingEventLogName.Equals(String.Empty))
                {
                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "HUBServiceSettings", "Service Settings: Failed to Read/Update settings", ex);
            }

            return bRes;
        }

        #endregion
        private SourceLevels GetSourceLevelFromString(String strSourceLevel)
        {
            SourceLevels resSourceLevels = SourceLevels.Off;

            switch (strSourceLevel)
            {
                case "ALL":
                    resSourceLevels = SourceLevels.All;
                    break;
                case "VERBOSE":
                    resSourceLevels = SourceLevels.Verbose;
                    break;
                case "INFORMATION":
                    resSourceLevels = SourceLevels.Information;
                    break;
                case "WARNING":
                    resSourceLevels = SourceLevels.Warning;
                    break;
                case "ERROR":
                    resSourceLevels = SourceLevels.Error;
                    break;
                case "CRITICAL":
                    resSourceLevels = SourceLevels.Critical;
                    break;
                case "OFF":
                    resSourceLevels = SourceLevels.Off;
                    break;
                default:
                    resSourceLevels = SourceLevels.Off;
                    break;
            };

            return resSourceLevels;
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
