using System;
using System.Xml.Serialization;
namespace Export.Entities
{
    [Serializable]
    public abstract class BaseConcept 
    {
        public int Id { get; set; }
        public int OOC { get; set; }
        public string Name { get; set; }
        public int OrderNumber { get; set; }
        public object Value { get; set; }
        public object OriginalValue { get; set; }
 
        public BaseConcept()
        {
            Name = String.Empty;
        }    
    }
}

