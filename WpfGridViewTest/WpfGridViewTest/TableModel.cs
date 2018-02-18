using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfGridViewTest
{
    public class TableModel : NotificationObject
    {
        public TableModel()
        {
            MyCommand = new DelegateCommand(AAA);
        }

        void AAA()
        {
        }

        public DelegateCommand MyCommand{ get; set; }

        public int ID { get; set; }
        public DateTime Time { get; set; }
        public String Text { private get; set; }
        public DateTime EntryTime { get; set; }
        public String Name { get; set; }
        public String ExpanderHeader 
        { 
            get 
            {
                String toRet = Text.Trim();
                int ind = Text.Trim().IndexOf("\n");
                if (ind > 0)
                    toRet = Text.Trim().Remove(ind).Trim();

                return toRet; 
            } 
        }

        public String ExpanderText
        {
            get
            {
                String toRet = String.Empty;
                int ind = Text.Trim().IndexOf("\n");
                if (ind > 0)
                    toRet = Text.Trim().Remove(0, ind).Trim();

                return toRet;
            }
        }
    }
}
