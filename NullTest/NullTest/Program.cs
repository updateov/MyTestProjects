using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NullTest
{
    class Program
    {
        static void Main(string[] args)
        {
            XElement x = null;
            XElement y = new XElement("Element", new XAttribute("attr", 1));
            //Console.WriteLine("X = " + x ?? "EmptyStr");
            Console.WriteLine("Y = " + y ?? "Empty");
            Console.WriteLine("Attr = " + y.Attribute("attr").Value ?? "Empty");
            Console.WriteLine("Attr = " + y.Attribute("gdsg").Value ?? "Empty");
            Console.ReadLine();
        }
    }
}
