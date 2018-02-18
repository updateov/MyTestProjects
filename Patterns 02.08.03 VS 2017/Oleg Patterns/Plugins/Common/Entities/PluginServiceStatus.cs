using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternsEntities
{
    public class PluginServiceStatus
    {
        public string PluginServiceName { get; set; }
        public string Version { get; set; }
        public DateTime StartTime { get; set; }

        public PluginServiceStatus()
        {
            PluginServiceName = String.Empty;
            Version = String.Empty;
            StartTime = DateTime.MinValue;
        }
    }
}
