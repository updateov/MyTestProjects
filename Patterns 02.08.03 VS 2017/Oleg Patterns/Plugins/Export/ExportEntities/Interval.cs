using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Export.Entities
{
    [Serializable]
    public class Interval
    {
        public int IntervalId { get; set; }
        public int ExportId { get; set; }
        public string LoginName { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int IntervalDuration { get; set; } // In minutes!!

        [XmlElement("ComboConcept", typeof(ComboConcept))]
        [XmlElement("ComboMultiValueConcept", typeof(ComboMultiValueConcept))]
        [XmlElement("DoubleConcept", typeof(DoubleConcept))]
        [XmlElement("IntConcept", typeof(IntConcept))]
        [XmlElement("StringConcept", typeof(StringConcept))]
        [XmlElement("CalculatedComboConcept", typeof(CalculatedComboConcept))]
        [XmlElement("CalculatedCheckboxGroupConcept", typeof(CalculatedCheckboxGroupConcept))]
        public List<BaseConcept> Concepts { get; set; }
        
        public Interval()
        {
            IntervalId = -1;
            ExportId = -1;
            LoginName = String.Empty;

            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;
            IntervalDuration = -1;

            Concepts = new List<BaseConcept>();
        }   
        
        public void RemoveSubItems()
        {
            foreach(BaseConcept concept in Concepts)
            {
                if(concept is CalculatedCheckboxGroupConcept)
                {
                    ((CalculatedCheckboxGroupConcept)concept).Items.Clear();
                }

                if (concept is CalculatedComboConcept)
                {
                    ((CalculatedComboConcept)concept).Items.Clear();
                }

                if (concept is ComboConcept)
                {
                    ((ComboConcept)concept).Items.Clear();
                }

                if (concept is ComboMultiValueConcept)
                {
                    ((ComboMultiValueConcept)concept).Items.Clear();
                }
            }
        }       
    }

    public class IntervaIdComparer : IEqualityComparer<Interval>
    {
        bool IEqualityComparer<Interval>.Equals(Interval x, Interval y)
        {
            return x.IntervalId.Equals(y.IntervalId);
        }

        int IEqualityComparer<Interval>.GetHashCode(Interval obj)
        {
            if (Object.ReferenceEquals(obj, null))
                return 0;

            return obj.IntervalId.GetHashCode() + obj.IntervalId;
        }
    }
}
