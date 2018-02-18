using System.Collections.Generic;

namespace InterfaceTest
{
    public class Patient
    {
        public List<Contraction> Contractions { get; set; }
        public List<Fetus> Fetuses { get; set; }

        public Patient()
        {
            Contractions = new List<Contraction>();
            Fetuses = new List<Fetus>();
        }
    }
}
