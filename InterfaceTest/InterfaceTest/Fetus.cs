using System.Collections.Generic;

namespace InterfaceTest
{
    public class Fetus
    {
        public List<Contractility> Contractilities { get; set; }
        public List<IEvent> Events { get; set; }

        public Fetus()
        {
            Contractilities = new List<Contractility>();
            Events = new List<IEvent>();
        }
    }
}
