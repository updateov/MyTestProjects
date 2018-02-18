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

namespace WpfApplication6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<VM> vm = new List<VM>()
            {
                new VM(this, "AAA", "aaa"),
                new VM(this, "BBB", "bbb"),
                new VM(this, "CCC", "ccc"),
                new VM(this, "DDD", "ddd"),
                new VM(this, "EEE", "eee")
            };

            DataContext = vm;
        }

        public event EventHandler ExpandClicked;
        public event EventHandler CollapseClicked;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ExpandClicked != null)
                ExpandClicked(this, EventArgs.Empty);
        }

        private void buttonCollapse_Click(object sender, RoutedEventArgs e)
        {
            if (CollapseClicked != null)
                CollapseClicked(this, EventArgs.Empty);
        }
    }

    public class VM : INotifyPropertyChanged
    {
        public VM(MainWindow wnd, String header, String content)
        {
            Model = new M(header, content);
            Expand = false;
            wnd.ExpandClicked += wnd_ExpandClicked;
            wnd.CollapseClicked += wnd_CollapseClicked;
        }

        void wnd_ExpandClicked(object sender, EventArgs e)
        {
            Expand = true;
        }

        void wnd_CollapseClicked(object sender, EventArgs e)
        {
            Expand = false;
        }

        public M Model { get; set; }
        private bool m_bExpand;
        public bool Expand
        {
            get { return m_bExpand; }
            set
            {
                m_bExpand = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Expand"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class M
    {
        public M(String header, String content)
        {
            Header = header;
            Content = content;
        }

        public String Header { get; set; }
        public String Content { get; set; }
    }

}
