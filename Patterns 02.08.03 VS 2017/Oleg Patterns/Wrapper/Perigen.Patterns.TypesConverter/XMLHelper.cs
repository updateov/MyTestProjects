using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.BatchProcessor
{
    public class XMLHelper
    {
        public static XElement LoadTracingsFromXML(String xmlPath, List<byte> UPList, List<byte> FHRList)
        {
            XElement XML = XElement.Load(xmlPath);
            LoadTracingsFromXMLInternal(UPList, FHRList, XML);
            return XML;
        }

        private static void LoadTracingsFromXMLInternal(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            if (XML.Elements("fhr-sample").Count() > 0 && XML.Elements("up-sample").Count() > 0)
                LoadTracingsXML(UPList, FHRList, XML);

            if (XML.Elements("visit").Count() > 0)
                LoadArchiveTracings(UPList, FHRList, XML);
        }

        private static void LoadTracingsXML(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            var FHRElems = from b in XML.Descendants("fhr-sample")
                           select b.Attribute("value").Value;

            var UPElems = from c in XML.Descendants("up-sample")
                          select c.Attribute("value").Value;

            foreach (var item in UPElems)
            {
                var intPart = item.IndexOf(".") > 0 ? item.Remove(item.IndexOf(".")) : item;
                byte val;
                if (!Byte.TryParse(intPart, out val))
                    continue;

                UPList.Add(val);
            }

            foreach (var item in FHRElems)
            {
                var intPart = item.IndexOf(".") > 0 ? item.Remove(item.IndexOf(".")) : item;
                byte val;
                if (!Byte.TryParse(intPart, out val))
                    continue;

                FHRList.Add(val);
            }
        }

        private static void LoadArchiveTracings(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            var tracingUpElem = from b in XML.Descendants("tracing")
                         where b.Attribute("type").Value.Equals("up")
                         select b;

            var tracingFhrElem = from b in XML.Descendants("tracing")
                          where b.Attribute("type").Value.Equals("fhr1")
                          select b;

            FillTracings(UPList, tracingUpElem);
            FillTracings(FHRList, tracingFhrElem);

            if (UPList.Count * 4 > FHRList.Count)
                EqualizeLengths(UPList, FHRList);

            if (UPList.Count * 4 < FHRList.Count)
                EqualizeLengths(UPList, FHRList, false);
        }

        private static void EqualizeLengths(List<byte> UPList, List<byte> FHRList, bool bUPLong = true)
        {
            if (bUPLong)
            {
                while (UPList.Count * 4 > FHRList.Count)
                {
                    FHRList.Add(TracingBlock.NoData);
                }
            }
            else
            {
                while (UPList.Count * 4 < FHRList.Count)
                {
                    UPList.Add(TracingBlock.NoData);
                }

                while (UPList.Count * 4 != FHRList.Count)
                {
                    FHRList.Add(TracingBlock.NoData);
                }
            }
        }

        private static void FillTracings(List<byte> curList, IEnumerable<XElement> tracingElem)
        {
            var segments = from c in tracingElem.Elements("segment")
                           select c;

            foreach (var item in segments)
            {
                String startTime = item.Attribute("start").Value;
                int startTimeVal;
                bool bSucc = Int32.TryParse(startTime, out startTimeVal);
                if (startTimeVal > curList.Count)
                    FillGap(curList, startTimeVal);
                else if (startTimeVal > curList.Count)
                    ResolveOverlap(curList, startTimeVal);

                String dataStr = item.Attribute("data").Value;
                var arrToFill = Convert.FromBase64String(dataStr);
                curList.AddRange(arrToFill);

            }
        }

        private static void FillGap(List<byte> curList, int startTimeVal)
        {
            while (startTimeVal > curList.Count)
            {
                curList.Add(TracingBlock.NoData);
            }
        }

        private static void ResolveOverlap(List<byte> curList, int startTimeVal)
        {
            curList.RemoveRange(startTimeVal, curList.Count - startTimeVal);
        }
    }
}
