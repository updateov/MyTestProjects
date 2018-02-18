using CommonLogger;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Export.Plugin
{
    public class PluginSettings : ServiceSettings
    {
        #region Properties & Members
        
        public int IntervalDuration { get; private set; }        
        public int CheckIntervalCompleteInterval { get; private set; }
        public string EncryptionPassword { get; private set; }
        public string CALMServerLink { get; private set; }
        public bool CALMServicesEnabled { get; private set; }

        #endregion

        #region Singleton Initialization

        private static Object s_lockObject = new Object();
        private static PluginSettings s_instance = null;

        private PluginSettings()
        {
            bool bRes = UpdateEngineSettings();
            //if (!bRes)
            //{
            //}
        }

        public static PluginSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new PluginSettings();
                    }
                }

                return s_instance;
            }
        }

        #endregion

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;
            try
            {
                IntervalDuration = GetSettingsIntValue(NameOf<PluginSettings>.Property(e => e.IntervalDuration));                
                CheckIntervalCompleteInterval = GetSettingsIntValue(NameOf<PluginSettings>.Property(e => e.CheckIntervalCompleteInterval));
                EncryptionPassword = GetSettingsStrValue(NameOf<PluginSettings>.Property(e => e.EncryptionPassword));
                CALMServerLink = GetSettingsStrValue(NameOf<PluginSettings>.Property(e => e.CALMServerLink));
                String strCALMServicesEnabled = GetSettingsStrValue(NameOf<PluginSettings>.Property(e => e.CALMServicesEnabled));
                CALMServicesEnabled = Convert.ToBoolean(strCALMServicesEnabled);

                //check that everything succeeded to load
                if (IntervalDuration != -1 && 
                    CheckIntervalCompleteInterval != -1 &&
                    !CALMServerLink.Equals(String.Empty) &&
                    !EncryptionPassword.Equals(String.Empty))
                {
                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "ExportPluginSettings", "PluginSettings: Failed to Read\\Update settings", ex);
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
