using System.Windows.Controls;
using PanelSW.Installer.JetBA.ViewModel;

namespace PatternsBA.View
{
    /// <summary>
    /// Interaction logic for RepairView.xaml
    /// </summary>
    public partial class RepairView : UserControl
    {
        public RepairView(ApplyViewModel apply, VariablesViewModel vars)
        {
            DataContext = this;
            ApplyViewModel = apply;
            VariablesViewModel = vars;
            InitializeComponent();
        }

        public ApplyViewModel ApplyViewModel { get; private set; }
        public VariablesViewModel VariablesViewModel { get; private set; }
    }
}
