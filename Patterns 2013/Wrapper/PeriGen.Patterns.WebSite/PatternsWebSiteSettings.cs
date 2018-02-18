using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Diagnostics;
using PeriGenSettingsManager;

namespace PeriGen.Patterns.WebSite
{
    public class PatternsWebSiteSettings : ServiceSettings
    {
        #region Settings Values

        public bool DisableAdminAPILocalIPValidation { get; private set; }
        public bool PatternsEnabled { get; private set; }
        public bool CurveEnabled { get; private set; }
        public bool PODISendVaginalDelivery { get; private set; }
        public bool PODISendVBAC { get; private set; }
        public bool CurveReviewModeEnabled { get; private set; }
        public bool PODISendEpidural { get; private set; }
        public bool EnableCheckList { get; private set; }

        public string DataFeedURL { get; private set; }
        public string UrlOpenPatterns { get; private set; }
        public string UrlOpenCurve { get; private set; }
        public string URLPassword { get; private set; }
        public string ActiveXClient { get; private set; }
        public string VersionPatterns { get; private set; }

        public int Banner { get; private set; }
        public int CurveClientRefresh { get; private set; }
        public int PatternsClientRefresh { get; private set; }
        public int CR_limit { get; private set; }
        public int CR_window { get; private set; }
        public int CR_stage1 { get; private set; }
        public int CR_stage2 { get; private set; }
        public int PatternsClientTimeout { get; private set; }

        #endregion

        #region Singleton Initialization

        private static Object s_lockObject = new Object();
        private static PatternsWebSiteSettings s_instance = null;
        public static PatternsWebSiteSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new PatternsWebSiteSettings();
                    }
                }

                return s_instance;
            }
        }

        private PatternsWebSiteSettings()
            :base(true)
        {
            bool bRes = UpdateEngineSettings();
            if (!bRes)
            {
                throw new InvalidProgramException("You must configure the settings before you can start the service!");
            }
        }

        #endregion

        public override bool UpdateEngineSettings()
        {
            bool bRes = false;

            try
            {
                string strDisableAdminAPILocalIPValidation = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.DisableAdminAPILocalIPValidation));
                DisableAdminAPILocalIPValidation = Convert.ToBoolean(strDisableAdminAPILocalIPValidation);

                string strPatternsEnabled = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.PatternsEnabled));
                PatternsEnabled = Convert.ToBoolean(strPatternsEnabled);

                string strCurveEnabled = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.CurveEnabled));
                CurveEnabled = Convert.ToBoolean(strCurveEnabled);

                string strPODISendVaginalDelivery = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.PODISendVaginalDelivery));
                PODISendVaginalDelivery = Convert.ToBoolean(strPODISendVaginalDelivery);

                string strPODISendVBAC = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.PODISendVBAC));
                PODISendVBAC = Convert.ToBoolean(strPODISendVBAC);

                string strCurveReviewModeEnabled = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.CurveReviewModeEnabled));
                CurveReviewModeEnabled = Convert.ToBoolean(strCurveReviewModeEnabled);

                string strPODISendEpidural = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.PODISendEpidural));
                PODISendEpidural = Convert.ToBoolean(strPODISendEpidural);

                string strEnableCheckList = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.EnableCheckList));
                EnableCheckList = Convert.ToBoolean(strEnableCheckList);

                DataFeedURL = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.DataFeedURL));
                UrlOpenPatterns = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.UrlOpenPatterns));
                UrlOpenCurve = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.UrlOpenCurve));
                URLPassword = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.URLPassword));
                ActiveXClient = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.ActiveXClient));
                VersionPatterns = GetSettingsStrValue(NameOf<PatternsWebSiteSettings>.Property(e => e.VersionPatterns));

                Banner = GetSettingsIntValue(NameOf<PatternsWebSiteSettings>.Property(e => e.Banner));
                CurveClientRefresh = GetSettingsIntValue(NameOf<PatternsWebSiteSettings>.Property(e => e.CurveClientRefresh));
                PatternsClientRefresh = GetSettingsIntValue(NameOf<PatternsWebSiteSettings>.Property(e => e.PatternsClientRefresh));
                CR_limit = GetSettingsIntValue(NameOf<PatternsWebSiteSettings>.Property(e => e.CR_limit));
                CR_window = GetSettingsIntValue(NameOf<PatternsWebSiteSettings>.Property(e => e.CR_window));
                CR_stage1 = GetSettingsIntValue(NameOf<PatternsWebSiteSettings>.Property(e => e.CR_stage1));
                CR_stage2 = GetSettingsIntValue(NameOf<PatternsWebSiteSettings>.Property(e => e.CR_stage2));
                PatternsClientTimeout = GetSettingsIntValue(NameOf<PatternsWebSiteSettings>.Property(e => e.PatternsClientTimeout));

                //check that everything succeeded to load
                if (DataFeedURL != String.Empty &&
                    UrlOpenPatterns != String.Empty &&
                    UrlOpenCurve != String.Empty &&
                    URLPassword != String.Empty &&
                    ActiveXClient != String.Empty &&
                    VersionPatterns != String.Empty &&
                    Banner != -1 &&
                    CurveClientRefresh != -1 &&
                    PatternsClientRefresh != -1 &&
                    CR_limit != -1 &&
                    CR_window != -1 &&
                    CR_stage1 != -1 &&
                    CR_stage2 != -1 &&
                    strDisableAdminAPILocalIPValidation != String.Empty &&
                    strPatternsEnabled != String.Empty &&
                    strCurveEnabled != String.Empty &&
                    strPODISendVaginalDelivery != String.Empty &&
                    strPODISendVBAC != String.Empty &&
                    strCurveReviewModeEnabled != String.Empty &&
                    strPODISendEpidural != String.Empty &&
                    strEnableCheckList != String.Empty)
                {
                    bRes = true;
                }

            }
            catch (Exception ex)
            {
                PeriGenLogger.Logger.WriteLogEntry(TraceEventType.Critical, "PetternsWebSiteSettings", "Failed to Read\\Update settings", ex);
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