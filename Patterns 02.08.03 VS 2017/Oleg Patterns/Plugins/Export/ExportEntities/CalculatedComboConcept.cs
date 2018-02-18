using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.Entities
{
    public class CalculatedComboConcept : BaseConcept
    {
        public List<CalculatedComboConceptItem> Items { get; set; }

        public CalculatedComboConcept()
        {
            Items = new List<CalculatedComboConceptItem>();
        }
    }

    public class CalculatedComboConceptItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int OrderNumber {get; set; }
    }
}
