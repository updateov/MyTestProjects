using PluginsAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatternsPluginsCommon
{
    public class DATFileReader
    {
        public static PluginDetectionArtifact ReadDAT(String line, long absoluteStart)
        {
            var eventData = line.Split('|');
            if (eventData[0].Equals("CTR"))
                return ReadContractionDAT(eventData, absoluteStart);
            else
            {
                switch (eventData[1])
                {
                    case "9":
                        return ReadBaselineDAT(eventData, absoluteStart);
                    case "1":
                        return ReadAccelerationDAT(eventData, absoluteStart);
                    default:
                        return ReadDecelerationDAT(eventData, absoluteStart);
                }
            }
        }

        private static PluginDetectionArtifact ReadContractionDAT(String[] item, long absoluteStart)
        {
            PluginContraction toRet = new PluginContraction();
            toRet.EventType = ArtifactType.Contraction;
            long time;

            // Start time
            bool bSucc = Int64.TryParse(item[1], out time);
            toRet.StartTime = time + absoluteStart;

            // Peak time
            bSucc = Int64.TryParse(item[2], out time);
            toRet.PeakTime = time + absoluteStart;

            // End time
            bSucc = Int64.TryParse(item[3], out time);
            toRet.EndTime = time + absoluteStart;

            // Strikeout
            toRet.IsStrikedOut = item[5] != null && item[5].ToLower().Equals("y");

            // Id
            int id;
            bSucc = Int32.TryParse(item.Last(), out id);
            toRet.Id = id;

            return toRet;
        }

        public static PluginDetectionArtifact ReadBaselineDAT(String[] item, long absoluteStart)
        {
            PluginBaseline toRet = new PluginBaseline();
            toRet.EventType = ArtifactType.Baseline;
            long time;

            // Start time
            bool bSucc = Int64.TryParse(item[2], out time);
            toRet.StartTime = (time / 4) + absoluteStart;

            // End time
            bSucc = Int64.TryParse(item[4], out time);
            toRet.EndTime = (time / 4) + absoluteStart;

            Double val;
            // Y1
            bSucc = Double.TryParse(item[5], out val);
            toRet.Y1 = val;

            // Y2
            bSucc = Double.TryParse(item[6], out val);
            toRet.Y2 = val;

            // Baseline Variability
            bSucc = Double.TryParse(item[13], out val);
            toRet.BaselineVariability = val;

            // Id
            int id;
            bSucc = Int32.TryParse(item.Last(), out id);
            toRet.Id = id;

            return toRet;
        }

        public static void FillAccelDecelValues(PluginAcceleration toFill, String[] item, long absoluteStart)
        {
            long time;

            // Start time
            bool bSucc = Int64.TryParse(item[2], out time);
            toFill.StartTime = (time / 4) + absoluteStart;

            // Peak time
            bSucc = Int64.TryParse(item[3], out time);
            toFill.PeakTime = (time / 4) + absoluteStart;

            // End time
            bSucc = Int64.TryParse(item[4], out time);
            toFill.EndTime = (time / 4) + absoluteStart;

            // Strikeout
            toFill.IsStrikedOut = item[9] != null && item[9].ToLower().Equals("y");

            double val;

            // Confidence
            bSucc = Double.TryParse(item[10], out val);
            toFill.Confidence = val;

            // Repair
            bSucc = Double.TryParse(item[11], out val);
            toFill.Repair = val;

            // Height
            bSucc = Double.TryParse(item[12], out val);
            toFill.Height = val;

            // Peak Value
            bSucc = Double.TryParse(item[14], out val);
            toFill.PeakValue = val;

            // Id
            int id;
            bSucc = Int32.TryParse(item.Last(), out id);
            toFill.Id = id;

            toFill.IsNonInterpretable = item[19] != null && item[19].Equals("y");
        }

        public static PluginDetectionArtifact ReadAccelerationDAT(String[] item, long absoluteStart)
        {
            PluginAcceleration toRet = new PluginAcceleration();
            toRet.EventType = ArtifactType.Acceleration;
            FillAccelDecelValues(toRet, item, absoluteStart);
            return toRet;
        }

        public static PluginDetectionArtifact ReadDecelerationDAT(String[] item, long absoluteStart)
        {
            PluginDeceleration toRet = new PluginDeceleration();
            toRet.EventType = ArtifactType.Deceleration;

            int decelType;
            bool bSucc = Int32.TryParse(item[1], out decelType);
            DecelerationCategories category = GetDecelType(decelType);
            toRet.DecelerationCategory = category;

            FillAccelDecelValues(toRet, item, absoluteStart);

            toRet.LateTiming = item[15] != null && item[15].Equals("y");

            return toRet;
        }

        private static DecelerationCategories GetDecelType(int decelType)
        {
            DecelerationCategories toRet = DecelerationCategories.None;
            switch (decelType)
            {
                case 3:
                    toRet = DecelerationCategories.Early;
                    break;
                case 4:
                case 5:
                    toRet = DecelerationCategories.Variable;
                    break;
                case 14:
                    toRet = DecelerationCategories.Prolonged;
                    break;
                case 6:
                    toRet = DecelerationCategories.Late;
                    break;
                case 7:
                    toRet = DecelerationCategories.NonAssociated;
                    break;
                default:
                    throw new InvalidOperationException("Unable to match the deceleration type to an engine type");
            }

            return toRet;
        }
    }
}
