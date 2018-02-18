//Review: 17/02/15
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using CRIEntities;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace PatternsCRIClient.Data
{
    public class PatientData : DependencyObject, INotifyPropertyChanged
    {            
        private bool m_isSelectedPatient;
        private bool m_isCurrentBed;

        private string m_key;
        private int m_bedId;
        private CRIState m_CRIStatus;
        private string m_bedName;
        private string m_firstName;
        private string m_lastName;
        private string m_GA;
        private DateTime? m_EDD;
        private string m_patientDisplayName;
        private bool m_isLast;
        private int m_fetuses;
        private CRIObject m_currentDisplayCRI;
        private ObservableCollection<CRIObject> m_notRevewedPositiveCRIs;
        private bool m_isCurrentPositive;
        private string m_displayStartTime;
        private string m_displayEndTime;
        private string m_duration;
        private bool m_isADT;


        public List<ContractilityDisplay> LastHourContractilities { get; private set; }
        private EventCounterDouble m_variability;
        private EventCounterLong m_accels;
        private EventCounterLong m_contractions;
        private EventCounterLong m_longContractions;
        private EventCounterLong m_largeDeceles;
        private EventCounterLong m_lateDecels;
        private EventCounterLong m_prolongedDecels;

        public PatientData()
        {
            this.NotRevewedPositiveCRIs = new ObservableCollection<CRIObject>();
            this.LastHourContractilities = new List<ContractilityDisplay>();
            this.CurrentDisplayCRI = new CRIObject();
        }

        public PatientData(Visit visit)
        {
            Key = visit.VisitKey;
            BedId = visit.Bed.Id;
            BedName = visit.Bed.Name;
            FirstName = visit.FirstName;
            LastName = visit.LastName;
            Fetuses = visit.Fetuses;
            GA = visit.GA;
            EDD = visit.EDD;
            CRIStatus = visit.CurrentDisplayCRI.CRIStatus;
            PatientDisplayName = visit.GetDisplayName();
            IsLast = false;
            CurrentDisplayCRI = visit.CurrentDisplayCRI;
            visit.PositivePastNotYetReviewedCRIs.Sort((a, b) => b.StartTime.CompareTo(a.StartTime));
            NotRevewedPositiveCRIs = new ObservableCollection<CRIObject>(visit.PositivePastNotYetReviewedCRIs);
            IsCurrentPositive = visit.IsCurrentPositive;
            DisplayStartTime = visit.CurrentDisplayCRI.DisplayStartTime;
            DisplayEndTime = visit.CurrentDisplayCRI.DisplayEndTime;
            Duration = visit.CurrentDisplayCRI.Duration;

            if (visit.LastHourContractilities != null)
            {
                LastHourContractilities = new List<ContractilityDisplay>(visit.LastHourContractilities);
            }
            else
            {
                LastHourContractilities = new List<ContractilityDisplay>();
            }

            Variability = visit.CurrentDisplayCRI.CRIStatusEvents.Variability;
            Accels = visit.CurrentDisplayCRI.CRIStatusEvents.Accels;
            Contractions = visit.CurrentDisplayCRI.CRIStatusEvents.Contractions;
            LongContractions = visit.CurrentDisplayCRI.CRIStatusEvents.LongContractions;
            LargeDeceles = visit.CurrentDisplayCRI.CRIStatusEvents.LargeDeceles;
            LateDecels = visit.CurrentDisplayCRI.CRIStatusEvents.LateDecels;
            ProlongedDecels = visit.CurrentDisplayCRI.CRIStatusEvents.ProlongedDecels;
            IsCurrentBed = false;
            IsADT = visit.IsADT;
        }

        public void CopyData(PatientData patient)
        {
            this.Key = patient.Key;
            this.BedId = patient.BedId;
            this.BedName = patient.BedName;
            this.FirstName = patient.FirstName;
            this.LastName = patient.LastName;
            this.CRIStatus = patient.CRIStatus;
            this.Fetuses = patient.Fetuses;
            this.GA = patient.GA;
            this.EDD = patient.EDD;
            this.PatientDisplayName = patient.PatientDisplayName;
            this.IsLast = patient.IsLast;
            this.CurrentDisplayCRI = patient.CurrentDisplayCRI;

            List<CRIObject> listCRI = new List<CRIObject>();
            if (patient.NotRevewedPositiveCRIs != null)
            {
                listCRI.AddRange(patient.NotRevewedPositiveCRIs.OrderByDescending(a => a.StartTime).ToList());

                this.DisplayStartTime = patient.CurrentDisplayCRI.DisplayStartTime;
                this.DisplayEndTime = patient.CurrentDisplayCRI.DisplayEndTime;
            }
            this.NotRevewedPositiveCRIs = new ObservableCollection<CRIObject>(listCRI);
            LastHourContractilities = new List<ContractilityDisplay>(patient.LastHourContractilities);

            if (patient.LastHourContractilities != null)
            {
                LastHourContractilities = new List<ContractilityDisplay>(patient.LastHourContractilities);
            }
            else
            {
                LastHourContractilities = new List<ContractilityDisplay>();
            }

            this.IsCurrentPositive = patient.IsCurrentPositive;
            this.IsSelectedPatient = patient.IsSelectedPatient;
            this.IsCurrentBed = patient.IsCurrentBed;
            this.SortBedNameIndex = patient.SortBedNameIndex;
            this.SortDisplayNameIndex = patient.SortDisplayNameIndex;
            this.Duration = patient.CurrentDisplayCRI.Duration;

            this.Variability = patient.Variability;
            this.Accels = patient.Accels;
            this.Contractions = patient.Contractions;
            this.LongContractions = patient.LongContractions;
            this.LargeDeceles = patient.LargeDeceles;
            this.LateDecels = patient.LateDecels;
            this.ProlongedDecels = patient.ProlongedDecels;
            this.IsADT = patient.IsADT;
        }

        #region Property

        public bool IsADT
        {
            get
            {
                return m_isADT;
            }
            set
            {
                if (m_isADT != value)
                {
                    m_isADT = value;
                    RaisePropertyChanged("IsADT");
                }
            }
        }

        public string Key
        {
            get
            {
                return m_key;
            }
            set
            {
                if (m_key != value)
                {
                    m_key = value;
                    RaisePropertyChanged("Key");
                }
            }
        }

        public int BedId
        {
            get
            {
                return m_bedId;
            }
            set
            {
                if (m_bedId != value)
                {
                    m_bedId = value;
                    RaisePropertyChanged("BedId");
                }
            }
        }
       
        public string BedName
        {
            get
            {
                return m_bedName;
            }
            set
            {
                if (m_bedName != value)
                {
                    m_bedName = value;
                    RaisePropertyChanged("BedName");
                }
            }
        }
       
        public string FirstName
        {
            get
            {
                return m_firstName;
            }
            set
            {
                if (m_firstName != value)
                {
                    m_firstName = value;
                    RaisePropertyChanged("FirstName");
                }
            }
        }

        public string LastName
        {
            get
            {
                return m_lastName;
            }
            set
            {
                if (m_lastName != value)
                {
                    m_lastName = value;
                    RaisePropertyChanged("LastName");
                }
            }
        }

        public string GA
        {
            get
            {
                return m_GA;
            }
            set
            {
                if (m_GA != value)
                {
                    m_GA = value;
                    RaisePropertyChanged("GA");
                }
            }
        }

        public DateTime? EDD
        {
            get
            {
                return m_EDD;
            }
            set
            {
                if (m_EDD != value)
                {
                    m_EDD = value;
                    RaisePropertyChanged("EDD");
                }
            }
        }
  
        public string PatientDisplayName
        {
            get
            {
                return m_patientDisplayName;
            }
            set
            {
                if (m_patientDisplayName != value)
                {
                    m_patientDisplayName = value;
                    RaisePropertyChanged("PatientDisplayName");
                }
            }
        }

        public bool IsLast
        {
            get
            {
                return m_isLast;
            }
            set
            {
                if (m_isLast != value)
                {
                    m_isLast = value;
                    RaisePropertyChanged("IsLast");
                }
            }
        }

        public int Fetuses
        {
            get
            {
                return m_fetuses;
            }
            set
            {
                if (m_fetuses != value)
                {
                    m_fetuses = value;
                    RaisePropertyChanged("Fetuses");
                }
            }
        }

        public CRIObject CurrentDisplayCRI
        {
            get
            {
                return m_currentDisplayCRI;
            }
            set
            {
                if (m_currentDisplayCRI != value)
                {
                    m_currentDisplayCRI = value;
                    RaisePropertyChanged("CurrentDisplayCRI");
                }
            }
        }

        public string DisplayStartTime
        {
            get { return m_displayStartTime; }
            set
            {
                if (m_displayStartTime != value)
                {
                    m_displayStartTime = value;
                    RaisePropertyChanged("DisplayStartTime");
                }
            }
        }

        public string DisplayEndTime
        {
            get { return m_displayEndTime; }
            set
            {
                if (m_displayEndTime != value)
                {
                    m_displayEndTime = value;
                    RaisePropertyChanged("DisplayEndTime");
                }
            }
        }

        public string Duration
        {
            get { return m_duration; }
            set
            {
                if (m_duration != value)
                {
                    m_duration = value;
                    RaisePropertyChanged("Duration");
                }
            }
        }

        public ObservableCollection<CRIObject> NotRevewedPositiveCRIs
        {
            get
            {
                return m_notRevewedPositiveCRIs;
            }
            set
            {
                m_notRevewedPositiveCRIs = value;
                RaisePropertyChanged("NotRevewedPositiveCRIs");
            }
        }

        public bool IsCurrentPositive
        {
            get
            {
                return m_isCurrentPositive;
            }
            set
            {
                if (m_isCurrentPositive != value)
                {
                    m_isCurrentPositive = value;
                    RaisePropertyChanged("IsCurrentPositive");
                }
            }
        }

        public int SortDisplayNameIndex { get; set; }

        public int SortBedNameIndex { get; set; }

        public CRIState CRIStatus
        {
            get
            {
                return m_CRIStatus;
            }
            set
            {
                if (m_CRIStatus != value)
                {
                    m_CRIStatus = value;
                    RaisePropertyChanged("CRIStatus");
                }
            }
        }

        public bool IsSelectedPatient
        {
            get
            {
                return m_isSelectedPatient;
            }
            set
            {
                if (m_isSelectedPatient != value)
                {
                    m_isSelectedPatient = value;
                    RaisePropertyChanged("IsSelectedPatient");
                }
            }
        }

        public bool IsCurrentBed
        {
            get
            {
                return m_isCurrentBed;
            }
            set
            {
                if (m_isCurrentBed != value)
                {
                    m_isCurrentBed = value;
                    RaisePropertyChanged("IsCurrentBed");
                }
            }
        }

        #endregion

        #region CRI Events

        public EventCounterDouble Variability 
        {
            get { return m_variability; }
            set
            {
                if (m_variability != value)
                {
                    m_variability = value;
                    RaisePropertyChanged("Variability.Value");
                    RaisePropertyChanged("Variability.IsReason");
                }
            }
        }

        public EventCounterLong Accels
        {
            get { return m_accels; }
            set
            {
                if (m_accels != value)
                {
                    m_accels = value;
                    RaisePropertyChanged("Accels.Value");
                    RaisePropertyChanged("Accels.IsReason");
                }
            }
        }

        public EventCounterLong Contractions
        {
            get { return m_contractions; }
            set
            {
                if (m_contractions != value)
                {
                    m_contractions = value;
                    RaisePropertyChanged("Contractions.Value");
                    RaisePropertyChanged("Contractions.IsReason");
                }
            }
        }

        public EventCounterLong LongContractions
        {
            get { return m_longContractions; }
            set
            {
                if (m_longContractions != value)
                {
                    m_longContractions = value;
                    RaisePropertyChanged("LongContractions.Value");
                    RaisePropertyChanged("LongContractions.IsReason");
                }
            }
        }

        public EventCounterLong LargeDeceles
        {
            get { return m_largeDeceles; }
            set
            {
                if (m_largeDeceles != value)
                {
                    m_largeDeceles = value;
                    RaisePropertyChanged("LargeDeceles.Value");
                    RaisePropertyChanged("LargeDeceles.IsReason");
                }
            }
        }

        public EventCounterLong LateDecels
        {
            get { return m_lateDecels; }
            set
            {
                if (m_lateDecels != value)
                {
                    m_lateDecels = value;
                    RaisePropertyChanged("LateDecels.Value");
                    RaisePropertyChanged("LateDecels.IsReason");
                }
            }
        }

        public EventCounterLong ProlongedDecels
        {
            get { return m_prolongedDecels; }
            set
            {
                if (m_prolongedDecels != value)
                {
                    m_prolongedDecels = value;
                    RaisePropertyChanged("ProlongedDecels.Value");
                    RaisePropertyChanged("ProlongedDecels.IsReason");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
