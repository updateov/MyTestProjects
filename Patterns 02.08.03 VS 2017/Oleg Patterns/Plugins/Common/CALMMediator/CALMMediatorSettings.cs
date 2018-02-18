using CommonLogger;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PatternsCALMMediator
{
    public class CALMMediatorSettings : ServiceSettings
    {
        #region Settings Values

        public string CRIPluginURL { get; private set; }
        public int CRIPluginRequestInterval { get; private set; }
        public int CRIPluginRequestTimeOut { get; private set; }

        public string CALMUser { get; private set; }
        public bool CanExecutePatternsActions { get; private set; }

        #endregion

        private static Object s_lockObject = new Object();
        private static CALMMediatorSettings s_instance = null;
        public static CALMMediatorSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new CALMMediatorSettings();
                    }
                }

                return s_instance;
            }
        }

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;

            try
            {
                CRIPluginURL = GetSettingsStrValue(NameOf<CALMMediatorSettings>.Property(e => e.CRIPluginURL));
                CRIPluginRequestInterval = GetSettingsIntValue(NameOf<CALMMediatorSettings>.Property(e => e.CRIPluginRequestInterval));
                CRIPluginRequestTimeOut = GetSettingsIntValue(NameOf<CALMMediatorSettings>.Property(e => e.CRIPluginRequestTimeOut));

                CALMUser = GetSettingsStrValue(NameOf<CALMMediatorSettings>.Property(e => e.CALMUser));

                string strCanExecutePatternsActions = GetSettingsStrValue(NameOf<CALMMediatorSettings>.Property(e => e.CanExecutePatternsActions));
                CanExecutePatternsActions = Convert.ToBoolean(strCanExecutePatternsActions);                    

                //check that everything succeeded to load
                if (CRIPluginURL != String.Empty &&
                    CRIPluginRequestInterval != -1 &&
                    CRIPluginRequestTimeOut != -1 &&
                    CALMUser != String.Empty)
                {
                    bRes = true;
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediatorSettings", "CALMMediator Settings: Failed to Read\\Update settings", ex);
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
