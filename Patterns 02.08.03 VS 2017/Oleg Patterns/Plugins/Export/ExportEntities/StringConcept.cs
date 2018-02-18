using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Export.Entities
{    
    public class StringConcept : BaseConcept
    {
        public StringConcept()
        {
            Value = String.Empty;
            OriginalValue = String.Empty;
        }   
    }
}
