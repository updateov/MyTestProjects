using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.Algorithm
{
    public class CalculatedInterval
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int IntervalDuration { get; set; } // In minutes!!

        public int IntervalID { get; set; }
        public int ExportID { get; set; }

        public String ContractionIntervalRange { get; set; }
        public String ContractionInterval { get; set; }
        public double MeanContractionInterval { get; set; }
        public String ContractionDurationRange { get; set; }
        public String OriginalContractionDurationRange { get; set; }
        public String ContractionIntensityRange { get; set; }
        public String OriginalContractionIntensityRange { get; set; }

        // DO NOT USE TotalNumOfContractions and NumOfContractions setters  - we load NumOfContractions from DB and perform logic on it
        public int TotalNumOfContractions { get; internal set; }
        public int NumOfContractions 
        {
            get 
            { 
                double mid = ((double)(TotalNumOfContractions * 10)) / IntervalDuration;
                return (int)Math.Round(mid); 
            }
            
            set 
            { 
                TotalNumOfContractions = (value * IntervalDuration) / 10; 
            } 
        }

        public int NumOfLongContractions { get; set; }
        public int MeanMontevideoUnits { get; set; }
        public int MeanBaseline { get; set; }
        public int MeanBaselineVariability { get; set; }
        public int NumOfAccelerations { get; set; }
        public int NumOfDecelerations { get; set; }
        public int NumOfEarlyDecelerations { get; set; }
        public int NumOfVariableDecelerations { get; set; }
        public int NumOfLateDecelerations { get; set; }
        public int NumOfProlongedDecelerations { get; set; }
        public int NumOfOtherDecelerations { get; set; }

        public CalculatedInterval(int intervalDuration)
        {
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;
            
            IntervalID = -1;
            ExportID = -1;
            IntervalDuration = intervalDuration;
        }

        /// <summary>
        /// Resets all the calculated data.
        /// StartTime, EndTime, IntervalDuration, IntervalID, ExportID remain unchanged
        /// </summary>
        public void ResetData()
        {
            ContractionIntervalRange = String.Empty;
            ContractionIntensityRange = String.Empty;
            OriginalContractionIntensityRange = String.Empty;
            ContractionDurationRange = String.Empty;
            OriginalContractionDurationRange = String.Empty;
            MeanContractionInterval = 0;
            NumOfContractions = 0;
            NumOfLongContractions = 0;
            MeanMontevideoUnits = 0;
            MeanBaseline = 0;
            MeanBaselineVariability = 0;
            NumOfAccelerations = 0;
            NumOfDecelerations = 0;
            NumOfEarlyDecelerations = 0;
            NumOfVariableDecelerations = 0;
            NumOfLateDecelerations = 0;
            NumOfProlongedDecelerations = 0;
            NumOfOtherDecelerations = 0;
        }
    }
}
