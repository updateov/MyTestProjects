using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Perigen.Patterns.NnetControls
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPatternEvent
    {
        void RaiseMouseOverEvent(DateTime from, DateTime to);
        void RaiseMouseLeaveEvent(DateTime from, DateTime to);
        void RaiseBtnPressedEvent(DateTime from, DateTime to, int intervalID);
        void RaiseExportDialogClosedEvent(DateTime from, DateTime to);
        void RaiseTimeRangeChangedEvent(int timeRange);

        void SetExportableChunks(DateTime startTime);

        void RaiseToggleSwitchEvent(bool bRight);
        void RaiseViewSwitchEvent(bool to15Min);
        void RaiseViewSwitchToLeft15MinEvent();
        void RaiseViewSwitchToRight15MinEvent();
    }
}
