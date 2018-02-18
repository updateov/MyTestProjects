using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PatternsAddOnManager;
using System.Xml.Linq;

namespace PatternsAddOnWinformTestApp
{
    public class ArtifactsHelper
    {
        public static int GetDecelType(Artifact item)
        {
            switch ((item.ArtifactData as Deceleration).DecelerationCategory)
            {
                case "NonAssociated":
                    return 7;
                case "Early":
                    return 3;
                case "Late":
                    return 6;
                case "Variable":
                    var tmp = (item.ArtifactData as Deceleration).NonReassuringFeatures;
                    if (tmp.Equals("Prolonged"))
                        return 14;  // this.IsVariableDeceleration && (this.NonReassuringFeatures != AtypicalCharacteristics.None);
                    else if (tmp.Equals("None"))
                        return 4;
                    return 5;
                default:
                    return 5;
            }
        }

        public static int GetDecelNonReassuring(Artifact item)
        {
            String valStr = (item.ArtifactData as Deceleration).NonReassuringFeatures;
            int atypicalValue = 0;
            if (valStr.Equals("Biphasic"))
            {
                atypicalValue |= 1;
            }
            if (valStr.Equals("LossRise"))
            {
                atypicalValue |= 2;
            }
            if (valStr.Equals("LossVariability"))
            {
                atypicalValue |= 4;
            }
            if (valStr.Equals("LowerBaseline"))
            {
                atypicalValue |= 8;
            }
            if (valStr.Equals("ProlongedSecondRise"))
            {
                atypicalValue |= 16;
            }
            if (valStr.Equals("Sixties") || (valStr.Equals("Prolonged") && (item.ArtifactData as Deceleration).HasSixtiesNonReassuringFeature))
            {
                atypicalValue |= 32;
            }
            if (valStr.Equals("SlowReturn"))
            {
                atypicalValue |= 64;
            }

            return atypicalValue;
        }
        public static ArtifactType CreateBaselineData(XElement artData)
        {
            var toRet = new PatternsAddOnManager.Baseline() { Category = "Baseline" };
            double nVal;
            if (!Double.TryParse(artData.Element("BaselineVariability").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.BaselineVariability = nVal;

            if (!Double.TryParse(artData.Element("Y1").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Y1 = nVal;

            if (!Double.TryParse(artData.Element("Y2").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Y2 = nVal;
            return toRet;
        }

        public static ArtifactType CreateContractionData(XElement artData)
        {
            var toRet = new PatternsAddOnManager.Contraction() { Category = "Contraction" };
            toRet.PeakTime = GetDateTime(artData.Element("PeakTime").Value);
            int nId;
            if (!Int32.TryParse(artData.Element("Id").Value, out nId) || nId < 0)
                nId = 0;

            toRet.Id = nId;
            return toRet;
        }

        public static ArtifactType CreateAccelerationData(XElement artData)
        {
            var toRet = new PatternsAddOnManager.Acceleration() { Category = "Acceleration" };
            CreateAccelerationDataInternal(artData, toRet);
            return toRet;
        }

        public static void CreateAccelerationDataInternal(XElement artData, PatternsAddOnManager.Acceleration toRet)
        {
            double nVal;
            if (!Double.TryParse(artData.Element("Confidence").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Confidence = nVal;

            if (!Double.TryParse(artData.Element("Height").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Height = nVal;
            toRet.IsNonInterpretable = artData.Element("IsNonInterpretable").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.PeakTime = GetDateTime(artData.Element("PeakTime").Value);
            if (!Double.TryParse(artData.Element("PeakValue").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.PeakValue = nVal;

            if (!Double.TryParse(artData.Element("Repair").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Repair = nVal;
        }

        public static ArtifactType CreateDecelerationData(XElement artData)
        {
            var toRet = new PatternsAddOnManager.Deceleration() { Category = "Deceleration" };
            CreateAccelerationDataInternal(artData, toRet);
            toRet.ContractionStart = GetRefDateTime(artData.Element("ContractionStart").Value);
            toRet.DecelerationCategory = artData.Element("DecelerationCategory").Value;
            toRet.HasSixtiesNonReassuringFeature = artData.Element("HasSixtiesNonReassuringFeature").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.IsEarlyDeceleration = artData.Element("IsEarlyDeceleration").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.IsLateDeceleration = artData.Element("IsLateDeceleration").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.IsNonAssociatedDeceleration = artData.Element("IsNonAssociatedDeceleration").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.IsVariableDeceleration = artData.Element("IsVariableDeceleration").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.NonReassuringFeatures = artData.Element("NonReassuringFeatures").Value;

            return toRet;
        }

        public static DateTime GetDateTime(String dateTime)
        {
            return DateTime.Parse(dateTime);
        }

        public static DateTime? GetRefDateTime(String dateTime)
        {
            if (dateTime.Equals(String.Empty))
                return null;

            return DateTime.Parse(dateTime);
        }

        public static List<Artifact> DeserializeResults(IEnumerable<XElement> newResults)
        {
            var toRet = new List<Artifact>();
            foreach (var item in newResults)
            {
                var curArt = new Artifact();
                String curVal = XMLHelper.GetXMLValByLocalName(item, "StartTime");
                curArt.StartTime = ArtifactsHelper.GetDateTime(curVal);
                curVal = XMLHelper.GetXMLValByLocalName(item, "EndTime");
                curArt.EndTime = ArtifactsHelper.GetDateTime(curVal);
                curVal = XMLHelper.GetXMLValByLocalName(item, "Category");
                curArt.Category = curVal;
                var artData = XMLHelper.GetXMLElementByLocalName(item, "ArtifactData");
                if (artData == null)
                    continue;

                switch (curVal)
                {
                    case "Baseline":
                        curArt.ArtifactData = CreateBaselineData(artData);
                        break;
                    case "Contraction":
                        curArt.ArtifactData = CreateContractionData(artData);
                        break;
                    case "Acceleration":
                        curArt.ArtifactData = CreateAccelerationData(artData);
                        break;
                    case "Deceleration":
                        curArt.ArtifactData = CreateDecelerationData(artData);
                        break;
                    default:
                        break;
                }

                toRet.Add(curArt);
            }

            return toRet;
        }

        public static bool SameArtifactExists(List<Artifact> data, Artifact newData)
        {
            foreach (var item in data)
            {
                if (item.StartTime.Equals(newData.StartTime) &&
                    item.EndTime.Equals(newData.EndTime) &&
                    item.Category == newData.Category)
                    return true;
            }

            return false;
        }

    }
}
