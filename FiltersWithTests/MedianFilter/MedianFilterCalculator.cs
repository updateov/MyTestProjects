using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedianFilter
{
    public class MedianFilterCalculator
    {
        public MedianFilterCalculator()
        {
            TracingsData = new List<byte>();
            Sorted20MinTracing = new List<byte>();
            Sorted8MinTracing = new List<byte>();
            Median20MinResult = new List<byte>();
            Median8MinResult = new List<byte>();
            m_lastInsertedIndex = 0;
        }

        public void CalculateMedian()
        {
            byte firstElem = TracingsData[0];
            if (Sorted20MinTracing.Count <= 0)
                for (int i = 0; i < 24; Sorted20MinTracing.Add(firstElem), i++) ;

            if (Sorted8MinTracing.Count <= 0)
                for (int i = 0; i < 14; Sorted8MinTracing.Add(firstElem), i++) ;

            //vector<unsigned char>::iterator itr = m_tracingsData.begin() + m_lastInsertedIndex;
            //for (m_lastInsertedIndex; m_lastInsertedIndex < (int)m_tracingsData.size(); m_lastInsertedIndex++)
            while (m_lastInsertedIndex < TracingsData.Count)
            {
                // Insertion to sorted array
                //char toIns = m_tracingsData.at(m_lastInsertedIndex);
                byte toIns = TracingsData[m_lastInsertedIndex];
                int size = Sorted20MinTracing.Count;
                if (toIns == Sorted20MinTracing[0])
                    Sorted20MinTracing.Insert(0, toIns);
                else
                    InsertSort(Sorted20MinTracing, toIns, 0, size - 1);

                size = Sorted8MinTracing.Count;
                if (toIns == Sorted8MinTracing[0])
                    Sorted8MinTracing.Insert(0, toIns);
                else
                    InsertSort(Sorted8MinTracing, toIns, 0, size - 1);

                // After insertion, check whether there's enough data, add result and remove head from sorted array
                size = Sorted20MinTracing.Count;
                if (size >= 49) // 4800 = 10 minutes in quarter seconds
                {
                    Median20MinResult.Add(Sorted20MinTracing[24]);
                    byte toRem;
                    if (m_lastInsertedIndex >= 49)
                        toRem = TracingsData[m_lastInsertedIndex - 48];
                    else
                        toRem = TracingsData[0];

                    int indexToRem = Sorted20MinTracing.IndexOf(toRem);
                    Sorted20MinTracing.RemoveAt(indexToRem);
                }

                size = Sorted8MinTracing.Count;
                if (size >= 29) // 2880 = 4 minutes in quarter seconds 
                {
                    Median8MinResult.Add(Sorted8MinTracing[14]);
                    byte toRem;
                    if (m_lastInsertedIndex >= 29)
                        toRem = TracingsData[m_lastInsertedIndex - 28];
                    else
                        toRem = TracingsData[0];

                    int indexToRem = Sorted8MinTracing.IndexOf(toRem);
                    Sorted8MinTracing.RemoveAt(indexToRem);
                }

                if (m_lastInsertedIndex > 48)
                    TracingsData.RemoveAt(0);
                else
                    m_lastInsertedIndex++;
            }
        }

        public void InsertSort(List<byte> vecToIns, byte val, int start, int end)
        {
            int mid = (start + end) / 2;
            if (end == start + 1)
            {
                if (val >= vecToIns[end])
                {
                    vecToIns.Add(val);
                    return;
                }
                else if (val >= vecToIns[start])
                {
                    vecToIns.Insert(end, val);
                    return;
                }
            }

            if (start == end && val <= vecToIns[start])
            {
                vecToIns.Insert(start, val);
                return;
            }

            if (val == vecToIns[mid])
                vecToIns.Insert(mid, val);
            else if (val < vecToIns[mid])
                InsertSort(vecToIns, val, start, mid);
            else
                InsertSort(vecToIns, val, mid, end);
        }

        public List<byte> TracingsData { get; set; }
        public List<byte> Sorted20MinTracing { get; set; }
        public List<byte> Sorted8MinTracing { get; set; }
        public List<byte> Median20MinResult { get; set; }
        public List<byte> Median8MinResult { get; set; }

        private int m_lastInsertedIndex;
    }
}
