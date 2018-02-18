using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Perigen.Patterns.NnetControls
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPatternsExportableChunk
    {
        void putExportID(int id);
        int getExportID();

        void putIntervalID(int id);
        int getIntervalID();

        void putStartTime(DateTime startTime);
        DateTime getStartTime();

        void putTimeRange(int timeRange);
        int getTimeRange();

        void putIsExported(bool isExported);
        bool getIsExported();

        void putX1(int x1);
        int getX1();

        void putX2(int x2);
        int getX2();
    }
}
