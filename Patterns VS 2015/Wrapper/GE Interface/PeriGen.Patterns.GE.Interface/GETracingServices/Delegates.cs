using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.GE.Interface.TracingServices
{
    public static class Delegates
    {
         public delegate void ReceivedBytesDelegate(byte[] data, int dataType);
         public delegate void ReceivedBytesDelegateWithTime(byte[] data, int dataType, DateTime time);
         public delegate void ThreadFinishedDelegate();
    }
}
