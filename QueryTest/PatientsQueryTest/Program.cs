using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsQueryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Patient> patients = new List<Patient>()
            {
                new Patient() { ID = 1, Name = "AAA"},
                new Patient() { ID = 2, Name = "BBB"},
                new Patient() { ID = 3, Name = "CCC"},
                new Patient() { ID = 4, Name = "DDD"},
                new Patient() { ID = 5, Name = "XXX"}
            };

            List<int> locations = new List<int>()
            {
                1, 
                2,
                3,
                6,
                7,
                8
            };

            var uncharted = locations.Where(newLocation => !patients.Select(currentPatient => currentPatient.ID).Contains(newLocation));
            foreach (var item in uncharted)
            {
                patients.Add(new Patient() { ID = -item, Name = "NP-" + item.ToString() });
                Console.WriteLine(String.Format("Location = {0}", -item));
            }

            Console.WriteLine("\n\nCurrent patients:");
            foreach (var item in patients)
            {
                Console.WriteLine(String.Format("Patient name = {0}, ID = {1}", item.Name, item.ID));
            }

            Console.ReadLine();

        }
    }

    public class Patient
    {
        public int ID { get; set; }
        public String Name { get; set; }
    }
}
