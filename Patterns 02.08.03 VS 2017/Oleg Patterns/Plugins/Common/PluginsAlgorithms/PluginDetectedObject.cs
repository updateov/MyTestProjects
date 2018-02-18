using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginsAlgorithms
{
    public abstract class PluginDetectedObject
    {
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            var pObj = obj as PluginDetectedObject;
            if (pObj == null)
                return false;

            if (pObj.Id == Id)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(PluginDetectedObject obj1, PluginDetectedObject obj2)
        {
            if (Object.ReferenceEquals(obj1, obj2))
                return true;

            if ((obj1 as Object) == null || (obj2 as Object) == null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(PluginDetectedObject obj1, PluginDetectedObject obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
