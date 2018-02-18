using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PeriGen.Patterns.Engine.Data;

namespace PatternsAddOnManager
{
    public static class PatternsExtensions
    {
        public static void RemoveCalculatedTracings(this TracingBlock tracings, int seconds)
        {
            tracings.UPs.RemoveRange(0, seconds);
            tracings.HRs.RemoveRange(0, seconds * 4);
            tracings.Start = tracings.Start.AddSeconds(seconds);
        }

        public static void RemoveCalculatedTracings(this TracingBlock tracings, DateTime newStartTime)
        {
            var seconds = Math.Max(0, (newStartTime - tracings.Start).TotalSeconds);
            tracings.RemoveCalculatedTracings((int)seconds);
        }

        public static void Clear<TKey, TValue>(this Dictionary<TKey, TValue> dict, bool bDispose)
        {
            bool bIDisp = false; // checks for IDispozable object in dictionary value
            foreach (var item in dict)
            {
                if (!bIDisp)
                {
                    if (item.Value is IDisposable)
                        bIDisp = true;
                    else
                        break;
                }

                if (item.Value is IDisposable)
                {
                    var i = item.Value as IDisposable;
                    i.Dispose();
                }
            }

            dict.Clear();
        }

        public static void RemoveRange<TKey, TValue>(this Dictionary<TKey, TValue> dict, IEnumerable<TKey> collection)
        {
                var items = collection.ToList();
                for ( ; items.Count() > 0; )
                {
                    var item = items.ElementAt(0);
                    items.RemoveAt(0);
                    dict.Remove(item);
                }
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static List<T> SubList<T>(this List<T> data, int index, int length)
        {
            List<T> result = new List<T>();
            for (int i = 0; i < length && i + index < data.Count; i++)
            {
                result.Add(data[i + index]);
            }

            return result;
        }

        public static bool SameArtifactExists(this List<DetectionArtifact> data, DetectionArtifact newData)
        {
            foreach (var item in data)
            {
                if (item.StartTime.Equals(newData.StartTime) && 
                    item.EndTime.Equals(newData.EndTime) && 
                    item.Category == newData.Category)
                    return true;
            }

                return false;
        }
    }
}
