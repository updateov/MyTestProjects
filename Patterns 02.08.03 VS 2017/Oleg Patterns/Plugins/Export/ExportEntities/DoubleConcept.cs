using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Export.Entities
{    
    public class DoubleConcept : BaseConcept
    {
        public double Min { get; set; }
        public double Max { get; set; }

        public DoubleConcept()            
        {                        
        }
    }
}
