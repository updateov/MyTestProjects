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

namespace WpfApplication4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ExpandAllProperty;


        static MainWindow()
        {
            MainWindow.ExpandAllProperty = DependencyProperty.RegisterAttached("ExpandAll",
                                            typeof(bool),
                                            typeof(MainWindow));
        }

        public static void SetExpandAll(DependencyObject element, bool value)
        {
            element.SetValue(ExpandAllProperty, value);
        }

        public static bool GetExpandAll(DependencyObject element)
        {
            return (bool)element.GetValue(ExpandAllProperty);
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
