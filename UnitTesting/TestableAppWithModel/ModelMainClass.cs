using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestableAppWithModel
{
    public class ModelMainClass
    {
        public IModelClass Person { get; private set; }

        public DateTime Date { get; set; }

        public ModelMainClass()
        {
            Person = new ModelClass();
            Date = DateTime.Now;
        }

        public ModelMainClass(IModelClass person)
        {
            Person = person;
            Date = DateTime.Now;
        }

        public bool F()
        {
            return Person.ValidateModel();
        }
    }
}
