using CommonLogger;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRIAlgorithm
{
    public class CRIAlgorithmSettings : ServiceSettings
    {
        #region Consts

        private const float MINIMAL_LATE_DECEL_CONFIDENCE = 0.7f;
        private const int MINIMAL_PROLONGED_DECEL_HEIGHT = 20;
        private const int MINIMAL_QUALIFICATION_WINDOW_SIZE = 1800;
        private const int MINIMAL_AMOUNT_OF_DATA_IN_WINDOW = 75;

        private const int MINIMAL_BASELINE_VARIABILITY = 6;
        private const int ACCEL_RATE_AMOUNT = 0;
        private const int LATE_DECEL_RATE_AMOUNT = 2;
        private const int LATE_DECEL_AND_LARGE_ANDLONG_RATE_AMOUNT = 2;
        private const int LATE_DECEL_AND_PROLONGED_RATE_AMOUNT = 2;
        private const int LARGE_AND_LONG_DECEL_RATE_AMOUNT = 3;
        private const int CONTRACTIONS_RATE_AMOUNT = 16;
        private const int LONG_CONTRACTIONS_RATE_AMOUNT = 2;

        #endregion

        #region Properties & Members

        private static Object s_lock = new Object();
        private static CRIAlgorithmSettings s_instance = null;
        public static CRIAlgorithmSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new CRIAlgorithmSettings();
                    }
                }

                return s_instance;
            }
        }

        public float MinimalLateDecelConfidence { get; private set; }
        public int MinimalLateDecelAmount { get; private set; }
        public int MinimalLargeAndLongDecelAmount { get; private set; }
        public int MinimalLateAndLargeAndLongDecelAmount { get; private set; }
        public int MinimalLateAndProlongedDecelAmount { get; private set; }
        public int MinimalProlongedDecelHeight { get; private set; }
        public int MinimalContractionsAmount { get; private set; }
        public int MinimalLongContractionsAmount { get; private set; }
        public int MinimalAccelerationsAmount { get; private set; }
        public int MinimalBaselineVariability { get; private set; }

        public int CRIStateQualificationWindowSize { get; private set; }
        public int MinimalAmountOfDataInQualificationWindow { get; private set; }

        #endregion

        private CRIAlgorithmSettings()
        {
            bool bRes = UpdateEngineSettings();
            if (!bRes)
            {
                MinimalLateDecelConfidence = MINIMAL_LATE_DECEL_CONFIDENCE;
                MinimalLateDecelAmount = LATE_DECEL_RATE_AMOUNT;
                MinimalLargeAndLongDecelAmount = LARGE_AND_LONG_DECEL_RATE_AMOUNT;
                MinimalLateAndLargeAndLongDecelAmount = LATE_DECEL_AND_LARGE_ANDLONG_RATE_AMOUNT;
                MinimalLateAndProlongedDecelAmount = LATE_DECEL_AND_PROLONGED_RATE_AMOUNT;
                MinimalProlongedDecelHeight = MINIMAL_PROLONGED_DECEL_HEIGHT;
                MinimalContractionsAmount = CONTRACTIONS_RATE_AMOUNT;
                MinimalLongContractionsAmount = LONG_CONTRACTIONS_RATE_AMOUNT;
                MinimalAccelerationsAmount = ACCEL_RATE_AMOUNT;
                MinimalBaselineVariability = MINIMAL_BASELINE_VARIABILITY;
                CRIStateQualificationWindowSize = MINIMAL_QUALIFICATION_WINDOW_SIZE;
                MinimalAmountOfDataInQualificationWindow = MINIMAL_AMOUNT_OF_DATA_IN_WINDOW;
            }            
        }

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;
            try
            {                
                MinimalLateDecelConfidence = GetSettingsRealValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalLateDecelConfidence));
                MinimalLargeAndLongDecelAmount = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalLargeAndLongDecelAmount));
                MinimalLateDecelAmount = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalLateDecelAmount));
                MinimalLateAndLargeAndLongDecelAmount = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalLateAndLargeAndLongDecelAmount));
                MinimalLateAndProlongedDecelAmount = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalLateAndProlongedDecelAmount));
                MinimalProlongedDecelHeight = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalProlongedDecelHeight));
                MinimalContractionsAmount = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalContractionsAmount));
                MinimalLongContractionsAmount = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalLongContractionsAmount));
                MinimalAccelerationsAmount = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalAccelerationsAmount));
                MinimalBaselineVariability = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalBaselineVariability));

                CRIStateQualificationWindowSize = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.CRIStateQualificationWindowSize));
                MinimalAmountOfDataInQualificationWindow = GetSettingsIntValue(NameOf<CRIAlgorithmSettings>.Property(e => e.MinimalAmountOfDataInQualificationWindow));

                //check that everything succeeded to load
                if (MinimalLateDecelConfidence != -1.0 &&
                    CRIStateQualificationWindowSize != -1 &&
                    MinimalLateDecelAmount != -1 &&
                    MinimalLateAndLargeAndLongDecelAmount != -1 &&
                    MinimalLateAndProlongedDecelAmount != -1 &&
                    MinimalProlongedDecelHeight != -1 &&
                    MinimalContractionsAmount != -1 &&
                    MinimalLongContractionsAmount != -1 &&
                    MinimalAccelerationsAmount != -1 &&
                    MinimalBaselineVariability != -1 &&
                    MinimalProlongedDecelHeight != -1 &&
                    MinimalAmountOfDataInQualificationWindow != -1)
                {
                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CRIAlgorithm", "Service Settings: Failed to Read\\Update settings", ex);
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
