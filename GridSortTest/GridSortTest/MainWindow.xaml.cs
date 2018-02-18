using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GridSortTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ModelRows = new ObservableCollection<Model>()
            {
                new Model() { Time = DateTime.UtcNow.ToString(), EnteredBy = "avba", Text = "vafdsb dsbg ewge"},
                new Model() { Time = DateTime.UtcNow.AddMinutes(15).ToString(), EnteredBy = "sbdf", Text = "bxdfffxb fd urer"},
                new Model() { Time = DateTime.UtcNow.AddHours(2).ToString(), EnteredBy = "bfgd", Text = "vaswhbvasbv regg"},
                new Model() { Time = DateTime.UtcNow.AddDays(1).AddMinutes(-30).ToString(), EnteredBy = "dfxb", Text = "bdfx bxfbxd greg"}
            };

            dataGrid.DataContext = ModelRows;
        }

        public ObservableCollection<Model> ModelRows { get; set; }
    }

    public class Model
    {
        public Model()
        {
        }

        public String Time { get; set; }
        public String Text { get; set; }
        public String EnteredBy { get; set; }
        public String EntryTime { get; set; }
    }
}
