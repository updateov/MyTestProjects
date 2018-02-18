using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsData
{
    public class Patient
    {
        public String Key { get; set; }
        public String MRN { get; set; }
        public String BedId { get; set; }
        public String BedName { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Fetuses { get; set; }
        public String GestationalAge { get; set; }

        public String Text { get { return ToString(); } }

        public override string ToString()
        {
            String toRet = BedName + " " + FirstName[0] + LastName[0];
            return toRet;
        }
    }
}
