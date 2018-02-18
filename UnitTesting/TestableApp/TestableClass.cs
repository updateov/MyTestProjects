using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestableApp
{
    public class TestableClass
    {
        public TestableClass()
        {
        }

        public long Calculate(long param1, long param2)
        {
            long toRet = param1 + param2;
            return toRet;
        }

        public long Product(long param1, long param2)
        {
            long toRet = param1 * param2;
            return toRet;
        }
    }
}
