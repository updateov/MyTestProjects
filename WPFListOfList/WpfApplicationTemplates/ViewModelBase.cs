using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplicationTemplates
{
    public interface IViewModelBase
    {
        //public String ItemName { get; set; }
    }

    public class ViewModelInteger : IViewModelBase
    {
        public int Val { get; set; }
    }

    public class ViewModelFloat : IViewModelBase
    {
        public ViewModelFloat()
        {
            ListVal = new List<string>()
            {
                "item 1",
                "item 2",
                "item 3",
                "item 4"
            };
        }
     
        public List<String> ListVal { get; set; }
    }

    public class ViewModelBool : IViewModelBase
    {
        public String Name { get; set; }
        public bool Checked { get; set; }
    }

    public class ViewModelSeparator : IViewModelBase
    {
    }
}
