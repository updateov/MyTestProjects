using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLRead
{
    class Program
    {
        static void Main(string[] args)
        {
            XElement xElemPatients = new XElement("patients");
            xElemPatients.SetAttributeValue("version", "01.02");
            xElemPatients.SetAttributeValue("user", "CALMLINK");

            for (int i = 0; i < 10; i++)
            {
                XElement xElemRequest = new XElement("request");
                xElemRequest.SetAttributeValue("key", "ep_" + i + "_b");
                xElemRequest.SetAttributeValue("tracing", "tracing");
                xElemPatients.Add(xElemRequest);
            }

            var x = (from c in xElemPatients.Elements("request")
                    select c.Attribute("key").Value).ToList();

            if (x.Contains("ep_5_b"))
                Console.WriteLine("ep_5_b exists");

            x.Add("myKey");
            if (x.Contains("myKey"))
                Console.WriteLine("myKey exists");

            Console.ReadLine();
        }
    }
}
