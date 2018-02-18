using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReflectedFields
{
    public class Program
    {
        static void Main(string[] args)
        {
            Assembly o = Assembly.Load("TestClasses");
            var types = o.GetTypes();
            foreach (var item in types)
            {
                var members = item.GetMembers();
                Console.WriteLine("Members:");
                foreach (var mem in members)
                {
                    Console.WriteLine(mem.Name);
                }

                Console.WriteLine();
                var methods = item.GetMethods();
                Console.WriteLine("Methods:");
                foreach (var met in methods)
                {
                    Console.WriteLine(met.Name);
                }

                var props = from c in members
                            where (!(methods.Select(d => d.Name).Contains(c.Name)) && !c.Name.ToLower().Contains(".ctor"))
                            select c;
                
                Console.WriteLine();
                Console.WriteLine("Properties only:");
                foreach (var prop in props)
                {
                    Console.WriteLine(prop.Name);
                }

            }

            Console.ReadLine();
        }
    }
}
