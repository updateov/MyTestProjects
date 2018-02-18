using CommonLogger;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PatternsPluginsService
{
    public class PluginsServiceSettings : ServiceSettings
    {
        #region Properties & Members

        public SourceLevels LoggingLevelDebug { get; private set; }
        public SourceLevels LoggingLevelConsole { get; private set; }
        public SourceLevels LoggingLevelEventLog { get; private set; }
        public string LoggingEventLogName { get; private set; }

        #endregion

        private static Object s_lockObject = new Object();
        private static PluginsServiceSettings s_instance = null;
        public static PluginsServiceSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new PluginsServiceSettings();
                    }
                }

                return s_instance;
            }
        }

        private PluginsServiceSettings()
            :base(true)
        { }

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;
            try
            {
                string loggingLevelDebug = GetSettingsStrValue(NameOf<PluginsServiceSettings>.Property(e => e.LoggingLevelDebug));
                LoggingLevelDebug = GetSourceLevelFromString(loggingLevelDebug);

                string loggingLevelConsole = GetSettingsStrValue(NameOf<PluginsServiceSettings>.Property(e => e.LoggingLevelConsole));
                LoggingLevelConsole = GetSourceLevelFromString(loggingLevelConsole);

                string loggingLevelEventLog = GetSettingsStrValue(NameOf<PluginsServiceSettings>.Property(e => e.LoggingLevelEventLog));
                LoggingLevelEventLog = GetSourceLevelFromString(loggingLevelEventLog);

                LoggingEventLogName = GetSettingsStrValue(NameOf<PluginsServiceSettings>.Property(e => e.LoggingEventLogName));

                //check that everything succeeded to load
                if (LoggingEventLogName != String.Empty)
                {
                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "PluginsServiceSettings", "Service Settings: Failed to Read\\Update settings", ex);
            }

            return bRes;
        }

        private SourceLevels GetSourceLevelFromString(string strSourceLevel)
        {
            SourceLevels resSourceLevels = SourceLevels.Off;

            switch (strSourceLevel)
            {
                case "ALL": resSourceLevels = SourceLevels.All; break;
                case "VERBOSE": resSourceLevels = SourceLevels.Verbose; break;
                case "INFORMATION": resSourceLevels = SourceLevels.Information; break;
                case "WARNING": resSourceLevels = SourceLevels.Warning; break;
                case "ERROR": resSourceLevels = SourceLevels.Error; break;
                case "CRITICAL": resSourceLevels = SourceLevels.Critical; break;
                case "OFF": resSourceLevels = SourceLevels.Off; break;
                default: resSourceLevels = SourceLevels.Off; break;
            }

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
