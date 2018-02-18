using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGenSettingsManager;
using System.Diagnostics;
using System.Linq.Expressions;
using PeriGenLogger;

namespace PeriGen.Patterns.Service
{
    public class PatternsServiceSettings : ServiceSettings
    {
        #region Settings Values

        public string PODILinkURL { get; private set; }
        public string PODICertificateHash { get; private set; }
        public int MaximumDurationWithoutRefreshToGoOffline { get; private set; }
        public int TracingSplitMaximumBlock { get; private set; }
        public int MaintenanceDelay { get; private set; }
        public int ChalkboardRefreshDelay { get; private set; }
        public int CommitEpisodesDelay { get; private set; }
        public int CompressCachedTracingDelay { get; private set; }
        public int ProcessDataDelay { get; private set; }
        public int ServiceStopTimeout { get; private set; }
        public int SystemRequirements_Core_Count { get; private set; }
        public int SystemRequirements_Memory_MB { get; private set; }
        public string PatternsDBPath { get; private set; }
        public string ArchiveDBPath { get; private set; }
        public bool PatientsDiscoverability { get; private set; }
        public string CatalogDatabaseFilename { get; private set; }
        public string ArchiveCatalogDatabaseFilename { get; private set; }
        public string DBConnectionStringFormat { get; private set; }
        public int DelayBeforeDeleteDischargedVisits { get; private set; }

        public int PatternsEngineMinimumDurationToAppend { get; private set; }
        public int PatternsEngineMaximumBridgeableGap { get; private set; }
        public int PatternsEngineMaximumMergeBlockSize { get; private set; }
        public int PatternsEngineBufferPrimingDelay { get; private set; }
        public int CompressCachedTracingMaximumGap { get; private set; }
        public int CompressCachedTracingMaximumDuration { get; private set; }
        public int CompressCachedTracingExcludeLastBlocks { get; private set; }
        public int DatabaseMergeBridgeableGap { get; private set; }
        public int DatabaseMergeMaximumBlock { get; private set; }
        public int MaxDaysOfTracingReturnedToActiveX { get; private set; }
        public int TracingLateCollectingLive { get; private set; }
        public bool WriteLogOfTracingsDataToPatternsEngine { get; private set; }
        public string[] CephalicCodes { get; private set; }
        public int VBACPercentileLimits { get; private set; }
        public int VBACDilatationUnchanged { get; private set; }
        public int NonVBACPercentileLimits { get; private set; }
        public int NonVBACDilatationUnchanged { get; private set; }
        public bool Display50PercentileCurve { get; private set; }
        public bool PODISendVBAC { get; private set; }
        public bool PODISendVaginalDelivery { get; private set; }
        public bool PODISendEpidural { get; private set; }

        public bool TraceExtendedPatternsData { get; private set; }
        public bool TraceExtendedCurveData { get; private set; }
        public bool DisableAdminAPILocalIPValidation { get; private set; }
        public string VersionCurve { get; private set; }
        public string VersionPatterns { get; private set; }
        public string WCFServiceBaseAddress { get; private set; }

        public bool PatternsEnabled { get; private set; }
        public bool CurveEnabled { get; private set; }

        public int DefaultConnectionLimit { get; private set; }
        public int MaxServicePointIdleTime { get; private set; }
        public string PositionMappings { get; private set; }

        #endregion

        #region Singleton Initialization

