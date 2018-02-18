//Review: 17/02/15
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CRIEntities;
using RestSharp;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using PatternsCALMMediator;
using PatternsCRIClient.Data;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommonLogger;

namespace PatternsCRIClient
{
    public class ClientManager
    {
        #region Properties & Members

        private CALMMediator m_calmMediator; 

        public ClientSettings Settings { get; private set; }
        public List<PatientData> Patients { get; private set; }
        public PatientData CurrentPatient { get; set; }
      
        #endregion

        #region Singleton functionality

        private static volatile ClientManager s_instance;
        private static object s_lockObject = new Object();

        private ClientManager()
        {
            Settings = new ClientSettings();
            Patients = new List<PatientData>();

            Task.Factory.StartNew(() =>
            {
                m_calmMediator = CALMMediator.Instance;

                AppLoadCompleted();

                m_calmMediator.VisitsUpdatedEvent += OnVisitsUpdated;
            });          
        }

        public static ClientManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new ClientManager();
                    }
                }
                return s_instance;
            }
        }

        #endregion

        #region Events & Delegates

        private void OnVisitsUpdated(object sender, VisitsUpdatedEventArgs e)
        {
            FillPatientsDataInternal(e.Visits);
        }

        public EventHandler<UpdateDataEventArgs> UpdateDataEvent;
        public void FireEventUpdateData()
        {
            var tempHandler = UpdateDataEvent;
            if (tempHandler != null)
            {
                tempHandler(this, new UpdateDataEventArgs(Patients));
            }
        }

        public EventHandler<VisibilityEventArgs> UpdateVisibilityEvent;
        public void FireEventUpdateVisibility(Visibility state, bool openList, bool isMainWindowActive)
        {
            var tempHandler = UpdateVisibilityEvent;
            if (tempHandler != null)
            {
                tempHandler(this, new VisibilityEventArgs(state, openList, isMainWindowActive));
            }
        }

        #endregion

        private void AppLoadCompleted()
        {
            try
            {
                CALMNavigationParameters navParams = new CALMNavigationParameters("", IntPtr.Zero);

                if (navParams.SourceUrl.ToUpper().Contains("PATTERNS") == true)
                {
                    PatternsCRIClient.Downloader.DownloadsManager.Instance.UpdateVersionEvent += VersionUpdated;
                    PatternsCRIClient.Downloader.DownloadsManager.Instance.CheckDependencies(navParams.SourceUrl);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Error, PatternsCRIClient.Properties.Resources.CRIClient_ModuleName, ex.Message);
            }
        }

        private void VersionUpdated(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                PatternsCRIClient.Screens.UpgradeVersionWindow upgradeWindow = new PatternsCRIClient.Screens.UpgradeVersionWindow();
                upgradeWindow.Show();
            }));
        }

        public bool SetPositiveCRIsToReviewed(string visitKey)
        {
            return m_calmMediator.SetPositiveCRIsToReviewed(visitKey);
        }

        public List<Bed> GetAvailableBeds()
        {
            return m_calmMediator.GetBeds(true);
        }

        public bool EmergencyAdmit(string firstName, string lastName, Bed bed, int fetusCount, DateTime? edd)
        {
            return m_calmMediator.EmergencyAdmit(firstName, lastName, bed, fetusCount, edd);
        }

        public bool EditPatientData(string visitKey, string firstName, string lastName, int fetuses, DateTime? edd, string bedId)
        {
            bool result = true;

            result &= m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_FIRSTNAME, CALMMediator.OOC_MOTHER, firstName);
            result &= m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_LASTNAME, CALMMediator.OOC_MOTHER, lastName);
            result &= m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_FETUSCOUNT, CALMMediator.OOC_MOTHER, fetuses.ToString());

            if (edd.HasValue)
            {
                string strEDD = edd.Value.ToString(CALMMediator.DATE_FORMAT);
                result &= m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_EDD, CALMMediator.OOC_MOTHER, strEDD);
            }
            else
            {
                result &= m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_EDD, CALMMediator.OOC_MOTHER, null);
            }

            result &= m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_BEDID, CALMMediator.OOC_MOTHER, bedId);

            if (result == true)
            {
                return m_calmMediator.SaveAllChanges();
            }
            else
            {
                m_calmMediator.CancelAllChanges();
                return false;
            }
        }

        public bool Login(string username, string password)
        {
            return m_calmMediator.Login(username, password);
        }

        public bool Logout()
        {
            m_calmMediator.Logout();

            return m_calmMediator.Login(Settings.CALMUser, String.Empty);
        }

        public bool CheckUserRights(int function, int action)
        {
            return m_calmMediator.CheckUserRights(function, action);
        }

        public bool CheckADTUserRights()
        {
            return m_calmMediator.CheckUserRights(CALMMediator.USERGROUP_F_ADT, CALMMediator.USERGROUP_A_MODIFY);
        }

        public bool CheckMergeUserRights()
        {
            return m_calmMediator.CheckUserRights(CALMMediator.USERGROUP_F_PATIENT_MERGE, CALMMediator.USERGROUP_A_YES);
        }

        public bool QuickEditPatient(string visitKey, int fetuses, DateTime? edd)
        {
            m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_FETUSCOUNT, CALMMediator.OOC_MOTHER, fetuses.ToString());

            if (edd.HasValue)
            {
                string strEDD = edd.Value.ToString(CALMMediator.DATE_FORMAT);
                m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_EDD, CALMMediator.OOC_MOTHER, strEDD);
            }
            else
            {
                m_calmMediator.AddEditObservation(visitKey, CALMMediator.CONCEPT_EDD, CALMMediator.OOC_MOTHER, null);
            }

            return m_calmMediator.SaveAllChanges();
        }

        public bool DischargePatient(string visitKey)
        {
            return m_calmMediator.EmergencyDischarge(visitKey);
        }

        public bool IsMonitorActive(int bedId)
        {
            return m_calmMediator.IsActivelyCollectingFetalTracingData(bedId);
        }

        public bool EmergencyMerge(PatientData mergePatient, PatientData withPatient)
        {
            return m_calmMediator.EmergencyMerge(mergePatient.BedId, withPatient.BedId, true, mergePatient.IsADT ? true : false, true, true);
        }

        public void SetSelectedPatient(string visitKey)
        {
            ResetSelection();

            if (String.IsNullOrEmpty(visitKey) == false)
            {
                PatientData patData = Patients.FirstOrDefault(t => t.Key == visitKey);

                if (patData != null)
                {
                    patData.IsSelectedPatient = true;
                }
            }
        }

        public string GetCurrentBed()
        {
            return m_calmMediator.DefaultBed;
        }

        public AlgorithmParameters GetAlgorithmParameters()
        {
            return m_calmMediator.GetAlgorithmParameters();
        }

        public void ResetSelection()
        {
            PatientData data = Patients.FirstOrDefault(t => t.IsSelectedPatient == true);

            if (data != null)
            {
                data.IsSelectedPatient = false;
            }
        }


        private PatientData GetCurrentPatient()
        {
            if (Settings.IsCentralMode == false)
            {
                PatientData patient = null;

                try
                {
                    patient = Patients.FirstOrDefault(t => t.BedName == m_calmMediator.DefaultBed);

                    if (patient != null)
                    {
                        string msg = String.Format("CheckList State of '{0}' was set to '{1}'", patient.CRIStatus, patient.Key);
                        Logger.WriteLogEntry(TraceEventType.Verbose, Properties.Resources.CRIClient_ModuleName, msg);
                    }
                    else
                    {
                        patient = SetUnknownPatient();
                    }
                }
                catch(Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Error, Properties.Resources.CRIClient_ModuleName, ex.Message);

                    patient = SetUnknownPatient();
                }
                             
                return patient;              
            }

            return null;
        }

        private PatientData SetUnknownPatient()
        {
            PatientData patient = new PatientData();
            patient.CRIStatus = CRIState.Off;
            patient.BedName = m_calmMediator.DefaultBed;

            return patient;
        }

        private void FillPatientsDataInternal(List<Visit> data)
        {
            //lock (s_lockFillData)
            //{
                List<PatientData> patientsData = new List<PatientData>();

                if (data != null)
                {
                    ////// FOR TEST ONLY!!!
#if DEBUG
                    data.ForEach(visit =>
                    {
                        if (visit.FirstName != null)
                        {
                            int index = visit.FirstName.LastIndexOf("_");

                            if (index > -1)
                            {
                                string state = visit.FirstName.Substring(index + 1);
                                visit.CurrentDisplayCRI.CRIStatus = (CRIState)Convert.ToInt32(state);
                            }
                        }
                    });
#endif
                    ///////////////////////

                    try
                    {
                        patientsData.GetSortedRange(data);
                        Patients.MergePatients(patientsData);
  
                        this.CurrentPatient = GetCurrentPatient();

                        if (this.CurrentPatient != null)
                        {
                            this.CurrentPatient.IsCurrentBed = true;
                        }

                        FireEventUpdateData();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, Properties.Resources.CRIClient_ModuleName, ex.Message);

                        //Patients.ForEach(patient =>
                        //{
                        //    patient.CRIStatus = CRIState.Off;
                        //});

                        //FireEventUpdateData();
                    }                  
                }
                else
                {
                    Logger.WriteLogEntry(TraceEventType.Error, Properties.Resources.CRIClient_ModuleName, "Empty Patients data.");

                    Patients.ForEach(patient =>
                    {
                        patient.CRIStatus = CRIState.Off;
                    });

                    FireEventUpdateData();
                }              
            //}
        }
        
        public string CalcGestationalAge(DateTime edd)
        {
            string ga = String.Empty;

            if (edd != DateTime.MinValue)
            {
                DateTime startDate = edd.AddDays(-280).ToUniversalTime();
                DateTime nowDate = DateTime.UtcNow;

                double nDays = (nowDate - startDate).TotalDays;

                int weeks = Convert.ToInt32(nDays) / 7;
                int days = Convert.ToInt32(nDays) % 7;

                ga = String.Format("{0}+{1}", weeks, days);
            }

            return ga;
        }

        public void SetExpandedViewVisibility(Visibility state, bool openList, bool isMainWindowActive)
        {
            FireEventUpdateVisibility(state, openList, isMainWindowActive);
        }
    }


    public class UpdateDataEventArgs : EventArgs
    {
        public UpdateDataEventArgs(List<PatientData> data)
        {
            PatientsData = new List<PatientData>(data);
        }

        public List<PatientData> PatientsData { get; set; }
    }

    public class VisibilityEventArgs : EventArgs
    {
        public VisibilityEventArgs(Visibility state, bool openList, bool isMainWindowActive)
        {
            Visibility = state;
            OpenPatientList = openList;
            IsMainWindowActive = isMainWindowActive;
        }

        public Visibility Visibility { get; set; }
        public bool OpenPatientList { get; set; }
        public bool IsMainWindowActive { get; set; }
    }
}
