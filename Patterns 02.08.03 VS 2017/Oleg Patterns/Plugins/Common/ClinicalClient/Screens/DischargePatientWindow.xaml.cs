using PatternsCRIClient.Data;
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

namespace PatternsCRIClient.Screens
{
    /// <summary>
    /// Interaction logic for DischargePatientWindow.xaml
    /// </summary>
    public partial class DischargePatientWindow : Window
    {
        public PatientData CurrentPatient
        {
            get { return (PatientData)GetValue(CurrentPatientProperty); }
            set { SetValue(CurrentPatientProperty, value); }
        }

        public static readonly DependencyProperty CurrentPatientProperty =
            DependencyProperty.Register("CurrentPatient", typeof(PatientData), typeof(EditPatientWindow), new PropertyMetadata(null));

        public DischargePatientWindow(Rect dimensions, PatientData currentPatient)
        {
            this.CurrentPatient = new PatientData();
            this.CurrentPatient.CopyData(currentPatient);

            InitializeComponent();

            this.Top = dimensions.Top;
            this.Left = dimensions.Left;
            this.Width = dimensions.Width;
            this.Height = dimensions.Height;

            Color color = Color.FromArgb(130, 63, 63, 63);
            this.Background = new SolidColorBrush(color);
        }

        private void dischargePatientWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DischargePatient()
        {
            if(App.ClientManager.IsMonitorActive(this.CurrentPatient.BedId) == true)
            {
                this.Topmost = false;
                this.Opacity = 0;

                MessageBoxWindow msgBox = new MessageBoxWindow();
                msgBox.MessageTitle.Text = "Discharge Patient Confirmation";
                msgBox.MessageDescription.Text = "Fetal monitoring is still active and collecting data. \nAre you sure you want to discharge the patient?";

                Nullable<bool> dialogResult = msgBox.ShowDialog();

                if (dialogResult.HasValue == true && dialogResult.Value == false)
                {
                    this.Close();
                    return;
                }
            }

            bool result = App.ClientManager.DischargePatient(this.CurrentPatient.Key);

            if (result == false)            
            {
                AutomaticMessageWindow messageWnd = new AutomaticMessageWindow();
                messageWnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                messageWnd.MessageTitle.Text = "Failed to discharge patient.";
                messageWnd.MessageDescription.Text = (string)Application.Current.FindResource("MSG_Auto_disapear");
                messageWnd.ShowDialog();
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DischargePatient();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void dischargePatientWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                this.DischargePatient();
                this.Close();
            }
        }
    }
}
