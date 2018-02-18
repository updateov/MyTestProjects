using PatternsCRIClient.Screens;
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
    /// Interaction logic for AddPatientWindow.xaml
    /// </summary>
    public partial class AddPatientWindow : Window, INotifyPropertyChanged
    {
        public List<Bed> Beds { get; set; }
        public List<int> Fetuses { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;

        public bool m_isEnableOK;
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

        public AddPatientWindow(Rect dimensions, string bedName)
        {
            this.Beds = App.ClientManager.GetAvailableBeds();
            this.Fetuses = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

            InitializeComponent();

            this.Top = dimensions.Top;
            this.Left = dimensions.Left;
            this.Width = dimensions.Width;
            this.Height = dimensions.Height;

            Color color = Color.FromArgb(130, 63, 63, 63);
            this.Background = new SolidColorBrush(color);

            if (String.IsNullOrEmpty(bedName) == false)
            {
                Bed bed = this.Beds.FirstOrDefault(t => t.Name == bedName);
                this.cmbBed.SelectedItem = bed;
            }

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutMessage");

            txtFirstName.Focus();
        }
 
        #region Events

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnableOK == true)
            {
                AdmitPatient();
            }
        }

        private void AdmitPatient()
        {
            Bed selectedBed = (Bed)cmbBed.SelectedItem;
            int fetusNum = Int32.Parse(cmbFetusNum.Text);

            this.admitPatientGrid.Width = 0;
            this.admitPatientGrid.Height = 0;
            this.btnClose.Visibility = System.Windows.Visibility.Hidden;

            bool result = App.ClientManager.EmergencyAdmit(txtFirstName.Text, txtLastName.Text, selectedBed, fetusNum, dtEDD.SelectedDate);

            AutomaticMessageWindow messageWnd = new AutomaticMessageWindow();
            messageWnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            if (result)
            {
                messageWnd.MessageTitle.Text = (string)Application.Current.FindResource("MSG_Patient_Added");
                messageWnd.MessageDescription.Text = (string)Application.Current.FindResource("MSG_Auto_disapear");
            }
            else
            {
                messageWnd.MessageTitle.Text = (string)Application.Current.FindResource("MSG_Patient_Failed");
                messageWnd.MessageDescription.Text = (string)Application.Current.FindResource("MSG_Auto_disapear");
            }

            messageWnd.ShowDialog();

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            this.Close();
        }

        private void OnTextLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;

            if(String.IsNullOrEmpty(txtBox.Text) == true)
            {
                txtBox.BorderBrush = Brushes.Red;
            }
            else
            {
                txtBox.BorderBrush = Brushes.LightGray;
            }
        }

        private void admitPatientWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                m_gridFadeOutStoryBoard.Begin();
                this.Close();
            }
            else if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (IsEnableOK == true)
                {
                    AdmitPatient();
                }
            }
            else
            {
                Validate();
            }
        }

        private void admitPatientWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Validate();
        }

        private void cmbBed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Validate();
        }
    
        private void admitPatientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();
        }

        private void cmbBed_LostFocus(object sender, RoutedEventArgs e)
        {
            if(cmbBed.SelectedIndex == -1)
            {
                cmbBed.BorderBrush = Brushes.Red;
            }
            else
            {
                cmbBed.BorderBrush = Brushes.LightGray;
            }
        }

      
        private void cmbBed_DropDownClosed(object sender, EventArgs e)
        {
            if (cmbBed.SelectedIndex == -1)
            {
                cmbBed.BorderBrush = Brushes.Red;
            }
            else
            {
                cmbBed.BorderBrush = Brushes.LightGray;
            }
        }        
        
        #endregion

        private void Validate()
        {
            if (String.IsNullOrEmpty(txtFirstName.Text) == false &&
                String.IsNullOrEmpty(txtLastName.Text) == false &&
                cmbBed.SelectedItem != null)
            {
                this.IsEnableOK = true;
            }
            else
            {
                this.IsEnableOK = false;
            }
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
}
