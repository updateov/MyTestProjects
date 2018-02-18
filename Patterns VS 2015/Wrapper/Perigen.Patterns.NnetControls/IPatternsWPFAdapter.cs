using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Perigen.Patterns.NnetControls
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPatternsWPFAdapter
    {
        void DisposeControls();
        void SetPluginURL(string url);
        void SetInitParams(string userID, int episodeID, bool canModify, IntPtr CRIHandle);

        bool InitNavigationPanel(IntPtr parentHandle, int x, int y, int width, int height);
        void ResizeNavigationPanel(int x, int y, int width, int height);

        bool InitExportButton(IntPtr parentHandle, int x, int y, int width, int height);
        void ResizeExportButton(int x, int y, int width, int height);
        void SetTimeRange(int timeRange);

        void SetExportDialogParamsEx(double meanContractionInterval, int meanBaseline, int meanBaselineVariability, double montevideoUnits);
        //void StamMessage(string msg);

        void AddListener(IPatternEvent evt);

        void SetStartTime(DateTime startTime);
        void SetScreenWidth(double screenWidth);

        void BeginUpdateChunks();
        void AddChunk(IPatternsExportableChunk section);
        void AddChunkEx(int exportID, int intervalID, DateTime startTime, int timeRange, bool isExported, int x1, int x2);
        void EndUpdateChunks();

        void SetMontevideoVisible(bool isPressed);
        void SetPanelTooltip(string message, DateTime end36Weeks);

        void HideControls();

        DateTime GetTestStartTime();
    }
}
