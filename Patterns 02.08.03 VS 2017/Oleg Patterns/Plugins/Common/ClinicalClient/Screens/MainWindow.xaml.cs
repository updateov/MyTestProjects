//Review: 27/04/15
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using EncDecWrapper;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;
using CRIEntities;
using System.ComponentModel;
using PatternsCALMMediator;
using System.Threading.Tasks;
using PatternsCRIClient.Data;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Interop;
using PatternsCRIClient.Downloader;
using CommonLogger;

namespace PatternsCRIClient.Screens
{
    public enum SortType
    {
        CRI_Positive,
        Names_AZ,
        Names_ZA,
        BedName_Asc,
        BedName_Desc
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private SortType m_currentSortingType = SortType.CRI_Positive;
        private string m_selectedVisitKey = String.Empty;
        private Point m_LastPoint;
        private IntPtr m_windowHandle = IntPtr.Zero;

        private DispatcherTimer m_activityTimer;

        private NotifyPopup m_notifyPopup = null;      

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

        #region Dependency Property

        public string CurrentBedName
        {
            get { return (string)GetValue(CurrentBedNameProperty); }
            set 
            {
                SetValue(CurrentBedNameProperty, value);
            }
        }

        public static readonly DependencyProperty CurrentBedNameProperty =
            DependencyProperty.Register("CurrentBedName", typeof(string), typeof(MainWindow), new PropertyMetadata(String.Empty));

        
        public TreeViewData TreeData
        {
            get 
            {
                return (TreeViewData)GetValue(TreeDataProperty);          
            }

            set 
            {
                SetValue(TreeDataProperty, value);
            }
        }

        public static readonly DependencyProperty TreeDataProperty =
            DependencyProperty.Register("TreeData", typeof(TreeViewData), typeof(MainWindow), new PropertyMetadata(new TreeViewData(App.ClientManager.Patients)));
   

        public ListViewData ListDataNamesAZ
        {
            get  {return (ListViewData)GetValue(ListDataNamesAZProperty);}
            set  {SetValue(ListDataNamesAZProperty, value);}
        }
        public static readonly DependencyProperty ListDataNamesAZProperty =
            DependencyProperty.Register("ListDataNamesAZ", typeof(ListViewData), typeof(MainWindow), new PropertyMetadata(new ListViewData(App.ClientManager.Patients, SortType.Names_AZ)));


        public ListViewData ListDataNamesZA
        {
            get { return (ListViewData)GetValue(ListDataNamesZAProperty); }
            set { SetValue(ListDataNamesZAProperty, value); }
        }
        public static readonly DependencyProperty ListDataNamesZAProperty =
            DependencyProperty.Register("ListDataNamesZA", typeof(ListViewData), typeof(MainWindow), new PropertyMetadata(new ListViewData(App.ClientManager.Patients, SortType.Names_ZA)));


        public ListViewData ListDataBedsAZ
        {
            get { return (ListViewData)GetValue(ListDataBedsAZProperty); }
            set { SetValue(ListDataBedsAZProperty, value); }
        }
        public static readonly DependencyProperty ListDataBedsAZProperty =
            DependencyProperty.Register("ListDataBedsAZ", typeof(ListViewData), typeof(MainWindow), new PropertyMetadata(new ListViewData(App.ClientManager.Patients, SortType.BedName_Asc)));


        public ListViewData ListDataBedsZA
        {
            get { return (ListViewData)GetValue(ListDataBedsZAProperty); }
            set { SetValue(ListDataBedsZAProperty, value); }
        }
        public static readonly DependencyProperty ListDataBedsZAProperty =
            DependencyProperty.Register("ListDataBedsZA", typeof(ListViewData), typeof(MainWindow), new PropertyMetadata(new ListViewData(App.ClientManager.Patients, SortType.BedName_Desc)));


        public PatientData SelectedPatient
        {
            get { return (PatientData)GetValue(SelectedPatientProperty); }

            set 
            {              
                if (value != null)
                {
                    m_selectedVisitKey = value.Key;
                }
                else
                {
                    m_selectedVisitKey = String.Empty;
                }

                SetValue(SelectedPatientProperty, value);
            }          
        }
    
