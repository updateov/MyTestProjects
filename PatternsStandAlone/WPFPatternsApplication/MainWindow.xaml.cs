using NavigationData;
using PatientsData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFPatternsApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
    public partial class MainWindow : Window
    {
        private String m_currentParameters;
        private ProgressWindow m_progress = null;

        BackgroundWorker m_worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            //var patients = PatientsLoader.LoadPatients();
            m_patients = PatientsLoader.LoadPatients();
            m_worker.DoWork += m_worker_DoWork;
            //m_patients = new List<String>() { "huag", "huaisv", "nbuhvsa" };
            listPatients.ItemsSource = MyProperty;
            /// Go to the page !

            Navigate();
            //m_worker.RunWorkerAsync();
        }

        void m_worker_DoWork(object sender, DoWorkEventArgs e)
        {
        }

        private List<String> m_patients;
        public List<String> MyProperty
        {
            get { return m_patients; }
            set { m_patients = value; }
        }

        /// <summary>
        /// Navigate to the proper page
        /// </summary>
        private void Navigate()
        {
            m_progress = new ProgressWindow();
            Navigate(Navigation.BuildParameters());
            m_progress.Show();
            //MessageBox.Show("njsnb");
        }

        /// <summary>
        /// Navigate to the proper page
        /// </summary>
        /// <param name="parameters"></param>
        private void Navigate(String parameters)
        {
            try
            {
                m_currentParameters = parameters;
                if (m_currentParameters.Equals(String.Empty))
                {
                    webBrowserTracingPatterns.Visibility = Visibility.Hidden;
                    webBrowserTracingPatterns.Navigate("about:blank");
                    return;
                }

                if (webBrowserTracingPatterns.Visibility != Visibility.Visible)
                    webBrowserTracingPatterns.Visibility = Visibility.Visible;

                // If there is a password configured for the page, then encrypt the parameters
                String pwd = Navigation.EncryptionPassword;
                if (!String.IsNullOrEmpty(pwd))
                    parameters = EncDec.RijndaelEncrypt(parameters, pwd, Navigation.s_IV, Navigation.s_Salt, Navigation.s_KeySize);

                webBrowserTracingPatterns.Navigate(Navigation.ApplicationURL, String.Empty, Encoding.UTF8.GetBytes(parameters), "Content-Type: application/xml; charset=utf-8");
            }
            catch (Exception e)
            {
                webBrowserTracingPatterns.Visibility = Visibility.Hidden;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gridPatientWeb.ColumnDefinitions[0].Width = new GridLength(0);
            PatientList.IsChecked = false;
        }

        private void PatientList_Click(object sender, RoutedEventArgs e)
        {
            gridPatientWeb.ColumnDefinitions[0].Width = new GridLength(PatientList.IsChecked ? 200 : 0);
        }

        private void webBrowserTracingPatterns_Loaded(object sender, RoutedEventArgs e)
        {
            m_progress.Close();
            m_progress = null;
        }
    }
}
