using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestableApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var testableObj = new TestableClass();
            long result = testableObj.Calculate(15, 27);
            Console.WriteLine("Result = :" + result.ToString());
            Console.ReadLine();
        }
    }
}
