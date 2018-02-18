using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplicationUnits
{
    public class DataModel
    {
        public DataModel()
        {
        }

        public int LabelWidth { get; set; }
        public int ControlHeight { get; set; }
        public string ItemName { get; set; }
        public string Units { get; set; }
        public String SubCard { get; set; }
    }

    public class CollectionDataModel : List<DataModel>
    {
    }
}