        private static Object s_lockObject = new Object();
        private static PatternsServiceSettings s_instance = null;
        public static PatternsServiceSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new PatternsServiceSettings();
                    }
                }

                return s_instance;
            }
        }

        private PatternsServiceSettings()
        {
            bool bRes = UpdateEngineSettings();
            if (!bRes)
            {
                throw new InvalidProgramException("You must configure the settings before you can start the service!");
            }
            // Log Settings 
            //Common.Source.TraceEvent(TraceEventType.Verbose, 7120, "Settings loaded");
        }

        #endregion

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;

            try
            {
                PODILinkURL = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PODILinkURL));
                PODICertificateHash = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PODICertificateHash));
                MaximumDurationWithoutRefreshToGoOffline = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.MaximumDurationWithoutRefreshToGoOffline));
                TracingSplitMaximumBlock = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.TracingSplitMaximumBlock));
                MaintenanceDelay = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.MaintenanceDelay));
                ChalkboardRefreshDelay = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.ChalkboardRefreshDelay));
                CommitEpisodesDelay = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.CommitEpisodesDelay));
                CompressCachedTracingDelay = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.CompressCachedTracingDelay));
                ProcessDataDelay = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.ProcessDataDelay));
                ServiceStopTimeout = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.ServiceStopTimeout));
                SystemRequirements_Core_Count = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.SystemRequirements_Core_Count));
                SystemRequirements_Memory_MB = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.SystemRequirements_Memory_MB));
                PatternsDBPath = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PatternsDBPath));
                ArchiveDBPath = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.ArchiveDBPath));
                String patientsDiscoverability = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PatientsDiscoverability));
                PatientsDiscoverability = Convert.ToBoolean(patientsDiscoverability);
                CatalogDatabaseFilename = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.CatalogDatabaseFilename));
                ArchiveCatalogDatabaseFilename = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.ArchiveCatalogDatabaseFilename));
                DBConnectionStringFormat = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.DBConnectionStringFormat));
                DelayBeforeDeleteDischargedVisits = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.DelayBeforeDeleteDischargedVisits));

                PatternsEngineMinimumDurationToAppend = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.PatternsEngineMinimumDurationToAppend));
                PatternsEngineMaximumBridgeableGap = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.PatternsEngineMaximumBridgeableGap));
                PatternsEngineMaximumMergeBlockSize = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.PatternsEngineMaximumMergeBlockSize));
                PatternsEngineBufferPrimingDelay = Math.Max(0, GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.PatternsEngineBufferPrimingDelay)));
                CompressCachedTracingMaximumGap = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.CompressCachedTracingMaximumGap));
                CompressCachedTracingMaximumDuration = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.CompressCachedTracingMaximumDuration));
                CompressCachedTracingExcludeLastBlocks = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.CompressCachedTracingExcludeLastBlocks));
                DatabaseMergeBridgeableGap = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.DatabaseMergeBridgeableGap));
                DatabaseMergeMaximumBlock = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.DatabaseMergeMaximumBlock));
                MaxDaysOfTracingReturnedToActiveX = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.MaxDaysOfTracingReturnedToActiveX));
                TracingLateCollectingLive = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.TracingLateCollectingLive));
                string strWriteLogOfTracingsDataToPatternsEngine = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.WriteLogOfTracingsDataToPatternsEngine));
                WriteLogOfTracingsDataToPatternsEngine = Convert.ToBoolean(strWriteLogOfTracingsDataToPatternsEngine);
                string strCephalicCodes = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.CephalicCodes));
                CephalicCodes = strCephalicCodes.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(v => v.ToUpperInvariant()).ToArray();
                VBACPercentileLimits = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.VBACPercentileLimits));
                VBACDilatationUnchanged = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.VBACDilatationUnchanged));
                NonVBACPercentileLimits = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.NonVBACPercentileLimits));
                NonVBACDilatationUnchanged = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.NonVBACDilatationUnchanged));
                string strDisplay50PercentileCurve = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.Display50PercentileCurve));
                Display50PercentileCurve = Convert.ToBoolean(strDisplay50PercentileCurve);
                string strPODISendVBAC = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PODISendVBAC));
                PODISendVBAC = Convert.ToBoolean(strPODISendVBAC);
                string strPODISendVaginalDelivery = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PODISendVaginalDelivery));
                PODISendVaginalDelivery = Convert.ToBoolean(strPODISendVaginalDelivery);
                string strPODISendEpidural = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PODISendEpidural));
                PODISendEpidural = Convert.ToBoolean(strPODISendEpidural);

                string strTraceExtendedPatternsData = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PODISendEpidural));
                TraceExtendedPatternsData = Convert.ToBoolean(strTraceExtendedPatternsData);
                string strTraceExtendedCurveData = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.TraceExtendedCurveData));
                TraceExtendedCurveData = Convert.ToBoolean(strTraceExtendedCurveData);
                string strDisableAdminAPILocalIPValidation = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.DisableAdminAPILocalIPValidation));
                DisableAdminAPILocalIPValidation = Convert.ToBoolean(strDisableAdminAPILocalIPValidation);
                VersionCurve = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.VersionCurve));
                VersionPatterns = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.VersionPatterns));
                WCFServiceBaseAddress = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.WCFServiceBaseAddress));

                string strPatternsEnabled = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PatternsEnabled));
                PatternsEnabled = Convert.ToBoolean(strPatternsEnabled);

                string strCurveEnabled = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.CurveEnabled));
                CurveEnabled = Convert.ToBoolean(strCurveEnabled);

                DefaultConnectionLimit = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.DefaultConnectionLimit));
                MaxServicePointIdleTime = GetSettingsIntValue(NameOf<PatternsServiceSettings>.Property(e => e.MaxServicePointIdleTime));
                PositionMappings = GetSettingsStrValue(NameOf<PatternsServiceSettings>.Property(e => e.PositionMappings));

                //check that everything succeeded to load
                if (PODILinkURL != String.Empty &&
                    PODICertificateHash != String.Empty &&
                    MaximumDurationWithoutRefreshToGoOffline != -1 &&
                    TracingSplitMaximumBlock != -1 &&
                    MaintenanceDelay != -1 &&
                    ChalkboardRefreshDelay != -1 &&
                    CommitEpisodesDelay != -1 &&
                    CompressCachedTracingDelay != -1 &&
                    ProcessDataDelay != -1 &&
                    ServiceStopTimeout != -1 &&
                    SystemRequirements_Core_Count != -1 &&
                    SystemRequirements_Memory_MB != -1 &&
                    PatternsDBPath != String.Empty &&
                    ArchiveDBPath != String.Empty &&
                    patientsDiscoverability != String.Empty &&
                    CatalogDatabaseFilename != String.Empty &&
                    ArchiveCatalogDatabaseFilename != String.Empty &&
                    DBConnectionStringFormat != String.Empty &&
                    DelayBeforeDeleteDischargedVisits != -1 &&
                    PatternsEngineMinimumDurationToAppend != -1 &&
                    PatternsEngineMaximumBridgeableGap != -1 &&
                    PatternsEngineMaximumMergeBlockSize != -1 &&
                    PatternsEngineBufferPrimingDelay != -1 &&
                    CompressCachedTracingMaximumGap != -1 &&
                    CompressCachedTracingMaximumDuration != -1 &&
                    CompressCachedTracingExcludeLastBlocks != -1 &&
                    DatabaseMergeBridgeableGap != -1 &&
                    DatabaseMergeMaximumBlock != -1 &&
                    MaxDaysOfTracingReturnedToActiveX != -1 &&
                    TracingLateCollectingLive != -1 &&
                    strWriteLogOfTracingsDataToPatternsEngine != String.Empty &&
                    strCephalicCodes != String.Empty &&
                    VBACPercentileLimits != -1 &&
                    VBACDilatationUnchanged != -1 &&
                    NonVBACPercentileLimits != -1 &&
                    NonVBACDilatationUnchanged != -1 &&
                    strDisplay50PercentileCurve != String.Empty &&
                    strPODISendVBAC != String.Empty &&
                    strPODISendVaginalDelivery != String.Empty &&
                    strPODISendEpidural != String.Empty &&
                    strTraceExtendedPatternsData != String.Empty &&
                    strTraceExtendedCurveData != String.Empty &&
                    strDisableAdminAPILocalIPValidation != String.Empty &&
                    VersionCurve != String.Empty &&
                    VersionPatterns != String.Empty &&
                    WCFServiceBaseAddress != String.Empty &&
                    strPatternsEnabled != String.Empty &&
                    strCurveEnabled != String.Empty &&
                    DefaultConnectionLimit != -1 &&
                    MaxServicePointIdleTime != -1 &&
                    PositionMappings != String.Empty)
                {
                    bRes = true;
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "PatternsServiceSettings", "Failed to Read\\Update settings", ex);
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
