using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeWpfApplication4
{
    public class Node
    {
        public Node()
        {
            Items = new ObservableCollection<Node>();
        }

        public string Name { get; set; }
        public ObservableCollection<Node> Items { get; set; }
    }
}
