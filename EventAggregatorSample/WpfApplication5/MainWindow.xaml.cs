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

namespace WpfApplication5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Model = new List<Z>()
            {
                new Z() { Content="aaa", Header="AAA"},
                new Z() { Content="bbb", Header="BBB"},
                new Z() { Content="ccc", Header="CCC"}
            };

            DataContext = Model;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!m_bExpand)
                exIC.IsExpand = false;
            //exIC.IsExpand = null;
            exIC.IsExpand = true;
            m_bExpand = true;
        }

        private bool m_bExpand = false;
        public List<Z> Model { get; private set; }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!m_bExpand)
                exIC.IsExpand = true;
            //exIC.IsExpand = null;
            exIC.IsExpand = false;
            m_bExpand = true;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            m_bExpand = false;
            //exIC.IsExpand = false;
        }
    }

    public class ExpandedItemsControl : ItemsControl
    {


        public bool IsExpand
        {
            get { return (bool)GetValue(IsExpandProperty); }
            set { SetValue(IsExpandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandProperty =
            DependencyProperty.Register("IsExpand", typeof(bool), typeof(ExpandedItemsControl), new PropertyMetadata(false));

    }

    public class Z
    {
        public String Content { get; set; }
        public String Header { get; set; }
    }
}
