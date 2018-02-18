using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.Entities
{
    public class CalculatedCheckboxGroupConcept : BaseConcept
    {
        public List<CalculatedCheckboxGroupConceptItem> Items { get; set; }

        public CalculatedCheckboxGroupConcept()
        {
            Items = new List<CalculatedCheckboxGroupConceptItem>();
        }    
    }

    public class CalculatedCheckboxGroupConceptItem
    {
        public int Id { get; set; }
        public int Ooc { get; set; }
        public string Value { get; set; }
        public string CalculatedValue { get; set; }
        public int OrderNumber { get; set; }
        public bool IsGrouping { get; set; }
    }
}
