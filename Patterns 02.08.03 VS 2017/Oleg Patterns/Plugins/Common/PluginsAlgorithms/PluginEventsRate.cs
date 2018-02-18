using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginsAlgorithms
{
    public class PluginEventsRate
    {
        public long AccelerationRate { get; set; }
        public long LateDecelRate { get; set; }
        public long ProlongedDecelRate { get; set; }
    }

    public abstract class PluginsEventsRateWithGenericDecel : PluginEventsRate
    {
        public long DecelerationRate { get; set; }
    }
}
