using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.GE.Interface.TracingServices
{
    public interface ITracingArgs
    {
        /// <summary>
        /// Bed Id (number) used to collect tracing
        /// </summary>
        String BedId { get; set; }

        /// <summary>
        /// Patient Id used to collect tracing
        /// </summary>
        String PatientId { get; set; }

        /// <summary>
        /// Host/Web service Address where we can collect tracing
        /// </summary>
        String HostServiceAddress { get; set; }
        
        /// <summary>
        /// Used to retrieve tracing in the past. Time in seconds.
        /// </summary>
        Int32 TimeAgo { get; set; }

        /// <summary>
        /// Start time used to retrieve historic tracing
        /// </summary>
        DateTime StartTimeAgo { get; set; }
        
        /// <summary>
        /// Indicate if we are asking for historic tracing
        /// </summary>
        Boolean IsHistoric { get; set; }
    }
}
