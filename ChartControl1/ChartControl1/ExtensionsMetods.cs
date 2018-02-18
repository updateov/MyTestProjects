using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChartControl1
{
    public static class ExtensionsMetods
    {
        static DateTime EpochReferenceDateTime = new DateTime(1970, 1, 1);

        /// <summary>
        /// Convert a datetime to the Epoch value
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ToEpoch(this DateTime date)
        {
            return (long)((date - EpochReferenceDateTime).TotalSeconds);
        }

        /// <summary>
        /// Convert a datetime to the Epoch value
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static System.Nullable<long> ToEpoch(this System.Nullable<DateTime> date)
        {
            if (date.HasValue)
            {
                return (long)((date.Value - EpochReferenceDateTime).TotalSeconds);
            }

            return null;
        }

        /// <summary>
        /// Convert an Epoch encoded datetime to a datetime
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long date)
        {
            return EpochReferenceDateTime.AddSeconds(date);
        }

        /// <summary>
        /// Convert an Epoch encoded datetime to a datetime
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static System.Nullable<DateTime> ToDateTime(this System.Nullable<long> date)
        {
            if (date.HasValue)
            {
                return EpochReferenceDateTime.AddSeconds(date.Value);
            }

            return null;
        }
    }
}
