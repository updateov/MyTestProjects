//Review: 27/04/15
using CommonLogger;
using Microsoft.Win32;
using PeriGenSettingsManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRIPlugin
{
    public class CRIPluginSettings : ServiceSettings
    {
        #region Const        

        private const float CONTRACTILITY_POSITIVE_DURABILITY = 1;
        private const int CONTRACTILITY_POSITIVE_PERCENTAGE = 100;
        private const float CONTRACTILITY_NEGATIVE_DURABILITY = 1;
        private const int CONTRACTILITY_NEGATIVE_PERCENTAGE = 100;

        private const String CALM_SERVER_LINK = "net.tcp://localhost:7110/MMService";
        private const string PATTERNS_URL = "http://localhost:7802/PatternsDataFeed/";
        private const int PATTERNS_REQUEST_INTERVAL = 15000;
        private const int PATTERNS_REQUEST_TIMEOUT = 3000;
        private const string PATTERNS_VERSION = "01.00.00.00";

        private const bool SAVESTATISTICALDATASET = false;
        private const int MINIMAL_DOWNTIME_DURATION_FOR_SYSTEM_ACK = 15;

        private const string PATTERNS_SERVICE_REGISTRY_KEY = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\PeriCALMPatternsService";

        private const int CRI_STATE_CONCEPT_ID = 0;

        #endregion

        #region Properties & Members        

        public float ContractilityPositiveDurability { get; private set; }
        public int ContractilityPositivePercentage { get; private set; }
        public float ContractilityNegativeDurability { get; private set; }
        public int ContractilityNegativePercentage { get; private set; }

        public string PatternsDataFeed { get; private set; }
        public int PatternsRequestInterval { get; private set; }
        public int PatternsRequestTimeOut { get; private set; }
        public string PatternsVersion { get; private set; }
        
        public bool SaveStatisticalDataset { get; private set; }
        public int MinimalDowntimeDurationForSystemAck { get; private set; }

        public int CRIStateConceptID { get; private set; }
        public String CALMServerLink { get; set; }

        #endregion

        #region Algorithm Parameters

        public int MinimalBaselineVariability { get; set; }
        public double MinimalLateDecelConfidence { get; set; }
        public int MinimalAccelerationsAmount { get; set; }
        public int MinimalLateDecelAmount { get; set; }
        public int MinimalLargeAndLongDecelAmount { get; set; }
        public int MinimalLateAndLargeAndLongDecelAmount { get; set; }
        public int MinimalLateAndProlongedDecelAmount { get; set; }
        public int MinimalProlongedDecelHeight { get; set; }
        public int MinimalContractionsAmount { get; set; }
        public int MinimalLongContractionsAmount { get; set; }
        public int CRIStateQualificationWindowSize { get; set; }
        public int MinimalAmountOfDataInQualificationWindow { get; set; }

        #endregion 

        #region Singleton Initialization
        
        private static Object s_lockObject = new Object();
        private static CRIPluginSettings s_instance = null;

        private CRIPluginSettings()
        {
            bool bRes = UpdateEngineSettings();
            if (!bRes)
            {
                ContractilityPositiveDurability = CONTRACTILITY_POSITIVE_DURABILITY;
                ContractilityPositivePercentage = CONTRACTILITY_POSITIVE_PERCENTAGE;
                ContractilityNegativeDurability = CONTRACTILITY_NEGATIVE_DURABILITY;
                ContractilityNegativePercentage = CONTRACTILITY_NEGATIVE_PERCENTAGE;

                PatternsDataFeed = PATTERNS_URL;
                PatternsRequestInterval = PATTERNS_REQUEST_INTERVAL;
                PatternsRequestTimeOut = PATTERNS_REQUEST_TIMEOUT;
                PatternsVersion = PATTERNS_VERSION;

                SaveStatisticalDataset = SAVESTATISTICALDATASET;
                MinimalDowntimeDurationForSystemAck = MINIMAL_DOWNTIME_DURATION_FOR_SYSTEM_ACK;
                CRIStateConceptID = CRI_STATE_CONCEPT_ID;
                CALMServerLink = CALM_SERVER_LINK;
            }

            UpdateAlgorithmParameters();
        }

        public static CRIPluginSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new CRIPluginSettings();
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
                ContractilityPositiveDurability = GetSettingsRealValue(NameOf<CRIPluginSettings>.Property(e => e.ContractilityPositiveDurability));
                ContractilityPositivePercentage = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.ContractilityPositivePercentage));
                ContractilityNegativeDurability = GetSettingsRealValue(NameOf<CRIPluginSettings>.Property(e => e.ContractilityNegativeDurability));
                ContractilityNegativePercentage = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.ContractilityNegativePercentage));

                PatternsDataFeed = GetSettingsStrValue(NameOf<CRIPluginSettings>.Property(e => e.PatternsDataFeed));
                PatternsRequestInterval = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.PatternsRequestInterval));
                PatternsRequestTimeOut = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.PatternsRequestTimeOut));
                                
                string strSaveStatisticalDataset = GetSettingsStrValue(NameOf<CRIPluginSettings>.Property(e => e.SaveStatisticalDataset));
                SaveStatisticalDataset = Convert.ToBoolean(strSaveStatisticalDataset);
                MinimalDowntimeDurationForSystemAck = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalDowntimeDurationForSystemAck));

                string servicePath = Registry.GetValue(PATTERNS_SERVICE_REGISTRY_KEY, "ImagePath", String.Empty) as String;
                if (!String.IsNullOrEmpty(servicePath))
                {
                    servicePath = servicePath.Replace("\"", String.Empty);
                    Logger.WriteLogEntry(TraceEventType.Information, "CRIPluginSettings", "servicePath = " + servicePath);
                    FileVersionInfo serviceFileInfo = FileVersionInfo.GetVersionInfo(servicePath);
                    PatternsVersion = serviceFileInfo.FileVersion;
                }
                else
                {
                    PatternsVersion = PATTERNS_VERSION;
                }

                CRIStateConceptID = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.CRIStateConceptID));
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginSettings", "CRIStateConceptID = " + CRIStateConceptID);

                CALMServerLink = GetSettingsStrValue(NameOf<CRIPluginSettings>.Property(e => e.CALMServerLink));

                //check that everything succeeded to load
                if (ContractilityPositiveDurability != -1 &&
                    ContractilityPositivePercentage != -1 &&
                    ContractilityNegativeDurability != -1 &&
                    ContractilityNegativePercentage != -1 &&
                    PatternsDataFeed != String.Empty &&
                    PatternsRequestInterval != -1 &&
                    PatternsRequestTimeOut != -1 &&
                    strSaveStatisticalDataset != String.Empty &&
                    MinimalDowntimeDurationForSystemAck != -1)
                {
                    bRes = true;
                }                               
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginSettings", "Service Settings: Failed to Read\\Update settings", ex);
            }

            return bRes;
        }

        public bool UpdateAlgorithmParameters()
        {
            bool bRes = false;
            try
            {
                MinimalBaselineVariability = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalBaselineVariability));
                MinimalLateDecelConfidence = GetSettingsRealValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalLateDecelConfidence));
                MinimalAccelerationsAmount = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalAccelerationsAmount));
                MinimalLateDecelAmount = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalLateDecelAmount));
                MinimalLargeAndLongDecelAmount = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalLargeAndLongDecelAmount));
                MinimalLateAndLargeAndLongDecelAmount = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalLateAndLargeAndLongDecelAmount));
                MinimalLateAndProlongedDecelAmount = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalLateAndProlongedDecelAmount));
                MinimalProlongedDecelHeight = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalProlongedDecelHeight));
                MinimalContractionsAmount = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalContractionsAmount));
                MinimalLongContractionsAmount = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalLongContractionsAmount));
                CRIStateQualificationWindowSize = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.CRIStateQualificationWindowSize));
                MinimalAmountOfDataInQualificationWindow = GetSettingsIntValue(NameOf<CRIPluginSettings>.Property(e => e.MinimalAmountOfDataInQualificationWindow));

                bRes = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginSettings", "Service Settings: Failed to Read\\Update settings", ex);
                bRes = false;
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
