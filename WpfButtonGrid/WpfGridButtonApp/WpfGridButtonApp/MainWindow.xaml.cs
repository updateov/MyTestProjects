using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

namespace WpfGridButtonApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            Manager.Instance.InitDataTable();
            InitializeComponent();
            dataGrid.ItemsSource = Manager.Instance.PatientsData.DefaultView;
        }

        /// <summary>
        /// Click Start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)((Button)e.Source).DataContext;
            MessageBox.Show("This patient is already running");
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)((Button)e.Source).DataContext;
            MessageBox.Show("Stop");
        }

        private void btnDischarge_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)((Button)e.Source).DataContext;
            MessageBox.Show("Discharge");
        }

        private void btnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            var mngr = Manager.Instance;
            mngr.AddPatient("fsda", "huiadosg", "b1", "1-", 1);
            buttonOutputFolderBrowse.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void buttonOutputFolderBrowse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "SQLite file (*.db3)|*.db3"
            };

            if (dlg.ShowDialog().Value)
            {
                textBoxOutputFolder.Text = dlg.FileName;
                btnAddPatient.IsEnabled = true;
            }
        }
    }
}
