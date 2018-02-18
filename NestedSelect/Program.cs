using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NestedSelect
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Strs> strLst = new List<Strs>()
            {
                new Strs() { ID = 1, Val = "1" },
                new Strs() { ID = 2, Val = "2" },
                new Strs() { ID = 3, Val = "3" },
                new Strs() { ID = 4, Val = "4" }
            };

            List<Cspts> csptLst = new List<Cspts>()
            {
                new Cspts() { ID = 1, ConceptVal = "1" },
                new Cspts() { ID = 4, ConceptVal = "4" }
            };


            var res = from c in strLst
                      where
                          (from d in csptLst
                           select d.ID).Contains(c.ID)
                      select c;
        }
    }

    public class Strs
    {
        public Strs()
        {
        }

        public int ID { get; set; }
        public String Val { get; set; }
    }

    public class Cspts
    {
        public Cspts()
        {
        }

        public int ID { get; set; }
        public String ConceptVal { get; set; }
    }
}
