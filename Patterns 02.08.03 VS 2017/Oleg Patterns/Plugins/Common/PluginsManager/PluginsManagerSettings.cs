using CommonLogger;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PatternsPluginsManager
{
    public class PluginsManagerSettings : ServiceSettings
    {
        #region Properties & Members

        public string PluginsDataFeed { get; private set; }

        public string PatternsDataFeed { get; private set; }
        public int PatternsRequestInterval { get; private set; }
        public int PatternsRequestTimeOut { get; private set; }
        public string PatternsVersion { get; private set; }
        public int MeanBaselineFHRRoundingTo { get; private set; }
        public int MVURoundingTo { get; private set; }

        #endregion

        private static Object s_lockObject = new Object();
        private static PluginsManagerSettings s_instance = null;
        public static PluginsManagerSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new PluginsManagerSettings();
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
                PluginsDataFeed = GetSettingsStrValue(NameOf<PluginsManagerSettings>.Property(e => e.PluginsDataFeed));

                PatternsDataFeed = GetSettingsStrValue(NameOf<PluginsManagerSettings>.Property(e => e.PatternsDataFeed));
                PatternsRequestInterval = GetSettingsIntValue(NameOf<PluginsManagerSettings>.Property(e => e.PatternsRequestInterval));
                PatternsRequestTimeOut = GetSettingsIntValue(NameOf<PluginsManagerSettings>.Property(e => e.PatternsRequestTimeOut));
                MeanBaselineFHRRoundingTo = GetSettingsIntValue(NameOf<PluginsManagerSettings>.Property(e => e.MeanBaselineFHRRoundingTo));
                MVURoundingTo = GetSettingsIntValue(NameOf<PluginsManagerSettings>.Property(e => e.MVURoundingTo));                                

                //check that everything succeeded to load
                if (PluginsDataFeed != String.Empty &&
                    PatternsDataFeed != String.Empty &&
                    PatternsRequestInterval != -1 &&
                    MeanBaselineFHRRoundingTo >= 1 &&
                    MVURoundingTo >= 1 &&
                    PatternsRequestTimeOut != -1)
                {
                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "PatternsManagerSettings", "Service Settings: Failed to Read\\Update settings", ex);
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
