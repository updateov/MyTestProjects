using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AttributesInXML
{
    class Program
    {
        static void Main(string[] args)
        {
            XElement elem = new XElement("criAlgorithmSettings",
                new XAttribute("accelsRate", "0"),
                new XAttribute("lateDecelConfidence", "2.0"),
                new XAttribute("lateDecelsRate", "2"),
                new XAttribute("prolongedDecelHeight", "20"),
                new XAttribute("lateAndProlongedDecelsRate", "2"),
                new XAttribute("lateAndLargeAndLongDecelRate", "2"),
                new XAttribute("largeAndLongDecelRate", "3"));

            var t = elem.ToString();
        }
    }
}
