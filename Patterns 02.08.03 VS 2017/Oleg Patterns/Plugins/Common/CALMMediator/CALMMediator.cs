//Review: 27/04/15
using CALMConnector;
using CommonLogger;
using CRIEntities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PatternsCALMMediator
{
    public class CALMMediator
    {
        #region Readonly

        public static readonly int CONCEPT_FIRSTNAME = LMS.Data.Concepts.Types._patientFirstName;
        public static readonly int CONCEPT_LASTNAME = LMS.Data.Concepts.Types._patientLastName;
        public static readonly int CONCEPT_BEDID = LMS.Data.Concepts.Types._area;
        public static readonly int CONCEPT_FETUSCOUNT = LMS.Data.Concepts.Types._fetus_count;
        public static readonly int CONCEPT_EDD = LMS.Data.Concepts.Types._edc;
        public static readonly int OOC_MOTHER = LMS.Data.ObjectOfCare.Mgr.MOTHER;
        public static readonly string DATE_FORMAT = "MMM dd yyyy";

        public static readonly int USERGROUP_F_ADT = 1071050;
        public static readonly int USERGROUP_F_PATIENT_MERGE = 1071326;
        public static readonly int WEB_BUTTON_1 = 4000600;

        public static readonly int USERGROUP_A_MODIFY = 1071010;
        public static readonly int USERGROUP_A_YES = 1071000;

        #endregion

        #region Properties & Members

        private object m_lock = new object();
        private CALMConnection m_calmConnection;

        private Timer m_updateVisitsTimer;
        private Timer m_calmConnectionTimer;

        public string DefaultBed
        {
            get
            {
                if (m_calmConnection != null)
                {
                    var bed = m_calmConnection.GetDefaultBed();
                    if (bed != null)
                    {
                        return bed.PatientAreaName;
                    }
                }

                return String.Empty;
            }
        }

        #endregion

        #region Initialization & Singleton functionality

        private static CALMMediator s_CALMMediator = null;
        private static Object s_lockObject = new Object();

        private CALMMediator()
        {
            m_calmConnection = CALMConnection.Instance;

            ConnectToCALM();

            //get initial data
            UpdateVisits();

            //start timers
            m_updateVisitsTimer = new Timer();
            m_updateVisitsTimer.Interval = CALMMediatorSettings.Instance.CRIPluginRequestInterval;
            m_updateVisitsTimer.Elapsed += OnUpdateVisitsEvent;
            m_updateVisitsTimer.Enabled = true;

            m_calmConnectionTimer = new Timer();
            m_calmConnectionTimer.Interval = CALMMediatorSettings.Instance.CRIPluginRequestInterval;
            m_calmConnectionTimer.Elapsed += OnCheckConnection;
            m_calmConnectionTimer.Enabled = true;
        }

        private void ConnectToCALM()
        {
            m_calmConnection.ADTModifiedEvent += OnCALMDataUpadedEvent;
            m_calmConnection.NewDataLoadedEvent += OnCALMDataUpadedEvent;
            m_calmConnection.LPMDataModifiedEvent += OnCALMDataUpadedEvent;
            m_calmConnection.RefreshRequestedEvent += OnCALMDataUpadedEvent;

            //connect to CALM
            bool connSuccess = m_calmConnection.Start();

            //login to CALM
            if (m_calmConnection.IsStandAloneMode())
            {
                bool loginSuccess = m_calmConnection.Login(CALMMediatorSettings.Instance.CALMUser);
            }
            else
            {
                bool loginSuccess = m_calmConnection.Login("\\" + CALMMediatorSettings.Instance.CALMUser, String.Empty, true);
            }
        }

        private void RemoveCALMEvents()
        {
            m_calmConnection.ADTModifiedEvent -= OnCALMDataUpadedEvent;
            m_calmConnection.NewDataLoadedEvent -= OnCALMDataUpadedEvent;
            m_calmConnection.LPMDataModifiedEvent -= OnCALMDataUpadedEvent;
            m_calmConnection.RefreshRequestedEvent -= OnCALMDataUpadedEvent;
        }

        public static CALMMediator Instance
        {
            get
            {
                if (s_CALMMediator == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_CALMMediator == null)
                        {
                            s_CALMMediator = new CALMMediator();
                        }
                    }
                }

                return s_CALMMediator;
            }
        }

        #endregion

        #region Events & Delegates

        private void ConnectionTimerEnabled(bool isEnabled)
        {
           if( m_calmConnectionTimer != null)
           {
               m_calmConnectionTimer.Enabled = isEnabled;
           }
        }

        private void OnCheckConnection(object sender, ElapsedEventArgs e)
        {
            ConnectionTimerEnabled(false);

            lock (m_lock)
            {
                try
                {
                    if (m_calmConnection.IsConnected() == false)
                    {
                        RemoveCALMEvents();
                        ConnectToCALM();

                        if (m_calmConnection.IsConnected() == true)
                        {
                            Logger.WriteLogEntry(TraceEventType.Warning, "CALMMediator", "Identified connection error. Succeeded to reconnect CALM Services");
                        }
                        else
                        {
                            Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Identified connection error. Failed to reconnect CALM Services");
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on OnCheckConnection", ex);
                }
            }

            ConnectionTimerEnabled(true);
        }

        private void UpdateVisitsTimerEnabled(bool isEnabled)
        {
            if (m_updateVisitsTimer != null)
            {
                m_updateVisitsTimer.Enabled = isEnabled;
            }
        }

        private void OnUpdateVisitsEvent(object sender, ElapsedEventArgs e)
        {
            UpdateVisitsTimerEnabled(false);

            lock (m_lock)
            {
                UpdateVisits();
            }

            UpdateVisitsTimerEnabled(true);
        }

        private void OnCALMDataUpadedEvent(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                UpdateVisits();
            }
        }

        public EventHandler<VisitsUpdatedEventArgs> VisitsUpdatedEvent;
        private void FireVisitsUpdatedEvent(List<Visit> visits)
        {
            Task.Factory.StartNew(() =>
            {
                var tempHandler = VisitsUpdatedEvent;
                if (tempHandler != null)
                {
                    tempHandler(this, new VisitsUpdatedEventArgs(visits));
                }
            });
        }

        #endregion

        public List<Bed> GetBeds(bool onlyUnOccupied)
        {
            lock (m_lock)
            {
                List<Bed> availableBeds = new List<Bed>();

                try
                {
                    availableBeds = GetBedsInternal(onlyUnOccupied);
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error On GetAvailableBeds", ex);
                }

                return availableBeds;
            }
        }

        private List<Bed> GetBedsInternal(bool onlyUnOccupied)
        {
            List<Bed> availableBeds = new List<Bed>();

            LMS.Data.Transient.PatientAreaSet beds = m_calmConnection.GetBedsSet(true);
            var enumerator = beds.GetEnumerator();
            while (enumerator.MoveNext())
            {
                // Retrieve the visit
                if (enumerator.Current == null)
                    continue;

                LMS.Data.Transient.PatientArea calmPatienArea = enumerator.Current as LMS.Data.Transient.PatientArea;
                Bed bed = new Bed()
                {
                    Id = calmPatienArea.PatientAreaNo,
                    Name = calmPatienArea.PatientAreaName
                };

                if (!onlyUnOccupied || (onlyUnOccupied && !calmPatienArea.IsOccupied))
                {
                    availableBeds.Add(bed);
                }
            }

            return availableBeds;
        }

        public bool SetPositiveCRIsToReviewed(string visitKey)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    var client = new RestClient(CALMMediatorSettings.Instance.CRIPluginURL);
                    var request = new RestRequest("CRIEpisodes/Review/{visitkey}/user/{username}", Method.POST);
                    request.AddParameter("visitkey", visitKey, ParameterType.UrlSegment);
                    request.AddParameter("username", CALMMediatorSettings.Instance.CALMUser, ParameterType.UrlSegment);
                    request.Timeout = CALMMediatorSettings.Instance.CRIPluginRequestTimeOut;

                    var response = client.Execute(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        UpdateVisits();
                        bRes = true;
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "SetPositiveCRIToReviewed succeeded to POST visit review, for visit " + visitKey);
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "SetPositiveCRIToReviewed failed to  to POST visit review, for visit " + visitKey);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error On SetPositiveCRIToReviewed", ex);
                }

                return bRes;
            }
        }

        public bool EmergencyAdmit(string firstName, string LastName, Bed bed, int fetusCount, DateTime? edd)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorString = String.Empty;
                    bRes = m_calmConnection.EmergencyAdmit(firstName, LastName, bed.Id, fetusCount, edd, out errorString);

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to admit on EmergencyAdmit");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to admit on EmergencyAdmit. error: " + errorString);
                    }
                }
                catch (System.Exception ex) // Catches every exception
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on EmergencyAdmit", ex);
                }

                return bRes;
            }
        }

        public bool EmergencyDischarge(string visitKey)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorString = String.Empty;
                    bRes = m_calmConnection.EmergencyDischarge(visitKey, out errorString);

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to discharge on EmergencyDischarge");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to discharge on EmergencyDischarge. error: " + errorString);
                    }
                }
                catch (System.Exception ex) // Catches every exception
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on EmergencyDischarge", ex);
                }

                return bRes;
            }
        }

        public bool EmergencyMerge(int bedIdA, int bedIdB, bool isVisitADestinationPriority, bool isVisitADemographicPriority, bool isVisitAClinicalPriority, bool isVisitATracingPriority)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorString = String.Empty;
                    bRes = m_calmConnection.EmergencyMerge(bedIdA, bedIdB, isVisitADestinationPriority, isVisitADemographicPriority, isVisitAClinicalPriority, isVisitATracingPriority, out errorString);

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to merge on EmergencyMerge");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to merge on EmergencyMerge. error: " + errorString);
                    }
                }
                catch (System.Exception ex) // Catches every exception
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on EmergencyMerge", ex);
                }

                return bRes;
            }
        }

        public bool IsActivelyCollectingFetalTracingData(int bedId)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorString = String.Empty;
                    bRes = m_calmConnection.IsActivelyCollectingFetalTracingData(bedId, out errorString);

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to IsActivelyCollectingFetalTracingData");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to IsActivelyCollectingFetalTracingData. error: " + errorString);
                    }
                }
                catch (System.Exception ex) // Catches every exception
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on IsActivelyCollectingFetalTracingData", ex);
                }

                return bRes;
            }
        }

        public bool AddEditObservation(string visitKey, int conceptNo, int ooc, string value)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorString = String.Empty;
                    if (conceptNo == CONCEPT_BEDID)
                    {
                        bRes = m_calmConnection.TransferPatient(visitKey, Convert.ToInt32(value), out errorString);
                    }
                    else
                    {
                        bRes = m_calmConnection.AddEditObservation(visitKey, conceptNo, ooc, value);
                    }

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to admit on AddEditObservation");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to admit on AddEditObservation. error: " + errorString);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on AddEditObservation", ex);
                }

                return bRes;
            }
        }

        public bool CheckUserRights(int function, int action)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorString = String.Empty;
                    bRes = m_calmConnection.CheckUserRights(function, action, out errorString);

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to CheckUserRights");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to CheckUserRights. error: " + errorString);
                    }
                }
                catch (System.Exception ex) // Catches every exception
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on CheckUserRights", ex);
                }

                return bRes;
            }
        }

        public bool Login(string username, string password)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorString = String.Empty;
                    
                    //login to CALM
                    if (username.Equals(CALMMediatorSettings.Instance.CALMUser) && !m_calmConnection.IsStandAloneMode())
                    {
                        bRes = m_calmConnection.Login("\\" + username, String.Empty, true);                     
                    }
                    else
                    {
                        bRes = m_calmConnection.Login(username, password);
                    }
                                                           
                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to Login");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to Login. error: " + errorString);
                    }
                }
                catch (System.Exception ex) // Catches every exception
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on Login", ex);
                }

                return bRes;
            }
        }

        public bool Logout()
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorString = String.Empty;
                    bRes = m_calmConnection.Logout();

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to Logout");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to Logout. error: " + errorString);
                    }
                }
                catch (System.Exception ex) // Catches every exception
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on Logout", ex);
                }

                return bRes;
            }
        }

        public bool SaveAllChanges()
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorStr = String.Empty;
                    bRes = m_calmConnection.SaveAllChanges(out errorStr);

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success to SaveAllChanges");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to SaveAllChanges. error = " + errorStr);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on SaveAllChanges", ex);
                }

                return bRes;
            }
        }

        public bool CancelAllChanges()
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    string errorStr = String.Empty;
                    bRes = m_calmConnection.CancelAllChanges(out errorStr);

                    if (bRes)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Success CancelAllChanges");
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to CancelAllChanges. error = " + errorStr);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Error on CancelAllChanges", ex);
                }

                return bRes;
            }
        }

        public AlgorithmParameters GetAlgorithmParameters()
        {
            AlgorithmParameters algParams = null;

            var client = new RestClient(CALMMediatorSettings.Instance.CRIPluginURL);
            var request = new RestRequest("AlgorithmParameters", Method.GET);
            request.Timeout = CALMMediatorSettings.Instance.CRIPluginRequestTimeOut;

            var response = client.Execute<AlgorithmParameters>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                algParams = response.Data;
                Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Succeeded to GetAlgorithmParameters");
            }
            else
            {
                Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to GetAlgorithmParameters, response status = " + response.StatusCode);
            }

            return algParams;
        }

        private void UpdateVisits()
        {
            try
            {
                List<Visit> calmVisits = GetCALMAdmittedVisits();
                List<CRIEpisode> criEpisodes = GetCRIEpisodes();

                foreach (var calmVisit in calmVisits)
                {
                    CRIEpisode episode = criEpisodes.FirstOrDefault(v => v.VisitKey.Equals(calmVisit.VisitKey));
                    if (episode != null)
                    {
                        calmVisit.CurrentDisplayCRI = episode.CurrentDisplayCRI;
                        calmVisit.PositivePastNotYetReviewedCRIs = episode.PositivePastNotYetReviewedCRIs;
                        calmVisit.LastHourContractilities = episode.LastHourContractilities;
                    }
                }

                FireVisitsUpdatedEvent(calmVisits);
                Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Succeeded to UpdateVisits");
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CALMMediator", "Failed to UpdateVisits", ex);
                FireVisitsUpdatedEvent(null);
            }
        }

        private List<CRIEpisode> GetCRIEpisodes()
        {
            List<CRIEpisode> episodes = new List<CRIEpisode>();

            var client = new RestClient(CALMMediatorSettings.Instance.CRIPluginURL);
            var request = new RestRequest("CRIEpisodes", Method.GET);
            request.Timeout = CALMMediatorSettings.Instance.CRIPluginRequestTimeOut;

            var response = client.Execute<List<CRIEpisode>>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                episodes = response.Data;
                Logger.WriteLogEntry(TraceEventType.Verbose, "CALMMediator", "Succeeded to GetVisitsFromCRIPlugin, visits count = " + episodes.Count);
            }
            else
            {
                Logger.WriteLogEntry(TraceEventType.Error, "CALMMediator", "Failed to GetVisitsFromCRIPlugin, response status = " + response.StatusCode);
            }

            return episodes;
        }

        private List<Visit> GetCALMAdmittedVisits()
        {
            List<Visit> visits = new List<Visit>();
            List<Bed> beds = GetBedsInternal(false);

            var visitsEnumerator = m_calmConnection.GetVisitsSet().GetEnumerator();
            while (visitsEnumerator.MoveNext())
            {
                // Retrieve the visit
                if (visitsEnumerator.Current == null)
                    continue;

                LMS.Data.Transient.Visit calmVisit = visitsEnumerator.Value as LMS.Data.Transient.Visit;
                LMS.Data.Transient.PatientArea calmPatientArea = m_calmConnection.GetBed(calmVisit.AreaNo);

                if (beds.FirstOrDefault(b => b.Id == calmVisit.AreaNo) == null)
                    continue;

                Visit visit = new Visit();
                visit.VisitKey = calmVisit.Key.Key2String();
                visit.Bed.Id = calmVisit.AreaNo;
                visit.Bed.Name = calmPatientArea.PatientAreaName;
                visit.FirstName = calmVisit.GetDirectValueAsString(LMS.Data.Concepts.Types._patientFirstName);
                visit.LastName = calmVisit.GetDirectValueAsString(LMS.Data.Concepts.Types._patientLastName);
                visit.Fetuses = calmVisit.FetusCount;
                visit.GA = calmVisit.GetDirectValueAsString(LMS.Data.Concepts.Types._gestational_age);
                visit.IsADT = !calmVisit.PatientHospitalNo.StartsWith("@@") ? true : false;

                string strEdd = calmVisit.GetDirectValueAsString(LMS.Data.Concepts.Types._edc);
                DateTime edd;
                bool bSuccess = DateTime.TryParse(strEdd, out edd);
                if (bSuccess)
                {
                    visit.EDD = edd;
                }

                visits.Add(visit);
            }

            return visits;
        }
    }

    public class VisitsUpdatedEventArgs : EventArgs
    {
        public VisitsUpdatedEventArgs(List<Visit> visits)
        {
            if (visits != null)
            {
                Visits = new List<Visit>(visits);
            }
        }

        public List<Visit> Visits { get; set; }
    }
}
