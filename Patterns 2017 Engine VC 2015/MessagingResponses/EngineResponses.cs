using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessagingResponses
{
    public enum EngineResponseCode
    {
        Success,
        Error
    };

    public class EngineResponseBase
    {
        public EngineResponseCode ResponseCode { get; set; }
        public String ErrorMessage { get; set; }
    }

    public class StatusEngineResponse : EngineResponseBase
    {
        public int ProcessId { get; set; }
        public string[] ActiveVisits { get; set; }
    }

    public class ResultsEngineResponse : EngineResponseBase
    {
        public StringBuilder ResultsData { get; set; }
        public bool MoreData { get; set; }
    }
}
