using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGenSettingsManager;
using System.Linq.Expressions;
using System.Diagnostics;
using CommonLogger;

namespace Export.Algorithm
{
    public class AlgorithmSettings : ServiceSettings
    {
        private const int MINIMAL_AMOUNT_OF_DATA_IN_WINDOW = 75;
        private const int CTR_DURATION_ROUND_TO = 5;
        private const int CTR_INTENSITY_ROUND_TO = 5;

        private static Object s_lock = new Object();
        private static AlgorithmSettings s_instance = null;
        public static AlgorithmSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new AlgorithmSettings();
                    }
                }

                return s_instance;
            }
        }

        public int MinimalAmountOfDataInQualificationWindow { get; private set; }
        public int CTRDurationRoundingTo { get; private set; }
        public int CTRIntesityRoundingTo { get; private set; }

        private AlgorithmSettings()
        {
            bool bRes = UpdateEngineSettings();
            if (!bRes)
            {
                MinimalAmountOfDataInQualificationWindow = MINIMAL_AMOUNT_OF_DATA_IN_WINDOW;
                CTRDurationRoundingTo = CTR_DURATION_ROUND_TO;
                CTRIntesityRoundingTo = CTR_INTENSITY_ROUND_TO;
            }            
        }

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;
            try
            {
                MinimalAmountOfDataInQualificationWindow = GetSettingsIntValue(NameOf<AlgorithmSettings>.Property(e => e.MinimalAmountOfDataInQualificationWindow));
                CTRDurationRoundingTo = GetSettingsIntValue(NameOf<AlgorithmSettings>.Property(e => e.CTRDurationRoundingTo));
                CTRIntesityRoundingTo = GetSettingsIntValue(NameOf<AlgorithmSettings>.Property(e => e.CTRIntesityRoundingTo));

                //check that everything succeeded to load
                if (MinimalAmountOfDataInQualificationWindow != -1 ||
                    CTRDurationRoundingTo >= 1 ||
                    CTRIntesityRoundingTo >= 1)
                {
                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "Export.Algorithm", "Service Settings: Failed to Read\\Update settings", ex);
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
