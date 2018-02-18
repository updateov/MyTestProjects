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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            data = new List<Z>() { new Z("AAA", "AAA"), new Z("GGG", "GGG") };
            this.DataContext = data;
        }

        public List<Z> data;// = new List<Z>() { new Z("AAA", "AAA"), new Z("GGG", "GGG") };
    }

    public class DepProperties : ItemsControl
    {


    }

    public class Z : DependencyObject
    {
        public Boolean ItemsExpanded
        {
            get { return (Boolean)GetValue(ItemsExpandedProperty); }
            set { SetValue(ItemsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsExpandedProperty =
                    DependencyProperty.RegisterAttached("ItemsExpanded", typeof(Boolean), typeof(Z), new FrameworkPropertyMetadata(new PropertyChangedCallback(Changed)));


        private static void Changed(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            MessageBox.Show("Expand/Collapse clicked", "Expand/Collapse", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public String Header { get; set; }
        public String Content { get; set; }

        public String Name { get; set; }

        public Z(String header, String content)
        {
            Header = header;
            Content = content;
            Name = "bbb";
        }
    }
}
