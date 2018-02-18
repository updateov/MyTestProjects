using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PatternsAddOnManager;
using System.Globalization;
using System.Reflection;

namespace PatternsAddOnWinformTestApp
{
    public class XMLWriter
    {
        public static XElement WriteBaselineXML(Artifact item, DateTime AbsoluteStart)
        {
            Baseline blItem = item.ArtifactData as Baseline;
            var value = new XElement("event",
                            new XAttribute("start", ((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("peak", "-1"),
                            new XAttribute("end", ((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("contraction", "-1"),
                            new XAttribute("type", "9"),
                            new XAttribute("y1", blItem.Y1.ToString("0.000000", CultureInfo.InvariantCulture)),
                            new XAttribute("y2", blItem.Y2.ToString("0.000000", CultureInfo.InvariantCulture)));

            return value;
        }

        public static XElement WriteContractionXML(Artifact item, DateTime AbsoluteStart)
        {
            Contraction ctrItem = item.ArtifactData as Contraction;
            XElement value = new XElement("contraction",
                                new XAttribute("start", ((int)((item.StartTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("peak", ((int)((ctrItem.PeakTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("end", ((int)((item.EndTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture)));

            return value;
        }

        public static XElement WriteAccelerationXML(Artifact item, DateTime AbsoluteStart)
        {
            Acceleration acItem = item.ArtifactData as Acceleration;
            var value = new XElement("event",
                            new XAttribute("start", ((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("peak", ((int)((acItem.PeakTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("end", ((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("contraction", "-1"),
                            new XAttribute("type", "1"),
                            new XAttribute("y1", "0.000"),
                            new XAttribute("y2", "0.000"));

            return value;
        }

        public static XElement WriteDecelerationXML(Artifact item, DateTime AbsoluteStart)
        {
            Deceleration decItem = item.ArtifactData as Deceleration;
            int decelType = ArtifactsHelper.GetDecelType(item);
            var value = new XElement("event",
                            new XAttribute("start", ((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("peak", ((int)((decItem.PeakTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("end", ((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("contraction", "-1"),
                            new XAttribute("type", decelType.ToString()),
                            new XAttribute("y1", "0.000"),
                            new XAttribute("y2", "0.000"));

            return value;
        }

        public static void FlushToArchiveFormat(String xmlPath, String resultXmlPath, List<String> ResultsDAT, DateTime absoluteStart)
        {
            XElement XML = null;
            List<byte> UPList = new List<byte>();
            List<byte> FHRList = new List<byte>();
            XML = XMLHelper.LoadTracingsFromXML(xmlPath, UPList, FHRList);
            FlushToArchiveFormat(resultXmlPath, absoluteStart, ResultsDAT, UPList, FHRList);
        }

        public static void FlushToArchiveFormat(String xmlPath, DateTime absoluteStart, List<String> ResultsDAT, List<byte> UPList, List<byte> FHRList)
        {
            String up = Convert.ToBase64String(UPList.ToArray());
            String fhr = Convert.ToBase64String(FHRList.ToArray());
            String resultsDATStr = String.Empty;
            foreach (var item in ResultsDAT)
            {
                resultsDATStr += "\r\n" + item;
            }

            byte[] resultsArray = Encoding.ASCII.GetBytes(resultsDATStr.Trim());
            String bytesStr = String.Empty;
            foreach (var item in resultsArray)
            {
                bytesStr += item.ToString() + "\r\n";
            }

            String artifacts = Convert.ToBase64String(resultsArray);
            DateTime now = absoluteStart;
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            String verStr = String.Format("{0:00}.{1:00}.{2:00}", ver.Major, ver.Minor, ver.Build);
            XElement outXML = new XElement("patternsarchive",
                                            new XAttribute("patternengine", verStr),
                                            new XAttribute("backupengine", "1.0"),
                                            new XAttribute("configuration", String.Empty),
                                        new XElement("visit",
                                                        new XAttribute("dischargetime", String.Empty),
                                                        new XAttribute("archivetime", now.Year.ToString() + "-" + now.Month.ToString() + "-" + now.Day.ToString() + " " + now.Hour.ToString() + ":" + now.Minute.ToString() + ":" + now.Second.ToString()),
                                                        new XAttribute("dob", String.Empty),
                                                        new XAttribute("age", String.Empty),
                                                        new XAttribute("edd", now.Year.ToString() + "-" + now.Month.ToString() + "-" + now.Day.ToString()),
                                                        new XAttribute("ga", String.Empty),
                                                        new XAttribute("fetuscount", "1"),
                                                    new XElement("patient",
                                                                    new XAttribute("patientid", "123456"),
                                                                    new XAttribute("accountno", "n/a"),
                                                                    new XAttribute("lastname", "n/a"),
                                                                    new XAttribute("firstname", "n/a")),
                                                    new XElement("data",
                                                                new XElement("tracings", new XAttribute("starttime", now.Year.ToString() + "-" + now.Month.ToString() + "-" + now.Day.ToString() + " " + now.Hour.ToString() + ":" + now.Minute.ToString() + ":" + now.Second.ToString()),
                                                                            new XElement("tracing", new XAttribute("type", "up"),
                                                                                        new XElement("segment",
                                                                                                        new XAttribute("start", "0"),
                                                                                                        new XAttribute("compress", "False"),
                                                                                                        new XAttribute("data", up))),
                                                                            new XElement("tracing", new XAttribute("type", "fhr1"),
                                                                                        new XElement("segment",
                                                                                                        new XAttribute("start", "0"),
                                                                                                        new XAttribute("compress", "False"),
                                                                                                        new XAttribute("data", fhr)))),
                                                                new XElement("patterns",
                                                                                new XAttribute("compress", "False"),
                                                                                new XAttribute("data", artifacts)))));

            String outPath = xmlPath;
            outPath = outPath.Remove(outPath.Length - 4);
            outPath += "-archive.xml";
            outXML.Save(outPath);
        }

    }
}
