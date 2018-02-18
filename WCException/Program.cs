using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCException
{
    class Program
    {
        static void Main(string[] args)
        {
            String time = "2016-12-06T16:09:00";
            DateTime dt = DateTime.ParseExact(time, "s", CultureInfo.InvariantCulture);
            Console.WriteLine(dt.ToString());
        }
    }
}
