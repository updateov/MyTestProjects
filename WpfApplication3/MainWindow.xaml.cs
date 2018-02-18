using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WpfApplication3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged 
    {
        public MainWindow()
        {
            InitializeComponent();
            m_str = "5";
            Loaded += MainWindow_Loaded;
            DataContext = this;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Str = "5";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int n;
            Int32.TryParse(Str, out n);
            Str = (++n).ToString();
        }

        private String m_str;
        public String Str
        {
            get
            {
                return m_str;
            }
            set
            {
                m_str = value;
                //PropertyChanged(this, new PropertyChangedEventArgs("Str"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Converter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //we're using OneWayToSource, so this will never be used.
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            // we want to convert the single 'IsChecked' value from the ToggleButton into 
            // 3 'IsExpanded' values
            var allValues = new object[targetTypes.Length];
            for (int i = 0; i < allValues.Length; i++)
            {
                allValues[i] = value;
            }

            return allValues;
        }
    }
}
