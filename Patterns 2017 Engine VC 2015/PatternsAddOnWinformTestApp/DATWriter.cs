using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PatternsAddOnManager;
using System.Globalization;

namespace PatternsAddOnWinformTestApp
{
    public class DATWriter
    {
        public static string WriteBaselineDAT(Artifact item, DateTime AbsoluteStart)
        {
            Baseline blItem = item.ArtifactData as Baseline;
            StringBuilder value = new StringBuilder(255);

            value.Append("EVT|");

            /* 00 */
            value.Append("9");  // event::tbaseline
            value.Append("|");
            /* 01 */
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 02 */
            value.Append(string.Empty); // Peak time
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
            value.Append(string.Empty); // Contraction start
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(string.Empty); // Strikeout
            value.Append("|");
            /* 09 */
            value.Append(string.Empty); // Confidence
            value.Append("|");
            /* 10 */
            value.Append(string.Empty); // Repair
            value.Append("|");
            /* 11 */
            value.Append(string.Empty); // Height
            value.Append("|");
            /* 12 */
            value.Append(blItem.BaselineVariability.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 13 */
            value.Append(string.Empty); // Peak value
            value.Append("|");
            /* 14 */
            value.Append(string.Empty); // Non reassuring features ?
            value.Append("|");
            /* 15 */
            value.Append(string.Empty); // Variable decel
            value.Append("|");
            /* 16 */
            value.Append(string.Empty); // Lag
            value.Append("|");
            /* 17 */
            value.Append(string.Empty); // Non reassuring features
            value.Append("|");
            /* 18 */
            value.Append(string.Empty); // Non Interpretable
            value.Append("|");
            /* 19 */
            value.Append(string.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.ArtifactData.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        public static string WriteContractionDAT(Artifact item, DateTime AbsoluteStart)
        {
            Contraction ctrItem = item.ArtifactData as Contraction;
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
            value.Append(item.ArtifactData.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        public static string WriteAccelerationDAT(Artifact item, DateTime AbsoluteStart)
        {
            Acceleration acItem = item.ArtifactData as Acceleration;
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
            value.Append(string.Empty); // Y1
            value.Append("|");
            /* 05 */
            value.Append(string.Empty); // Y2
            value.Append("|");
            /* 06 */
            value.Append(string.Empty); // Contraction start
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(string.Empty); // Strikeout
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
            value.Append(string.Empty); // Baseline variability
            value.Append("|");
            /* 13 */
            value.Append(acItem.PeakValue.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 14 */
            value.Append(string.Empty); // Non reassuring features ?
            value.Append("|");
            /* 15 */
            value.Append(string.Empty); // Variable decel
            value.Append("|");
            /* 16 */
            value.Append(string.Empty); // Lag
            value.Append("|");
            /* 17 */
            value.Append(string.Empty); // Non reassuring features
            value.Append("|");
            /* 18 */
            if (acItem.IsNonInterpretable) value.Append("y");
            value.Append("|");
            /* 19 */
            value.Append(string.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.ArtifactData.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        public static string WriteDecelerationDAT(Artifact item, DateTime AbsoluteStart)
        {
            Deceleration decItem = item.ArtifactData as Deceleration;
            StringBuilder value = new StringBuilder(255);

            value.Append("EVT|");

            /* 00 */
            value.Append(ArtifactsHelper.GetDecelType(item).ToString(CultureInfo.InvariantCulture));
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
            value.Append(string.Empty); // Y1
            value.Append("|");
            /* 05 */
            value.Append(string.Empty); // Y2
            value.Append("|");
            /* 06 */
            value.Append(decItem.ContractionStart.HasValue ? ((int)((decItem.ContractionStart.Value - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture) : String.Empty);
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(string.Empty); // Strikeout
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
            value.Append(string.Empty); // Baseline variability
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
            value.Append(ArtifactsHelper.GetDecelNonReassuring(item).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 18 */
            if (decItem.IsNonInterpretable)
                value.Append("y");
            value.Append("|");
            /* 19 */
            value.Append(string.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.ArtifactData.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

    }
}