        public static readonly DependencyProperty SelectedPatientProperty =
            DependencyProperty.Register("SelectedPatient", typeof(PatientData), typeof(MainWindow), new UIPropertyMetadata(null));
   
         
        public string SortMenuHeader
        {
            get { return (string)GetValue(SortMenuHeaderProperty); }
            set { SetValue(SortMenuHeaderProperty, value); }
        }

        public static readonly DependencyProperty SortMenuHeaderProperty =
            DependencyProperty.Register("SortMenuHeader", typeof(string), typeof(MainWindow), 
            new UIPropertyMetadata((string)Application.Current.FindResource("CRI_Positive_first")));

        public PatientData PatientTooltipData
        {
            get { return (PatientData)GetValue(PatientTooltipDataProperty); }
            set { SetValue(PatientTooltipDataProperty, value); }
        }

        public static readonly DependencyProperty PatientTooltipDataProperty =
            DependencyProperty.Register("PatientTooltipData", typeof(PatientData), typeof(MainWindow), new PropertyMetadata(null));


        public bool IsCentralMode
        {
            get { return (bool)GetValue(IsCentralModeProperty); }
            set { SetValue(IsCentralModeProperty, value); }
        }

        public static readonly DependencyProperty IsCentralModeProperty =
            DependencyProperty.Register("IsCentralMode", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        
        #endregion

        public MainWindow()
        {
            IsCentralMode = App.ClientManager.Settings.IsCentralMode;

            InitializeComponent();

            if (IsCentralMode == true)
            {
                this.ResizeMode = System.Windows.ResizeMode.CanMinimize;
                this.WindowState = System.Windows.WindowState.Maximized;

                Rect winRect = this.GetScreenPosition();
                this.Top = winRect.Top;
                this.Left = winRect.Left;
                this.Width = winRect.Width;
                this.Height = winRect.Height;

                this.MaxWidth = Width;
                this.MinWidth = Width;
                this.MaxHeight = Height;
                this.MinHeight = Height;
            }
            else
            {
                patientListCol.Width = new GridLength(0);                     
            }

            SetWait(true);

            //TODO: rename to OnUpdateData and OnVisibilityChanged
            App.ClientManager.UpdateDataEvent += ClientManager_evUpdateDataEvent;
            App.ClientManager.UpdateVisibilityEvent += ClientManager_evVisibilityEvent; 

            this.btnAdmit.IsEnabled = false;

            SetGridsVisibility(m_currentSortingType);

            treePatients.ItemsSource = TreeData;

            listPatientNameAZ.ItemsSource = ListDataNamesAZ.PatientsList;
            listPatientNameZA.ItemsSource = ListDataNamesZA.PatientsList;
            listPatientBedsAZ.ItemsSource = ListDataBedsAZ.PatientsList;
            listPatientBedsZA.ItemsSource = ListDataBedsZA.PatientsList;

            //SetWait(false);
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            Activity();
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (m_LastPoint != e.GetPosition(this))
            {
                m_LastPoint = e.GetPosition(this);

                Activity();
            }
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Activity();
        }

        #region Private functions

        void Inactivity(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
                this.WindowState = WindowState.Normal;
            }

            m_activityTimer.Stop();

            if (this.Visibility != System.Windows.Visibility.Hidden)
            {
                this.Activate();
                this.Topmost = true;  // important
                this.Topmost = false; // important
                this.Focus();

                Rect rect = GetScreenPosition();

                TimeoutWindow timeout = new TimeoutWindow(rect);
                timeout.ShowDialog();

                //this.Topmost = true;

                if (timeout.DialogResult == true)
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.GetType().Name != "MainWindow" &&
                            window.GetType().Name != "NotifyPopup")
                        {
                            window.Close();
                        }
                    }

