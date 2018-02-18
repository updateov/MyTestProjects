using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NumberToString
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = 5;
            int n1 = 215;

            Console.Out.WriteLine("Single digit {0}, Two digit {1}", n.ToString("D2"), n1.ToString("D2"));

            Console.In.ReadLine();
        }
    }
}
