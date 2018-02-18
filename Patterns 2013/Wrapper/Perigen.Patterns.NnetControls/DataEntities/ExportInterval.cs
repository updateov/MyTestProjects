using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Perigen.Patterns.NnetControls.DataEntities
{
    [Serializable]
    [XmlType("Interval")]
    public class ExportInterval 
    {
        public int IntervalId { get; set; }
        public int ExportId { get; set; }
        public string LoginName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int IntervalDuration { get; set; }

        public ExportInterval() { Concepts = new List<BaseExportEntity>(); }

        [XmlElement("IntConcept", typeof(ExportIntEntity))]
        [XmlElement("DoubleConcept", typeof(ExportDoubleEntity))]
        [XmlElement("StringConcept", typeof(ExportStringEntity))]
        [XmlElement("ComboConcept", typeof(ExportComboEntity))]
        public List<BaseExportEntity> Concepts { get; set; }       
    }
}
