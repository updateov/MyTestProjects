using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perigen.Patterns.NnetControls
{
    public class PatternsExportableChunk : IPatternsExportableChunk
    {
        int m_exportID;
        int m_intervalID;
        DateTime m_startTime;
        int m_timeRange;
        bool m_isExported;
        int m_X1;
        int m_X2;

        public PatternsExportableChunk()
        {
            m_exportID = 0;
            m_intervalID = 0;
            m_startTime = DateTime.MinValue;
            m_timeRange = 30;
            m_isExported = false;
            m_X1 = 0;
            m_X2 = 0;
        }

        #region IPatternsExportableChunk Members

        public void putExportID(int id)
        {
            m_exportID = id;
        }

        public int getExportID()
        {
            return m_exportID;
        }

        public void putIntervalID(int id)
        {
            m_intervalID = id;
        }

        public int getIntervalID()
        {
            return m_intervalID;
        }

        public void putStartTime(DateTime startTime)
        {
            m_startTime = startTime;
        }

        public DateTime getStartTime()
        {
            return m_startTime;
        }

        public void putTimeRange(int timeRange)
        {
            m_timeRange = timeRange;
        }

        public int getTimeRange()
        {
            return m_timeRange;
        }

        public void putIsExported(bool isExported)
        {
            m_isExported = isExported;
        }

        public bool getIsExported()
        {
            return m_isExported;
        }

        public void putX1(int x1)
        {
            m_X1 = x1;
        }

        public int getX1()
        {
            return m_X1;
        }

        public void putX2(int x2)
        {
            m_X2 = x2;
        }

        public int getX2()
        {
            return m_X2;
        }

        #endregion
    }
}
