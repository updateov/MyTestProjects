using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIEntities
{
    public class Bed
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Bed()
        {
            Id = -1;
            Name = String.Empty;
        }
    }
}
