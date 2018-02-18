using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.Entities
{
    public class ComboConcept : BaseConcept
    {
        public List<string> Items { get; set; }

        public ComboConcept()
        {
            Items = new List<string>();
            Value = String.Empty;
            OriginalValue = String.Empty;
        }
    }
}

