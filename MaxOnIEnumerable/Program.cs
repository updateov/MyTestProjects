using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxOnIEnumerable
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15 };
            new Logic().F(list);
        }


    }

    public class Logic
    {
        public void F(List<int> l)
        {
            var ie = from c in l
                     where c > 5
                     select c;

            int maxNum = ie.Max(c => c);
        }
    }
}
