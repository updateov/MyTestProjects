using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatternsCALMMediator
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Convert the LMS Encounter key to a string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Key2String(this LMS.Key.Encounter key)
        {
            return string.Format("{0}-{1}-{2}-{3}", key.SiteNo, key.PatientNo, key.EOCNo, key.EncounterNo);
        }

        //public static string CalcGestationalAge(DateTime edd)
        //{
        //    DateTime delvery = edd.ToUniversalTime();
        //    DateTime startDate = edd.AddDays(-280).ToUniversalTime();
        //    DateTime now = DateTime.UtcNow;

        //    double nDays = (now - startDate).TotalDays;

        //    int weeks = Convert.ToInt32(nDays) / 7;
        //    int days = Convert.ToInt32(nDays) % 7;

        //    string ga = String.Format("{0} + {1}", weeks, days);
        //    return ga;
        //}
    }
}
