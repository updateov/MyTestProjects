using System.Windows.Controls;
using PanelSW.Installer.JetBA.ViewModel;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace PatternsBA.View
{
    /// <summary>
    /// Interaction logic for InstallLocationView.xaml
    /// </summary>
    public partial class InstallLocationView : UserControl
    {
        public InstallLocationView(VariablesViewModel vars, NavigationViewModel nav)
        {
            DataContext = this;
            NavigationViewModel = nav;
            VariablesViewModel = vars;
            InitializeComponent();
        }

        public VariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }

        private void browseInstallFolder_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.SelectedPath = pathTextBox.Text ?? "";
            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                pathTextBox.Text = fbd.SelectedPath;
            }
        }
    }
}
