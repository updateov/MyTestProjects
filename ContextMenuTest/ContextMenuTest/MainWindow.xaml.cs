using Microsoft.Practices.Prism.ViewModel;
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

namespace ContextMenuTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Menu = new ContextMenus();
            DataContext = Menu;
            //Menus.ItemsSource = Menu.Menus;
        }

        public ContextMenus Menu { get; set; }
    }

    public class ContextMenus : NotificationObject
    {
        private String m_mainHeader;
        public String MainHeader
        {
            get { return m_mainHeader.ToUpper(); }
            set
            {
                m_mainHeader = value.ToUpper();
                RaisePropertyChanged(() => this.MainHeader);
            }
        }

        public ContextMenus()
        {
            MainHeader = "menujlngb";
            Menus = new ObservableCollection<IMenuItem>() 
            { 
                new VM("All"), 
                //new TextVM("Part uhiloszbhufdbvdasf"), 
                new MySep(), 
                new VM("Part ujasdlgbjhnaghasdfgbasdlgbv"), 
                new VM("None") 
            };
        }

        public ObservableCollection<IMenuItem> Menus { get; private set; }
    }

    public interface IMenuItem
    {
    }

    public class VM : IMenuItem
    {
        public VM(String header)
        {
            Model = new M(header);
        }

        public M Model { get; private set; }
    }

    public class TextVM : IMenuItem
    {
        public TextVM(String header)
        {
            Header = header;
        }

        public String Header { get; private set; }
    }

    public class MySep : IMenuItem
    {
        public MySep()
        {
            Model = new M("gfdfb");
        }

        public M Model { get; private set; }
    }

    public class M
    {
        public M(String name)
        {
            ItemName = name;
            Checked = true;
        }

        public bool Checked { get; set; }
        public String ItemName { get; set; }
    }

}
