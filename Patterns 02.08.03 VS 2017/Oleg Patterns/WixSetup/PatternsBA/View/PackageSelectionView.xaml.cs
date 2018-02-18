using System.Windows.Controls;
using PanelSW.Installer.JetBA.ViewModel;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace PatternsBA.View
{
    /// <summary>
    /// Interaction logic for InstallLocationView.xaml
    /// </summary>
    public partial class PackageSelectionView : UserControl
    {
        public PackageSelectionView(VariablesViewModel vars, NavigationViewModel nav, ViewModel.PackageSelectionViewModel packages)
        {
            DataContext = this;
            NavigationViewModel = nav;
            VariablesViewModel = vars;
            InitializeComponent();
        }

        public VariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
    }
}
