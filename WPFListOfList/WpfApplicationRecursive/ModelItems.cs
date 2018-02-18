using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplicationRecursive
{
    public class ModelItems
    {
        public ModelItems()
        {
            Items = new List<ModelItems>();
        }

        public override string ToString()
        {
            return Text;
        }

        public String Text { get; set; }
        public List<ModelItems> Items { get; set; }
    }
}
