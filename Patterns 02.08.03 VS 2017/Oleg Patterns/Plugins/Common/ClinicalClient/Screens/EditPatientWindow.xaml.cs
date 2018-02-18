using PatternsCALMMediator;
using PatternsCRIClient.Data;
using CRIEntities;
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
using System.Windows.Shapes;

namespace PatternsCRIClient.Screens
{
    /// <summary>
    /// Interaction logic for EditPatientWindow.xaml
    /// </summary>
    public partial class EditPatientWindow : Window, INotifyPropertyChanged
    {
        public List<Bed> Beds { get; set; }
        public List<int> Fetuses { get; set; }
        public List<string> BedNames { get; set; }
        public bool m_isEnableOK;
         
        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;
        private PatientData m_patientData;
     
        public bool IsEnableOK
        {
            set
            {
                if (m_isEnableOK != value)
                {
                    m_isEnableOK = value;
                    RaisePropertyChanged("IsEnableOK");
                }
            }
            get
            {
                return m_isEnableOK;
            }
        }

        public PatientData EditPatient          
        {
            get { return (PatientData)GetValue(EditPatientProperty); }
            set { SetValue(EditPatientProperty, value); }
        }

        public static readonly DependencyProperty EditPatientProperty =
            DependencyProperty.Register("EditPatientt", typeof(PatientData), typeof(EditPatientWindow), new PropertyMetadata(null));

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

        public EditPatientWindow(Rect dimensions, PatientData currentPatient)
        {
            this.m_patientData = currentPatient;

            this.EditPatient = new PatientData();
            this.EditPatient.CopyData(currentPatient);
            this.EditPatient.PropertyChanged += CurrentPatient_PropertyChanged;
            this.Beds = App.ClientManager.GetAvailableBeds();
            
            Bed currentBed = new Bed();
            currentBed.Id = this.EditPatient.BedId;
            currentBed.Name = this.EditPatient.BedName;

            this.Beds.Add(currentBed);
            this.Beds.Sort((a, b) => a.Name.CompareTo(b.Name));

            this.BedNames = new List<string>();

            this.Beds.ForEach(bed =>
                {
                    this.BedNames.Add(bed.Name);
                });

            this.Fetuses = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

            InitializeComponent();

            this.Top = dimensions.Top;
            this.Left = dimensions.Left;
            this.Width = dimensions.Width;
            this.Height = dimensions.Height;

            Color color = Color.FromArgb(130, 63, 63, 63);
            this.Background = new SolidColorBrush(color);

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("editFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("editFadeOutMessage");

            this.IsEnableOK = false;

            if (App.ClientManager.Settings.BlockADTFunctions == true)
            {
                this.txtFirstName.IsReadOnly = true;
                this.txtLastName.IsReadOnly = true;
                this.brdFirstName.Background = this.txtFirstName.Background;
                this.brdLastName.Background = this.txtLastName.Background;
                this.cmbBed.IsEnabled = false; 
            }

            txtFirstName.Select(0, txtFirstName.Text.Length);
            txtFirstName.Focus();
        }

        private void Validate()
        {
            bool isEmptyFields = String.IsNullOrWhiteSpace(this.EditPatient.FirstName) ||
                                 String.IsNullOrWhiteSpace(this.EditPatient.LastName) ||
                                 String.IsNullOrWhiteSpace(this.EditPatient.BedName);

            bool isChanged = m_patientData.FirstName != this.EditPatient.FirstName ||
                             m_patientData.LastName != this.EditPatient.LastName ||
                             m_patientData.Fetuses != this.EditPatient.Fetuses ||
                             m_patientData.EDD != this.EditPatient.EDD ||
                             m_patientData.BedName != this.EditPatient.BedName;

            if (isEmptyFields == false && isChanged == true)
            {
                this.IsEnableOK = true;
            }
            else
            {
                this.IsEnableOK = false;
            }
        }

        #region Events

        void CurrentPatient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Validate();
        }

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableOK == true)
            {
                this.Topmost = false;
                m_gridFadeOutStoryBoard.Begin();

                string bedId = null;

                Bed bed = Beds.FirstOrDefault(t => t.Name == EditPatient.BedName);
                if (bed != null)
                {
                    bedId = bed.Id.ToString();
                }

                bool result = App.ClientManager.EditPatientData(m_patientData.Key, EditPatient.FirstName, EditPatient.LastName, EditPatient.Fetuses, EditPatient.EDD, bedId);

                if (result == false)
                {
                    AutomaticMessageWindow messageWnd = new AutomaticMessageWindow();
                    messageWnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    messageWnd.MessageTitle.Text = "Failed to change patient data.";
                    messageWnd.MessageDescription.Text = (string)Application.Current.FindResource("MSG_Auto_disapear");
                    messageWnd.ShowDialog();
                    m_gridFadeInStoryBoard.Begin();
                    this.Topmost = true;
                }
                else
                {
                    this.Close();
                }

                //this.Close();
            }
            else
            {
                e.Handled = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            this.Close();
        }

        private void editPatientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();
        }

        private void TextPreviewKeyDown(object sender, KeyEventArgs e)
        {
            Validate();
        }

        private void editPatientWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {               
                m_gridFadeOutStoryBoard.Begin();
                this.Close();
            }
        }      
        
        #endregion             
    }
}
