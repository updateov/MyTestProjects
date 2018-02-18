using System.Windows.Controls;
using PanelSW.Installer.JetBA.ViewModel;
namespace PatternsBA.View
{
    /// <summary>
    /// Interaction logic for ProgressView.xaml
    /// </summary>
    public partial class ProgressView : UserControl
    {
        public ProgressView(NavigationViewModel nav, ApplyViewModel apply, ProgressViewModel prog)
        {
            DataContext = this;
            NavigationViewModel = nav;
            ApplyViewModel = apply;
            ProgressViewModel = prog;
            InitializeComponent();
        }

        public NavigationViewModel NavigationViewModel { get; private set; }
        public ApplyViewModel ApplyViewModel { get; private set; }
        public ProgressViewModel ProgressViewModel { get; private set; }
    }
}
