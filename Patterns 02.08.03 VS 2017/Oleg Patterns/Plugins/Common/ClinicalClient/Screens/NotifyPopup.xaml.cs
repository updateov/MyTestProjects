using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PeriGen.Patterns.WPFLibrary.Screens;


namespace PatternsCRIClient.Screens
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Interop;
    using System.Diagnostics;
    using System.Threading;
    using System.Runtime.InteropServices;
    using CRIEntities;
    using PatternsCRIClient.Data;
    using System.ComponentModel;
    using System.Data;
    using System.Collections.ObjectModel;
    using PatternsCALMMediator;
    using CommonLogger;

    public class Win32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName,
                                            string lpWindowName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
    }

    public partial class NotifyPopup : Window, INotifyPropertyChanged
    {
        #region Properties & Members

        private Storyboard gridFadeInStoryBoard;
        private Storyboard gridFadeOutStoryBoard;
        private Process m_process = null;
        private IntPtr m_hHandle = IntPtr.Zero;
        private bool m_isAutoMode = false;
        private PatientData m_unknownPatient;
        private string m_displayFromToTime;
        private string m_pastTabLable;
        private Visibility m_historyTabVisibility;
        private int m_fetusNum = 1;
        private DateTime? m_EDD = DateTime.MinValue;
        private string m_GA;
        private bool m_isMainWindowActive = false;

        private string m_variability = String.Empty;
        private string m_accels = String.Empty;
        private string m_contractions = String.Empty;
        private string m_longContractions = String.Empty;
        private string m_largeDeceles = String.Empty;
        private string m_lateDecels = String.Empty;
        private string m_prolongedDecels = String.Empty;

        private bool m_isBoldVariability;
        private bool m_isBoldAccels;
        private bool m_isBoldContractions;
        private bool m_isBoldLongContractions;
        private bool m_isBoldLargeDeceles;
        private bool m_isBoldLateDecels;
        private bool m_isBoldProlongedDecels;

        public List<int> Fetuses { get; set; }

        public DateTime EDD
        {
            get
            {
                if (m_EDD.HasValue)
                    return m_EDD.Value;
                else
                    return DateTime.MinValue;
            }
            set
            {
                if (m_EDD != value)
                {
                    if (value == DateTime.MinValue)
                        m_EDD = null;
                    else
                        m_EDD = value;

                    RaisePropertyChanged("EDD");
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

        public int FetusNum
        {
            get
            {
                return m_fetusNum;
            }
            set
            {
                if (m_fetusNum != value)
                {
                    m_fetusNum = value;
                    RaisePropertyChanged("FetusNum");
                }
            }
        }

        public string DisplayFromToTime
        {
            get
            {
                return m_displayFromToTime;
            }
            set
            {
                if(m_displayFromToTime != value)
                {
                    m_displayFromToTime = value;
                    RaisePropertyChanged("DisplayFromToTime");
                }
            }
        }

        public string PastTabLable
        {
            get
            {
                return m_pastTabLable;
            }
            set
            {
                if (m_pastTabLable != value)
                {
                    m_pastTabLable = value;
                    RaisePropertyChanged("PastTabLable");
                }
            }
        }

        public Visibility HistoryTabVisibility
        {
            get
            {
                return m_historyTabVisibility;
            }
            set
            {
                if (m_historyTabVisibility != value)
                {
                    m_historyTabVisibility = value;
                    RaisePropertyChanged("HistoryTabVisibility");
                }
            }
        }
       
        #endregion

        #region CRI Events properties

        public string Variability
        {
            get
            {
                return m_variability;
            }
            set
            {
                if (m_variability != value)
                {
                    m_variability = value;
                    RaisePropertyChanged("Variability");
                }
            }
        }

        public bool IsBoldVariability
        {
            get
            {
                return m_isBoldVariability;
            }
            set
            {
                if (m_isBoldVariability != value)
                {
                    m_isBoldVariability = value;
                    RaisePropertyChanged("IsBoldVariability");
                }
            }
        }

        public string Accels
        {
            get
            {
                return m_accels;
            }
            set
            {
                if (m_accels != value)
                {
                    m_accels = value;
                    RaisePropertyChanged("Accels");
                }
            }
        }

        public bool IsBoldAccels
        {
            get
            {
                return m_isBoldAccels;
            }
            set
            {
                if (m_isBoldAccels != value)
                {
                    m_isBoldAccels = value;
                    RaisePropertyChanged("IsBoldAccels");
                }
            }
        }

        public string Contractions
        {
            get
            {
                return m_contractions;
            }
            set
            {
                if (m_contractions != value)
                {
                    m_contractions = value;
                    RaisePropertyChanged("Contractions");
                }
            }
        }

        public bool IsBoldContractions
        {
            get
            {
                return m_isBoldContractions;
            }
            set
            {
                if (m_isBoldContractions != value)
                {
                    m_isBoldContractions = value;
                    RaisePropertyChanged("IsBoldContractions");
                }
            }
        }

        public string LongContractions
        {
            get
            {
                return m_longContractions;
            }
            set
            {
                if (m_longContractions != value)
                {
                    m_longContractions = value;
                    RaisePropertyChanged("LongContractions");
                }
            }
        }

        public bool IsBoldLongContractions
        {
            get
            {
                return m_isBoldLongContractions;
            }
            set
            {
                if (m_isBoldLongContractions != value)
                {
                    m_isBoldLongContractions = value;
                    RaisePropertyChanged("IsBoldLongContractions");
                }
            }
        }

        public string LargeDeceles
        {
            get
            {
                return m_largeDeceles;
            }
            set
            {
                if (m_largeDeceles != value)
                {
                    m_largeDeceles = value;
                    RaisePropertyChanged("LargeDeceles");
                }
            }
        }

        public bool IsBoldLargeDeceles
        {
            get
            {
                return m_isBoldLargeDeceles;
            }
            set
            {
                if (m_isBoldLargeDeceles != value)
                {
                    m_isBoldLargeDeceles = value;
                    RaisePropertyChanged("IsBoldLargeDeceles");
                }
            }
        }

        public string LateDecels
        {
            get
            {
                return m_lateDecels;
            }
            set
            {
                if (m_lateDecels != value)
                {
                    m_lateDecels = value;
                    RaisePropertyChanged("LateDecels");
                }
            }
        }

        public bool IsBoldLateDecels
        {
            get
            {
                return m_isBoldLateDecels;
            }
            set
            {
                if (m_isBoldLateDecels != value)
                {
                    m_isBoldLateDecels = value;
                    RaisePropertyChanged("IsBoldLateDecels");
                }
            }
        }

        public string ProlongedDecels
        {
            get
            {
                return m_prolongedDecels;
            }
            set
            {
                if (m_prolongedDecels != value)
                {
                    m_prolongedDecels = value;
                    RaisePropertyChanged("ProlongedDecels");
                }
            }
        }

        public bool IsBoldProlongedDecels
        {
            get
            {
                return m_isBoldProlongedDecels;
            }
            set
            {
                if (m_isBoldProlongedDecels != value)
                {
                    m_isBoldProlongedDecels = value;
                    RaisePropertyChanged("IsBoldProlongedDecels");
                }
            }
        }

        #endregion

        #region Dependency Property

        public PatientData CurrentPatient
        {
            get { return (PatientData)GetValue(CurrentPatientProperty); }
            set { SetValue(CurrentPatientProperty, value); }
        }
        public static readonly DependencyProperty CurrentPatientProperty =
            DependencyProperty.Register("CurrentPatient", typeof(PatientData), typeof(NotifyPopup), new PropertyMetadata(null));


        public Visibility StatusDescriptionVisibility
        {
            get { return (Visibility)GetValue(StatusDescriptionVisibilityProperty); }
            set { SetValue(StatusDescriptionVisibilityProperty, value); }
        }
        public static readonly DependencyProperty StatusDescriptionVisibilityProperty =
            DependencyProperty.Register("StatusDescriptionVisibility", typeof(Visibility), typeof(NotifyPopup), new PropertyMetadata(Visibility.Collapsed));


        public bool IsEnableSave
        {
            get { return (bool)GetValue(IsEnableSaveProperty); }
            set { SetValue(IsEnableSaveProperty, value); }
        }
        public static readonly DependencyProperty IsEnableSaveProperty =
            DependencyProperty.Register("IsEnableSave", typeof(bool), typeof(NotifyPopup), new PropertyMetadata(false));


        public Visibility PatientMenuVisible
        {
            get { return (Visibility)GetValue(PatientMenuVisibleProperty); }
            set { SetValue(PatientMenuVisibleProperty, value); }
        }
        public static readonly DependencyProperty PatientMenuVisibleProperty =
            DependencyProperty.Register("PatientMenuVisible", typeof(Visibility), typeof(NotifyPopup), new PropertyMetadata(Visibility.Collapsed));


        public Visibility AdmitMenuVisible
        {
            get { return (Visibility)GetValue(AdmitMenuVisibleProperty); }
            set { SetValue(AdmitMenuVisibleProperty, value); }
        }
        public static readonly DependencyProperty AdmitMenuVisibleProperty =
            DependencyProperty.Register("AdmitMenuVisible", typeof(Visibility), typeof(NotifyPopup), new PropertyMetadata(Visibility.Collapsed));      
        
        #endregion

        #region Define Messages

        const int WM_ICONERASEBKGND = 0x0027;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONDBLCLK = 0x0203;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_APPCOMMAND = 0x0319;
        const int WM_COPYDATA = 0x004A;
        const int OWM_AGENT_MSG = 0x800B;

        #endregion

        /// <summary>
        /// Sets up the popup window and instantiates the notify icon
        /// </summary>
        public NotifyPopup()
        {
            this.Fetuses = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 }; 
            
            InitializeComponent();

            App.ClientManager.UpdateDataEvent += ClientManager_evUpdateDataEvent;   

            SetWindowToBottomRightOfScreen();
            this.Opacity = 0;
            uiGridMain.Opacity = 0;

            gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutStoryBoard");
            gridFadeOutStoryBoard.Completed += new EventHandler(gridFadeOutStoryBoard_Completed);
            gridFadeInStoryBoard = (Storyboard)TryFindResource("gridFadeInStoryBoard");
            gridFadeInStoryBoard.Completed += new EventHandler(gridFadeInStoryBoard_Completed);

            if (App.ClientManager.CurrentPatient != null)
            {
                this.CurrentPatient = new PatientData();
                this.CurrentPatient.CopyData(App.ClientManager.CurrentPatient);

                try
                {
                    string CRIStatusTime = String.Empty;
                    if (this.CurrentPatient.CurrentDisplayCRI.CRIStatusTime > DateTime.MinValue)
                    {
                        CRIStatusTime = String.Format("{0:HH:mm} - {1:HH:mm}", this.CurrentPatient.CurrentDisplayCRI.CRIStatusTime - new TimeSpan(0, 30, 0), this.CurrentPatient.CurrentDisplayCRI.CRIStatusTime);
                    }
                    this.DisplayFromToTime = CRIStatusTime;
                }
                catch(Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Error, PatternsCRIClient.Properties.Resources.CRIClient_ModuleName, "CRIStatusTime Exception", ex);
                }

                this.PastTabLable = String.Format("Past ({0})", this.CurrentPatient.NotRevewedPositiveCRIs.Count);
                this.HistoryTabVisibility = this.CurrentPatient.NotRevewedPositiveCRIs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                this.FetusNum = this.CurrentPatient.Fetuses;
                SetCRIEvents();

                if (this.CurrentPatient.EDD.HasValue)
                {
                    this.EDD = this.CurrentPatient.EDD.Value;
                    this.GA = this.CurrentPatient.GA;
                }
            }
            else
            {
                this.m_unknownPatient = new PatientData();
                this.m_unknownPatient.CRIStatus = CRIState.Off;
                this.CurrentPatient = this.m_unknownPatient;
            }

            SetActionMenuOptions();
            SetStatusDescriptionVisibility();
        }

        #region UI methods
       
        private void SetCRIEvents()
        {
            try
            {
                if (String.IsNullOrEmpty(this.CurrentPatient.Key) == false)
                {
                    this.Variability = GetVariability();
                    this.Accels = GetAccels();
                    this.Contractions = this.CurrentPatient.Contractions.Value.ToString();
                    this.LongContractions = this.CurrentPatient.LongContractions.Value.ToString();
                    this.LargeDeceles = this.CurrentPatient.LargeDeceles.Value.ToString();
                    this.LateDecels = this.CurrentPatient.LateDecels.Value.ToString();
                    this.ProlongedDecels = this.CurrentPatient.ProlongedDecels.Value.ToString();

                    this.IsBoldVariability = this.CurrentPatient.Variability.IsReason;
                    this.IsBoldAccels = this.CurrentPatient.Accels.IsReason;
                    this.IsBoldContractions = this.CurrentPatient.Contractions.IsReason;
                    this.IsBoldLongContractions = this.CurrentPatient.LongContractions.IsReason;
                    this.IsBoldLargeDeceles = this.CurrentPatient.LargeDeceles.IsReason;
                    this.IsBoldLateDecels = this.CurrentPatient.LateDecels.IsReason;
                    this.IsBoldProlongedDecels = this.CurrentPatient.ProlongedDecels.IsReason;
                }
                else
                {
                    this.Variability = String.Empty;
                    this.Accels = String.Empty;
                    this.Contractions = String.Empty;
                    this.LongContractions = String.Empty;
                    this.LargeDeceles = String.Empty;
                    this.LateDecels = String.Empty;
                    this.ProlongedDecels = String.Empty;

                    this.IsBoldVariability = false;
                    this.IsBoldAccels = false;
                    this.IsBoldContractions = false;
                    this.IsBoldLongContractions = false;
                    this.IsBoldLargeDeceles = false;
                    this.IsBoldLateDecels = false;
                    this.IsBoldProlongedDecels = false;
                }
            }
            catch(Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Error, Properties.Resources.CRIClient_ModuleName, ex.Message);
            }
        }

        private string GetVariability()
        {
            string val = String.Empty;

            if(this.CurrentPatient.Variability.Value == -1)
            {
                val = "N/A";
            }
            else
            {
                int intVal = (int)Math.Floor(CurrentPatient.Variability.Value);
                val = intVal.ToString();

                // TMP fix!!!
                //int ind = val.IndexOf(".");

                //if (ind >= 0 && val.Length >= (ind + 2))
                //{
                //    val = val.Substring(0, ind + 2);
                //}
                //else
                //{
                //    val = String.Format("{0:#.0}", this.CurrentPatient.Variability.Value);
                //}
            }

            return val;
        }

        private string GetAccels()
        {
            string val = String.Empty;

            if (this.CurrentPatient.Accels.Value == -1)
            {
                val = "N/A";
            }
            else
            {
                val = this.CurrentPatient.Accels.Value.ToString();
            }

            return val;
        }

        private void SetActionMenuOptions()
        {      
            if (App.ClientManager.CurrentPatient != null &&
                String.IsNullOrEmpty(App.ClientManager.CurrentPatient.Key) == false)
            {
                if (App.ClientManager.Settings.BlockADTFunctions == true)
                {
                    this.MenuSeparator.Visibility = Visibility.Visible;
                    this.AdmitPatient.Visibility = Visibility.Collapsed;
                    this.ChangePatient.Visibility = Visibility.Visible;
                    this.MergePatient.Visibility = Visibility.Collapsed;
                    this.DischargePatient.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.AdmitPatient.Visibility = Visibility.Collapsed;
                    this.ChangePatient.Visibility = Visibility.Visible;
                    this.MergePatient.Visibility = Visibility.Visible;
                    this.DischargePatient.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (App.ClientManager.Settings.BlockADTFunctions == true)
                {
                    this.MenuSeparator.Visibility = Visibility.Collapsed;
                    this.AdmitPatient.Visibility = Visibility.Collapsed;
                    this.ChangePatient.Visibility = Visibility.Collapsed;
                    this.MergePatient.Visibility = Visibility.Collapsed;
                    this.DischargePatient.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.AdmitPatient.Visibility = Visibility.Visible;
                    this.ChangePatient.Visibility = Visibility.Collapsed;
                    this.MergePatient.Visibility = Visibility.Collapsed;
                    this.DischargePatient.Visibility = Visibility.Collapsed;
                }
            }         
        }

        private void SetStatusDescriptionVisibility()
        {
            if (this.CurrentPatient != null && this.CurrentPatient.CRIStatus == CRIState.PositivePastNotYetReviewed)
            {
                StatusDescriptionVisibility = System.Windows.Visibility.Visible;
            }
            else
            {
                StatusDescriptionVisibility = System.Windows.Visibility.Collapsed;
            }
        }

        void ClientManager_evUpdateDataEvent(object sender, UpdateDataEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(delegate
            {
                if (App.ClientManager.CurrentPatient != null)
                {
                    CopyPatientData(App.ClientManager.CurrentPatient);

                    try
                    {
                        string CRIStatusTime = String.Empty;
                        if (this.CurrentPatient.CurrentDisplayCRI.CRIStatusTime > DateTime.MinValue)
                        {
                            CRIStatusTime = String.Format("{0:HH:mm} - {1:HH:mm}", 
                                this.CurrentPatient.CurrentDisplayCRI.CRIStatusTime - new TimeSpan(0, 30, 0), 
                                this.CurrentPatient.CurrentDisplayCRI.CRIStatusTime);
                        }
                        this.DisplayFromToTime = CRIStatusTime;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, PatternsCRIClient.Properties.Resources.CRIClient_ModuleName, "CRIStatusTime Exception", ex);
                    }

                    this.PastTabLable = String.Format("Past ({0})", this.CurrentPatient.NotRevewedPositiveCRIs.Count);
                    this.HistoryTabVisibility = this.CurrentPatient.NotRevewedPositiveCRIs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                    this.FetusNum = this.CurrentPatient.Fetuses;
                    SetCRIEvents();

                    if (this.CurrentPatient.EDD.HasValue)
                    {
                        this.EDD = this.CurrentPatient.EDD.Value;
                        this.GA = this.CurrentPatient.GA;
                    }

                    UpdateStatus(this.CurrentPatient.CRIStatus);

                    if (this.CurrentPatient.CRIStatus == CRIState.PositiveCurrent ||
                        this.CurrentPatient.CRIStatus == CRIState.PositivePastNotYetReviewed)
                    {
                        m_isAutoMode = true;
                        m_isMainWindowActive = App.IsMainWindowActive();
                        OnShowWindow();
                    }
                    else
                    {
                        if(m_isAutoMode == true)
                        {
                            OnHideWindow();
                            m_isAutoMode = false;
                        }
                    }
                }
                else
                {
                    this.CurrentPatient = this.m_unknownPatient;
                    UpdateStatus(CRIState.Off);
                }

                SetWindowToBottomRightOfScreen();
                SetActionMenuOptions();
                SetStatusDescriptionVisibility();
            }));          
        }

        private void CopyPatientData(PatientData patient)
        {
            this.CurrentPatient.Key = patient.Key;
            this.CurrentPatient.BedId = patient.BedId;
            this.CurrentPatient.BedName = patient.BedName;
            this.CurrentPatient.FirstName = patient.FirstName;
            this.CurrentPatient.LastName = patient.LastName;
            this.CurrentPatient.Fetuses = patient.Fetuses;
            this.CurrentPatient.GA = patient.GA;
            this.CurrentPatient.EDD = patient.EDD;
            this.CurrentPatient.PatientDisplayName = patient.PatientDisplayName;
            this.CurrentPatient.CurrentDisplayCRI = patient.CurrentDisplayCRI;        
            this.CurrentPatient.IsCurrentPositive = patient.IsCurrentPositive;
            this.CurrentPatient.IsCurrentBed = patient.IsCurrentBed;
            this.CurrentPatient.Duration = patient.CurrentDisplayCRI.Duration;
            this.CurrentPatient.Variability = patient.Variability;
            this.CurrentPatient.Accels = patient.Accels;
            this.CurrentPatient.Contractions = patient.Contractions;
            this.CurrentPatient.LongContractions = patient.LongContractions;
            this.CurrentPatient.LargeDeceles = patient.LargeDeceles;
            this.CurrentPatient.LateDecels = patient.LateDecels;
            this.CurrentPatient.ProlongedDecels = patient.ProlongedDecels;
            this.CurrentPatient.IsADT = patient.IsADT;

            List<CRIObject> listCRI = new List<CRIObject>();
            if (patient.NotRevewedPositiveCRIs != null)
            {
                listCRI.AddRange(patient.NotRevewedPositiveCRIs.OrderByDescending(a => a.StartTime).ToList());

                this.CurrentPatient.DisplayStartTime = patient.CurrentDisplayCRI.DisplayStartTime;
                this.CurrentPatient.DisplayEndTime = patient.CurrentDisplayCRI.DisplayEndTime;
            }
            this.CurrentPatient.NotRevewedPositiveCRIs = new ObservableCollection<CRIObject>(listCRI);

            if(this.CurrentPatient.CRIStatus != patient.CRIStatus)
            {
                this.CurrentPatient.CRIStatus = patient.CRIStatus;
            }
        }

        private void SetWindowToBottomRightOfScreen()
        {
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height;
        }

        private Rect GetScreenPosition()
        {
            double top;
            double left;
            double width;
            double height;

            top = SystemParameters.WorkArea.Top;
            left = SystemParameters.WorkArea.Left;
            width = SystemParameters.WorkArea.Width;
            height = SystemParameters.WorkArea.Height;
            
            return new Rect(left, top, width, height);
        }

        void OnShowWindow()
        {
            gridFadeOutStoryBoard.Stop();

            if (uiGridMain.Opacity != 1.0)
            {
                this.Opacity = 1; 
                this.Topmost = true;
                this.Activate();
                this.Focus();
            }

            if (uiGridMain.Opacity > 0 && uiGridMain.Opacity < 1) 
            {
                uiGridMain.Opacity = 1;
            }
            else if (uiGridMain.Opacity == 0)
            {
                gridFadeInStoryBoard.Begin();  
            }
        }

        void OnHideWindow()
        {
            gridFadeInStoryBoard.Stop(); 

            if (uiGridMain.Opacity == 1 && this.Opacity == 1)
                gridFadeOutStoryBoard.Begin();
            else 
            {
                uiGridMain.Opacity = 0;
                this.Opacity = 0;
            }
        }

        private void uiWindowMainNotification_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            gridFadeOutStoryBoard.Stop();
            uiGridMain.Opacity = 1;
        }

        private void uiWindowMainNotification_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            OnHideWindow();
        }

        void gridFadeOutStoryBoard_Completed(object sender, EventArgs e)
        {
            this.Opacity = 0;
        }

        void gridFadeInStoryBoard_Completed(object sender, EventArgs e)
        {
            this.Opacity = 1;
        }       

        private void uiMainNotifyWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_process != null && m_process.HasExited == false)
            {
                m_process.Close();
                m_process.Dispose();
            }

            IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource src = HwndSource.FromHwnd(windowHandle);
            src.RemoveHook(new HwndSourceHook(this.WndProc));
        }

        private void uiMainNotifyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartToolbar();

            if (this.CurrentPatient != null)
            {
                UpdateStatus(this.CurrentPatient.CRIStatus);
            }
            else
            {
                UpdateStatus(CRIState.Off);
            }
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_isAutoMode == true &&
                (this.CurrentPatient.CRIStatus == CRIState.PositiveCurrent ||
                 this.CurrentPatient.CRIStatus == CRIState.PositivePastNotYetReviewed))
            {
                m_isAutoMode = !App.ClientManager.SetPositiveCRIsToReviewed(this.CurrentPatient.Key);

                if (ClientManager.Instance.Settings.CloseNotificationWithoutTracing == false)
                {
                    App.ClientManager.SetExpandedViewVisibility(Visibility.Visible, false, m_isMainWindowActive);
                }
                m_isMainWindowActive = false;
            }

            OnHideWindow();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //this.CurrentPatient.Fetuses = this.FetusNum;
            //this.CurrentPatient.EDD = this.EDD;
            //this.CurrentPatient.GA = this.GA;

            int fetus = this.FetusNum;
            DateTime? edd = this.EDD;

            this.Topmost = false;

            bool resAuthentication = true;

            if (App.ClientManager.Settings.ChangeDataRequireAuthentication == true)
            {
                Rect rect = new Rect(SystemParameters.WorkArea.Left,
                                     SystemParameters.WorkArea.Top,
                                     SystemParameters.WorkArea.Width,
                                     SystemParameters.WorkArea.Height);

                LoginWindow login = new LoginWindow(rect, CALMMediator.USERGROUP_F_ADT, CALMMediator.USERGROUP_A_MODIFY);
                login.ShowDialog();

                resAuthentication = login.DialogResult.HasValue == true && login.DialogResult.Value == true;
            }

            if (resAuthentication == true)
            {
                bool result = App.ClientManager.QuickEditPatient(this.CurrentPatient.Key, fetus, edd);
                
                if (result == false)
                {
                    AutomaticMessageWindow messageWnd = new AutomaticMessageWindow();
                    messageWnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    messageWnd.MessageTitle.Text = "Failed to change patient data.";
                    messageWnd.MessageDescription.Text = (string)Application.Current.FindResource("MSG_Auto_disapear");
                    messageWnd.ShowDialog();
                }

                this.Topmost = true;

                if (App.ClientManager.Settings.ChangeDataRequireAuthentication == true)
                {
                    App.ClientManager.Logout();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            OnHideWindow();
        }    

        private void dpEDD_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.EDD != null)
            {
                this.GA = App.ClientManager.CalcGestationalAge((DateTime)this.EDD);
            }
        }

        private void TracingControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_isAutoMode == true &&
                (this.CurrentPatient.CRIStatus == CRIState.PositiveCurrent ||
                 this.CurrentPatient.CRIStatus == CRIState.PositivePastNotYetReviewed))
            {
                m_isAutoMode = !App.ClientManager.SetPositiveCRIsToReviewed(this.CurrentPatient.Key);
            }

            App.ClientManager.SetExpandedViewVisibility(Visibility.Visible, false, m_isMainWindowActive);
            m_isMainWindowActive = false;

            OnHideWindow();
        }

        
        #endregion

        #region ToolBar methods

        private void UpdateStatus(CRIState status)
        {
            if (m_hHandle == IntPtr.Zero)
            {
                m_hHandle = Win32.FindWindow(null, "CRI_ToolBar");
            }

            if (m_hHandle == IntPtr.Zero)
            {
                return;
            }
            else
            {  
                Win32.SendMessage(m_hHandle, OWM_AGENT_MSG, new IntPtr((int)status), IntPtr.Zero);             
            }
        }
      
        private void StopProcessIfExist(string procName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(procName);

                foreach (Process proc in processes)
                {
                    proc.Kill();
                    proc.WaitForExit(5000);
                    return;
                }
            }
            catch (Exception)
            {

            }
        }
     
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
 //           if (m_hHandle != hWnd)
 //               return IntPtr.Zero;

            switch (msg)
            {
                case WM_LBUTTONDOWN:
                    OnShowWindow();
                    break;

                case WM_LBUTTONDBLCLK:
                    OnShowWindow();
                    break;

                case WM_RBUTTONDOWN:
                    //OnHideWindow();
                    break;

                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private void StartToolbar()
        {
            IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource src = HwndSource.FromHwnd(windowHandle);
            src.AddHook(new HwndSourceHook(WndProc));

            StopProcessIfExist(App.CRIToolBar);

            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.Arguments = windowHandle.ToString();
            pInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            pInfo.FileName = @".\" + App.CRIToolBar + ".exe";
            pInfo.UseShellExecute = false;
            pInfo.CreateNoWindow = true;

            m_process = Process.Start(pInfo);
            m_process.WaitForInputIdle();
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

        #region Actions Menu Events

        private void actionsMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            actionsMenu.IsOpen = true;
        }

        private void OpenTracing_Click(object sender, RoutedEventArgs e)
        {
            if (m_isAutoMode == true &&
                (this.CurrentPatient.CRIStatus == CRIState.PositiveCurrent ||
                 this.CurrentPatient.CRIStatus == CRIState.PositivePastNotYetReviewed))
            {
                m_isAutoMode = !App.ClientManager.SetPositiveCRIsToReviewed(this.CurrentPatient.Key);
            }

            App.ClientManager.SetExpandedViewVisibility(Visibility.Visible, false, m_isMainWindowActive);
            m_isMainWindowActive = false;
            OnHideWindow();
        }

        private void CloseSynopsis_Click(object sender, RoutedEventArgs e)
        {
            OnHideWindow();
        }

        private void ClosePatternsApplication_Click(object sender, RoutedEventArgs e)     
        {
            MessageBoxWindow msgBox = new MessageBoxWindow();
            msgBox.MessageTitle.Text = "Close Application Confirmation";
            msgBox.MessageDescription.Text = "Are you sure you want to close the PeriCALM CheckList application?";

            Nullable<bool> dialogResult = msgBox.ShowDialog();

            if(dialogResult.HasValue == true && dialogResult.Value == true)
            {
                StopProcessIfExist(App.CRIToolBar);
                App.Current.Shutdown();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void ChangePatient_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = GetScreenPosition();

            bool resAuthentication = true;

            if (App.ClientManager.Settings.ChangeDataRequireAuthentication == true)
            {
                LoginWindow login = new LoginWindow(rect, CALMMediator.USERGROUP_F_ADT, CALMMediator.USERGROUP_A_MODIFY);
                login.ShowDialog();

                resAuthentication = login.DialogResult.HasValue == true && login.DialogResult.Value == true;
            }

            if (resAuthentication == true)
            {
                EditPatientWindow editPatient = new EditPatientWindow(rect, this.CurrentPatient);
                editPatient.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                editPatient.ShowDialog();
            }

            if (App.ClientManager.Settings.ChangeDataRequireAuthentication == true)
            {
                App.ClientManager.Logout();
            }
        }

        private void MergePatient_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = GetScreenPosition();

            bool resAuthentication = true;

            if (App.ClientManager.Settings.MergeRequireAuthentication == true)
            {
                LoginWindow login = new LoginWindow(rect, CALMMediator.USERGROUP_F_ADT, CALMMediator.USERGROUP_A_MODIFY);
                login.ShowDialog();

                resAuthentication = login.DialogResult.HasValue == true && login.DialogResult.Value == true;
            }

            if (resAuthentication == true)
            {
                MergePatientWindow mergePatient = new MergePatientWindow(rect, this.CurrentPatient);
                mergePatient.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                mergePatient.ShowDialog();
            }

            if (App.ClientManager.Settings.MergeRequireAuthentication == true)
            {
                App.ClientManager.Logout();
            }
        }

        private void DischargePatient_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = GetScreenPosition();

            DischargePatientWindow dischargePatient = new DischargePatientWindow(rect, this.CurrentPatient);
            dischargePatient.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            dischargePatient.ShowDialog();
        }

        private void AdmitPatient_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = GetScreenPosition();
            string currentBed = App.ClientManager.GetCurrentBed();

            AddPatientWindow addPatient = new AddPatientWindow(rect, currentBed);
            addPatient.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            addPatient.ShowDialog();
        }

        #endregion

        private void HelpButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Topmost = false;
            OnHideWindow();
           
            //AboutAlgorithmWindow about = new AboutAlgorithmWindow(false);
            AboutAlgorithmWindow about = new AboutAlgorithmWindow(false, CALMMediatorSettings.Instance.CRIPluginURL, true);
            about.ShowDialog();

            this.Topmost = true;
            OnShowWindow();
        }
    }
}
