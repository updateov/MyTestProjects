using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PatternsAddOnWinformTestApp
{
    public class XMLHelper
    {
        //Core recursion function
        public static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        public static String GetXMLValByLocalName(XElement xml, String localName)
        {
            var toRet = from c in xml.Elements()
                        where c.Name.LocalName.Equals(localName)
                        select c.Value;

            if (toRet == null || toRet.Count() <= 0)
                return String.Empty;

            return toRet.ElementAt(0);
        }

        public static XElement GetXMLElementByLocalName(XElement xml, String localName)
        {
            var toRet = from c in xml.Elements()
                        where c.Name.LocalName.Equals(localName)
                        select c;

            if (toRet == null || toRet.Count() <= 0)
                return null;

            return toRet.ElementAt(0);
        }

        public static XElement LoadTracingsFromXML(String xmlPath, List<byte> UPList, List<byte> FHRList)
        {
            XElement XML = XElement.Load(xmlPath);
            LoadTracingsFromXMLInternal(UPList, FHRList, XML);
            return XML;
        }

        private static void LoadTracingsFromXMLInternal(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            if (XML.Elements("fhr-sample").Count() > 0 && XML.Elements("up-sample").Count() > 0)
                LoadFamous6(UPList, FHRList, XML);

            if (XML.Elements("visit").Count() > 0)
                LoadRawTracings(UPList, FHRList, XML);
        }

        private static void LoadFamous6(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            var FHRElems = from b in XML.Descendants("fhr-sample")
                           select b.Attribute("value").Value;

            var UPElems = from c in XML.Descendants("up-sample")
                          select c.Attribute("value").Value;

            foreach (var item in UPElems)
            {
                int ind = item.IndexOf(".");
                var intPart = ind > 1 ? item.Remove(ind) : item;
                byte val;
                if (!Byte.TryParse(intPart, out val))
                    continue;

                UPList.Add(val);
            }

            foreach (var item in FHRElems)
            {
                int ind = item.IndexOf(".");
                var intPart = ind > 1 ? item.Remove(ind) : item;
                byte val;
                if (!Byte.TryParse(intPart, out val))
                    continue;

                FHRList.Add(val);
            }
        }

        public static String PrepareXML(List<byte> upSubList, List<byte> fhrSubList, DateTime dateTime, DateTime lastDetected)
        {
            var upString = Convert.ToBase64String(upSubList.ToArray());
            var fhrString = Convert.ToBase64String(fhrSubList.ToArray());

            XNamespace i = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace defaultNamespace = XNamespace.Get("http://schemas.datacontract.org/2004/07/PatternsAddOnManager");
            XElement xml = new XElement(defaultNamespace + "TracingData",
                            new XAttribute(XNamespace.Xmlns + "i", i.NamespaceName),
                        new XElement("Fhr", fhrString),
                        new XElement("PreviousDetectededEndTime", lastDetected),
                        new XElement("StartTime", dateTime),
                        new XElement("Up", upString));

            var toRet = xml.ToString();
            toRet = toRet.Replace("xmlns=\"\"", "");
            return toRet;
        }

        public static void LoadRawTracings(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            var upElem = from b in XML.Descendants("tracing")
                         where b.Attribute("type").Value.Equals("up")
                         select b.Element("segment");

            String upStr = upElem.Attributes("data").ElementAt(0).Value;
            var upArr = Convert.FromBase64String(upStr);
            UPList.AddRange(upArr);

            var fhrElem = from b in XML.Descendants("tracing")
                          where b.Attribute("type").Value.Equals("fhr1")
                          select b.Element("segment");

            String fhrStr = fhrElem.Attributes("data").ElementAt(0).Value;
            var fhrArr = Convert.FromBase64String(fhrStr);
            FHRList.AddRange(fhrArr);
        }

    }
}
