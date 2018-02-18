using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CurveChartControl
{
    public class CurveURLParameters
    {
        private string userName = string.Empty;
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        private string userId = string.Empty;
        public string UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        private string patientVisitKey = string.Empty;
        public string PatientVisitKey
        {
            get { return patientVisitKey; }
            set { patientVisitKey = value; }
        }

        private string curveURL = string.Empty;
        public string CurveURL
        {
            get { return curveURL; }
            set { curveURL = value; }
        }

        private string curveVersion = string.Empty;
        public string CurveVersion
        {
            get { return curveVersion; }
            set { curveVersion = value; }
        }

        private int curveClientRefresh = 5;
        public int CurveClientRefresh
        {
            get { return curveClientRefresh; }
            set { curveClientRefresh = value; }
        }

        private int banner = 0;
        public int Banner
        {
            get { return banner; }
            set { banner = value; }
        }

        private bool canModify = false;
        public bool CanModify
        {
            get { return canModify; }
            set { canModify = value; }
        }

        private bool canPrint = false;
        public bool CanPrint
        {
            get { return canPrint; }
            set { canPrint = value; }
        }

        private bool curveReviewModeEnabled = false;
        public bool CurveReviewModeEnabled
        {
            get { return curveReviewModeEnabled; }
            set { curveReviewModeEnabled = value; }
        }

        private bool podiSendEpidural = false;
        public bool PODISendEpidural
        {
            get { return podiSendEpidural; }
            set { podiSendEpidural = value; }
        }

        private bool podiSendVBAC = false;
        public bool PODISendVBAC
        {
            get { return podiSendVBAC; }
            set { podiSendVBAC = value; }
        }

        private bool podiSendVaginalDelivery = false;
        public bool PODISendVaginalDelivery
        {
            get { return podiSendVaginalDelivery; }
            set { podiSendVaginalDelivery = value; }
        }

        public CurveURLParameters()
        {
        }

        public string GetServiceURL()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("service={0}", curveURL));
            sb.Append(string.Format("&curve_client_refresh={0}", curveClientRefresh));
            sb.Append(string.Format("&banner={0}", banner));
            sb.Append(string.Format("&curve_review_mode_enabled={0}", curveReviewModeEnabled));
            sb.Append(string.Format("&PODI_send_epidural={0}", podiSendEpidural));
            sb.Append(string.Format("&PODI_send_vbac={0}", podiSendVBAC));
            sb.Append(string.Format("&PODI_send_vaginaldelivery={0}", podiSendVaginalDelivery));
            sb.Append(string.Format("&user_name={0}", userName));
            sb.Append(string.Format("&user_id={0}", userId));
            sb.Append(string.Format("&can_modify={0}", canModify));
            sb.Append(string.Format("&can_print={0}", canPrint));
            sb.Append(string.Format("&visit_key={0}", patientVisitKey));
            return sb.ToString();
        }
    }
}
