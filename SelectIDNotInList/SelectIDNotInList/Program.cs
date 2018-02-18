
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectIDNotInList
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Structura> struc1 = new List<Structura>()
            {
                new Structura(){ ID = 1, Name = "iasdf1"},
                new Structura(){ ID = 2, Name = "iasdf2"},
                new Structura(){ ID = 3, Name = "iasdf3"},
                new Structura(){ ID = 4, Name = "iasdf4"},
                new Structura(){ ID = 5, Name = "iasdf5"}
            };

            List<Structura> struc2 = new List<Structura>()
            {
                new Structura(){ ID = 1, Name = "iasdf1"},
                new Structura(){ ID = 6, Name = "iasdf6"},
                new Structura(){ ID = 3, Name = "iasdf3"},
                new Structura(){ ID = 8, Name = "iasdf8"},
                new Structura(){ ID = 5, Name = "iasdf5"}
            };

            var added = from c in struc1
                        where !struc2.Select(d => d.ID).Contains(c.ID)
                        select c;

            foreach (var item in added)
            {
                Console.WriteLine("Item: " + item.ID + ", Name: " + item.Name);
            }

            Console.ReadLine();

            struc1.Where(c => !struc2.Select(d => d.ID).Contains(c.ID));
            foreach (var item in added)
            {
                Console.WriteLine("Item: " + item.ID + ", Name: " + item.Name);
            }

            Console.ReadLine();
        }
    }

    public class Structura
    {
        public int ID { get; set; }
        public String Name { get; set; }
    }
}
