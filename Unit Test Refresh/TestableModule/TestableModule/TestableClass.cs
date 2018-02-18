using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestableModule
{
    public class TestableClass
    {
        public TestableClass()
        {
        }

        public String Reverse(String inStr)
        {
            String toRet = String.Empty;
            foreach (var item in inStr)
            {
                toRet = item + toRet;
            }

            return toRet;
        }
    }
}
