using System.Windows.Controls;

namespace PatternsBA.View
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : UserControl
    {
        public HelpView(ViewModel.NavigationViewModel nav)
        {
            DataContext = this;
            NavigationViewModel = nav;
            InitializeComponent();
        }

        public ViewModel.NavigationViewModel NavigationViewModel { get; private set; }
    }
}
