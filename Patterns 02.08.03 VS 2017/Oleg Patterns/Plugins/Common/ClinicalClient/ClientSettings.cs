using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PeriGenSettingsManager;
using System.Linq.Expressions;
using CommonLogger;

namespace PatternsCRIClient
{
    public class ClientSettings : ServiceSettings
    {
        #region Settings Values

        public SourceLevels LoggingLevelDebug { get; private set; }
        public SourceLevels LoggingLevelConsole { get; private set; }
        public SourceLevels LoggingLevelEventLog { get; private set; }
        public string LoggingEventLogName { get; private set; }

        public string CALMUser { get; private set; }
        public bool IsCentralMode { get; private set; }
        public bool CanOpenPatientList { get; private set; }
        public bool BlockADTFunctions { get; private set; }
        public bool AllowReview { get; private set; }
        public bool ChangeDataRequireAuthentication { get; private set; }
        public bool MergeRequireAuthentication { get; private set; }
        public int AutomaticClosingTimeout { get; private set; }
        public bool CloseNotificationWithoutTracing { get; private set; }
        #endregion

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;

            try
            {
                string loggingLevelDebug = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.LoggingLevelDebug));
                LoggingLevelDebug = GetSourceLevelFromString(loggingLevelDebug);

                string loggingLevelConsole = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.LoggingLevelConsole));
                LoggingLevelConsole = GetSourceLevelFromString(loggingLevelConsole);

                string loggingLevelEventLog = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.LoggingLevelEventLog));
                LoggingLevelEventLog = GetSourceLevelFromString(loggingLevelEventLog);

                LoggingEventLogName = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.LoggingEventLogName));

                CALMUser = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.CALMUser));

                string outVal = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.IsCentralMode));
                IsCentralMode = Convert.ToBoolean(outVal);

                outVal = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.CanOpenPatientList));
                CanOpenPatientList = Convert.ToBoolean(outVal);

                outVal = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.BlockADTFunctions));
                BlockADTFunctions = Convert.ToBoolean(outVal);

                outVal = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.ChangeDataRequireAuthentication));
                ChangeDataRequireAuthentication = Convert.ToBoolean(outVal);

                outVal = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.MergeRequireAuthentication));
                MergeRequireAuthentication = Convert.ToBoolean(outVal);

                outVal = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.AutomaticClosingTimeout));
                AutomaticClosingTimeout = Convert.ToInt32(outVal);

                if (IsCentralMode == true)
                {
                    outVal = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.AllowReview));
                    AllowReview = Convert.ToBoolean(outVal);

                    CloseNotificationWithoutTracing = false;
                }
                else
                {
                    AllowReview = false;

                    outVal = GetSettingsStrValue(NameOf<ClientSettings>.Property(e => e.CloseNotificationWithoutTracing));
                    CloseNotificationWithoutTracing = Convert.ToBoolean(outVal);
                }

                bRes = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, Properties.Resources.CRIClient_ModuleName, "Client Settings: Failed to Read\\Update settings", ex);
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
