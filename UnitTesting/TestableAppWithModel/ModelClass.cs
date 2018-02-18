using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestableAppWithModel
{
    public class ModelClass : IModelClass
    {
        public String Name { get; set; }
        public int Age { get; set; }

        public bool ValidateModel()
        {
            if (Name.Equals(String.Empty))
                return false;

            if (Age < 0 || Age > 100)
                return false;

            return true;
        }
    }
}
