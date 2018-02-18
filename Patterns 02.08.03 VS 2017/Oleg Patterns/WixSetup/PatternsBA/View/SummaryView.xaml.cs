using System.Windows.Controls;
using PanelSW.Installer.JetBA.ViewModel;

namespace PatternsBA.View
{
    /// <summary>
    /// Interaction logic for SummaryView.xaml
    /// </summary>
    public partial class SummaryView : UserControl
    {
        public SummaryView(NavigationViewModel nav, ApplyViewModel apply, VariablesViewModel vars)
        {
            DataContext = this;
            ApplyViewModel = apply;
            NavigationViewModel = nav;
            VariablesViewModel = vars;
            InitializeComponent();
        }

        public NavigationViewModel NavigationViewModel { get; private set; }
        public ApplyViewModel ApplyViewModel { get; private set; }
        public VariablesViewModel VariablesViewModel { get; private set; }
    }
}
