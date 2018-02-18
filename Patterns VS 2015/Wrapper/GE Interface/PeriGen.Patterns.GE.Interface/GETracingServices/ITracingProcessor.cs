using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.GE.Interface.TracingServices
{
    public interface ITracingProcessor
    {
        void Process(ITracingArgs args);
        Delegates.ReceivedBytesDelegate ReceivedBytes { get; set; }
        Delegates.ReceivedBytesDelegateWithTime ReceivedBytesWithTime { get; set; }
        Delegates.ThreadFinishedDelegate OnThreadFinished { get; set; }
    }
}
