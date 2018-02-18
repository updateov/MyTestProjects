using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIEntities
{
    public abstract class CalculatedObject
    {
        public int ID { get; set; }

        public CalculatedObject()
        {
            ID = -1;
        }
    }
}
