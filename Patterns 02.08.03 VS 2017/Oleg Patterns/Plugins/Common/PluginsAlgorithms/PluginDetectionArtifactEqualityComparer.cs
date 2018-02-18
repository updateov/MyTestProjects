using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginsAlgorithms
{
    class PluginDetectionArtifactEqualityComparer : IEqualityComparer<PluginDetectionArtifact>
    {

        public bool Equals(PluginDetectionArtifact x, PluginDetectionArtifact y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(PluginDetectionArtifact obj)
        {
            long nCode = obj.Id ^ obj.StartTime ^ obj.EndTime;
            return nCode.GetHashCode();
        }
    }
}
