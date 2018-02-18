using Export.Algorithm;
using PluginsAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportAlgorithmUnitTest
{
    internal class TestIntervalCalculator : IntervalsCalculator
    {
        public List<int> m_durations = new List<int>();
        public List<int> m_intervals = new List<int>();

        internal void FillTestContractions()
        {
            if (m_detectedContractions == null)
                m_detectedContractions = new List<PluginContraction>();

            long nLastEndCTR = 0;
            Random randDuration = new Random();
            Random randInterval = new Random();
            for (int i = 0; i < 50; i++)
            {
                int duration = randDuration.Next(27, 167);
                int interval = randInterval.Next(21, 55);
                var ctr = new PluginContraction()
                {
                    StartTime = nLastEndCTR + interval,
                    Id = i,
                    EventType = ArtifactType.Contraction,
                    EndTime = nLastEndCTR + interval + duration,
                    IsStrikedOut = false,
                    PeakTime = nLastEndCTR + interval + (duration / 2)
                };

                nLastEndCTR = ctr.EndTime;
                m_durations.Add(duration);
                m_intervals.Add(interval);
                m_detectedContractions.Add(ctr);
            }
        }

        internal long GetCTRStartTime(int ctrId)
        {
            return m_detectedContractions[ctrId].StartTime;
        }

        internal long GetCTREndTime(int ctrId)
        {
            return m_detectedContractions[ctrId].EndTime;
        }

        internal List<String> GetContractionDurationRange(long start, long end)
        {
            return GetContractionDurationRange(m_detectedContractions, start, end);
        }
    }
}
