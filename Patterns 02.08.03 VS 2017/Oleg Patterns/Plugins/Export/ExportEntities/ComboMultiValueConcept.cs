using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.Entities
{
    public class ComboMultiValueConcept : BaseConcept
    {
        public List<ComboMultiValueConceptItem> Items { get; set; }

        public ComboMultiValueConcept()
        {
            Items = new List<ComboMultiValueConceptItem>();
        }
    }

    public class ComboMultiValueConceptItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int OrderNumber { get; set; }
    }
}
