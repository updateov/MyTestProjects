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

namespace TreeWpfApplication4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<Node> node = new List<Node>()
            {
                new Node()
                {
                    Name = "1",
                    Items = new ObservableCollection<Node>() 
                    { 
                        new Node(){ Name = "2,1"}, 
                        new Node()
                        { 
                            Name = "2,2",
                            Items = new ObservableCollection<Node>()
                            {
                                new Node() { Name = "3,2,1"},
                                new Node() { Name = "3,2,2"},
                                new Node() { Name = "3,2,3"}
                            }
                        }, 
                        new Node(){ Name = "2,3"}, 
                        new Node(){ Name = "2,4"}
                    }
                }
            };

            this.DataContext = node;
        }
    }
}
