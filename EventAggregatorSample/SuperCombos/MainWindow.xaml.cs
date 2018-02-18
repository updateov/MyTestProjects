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

namespace SuperCombos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    public class RowViewModel
    {
        public Image Place1 { get; set; }
        public Image Place2 { get; set; }
        public Image Place3 { get; set; }
        public Image Place4 { get; set; }
        public Image Place5 { get; set; }
        public Image Place6 { get; set; }
        public Image Place7 { get; set; }
    }
}
