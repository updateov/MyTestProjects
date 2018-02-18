using PatternsCALMMediator;
using PatternsCRIClient.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PatternsCRIClient.Screens
{
    public class PatientForMerge : INotifyPropertyChanged
    {
        private bool m_isSelectedPatient;

        public string Key { get;set; }
        public string BedName { get;set; }
        public string PatientDisplayName { get;set; }

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

        public PatientForMerge()
        {
            m_isSelectedPatient = false;
        }

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

    /// <summary>
    /// Interaction logic for MergePatientWindow.xaml
    /// </summary>
    public partial class MergePatientWindow : Window
    {
        private string leftNonADTDescription = "This chart has priority for providing \nTracing, EDD (GA), Fetuses, Bed # \nto the merged chart.";
        private string leftADTDescription = "This chart has priority for providing \nTracing, EDD (GA), Fetuses, Bed #, \nDemographics to the merged chart.";
        private string rightADTDescription = "This chart has priority for providing \nDemographics to the merged chart.";

        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;

        public List<PatientForMerge> Patients { get; set; }

        public PatientData CurrentPatient
        {
            get { return (PatientData)GetValue(CurrentPatientProperty); }
            set { SetValue(CurrentPatientProperty, value); }
        }
        public static readonly DependencyProperty CurrentPatientProperty =
            DependencyProperty.Register("CurrentPatient", typeof(PatientData), typeof(MergePatientWindow), new PropertyMetadata(null));


        public PatientData SelectedPatient
        {
            get { return (PatientData)GetValue(SelectedPatientProperty); }
            set { SetValue(SelectedPatientProperty, value);}
        }
        public static readonly DependencyProperty SelectedPatientProperty =
            DependencyProperty.Register("SelectedPatient", typeof(PatientData), typeof(MergePatientWindow), new UIPropertyMetadata(null));
   

        public Visibility ResultVisibility
        {
            get { return (Visibility)GetValue(ResultVisibilityProperty); }
            set { SetValue(ResultVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ResultVisibilityProperty =
            DependencyProperty.Register("ResultVisibility", typeof(Visibility), typeof(MergePatientWindow), new PropertyMetadata(Visibility.Hidden));


        public bool IsEnableApply
        {
            get { return (bool)GetValue(IsEnableApplyProperty); }
            set { SetValue(IsEnableApplyProperty, value); }
        }
        public static readonly DependencyProperty IsEnableApplyProperty =
            DependencyProperty.Register("IsEnableApply", typeof(bool), typeof(MergePatientWindow), new PropertyMetadata(false));
     
        
        public MergePatientWindow(Rect dimensions, PatientData patient)
        {
            this.CurrentPatient = new PatientData();
            this.CurrentPatient.CopyData(patient);
            this.Patients = FillPatientsForMerge();

            InitializeComponent();

            this.Top = dimensions.Top;
            this.Left = dimensions.Left;
            this.Width = dimensions.Width;
            this.Height = dimensions.Height;

            Color color = Color.FromArgb(130, 63, 63, 63);
            this.Background = new SolidColorBrush(color);

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutMessage");

            if(this.CurrentPatient.IsADT == true)
            {
                txtLeftChartDescr.Text = leftADTDescription;
                txtRightChartDescr.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                txtLeftChartDescr.Text = leftNonADTDescription;
                txtRightChartDescr.Text = rightADTDescription;
            }
        }

        private List<PatientForMerge> FillPatientsForMerge()
        {
            List<PatientData> patientsList = App.ClientManager.Patients.FindAll(t => t.IsADT != CurrentPatient.IsADT);
            
            List<PatientForMerge> list = new List<PatientForMerge>();

            patientsList.ForEach(patient =>
                {
                    PatientForMerge item = new PatientForMerge();
                    item.Key = patient.Key;
                    item.BedName = patient.BedName;
                    item.PatientDisplayName = patient.PatientDisplayName;

                    list.Add(item);
                });

            return list;
        }      
        
        private void PatientSelected(object sender)
        {
            PatientForMerge patientForMerge = ((ListView)sender).SelectedItem as PatientForMerge;

            if (patientForMerge != null)
            {
                PatientForMerge prevSelected = this.Patients.FirstOrDefault(t => t.IsSelectedPatient == true);

                if (prevSelected != null)
                {
                    prevSelected.IsSelectedPatient = false;
                }

                patientForMerge.IsSelectedPatient = true;

                PatientData patient = App.ClientManager.Patients.FirstOrDefault(t => t.Key == patientForMerge.Key);

                if (patient != null)
                {
                    SelectedPatient = new PatientData();
                    SelectedPatient.CopyData(patient);

                    SelectedPatient.BedName = CurrentPatient.BedName;
                    SelectedPatient.Fetuses = CurrentPatient.Fetuses;

                    if (String.IsNullOrEmpty(CurrentPatient.GA) == false)
                    {
                        SelectedPatient.GA = CurrentPatient.GA;
                    }

                    if (CurrentPatient.IsADT == true)
                    {
                        SelectedPatient.FirstName = CurrentPatient.FirstName;
                        SelectedPatient.LastName = CurrentPatient.LastName;
                    }

                    ResultVisibility = System.Windows.Visibility.Visible;
                    this.IsEnableApply = true;
                    listPatients.IsEnabled = false;
                }
                else
                {
                    ResultVisibility = System.Windows.Visibility.Hidden;
                    listPatients.IsEnabled = true;
                }
            }
        }

        #region Events

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            this.Close();
        }

        private void mergePatientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            MergePatient();
        }

        private void MergePatient()
        {
            if (this.IsEnableApply == true)
            {
                /*** If both visits already exists ***/
                if (App.ClientManager.Patients.Exists(t => t.Key == CurrentPatient.Key) &&
                    App.ClientManager.Patients.Exists(t => t.Key == SelectedPatient.Key))
                {
                    this.Topmost = false;

                    bool result = App.ClientManager.EmergencyMerge(CurrentPatient, SelectedPatient);

                    if (result == false)
                    {
                        AutomaticMessageWindow messageWnd = new AutomaticMessageWindow();
                        messageWnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        messageWnd.MessageTitle.Text = "Failed to merge patient charts.";
                        messageWnd.MessageDescription.Text = (string)Application.Current.FindResource("MSG_Auto_disapear");
                        messageWnd.ShowDialog();
                    }
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            this.Close();
        }
   
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender).SelectedIndex != -1)
            {
                PatientSelected(sender);

                ((ListView)sender).SelectedIndex = -1;
            }
        }
    
        private void ContentControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedPatient = null;
            ResultVisibility = System.Windows.Visibility.Hidden;
            this.IsEnableApply = false;

            PatientForMerge prevSelected = this.Patients.FirstOrDefault(t => t.IsSelectedPatient == true);
            if (prevSelected != null)
            {
                prevSelected.IsSelectedPatient = false;
                listPatients.IsEnabled = true;
            }
        }        
        
        private void mergePatientWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                m_gridFadeOutStoryBoard.Begin();
                this.Close();
            }
            else if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                this.MergePatient();
            }
        }

        #endregion 
    }
}
