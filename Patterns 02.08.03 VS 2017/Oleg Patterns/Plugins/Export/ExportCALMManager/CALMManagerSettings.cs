using CommonLogger;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Export.CALMManager
{
    public class CALMManagerSettings : ServiceSettings
    {
        #region Properties & Members
        
        public int CALMExportGroup { get; private set; }
        public int CALMCheckConnectionInterval { get; private set; }
        public bool CALMServicesEnabled { get; private set; }
        #endregion

        #region Singleton Initialization

        private static Object s_lockObject = new Object();
        private static CALMManagerSettings s_instance = null;

        private CALMManagerSettings()
        {
            bool bRes = UpdateEngineSettings();
        }

        public static CALMManagerSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new CALMManagerSettings();
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
                string strCALMServicesEnabled = GetSettingsStrValue(NameOf<CALMManagerSettings>.Property(e => e.CALMServicesEnabled));
                CALMServicesEnabled = Convert.ToBoolean(strCALMServicesEnabled);

                CALMExportGroup = GetSettingsIntValue(NameOf<CALMManagerSettings>.Property(e => e.CALMExportGroup));
                CALMCheckConnectionInterval = GetSettingsIntValue(NameOf<CALMManagerSettings>.Property(e => e.CALMCheckConnectionInterval));
                //check that everything succeeded to load
                if (CALMExportGroup != -1 && CALMCheckConnectionInterval != -1)
                {
                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CALMManagerSettings", "CALMManagerSettings: Failed to Read\\Update settings", ex);
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
