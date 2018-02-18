using Bridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainEntry
{
    public class MainEntryClass
    {
        public MainEntryClass()
        {
        }

        public int GetNum(int num)
        {
            BridgeClass bridge = new BridgeClass();
            return bridge.GetNumBridge(num);
        }
    }
}
