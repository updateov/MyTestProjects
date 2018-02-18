using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class ToBase
    {
        public String BaseStr { get; set; }

        public override string ToString()
        {
            return "BaseStr = " + BaseStr + ", Type = " + this.GetType().ToString();
        }
    }

    public class ToDerived1 : ToBase
    {
        public String DerivedStr1 { get; set; }

        public override string ToString()
        {
            return base.ToString() + ", " + DerivedStr1;
        }
    }

    public class ToDerived2 : ToBase
    {
        public String DerivedStr2 { get; set; }

        public override string ToString()
        {
            return base.ToString() + ", " + DerivedStr2;
        }
    }

    public class ToDerived3 : ToBase
    {
        public String DerivedStr3 { get; set; }

        public override string ToString()
        {
            return base.ToString() + ", " + DerivedStr3;
        }
    }
}
