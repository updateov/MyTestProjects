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

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Person> Persons { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Persons = new List<Person>() { new Person(1, this.Width), new Person(3, this.Width), new Person(2, this.Width) };

            myItemsControl.ItemsSource = Persons;
        }
    }

    public class Person
    {
        public List<aaa> Name { get; set; }
        public int NameCount
        {
            get
            {
                return Name.Count;
            }
        }

        public Person(int num, double width)
        {
            Name = new List<aaa>();
            for (int i = 0; i < num; i++)
            {
                Name.Add(new aaa(num + i, width / (num + 1)));
            }
        }
    }

    public class aaa
    {
        public List<bbb> Name2 { get; set; }

        public aaa(int num, double itemwidth)
        {
            Name2 = new List<bbb>();

            for (int i = 0; i < num; i++)
            {
                String ccc = "bbb";
                if (i == 4)
                    ccc = "njlkdsf\nv;fdsn\nkm;sdfnb\nvjdsfk;\nvjfdsk;\nvsuitrthhrthrhrhtrhhbhndfbndklj@@@";

                if (i == 1)
                    ccc = "nnnnnnn";

                Name2.Add(new bbb() { Name = ccc + num.ToString() + " " + i.ToString() + " ", ItemWidth = itemwidth });
            }
        }
    }

    public class bbb
    {
        public override string ToString()
        {
            return (Name + ", W = " + ((int)ItemWidth).ToString());
        }

        public String Name { get; set; }
        public double ItemWidth { get; set; }
    }


}
