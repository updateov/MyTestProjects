using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Engine.Data
{
    public class DATWriter
    {
        public static String WriteBaselineDAT(DetectionArtifact item, DateTime AbsoluteStart)
        {
            Baseline blItem = item as Baseline;
            StringBuilder value = new StringBuilder(255);

            value.Append("EVT|");

            /* 00 */
            value.Append("9");  // event::tbaseline
            value.Append("|");
            /* 01 */
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 02 */
            value.Append(String.Empty); // Peak time
            value.Append("|");
            /* 03 */
            value.Append(((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 04 */
            value.Append(blItem.Y1.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 05 */
            value.Append(blItem.Y2.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 06 */
            value.Append(String.Empty); // Contraction start
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(String.Empty); // Strikeout
            value.Append("|");
            /* 09 */
            value.Append(String.Empty); // Confidence
            value.Append("|");
            /* 10 */
            value.Append(String.Empty); // Repair
            value.Append("|");
            /* 11 */
            value.Append(String.Empty); // Height
            value.Append("|");
            /* 12 */
            value.Append(blItem.BaselineVariability.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 13 */
            value.Append(String.Empty); // Peak value
            value.Append("|");
            /* 14 */
            value.Append(String.Empty); // Non reassuring features ?
            value.Append("|");
            /* 15 */
            value.Append(String.Empty); // Variable decel
            value.Append("|");
            /* 16 */
            value.Append(String.Empty); // Lag
            value.Append("|");
            /* 17 */
            value.Append(String.Empty); // Non reassuring features
            value.Append("|");
            /* 18 */
            value.Append(String.Empty); // Non Interpretable
            value.Append("|");
            /* 19 */
            value.Append(String.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        public static String WriteContractionDAT(DetectionArtifact item, DateTime AbsoluteStart)
        {
            Contraction ctrItem = item as Contraction;
            StringBuilder value = new StringBuilder(255);

            value.Append("CTR|");
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            value.Append(((int)((ctrItem.PeakTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            value.Append(((int)((item.EndTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            value.Append("y"); // Is final
            value.Append("|");
            value.Append("n"); // Is Strikeout
            value.Append("|");
            value.Append(item.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        public static String WriteAccelerationDAT(DetectionArtifact item, DateTime AbsoluteStart)
        {
            Acceleration acItem = item as Acceleration;
            StringBuilder value = new StringBuilder(255);

            value.Append("EVT|");

            /* 00 */
            value.Append("1"); // event::tacceleration
            value.Append("|");
            /* 01 */
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 02 */
            value.Append(((int)((acItem.PeakTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 03 */
            value.Append(((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 04 */
            value.Append(String.Empty); // Y1
            value.Append("|");
            /* 05 */
            value.Append(String.Empty); // Y2
            value.Append("|");
            /* 06 */
            value.Append(String.Empty); // Contraction start
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(String.Empty); // Strikeout
            value.Append("|");
            /* 09 */
            value.Append(acItem.Confidence.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 10 */
            value.Append(acItem.Repair.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 11 */
            value.Append(acItem.Height.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 12 */
            value.Append(String.Empty); // Baseline variability
            value.Append("|");
            /* 13 */
            value.Append(acItem.PeakValue.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 14 */
            value.Append(String.Empty); // Non reassuring features ?
            value.Append("|");
            /* 15 */
            value.Append(String.Empty); // Variable decel
            value.Append("|");
            /* 16 */
            value.Append(String.Empty); // Lag
            value.Append("|");
            /* 17 */
            value.Append(String.Empty); // Non reassuring features
            value.Append("|");
            /* 18 */
            if (acItem.IsNonInterpretable) value.Append("y");
            value.Append("|");
            /* 19 */
            value.Append(String.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        public static String WriteDecelerationDAT(DetectionArtifact item, DateTime AbsoluteStart)
        {
            Deceleration decItem = item as Deceleration;
            StringBuilder value = new StringBuilder(255);

            value.Append("EVT|");

            /* 00 */
            value.Append(GetDecelType(item).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 01 */
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 02 */
            value.Append(((int)((decItem.PeakTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 03 */
            value.Append(((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 04 */
            value.Append(String.Empty); // Y1
            value.Append("|");
            /* 05 */
            value.Append(String.Empty); // Y2
            value.Append("|");
            /* 06 */
            value.Append(decItem.ContractionStart.HasValue ? ((int)((decItem.ContractionStart.Value - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture) : String.Empty);
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(String.Empty); // Strikeout
            value.Append("|");
            /* 09 */
            value.Append(decItem.Confidence.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 10 */
            value.Append(decItem.Repair.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 11 */
            value.Append(decItem.Height.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 12 */
            value.Append(String.Empty); // Baseline variability
            value.Append("|");
            /* 13 */
            value.Append(decItem.PeakValue.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 14 */
            value.Append("|");
            /* 15 */
            if (decItem.DecelerationCategory.Equals("Variable"))
                value.Append("y");
            value.Append("|");
            /* 16 */
            value.Append("-1"); // Lag
            value.Append("|");
            /* 17 */
            value.Append(GetDecelNonReassuring(item).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 18 */
            if (decItem.IsNonInterpretable)
                value.Append("y");
            value.Append("|");
            /* 19 */
            value.Append(String.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        private static int GetDecelType(DetectionArtifact item)
        {
            Deceleration artifact = item as Deceleration;
            int eventType = 0;

            if (artifact.IsNonAssociatedDeceleration)
            {
                eventType = 7; // event::tnadeceleration  ***
            }
            else if (artifact.IsEarlyDeceleration)
            {
                eventType = 3; // event::tearly ***
            }
            else if (artifact.IsLateDeceleration)
            {
                eventType = 6; // event::tlate ***	
            }
            else if (artifact.IsVariableDeceleration)
            {
                if (artifact.HasProlongedNonReassuringFeature)
                {
                    eventType = 14; // event::tprolonged ***
                }
                else if (artifact.HasNonReassuringFeature)
                {
                    eventType = 5; // event::tatypical ***
                }
                else
                {
                    eventType = 4; // event::ttypical ***
                }
            }
            else
            {
                throw new InvalidOperationException("Unable to match the deceleration type to an engine type");
            }

            return eventType;
        }

        private static int GetDecelNonReassuring(DetectionArtifact item)
        {
            Deceleration artifact = item as Deceleration;
            int atypicalValue = 0;
            if (artifact.HasBiphasicNonReassuringFeature)
            {
                atypicalValue |= 1;
            }
            if (artifact.HasLossRiseNonReassuringFeature)
            {
                atypicalValue |= 2;
            }
            if (artifact.HasLossVariabilityNonReassuringFeature)
            {
                atypicalValue |= 4;
            }
            if (artifact.HasLowerBaselineNonReassuringFeature)
            {
                atypicalValue |= 8;
            }
            if (artifact.HasProlongedSecondRiseNonReassuringFeature)
            {
                atypicalValue |= 16;
            }
            if (artifact.HasSixtiesNonReassuringFeature)
            {
                atypicalValue |= 32;
            }
            if (artifact.HasSlowReturnNonReassuringFeature)
            {
                atypicalValue |= 64;
            }

            return atypicalValue;
        }
    }
}
