using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestableAppWithModel
{
    public interface IModelClass
    {
        String Name { get; set; }
        int Age { get; set; }

        bool ValidateModel();
    }
}
