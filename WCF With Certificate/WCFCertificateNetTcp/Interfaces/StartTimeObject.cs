using System;
using System.Runtime.Serialization;

namespace Interfaces
{
    [DataContract]
    public class StartTimeObject
    {
        [DataMember]
        public DateTime StartTime { get; set; }
    }
}