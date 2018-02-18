using System;
using System.Collections.Generic;
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

namespace WpfApplicationUnits
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var model = new CollectionDataModel()
            {
                new DataModel()
                {
                    ControlHeight = 25,
                    LabelWidth = 120,
                    ItemName = "blablalbl",
                    Units = "(F)"
                },
                new DataModel()
                {
                    ControlHeight = 25,
                    LabelWidth = 120,
                    ItemName = "blablalbl",
                    Units = "",
                    SubCard = "vbsdbvsdfbvfdsb"
                },
                new DataModel()
                {
                    ControlHeight = 25,
                    LabelWidth = 120,
                    ItemName = "blablalbl",
                    Units = "(F)"
                }
            };

            DataContext = model;
        }
    }
}
