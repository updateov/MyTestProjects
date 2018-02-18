using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PatternsPluginsCommon
{
    public static class ExstensionMethods
    {
        public static string Attribute2String(this XElement elt, string attribute, string defaultVal = "")
        {
            string result = defaultVal;

            var attr = elt.Attribute(attribute);
            if (attr != null)
            {
                result = attr.Value;
            }
            return result;
        }

        public static int Attribute2Int(this XElement elt, string attribute, int defaultVal = -1)
        {
            int result = defaultVal;

            var attr = elt.Attribute(attribute);
            if (attr != null)
            {
                result = Convert.ToInt32(attr.Value);
            }
            return result;
        }

        public static long Attribute2Long(this XElement elt, string attribute, long defaultVal = -1)
        {
            long result = defaultVal;

            var attr = elt.Attribute(attribute);
            if (attr != null)
            {
                result = Convert.ToInt64(attr.Value);
            }

            return result;
        }

        public static bool Attribute2Bool(this XElement elt, string attribute, bool defaultVal = true)
        {
            bool result = defaultVal;

            var attr = elt.Attribute(attribute);
            if (attr != null)
            {
                result = Convert.ToBoolean(attr.Value);
            }

            return result;
        }

        public static DateTime ToDateTime(this long seconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(seconds);
        }

        public static long ToEpochTime(this DateTime time)
        {
            var span = time - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)span.TotalSeconds;
        }

        //public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        //{
        //    return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        //}

        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            var delta = (d.Ticks - (dt.Ticks % d.Ticks)) % d.Ticks;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static DateTime RoundToNearest(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;

            return roundUp ? dt.RoundUp(d) : dt.RoundDown(d);
        }
    }
}
