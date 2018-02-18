using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatternsAddOnWinformTestApp
{
    public class CommonHelper
    {
        public static List<byte> Sublist(List<byte> data, int index, int length)
        {
            List<byte> toRet = new List<byte>();
            if (index >= data.Count)
                return toRet;

            for (int i = index; i < index + length && i < data.Count; i++)
            {
                toRet.Add(data[i]);
            }

            return toRet;
        }
    }
}
