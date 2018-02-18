using CRIEntities;
using PluginsAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRIAlgorithm
{
    public enum ContractilityClassification
    {
        Unknown = -1,
        Normal = 0,
        Alert = 1,
        Danger = 2
    };

    public class Contractility : PluginDetectedObject
    {
        public ContractilityClassification CRIClassification;
        public CRIEvents EventsCounted { get; set; }

        public long Length { get { return EndTime - StartTime; } }

        public Contractility()
        {
            Id = -1;
            StartTime = 0;
            EndTime = 0;
            CRIClassification = ContractilityClassification.Unknown;
            EventsCounted = new CRIEvents();
        }

        public Contractility(long start, long end, ContractilityClassification classification)
        {
            Id = -1;
            StartTime = start;
            EndTime = end;
            CRIClassification = classification;
            EventsCounted = new CRIEvents();
        }

        public Contractility(Contractility obj)
        {
            Id = obj.Id;
            StartTime = obj.StartTime;
            EndTime = obj.EndTime;
            CRIClassification = obj.CRIClassification;
            EventsCounted = new CRIEvents(obj.EventsCounted);
        }

        public bool Intersects(Contractility contractilityIndex)
        {
            return EndTime >= contractilityIndex.StartTime && StartTime <= contractilityIndex.EndTime;
        }
    }
}
