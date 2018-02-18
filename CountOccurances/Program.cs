using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountOccurances
{
    class Program
    {
        static void Main(string[] args)
        {
            String str666 = "666-18-1-1-2-5-8-9";
            int dashes = str666.Count(c => c == '-');
            Console.WriteLine("Dashes = " + dashes.ToString());
            Console.ReadLine();
        }
    }
}
