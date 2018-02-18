using Microsoft.Practices.Prism.ViewModel;
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

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            m_vm = new EVMM() { Expand = false };
            m_vm.AddRange(new VMM[]
            {
                new VMM()
                {
                    HeadText = "Header eeeeeeexpander1",
                    ContentText = "Content eeeeeeeeeexpander1"
                },
                new VMM()
                {
                    HeadText = "Header eeeeeeexpander2",
                    ContentText = "Content eeeeeeeeeexpander2"
                },
                new VMM()
                {
                    HeadText = "Header eeeeeeexpander3",
                    ContentText = "Content eeeeeeeeeexpander3"
                },
                new VMM()
                {
                    HeadText = "Header eeeeeeexpander4",
                    ContentText = "Content eeeeeeeeeexpander4"
                }
            });

            DataContext = m_vm;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in m_vm)
            {
                item.HeadText = "bla";
            }

            m_vm[0].Expanded = true;
            m_vm.Expand = true;
        }

        EVMM m_vm = null;
    }

    public class VM
    {
        public VM()
        {
            ExpData = new EVMM() { Expand = false };
            ExpData.AddRange(new VMM[]
            {
                new VMM()
                {
                    HeadText = "Header eeeeeeexpander1",
                    ContentText = "Content eeeeeeeeeexpander1"
                },
                new VMM()
                {
                    HeadText = "Header eeeeeeexpander2",
                    ContentText = "Content eeeeeeeeeexpander2"
                },
                new VMM()
                {
                    HeadText = "Header eeeeeeexpander3",
                    ContentText = "Content eeeeeeeeeexpander3"
                },
                new VMM()
                {
                    HeadText = "Header eeeeeeexpander4",
                    ContentText = "Content eeeeeeeeeexpander4"
                }
            });

        }

        public EVMM ExpData { get; private set; }

    }

    public class EVMM : List<VMM>, INotifyPropertyChanged
    {
        private bool m_bExpand;
        public bool Expand
        {
            get
            {
                return m_bExpand;
            }
            set
            {
                m_bExpand = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Expand"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class VMM : DependencyObject, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ExpandProperty = DependencyProperty.RegisterAttached("Expand", typeof(Boolean), typeof(VMM), new FrameworkPropertyMetadata(new PropertyChangedCallback(ccc)));

        private static void ccc(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static bool GetExpand(DependencyObject obj)
        {
            return (bool)obj.GetValue(ExpandProperty);
        }

        public static void SetExpand(DependencyObject obj, bool value)
        {
            obj.SetValue(ExpandProperty, value);
        }

        private bool m_bExpanded;
        public bool Expanded
        {
            get { return m_bExpanded; }
            set
            {
                m_bExpanded = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Expanded"));
            }
        }

        private String m_headerText;
        public String HeadText
        {
            get { return m_headerText; }
            set
            {
                m_headerText = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("HeadText"));
            }
        }
        public String ContentText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