                    this.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    m_activityTimer.Start();
                }
            }
        }

        void Activity()
        {
            if (App.ClientManager.Settings.IsCentralMode == false)
            {
                PatientData currentPatient = App.ClientManager.CurrentPatient;

                bool isVisible = this.Visibility != System.Windows.Visibility.Hidden && this.WindowState != System.Windows.WindowState.Minimized;

                if (isVisible && this.SelectedPatient != null && currentPatient != null && String.IsNullOrEmpty(currentPatient.Key) == false)
                {
                    if (this.SelectedPatient.Key != currentPatient.Key)
                    {
                        m_activityTimer.Stop();
                        m_activityTimer.Start();
                    }
                    else
                    {
                        m_activityTimer.Stop();
                    }
                }
                else
                {
                    m_activityTimer.Stop();
                }
            }
        }

        private void SetCentralMode()
        {           
            // TODO: comment or const all magic numbers
            double width = this.ActualWidth / 100 * 15;

            patientListCol.Width = new GridLength(width > 295 ? width : 295);

            attentionRow.Height = new GridLength(0);
            attentionMessage.Visibility = System.Windows.Visibility.Collapsed;
            patienListToggle.Visibility = System.Windows.Visibility.Collapsed;
            tracingGrid.Margin = new Thickness(0);

            SetCheckBoxReviewedVisibility();

            this.Title = "PeriCALM® CheckList™ – Central View";
            this.Activate();
            this.Focus();
        }

        private void SetBedSideMode(bool showPatientList)
        {
            this.Title = "PeriCALM® CheckList™ – Bedside View";
            SetCheckBoxReviewedVisibility();
            StartNotifyPopup();

            if (App.ClientManager.Settings.CanOpenPatientList)
            {
                SetPatienListVisibility(showPatientList);
            }
            else
            {
                patienListToggle.Visibility = System.Windows.Visibility.Collapsed;
                tracingGrid.Margin = new Thickness(0);
                SetPatienListVisibility(false);
            }

            SetWindowToBottomRightOfScreen();
            
            if (App.ClientManager.CurrentPatient != null)
            {
                this.CurrentBedName = App.ClientManager.CurrentPatient.BedName;

                m_selectedVisitKey = App.ClientManager.CurrentPatient.Key;

                SetSelectedPatient();

                new Thread(Navigate).Start();
            }

            this.MouseMove += OnMouseMove;
            this.MouseLeftButtonUp += OnMouseLeftButtonUp;
            this.KeyDown += OnKeyDown;

            double timeout = App.ClientManager.Settings.AutomaticClosingTimeout * 60 - 30; // Minutes * 60 sec - 30 sec Timeout screen

            m_activityTimer = new DispatcherTimer
            (
                TimeSpan.FromSeconds(timeout),
                DispatcherPriority.ApplicationIdle,
                Inactivity,
                Application.Current.Dispatcher
            );
            Activity();  
        }

        private void SetCheckBoxReviewedVisibility()
        {
            if(App.ClientManager.Settings.IsCentralMode == false || App.ClientManager.Settings.AllowReview == false)
            {
                ctrlReviewed.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ctrlReviewed.Init();

                if(this.SelectedPatient != null &&
                   (this.SelectedPatient.CRIStatus == CRIState.PositiveCurrent ||
                    this.SelectedPatient.CRIStatus == CRIState.PositivePastNotYetReviewed))
                {
                    ctrlReviewed.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    ctrlReviewed.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void SetWindowToBottomRightOfScreen()
        {
            this.Width = 730;
            this.Height = 660;

            this.Left = SystemParameters.WorkArea.Width - this.Width - 10;
            this.Top = SystemParameters.WorkArea.Height - this.Height;
        }

        private Rect GetScreenPosition()
        {
            double top;
            double left;
            double width;
            double height;

            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                top = SystemParameters.WorkArea.Top;
                left = SystemParameters.WorkArea.Left;
                width = SystemParameters.WorkArea.Width;
                height = SystemParameters.WorkArea.Height;
            }
            else
            {
                top = this.Top;
                left = this.Left;
                width = this.ActualWidth;
                height = this.ActualHeight;
            }

            return new Rect(left, top, width, height);
        }

        private void SetWait(bool isWait)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(delegate
            {
                LoadingAdorner.IsAdornerVisible = isWait;
                LoadingAdorner.UpdateLayout();
            }));
        }

        private void SetSelectedPatient()
        {
            try
            {
                SetDataSelection();

                //if (String.IsNullOrEmpty(m_selectedVisitKey) == false)
                //{
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        TreeData.SetSelectedPatient(m_selectedVisitKey);
                        ListDataNamesAZ.SetSelectedPatient(m_selectedVisitKey);
                        ListDataNamesZA.SetSelectedPatient(m_selectedVisitKey);
                        ListDataBedsAZ.SetSelectedPatient(m_selectedVisitKey);
                        ListDataBedsZA.SetSelectedPatient(m_selectedVisitKey);
                    }));
                //}
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, Properties.Resources.CRIClient_ModuleName, "Failed to Select Patient", ex);
            }
        }

        private void SetDataSelection()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
               if (String.IsNullOrEmpty(m_selectedVisitKey) == false)
               {
                   PatientData patData = App.ClientManager.Patients.FirstOrDefault(t => t.Key == m_selectedVisitKey);

                   if (patData != null)
                   {
                       PatientData data = App.ClientManager.Patients.FirstOrDefault(t => t.IsSelectedPatient == true);

                       if (data != null)
                       {
                           data.IsSelectedPatient = false;
                       }
                       patData.IsSelectedPatient = true;

                       SelectedPatient = patData;
                   }
                   else
                   {
                       App.ClientManager.ResetSelection();
                       SelectedPatient = null;
                       m_selectedVisitKey = String.Empty;
                       webBrowserTracingPatterns.Visibility = System.Windows.Visibility.Collapsed;
                   }
               }
               else
               {
                   App.ClientManager.ResetSelection();
                   SelectedPatient = null;
                   webBrowserTracingPatterns.Visibility = System.Windows.Visibility.Collapsed;
               }
            }));    
        }

        private void Navigate()
        {            
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (SelectedPatient == null)
                {
                    webBrowserTracingPatterns.Visibility = System.Windows.Visibility.Collapsed;
                    return;
                }

                webBrowserTracingPatterns.LoadCompleted -= new LoadCompletedEventHandler(webBrowserTracingPatterns_LoadCompleted);

                SetWait(true);
                webBrowserTracingPatterns.Visibility = System.Windows.Visibility.Collapsed;

                webBrowserTracingPatterns.LoadCompleted += new LoadCompletedEventHandler(webBrowserTracingPatterns_LoadCompleted);

                CALMNavigationParameters navParams = new CALMNavigationParameters(m_selectedVisitKey, m_windowHandle);          
               
                webBrowserTracingPatterns.Navigate(navParams.SourceUrl, navParams.TargetFrameName, navParams.PostData, navParams.AdditionalHeaders);
            }));          
        }

        private void StartNotifyPopup()
        {
            if (m_notifyPopup == null)
            {
                m_notifyPopup = new NotifyPopup();
            }
        }

        public void SetGridsVisibility(SortType sortVal)
        {
            switch (sortVal)
            {
                case SortType.CRI_Positive:
                    listPatientNameAZ.Visibility = System.Windows.Visibility.Hidden;
                    listPatientNameZA.Visibility = System.Windows.Visibility.Hidden;
                    listPatientBedsAZ.Visibility = System.Windows.Visibility.Hidden;
                    listPatientBedsZA.Visibility = System.Windows.Visibility.Hidden;
                    treePatients.Visibility = System.Windows.Visibility.Visible;
                    break;

                case SortType.Names_AZ:
                    listPatientNameAZ.Visibility = System.Windows.Visibility.Visible;
                    listPatientNameZA.Visibility = System.Windows.Visibility.Hidden;
                    listPatientBedsAZ.Visibility = System.Windows.Visibility.Hidden;
                    listPatientBedsZA.Visibility = System.Windows.Visibility.Hidden;
                    treePatients.Visibility = System.Windows.Visibility.Hidden;
                    break;

                case SortType.Names_ZA:
                    listPatientNameAZ.Visibility = System.Windows.Visibility.Hidden;
                    listPatientNameZA.Visibility = System.Windows.Visibility.Visible;
                    listPatientBedsAZ.Visibility = System.Windows.Visibility.Hidden;
                    listPatientBedsZA.Visibility = System.Windows.Visibility.Hidden;
                    treePatients.Visibility = System.Windows.Visibility.Hidden;
                    break;
                
                case SortType.BedName_Asc:
                    listPatientNameAZ.Visibility = System.Windows.Visibility.Hidden;
                    listPatientNameZA.Visibility = System.Windows.Visibility.Hidden;
                    listPatientBedsAZ.Visibility = System.Windows.Visibility.Visible;
                    listPatientBedsZA.Visibility = System.Windows.Visibility.Hidden;
                    treePatients.Visibility = System.Windows.Visibility.Hidden;
                    break;

                case SortType.BedName_Desc:
                    listPatientNameAZ.Visibility = System.Windows.Visibility.Hidden;
                    listPatientNameZA.Visibility = System.Windows.Visibility.Hidden;
                    listPatientBedsAZ.Visibility = System.Windows.Visibility.Hidden;
                    listPatientBedsZA.Visibility = System.Windows.Visibility.Visible;
                    treePatients.Visibility = System.Windows.Visibility.Hidden;
                    break;

                default:
                    break;
            }
        }

        private void SetPatienListVisibility(bool isShow)
        {
            GridLength width;

            this.Visibility = System.Windows.Visibility.Hidden;

            if (isShow == true)
            {
                double colWidth = this.ActualWidth / 100 * 15;

                width = new GridLength(colWidth > 290 ? colWidth : 290);

                if (this.WindowState == System.Windows.WindowState.Normal)
                {
                    this.Width += width.Value;
                    this.MinWidth += width.Value;

                    if (this.Left - width.Value > 0)
                    {
                        this.Left -= width.Value;
                    }
                }
            }
            else
            {
                width = new GridLength(0);

                if (this.WindowState == System.Windows.WindowState.Normal)
                {
                    this.MinWidth -= patientListCol.ActualWidth;
                    this.Width -= patientListCol.ActualWidth;

                    if (this.Left + this.Width + patientListCol.ActualWidth < SystemParameters.WorkArea.Width)
                    {
                        this.Left += patientListCol.ActualWidth;
                    }
                }
            }

            legendToggle.IsChecked = isShow;

            if (patienListToggle.IsChecked != isShow)
            {
                patienListToggle.IsChecked = isShow;
            }

            patientListCol.Width = width;

            this.Visibility = System.Windows.Visibility.Visible;
        }

        #endregion

        #region Events

        void ClientManager_evVisibilityEvent(object sender, VisibilityEventArgs e)
        {
            if (e.Visibility == System.Windows.Visibility.Visible)
            {
                bool isNotResize = (this.Visibility == System.Windows.Visibility.Visible &&
                                   (this.WindowState == System.Windows.WindowState.Normal || this.WindowState == WindowState.Maximized) &&
                                   (App.ClientManager.CurrentPatient != null && m_selectedVisitKey == App.ClientManager.CurrentPatient.Key) &&
                                   e.IsMainWindowActive == true);

                if (isNotResize == false && App.ClientManager.Settings.IsCentralMode == false)
                {
                    // TODO: Comment double Normal
                    if (this.WindowState != WindowState.Normal)
                    {
                        this.WindowState = WindowState.Normal;
                        this.WindowState = WindowState.Normal;
                    }

                    SetBedSideMode(e.OpenPatientList);                    
                }

                if (!this.IsVisible)
                {
                    this.Show();
                }

                // TODO: Comment double Normal
                if (this.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Normal;
                    this.WindowState = WindowState.Normal;
                }

                this.Activate();
                this.Topmost = true;  // important
                this.Topmost = false; // important
                this.Focus();         
           
                if (App.ClientManager.CurrentPatient != null)
                {
                    m_selectedVisitKey = App.ClientManager.CurrentPatient.Key;

                    SetSelectedPatient();

                    new Thread(Navigate).Start();
                }
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Hidden;
            }
        }
     
        void ClientManager_evUpdateDataEvent(object sender, UpdateDataEventArgs e)
        {
            try
            {
                SetDataSelection();

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    TreeData.UpdateData(e.PatientsData);
                    ListDataNamesAZ.UpdateData(e.PatientsData, SortType.Names_AZ);
                    ListDataNamesZA.UpdateData(e.PatientsData, SortType.Names_ZA);
                    ListDataBedsAZ.UpdateData(e.PatientsData, SortType.BedName_Asc);
                    ListDataBedsZA.UpdateData(e.PatientsData, SortType.BedName_Desc);

                    this.IsCentralMode = App.ClientManager.Settings.IsCentralMode;

                    if (App.ClientManager.Settings.IsCentralMode == false &&
                        App.ClientManager.CurrentPatient != null)
                    {
                        this.CurrentBedName = App.ClientManager.CurrentPatient.BedName;

                        if( App.ClientManager.CurrentPatient.CRIStatus == CRIState.PositiveCurrent ||
                            App.ClientManager.CurrentPatient.CRIStatus == CRIState.PositivePastNotYetReviewed)
                        {
                            Color color = (Color)ColorConverter.ConvertFromString("#CC5615"); //Orange
                            attentionBorder.Background = new SolidColorBrush(color);
                        }
                        else
                        {
                            Color color = (Color)ColorConverter.ConvertFromString("#315EAD"); //Blue
                            attentionBorder.Background = new SolidColorBrush(color);
                        }

                        if (this.Visibility != System.Windows.Visibility.Hidden &&
                            this.SelectedPatient == null)
                        {
                            m_selectedVisitKey = App.ClientManager.CurrentPatient.Key;
                            SetSelectedPatient();
                            new Thread(Navigate).Start();
                        }
                    }
                 
                    this.btnAdmit.IsEnabled = true;

                    if (App.ClientManager.Settings.IsCentralMode == false || App.ClientManager.Settings.AllowReview == false)
                    {
                        ctrlReviewed.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        if (this.SelectedPatient != null &&
                           (this.SelectedPatient.CRIStatus == CRIState.PositiveCurrent ||
                            this.SelectedPatient.CRIStatus == CRIState.PositivePastNotYetReviewed))
                        {
                            this.ctrlReviewed.Visibility = System.Windows.Visibility.Visible;
                            this.ctrlReviewed.Init();
                        }     
                        else
                        {
                            this.ctrlReviewed.Disappear();
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, Properties.Resources.CRIClient_ModuleName, "Failed to Update PatientsData", ex);
            }
            finally
            {
                SetWait(false);
            }
        } 

        void webBrowserTracingPatterns_LoadCompleted(object sender, NavigationEventArgs e)
        {
            SetWait(false);
            webBrowserTracingPatterns.Visibility = System.Windows.Visibility.Visible;
        }

        private void sortMenu_Click(object sender, RoutedEventArgs e)
        {
            SortType newSorting = (SortType)((ComboBoxItem)sender).Tag;

            if (newSorting != m_currentSortingType)
            {
                m_currentSortingType = newSorting;

                SetGridsVisibility(m_currentSortingType);
            }
        }

        private void listPatients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listPatients = (ListBox)sender;

            if (listPatients.Visibility == System.Windows.Visibility.Visible)
            {
                if (listPatients.SelectedItem != null && SelectedPatient != (PatientData)listPatients.SelectedItem)
                {
                    PatientData selectedPatient = (PatientData)listPatients.SelectedItem;
                    m_selectedVisitKey = selectedPatient.Key;

                    SetSelectedPatient();
                    SetCheckBoxReviewedVisibility();

                    new Thread(Navigate).Start(); 
                }
            }
        }

        private void treePatients_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treePatients.Visibility == System.Windows.Visibility.Visible &&
                treePatients.SelectedItem is PatientData)
            {
                if (treePatients.SelectedItem != null && (SelectedPatient == null ||
                    SelectedPatient.Key != ((PatientData)treePatients.SelectedItem).Key))
                {
                    PatientData selectedPatient = (PatientData)treePatients.SelectedItem;
                    m_selectedVisitKey = selectedPatient.Key;

                    SetSelectedPatient();
                    SetCheckBoxReviewedVisibility();

                    new Thread(Navigate).Start();
                }
            }
        }

        private void TreeViewItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (((TreeViewItem)sender).HasItems == true && 
                ((TreeViewItem)sender).IsSelected == true &&
                e.ButtonState == MouseButtonState.Released)
            {
                ((TreeViewItem)sender).IsExpanded = !((TreeViewItem)sender).IsExpanded;
            }
            else
            {
                if (((TreeViewItem)sender).Header is PatientData)
                {
                    PatientData selectedPatient = ((TreeViewItem)sender).Header as PatientData;

                    if(((TreeViewItem)sender).IsSelected == true &&
                        selectedPatient.IsSelectedPatient != ((TreeViewItem)sender).IsSelected)
                    {
                        m_selectedVisitKey = selectedPatient.Key;

                        SetSelectedPatient();
                        SetCheckBoxReviewedVisibility();

                        new Thread(Navigate).Start();
                    }
                }
            }
        }

        private void ViewItem_MouseEnter(object sender, MouseEventArgs e)     
        {
            if(sender is ListBoxItem)
            {
                if (((ListBoxItem)sender).DataContext is PatientData)
                {
                    this.PatientTooltipData = ((ListBoxItem)sender).DataContext as PatientData;
                }
            }
            if (sender is TreeViewItem)
            {
                if (((TreeViewItem)sender).DataContext is PatientData)
                {
                    this.PatientTooltipData = ((TreeViewItem)sender).DataContext as PatientData;
                }
            }
        }

        private void ViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                if (((ListBoxItem)sender).DataContext is PatientData)
                {
                    this.PatientTooltipData = null;
                }
            }
            if (sender is TreeViewItem)
            {
                if (((TreeViewItem)sender).DataContext is PatientData)
                {
                    this.PatientTooltipData = null;
                }
            }
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.ClientManager.Settings.IsCentralMode == false)
            {
                this.Visibility = System.Windows.Visibility.Hidden;
                this.WindowState = System.Windows.WindowState.Normal;

                e.Cancel = true;
            }
            else
            {
                App.Current.Shutdown();
            }
        }

        // TODO: be consistant with control naming
        private void btnAdmit_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = GetScreenPosition();

            AddPatientWindow addPatient = new AddPatientWindow(rect, String.Empty);
            addPatient.MouseMove += OnMouseMove;
            addPatient.KeyDown += OnKeyDown;
            addPatient.MouseLeftButtonUp += OnMouseLeftButtonUp;
            addPatient.Owner = this;
            addPatient.ShowDialog();
        }
      
        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.ClientManager.Settings.IsCentralMode == false)
            {
                if (this.WindowState != WindowState.Normal)
                {
                    this.WindowState = WindowState.Normal;
                }

                m_LastPoint = Mouse.GetPosition(this);

                SetBedSideMode(false);              
            }
            else
            {
                SetCentralMode();
            }

            m_windowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource src = HwndSource.FromHwnd(m_windowHandle);
            src.AddHook(new HwndSourceHook(WndProc));

            this.Visibility = App.ClientManager.Settings.IsCentralMode == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;                  
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 1124: //Button 'About' clicked                  
                    //AboutWindow about = new AboutWindow();
                    //about.ShowDialog();
                    //int res = RunAuthentication();
                    break;

                case 1224: //event strikeout
                case 1234: //event confirmation
                case 1244: // contraction strikeout
                    {                       
                        int status = RunAuthentication();
                        Win32.SendMessage(lParam, msg, new IntPtr((int)status), IntPtr.Zero);
                    }
                    break;

                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private int RunAuthentication()
        {
            Rect rect = GetScreenPosition();

            LoginWindow login = new LoginWindow(rect, CALMMediator.WEB_BUTTON_1, CALMMediator.USERGROUP_A_MODIFY, 60 /* 60 sec. */);
            login.ShowDialog();

            bool resAuthentication = login.DialogResult.HasValue == true && login.DialogResult.Value == true;

            return resAuthentication ? 1 : 0;
        }

        private void buttonDischarge_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = GetScreenPosition();

            DischargePatientWindow dischargePatient = new DischargePatientWindow(rect, this.PatientTooltipData);
            dischargePatient.MouseMove += OnMouseMove;
            dischargePatient.KeyDown += OnKeyDown;
            dischargePatient.MouseLeftButtonUp += OnMouseLeftButtonUp;
            dischargePatient.Owner = this;
            dischargePatient.ShowDialog();
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = GetScreenPosition();

            bool resAuthentication = true;

            PatientData patient = new PatientData();
            patient.CopyData(this.PatientTooltipData);

            if (App.ClientManager.Settings.ChangeDataRequireAuthentication == true)
            {
                LoginWindow login = new LoginWindow(rect, CALMMediator.USERGROUP_F_ADT, CALMMediator.USERGROUP_A_MODIFY);
                login.ShowDialog();

                resAuthentication = login.DialogResult.HasValue == true && login.DialogResult.Value == true;
            }

            if (resAuthentication == true)
            {
                EditPatientWindow editPatient = new EditPatientWindow(rect, patient);
                editPatient.MouseMove += OnMouseMove;
                editPatient.KeyDown += OnKeyDown;
                editPatient.MouseLeftButtonUp += OnMouseLeftButtonUp;
                editPatient.ShowDialog();
            }

            if (App.ClientManager.Settings.ChangeDataRequireAuthentication == true)
            {
                App.ClientManager.Logout();
            }
        }

        private void buttonMerge_Click(object sender, RoutedEventArgs e)
        {
            Rect rect = GetScreenPosition();

            bool resAuthentication = true;

            PatientData patient = new PatientData();
            patient.CopyData(this.PatientTooltipData);

            if (App.ClientManager.Settings.MergeRequireAuthentication == true)
            {
                LoginWindow login = new LoginWindow(rect, CALMMediator.USERGROUP_F_ADT, CALMMediator.USERGROUP_A_MODIFY);
                login.ShowDialog();

                resAuthentication = login.DialogResult.HasValue == true && login.DialogResult.Value == true;
            }

            if (resAuthentication == true)
            {
                MergePatientWindow mergePatient = new MergePatientWindow(rect, patient);
                mergePatient.MouseMove += OnMouseMove;
                mergePatient.KeyDown += OnKeyDown;
                mergePatient.MouseLeftButtonUp += OnMouseLeftButtonUp;
                mergePatient.ShowDialog();
            }

            if (App.ClientManager.Settings.MergeRequireAuthentication == true)
            {
                App.ClientManager.Logout();
            }
        }

        private void patienListToggle_Checked(object sender, RoutedEventArgs e)
        {
            SetPatienListVisibility((bool)((ToggleButton)sender).IsChecked);       
        }        
  
        private void txtBackToBed_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (App.ClientManager.CurrentPatient != null)
            {
                if (m_selectedVisitKey != App.ClientManager.CurrentPatient.Key)
                {
                    m_selectedVisitKey = App.ClientManager.CurrentPatient.Key;

                    SetSelectedPatient();

                    new Thread(Navigate).Start();
                }
            }
        }       

        #endregion                  

        private void mainWindow_StateChanged(object sender, EventArgs e)
        {
            if (App.ClientManager.Settings.IsCentralMode == false)
            {
                switch (this.WindowState)
                {
                    case WindowState.Maximized:
                        Activity();
                        break;
                    case WindowState.Minimized:
                        Activity();
                        break;
                    case WindowState.Normal:
                        Activity();
                        SetNormalState();
                        break;
                }
            }
        }

        private void SetNormalState()
        {
            double width = 730;
            width += patientListCol.ActualWidth;

            this.MinWidth = width;
            this.Width = width;

            this.Left = SystemParameters.WorkArea.Width - this.Width - 10;
            this.Top = SystemParameters.WorkArea.Height - this.Height;

            this.Activate();
            this.Focus();
        }

        private void chkReviewed_Checked(object sender, RoutedEventArgs e)
        {
            if(App.ClientManager.SetPositiveCRIsToReviewed(this.SelectedPatient.Key) == true)
            {
                ctrlReviewed.CheckAndDisappear();
            }
        }
    }
}
