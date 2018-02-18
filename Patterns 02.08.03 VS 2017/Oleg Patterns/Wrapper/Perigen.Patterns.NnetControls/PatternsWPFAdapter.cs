using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Perigen.Patterns.NnetControls.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;

namespace Perigen.Patterns.NnetControls
{
    public class Decels
    {
        public bool Early;
        public bool Variable;
        public bool Late;
        public bool None;
        public bool Prolonged;
        public bool Other;
    }

    public class IntervalEventArgs : EventArgs
    {
        //•	ExportHeader
        public string VisitKey { get; set; }
        public int IntervalID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int IntervalDuration { get; set; }

        //•	MaternalData
        public string ContractionIntervalRange { get; set; }
        public double MeanContractionInterval { get; set; }
        public string ContractionDurationRange { get; set; }
        public string OriginalContractionDurationRange { get; set; }
        public string ContractionIntensityRange { get; set; }
        public string OriginalContractionIntensityRange { get; set; }
        public int ContractionCount10min { get; set; }
        public int LongContractionCount { get; set; }
        public int? MontevideoUnits { get; set; }

        //•	FetalData
        public int BaselineFHR { get; set; }
        public int OriginalBaselineFHR { get; set; }
        public string BaselineVariability { get; set; }
        public bool Accels { get; set; }
        public Decels Decels { get; set; }

        public IntervalEventArgs()
        {
            Decels = new Decels();
        }
    }

    public class TimeRangeArgs : EventArgs
    {
        public int TimeRange { get; set; }
    }

    public class TracingsViewStatusArgs : EventArgs
    {
        public bool SwitchRight { get; set; }
        public bool SwitchTo15Min { get; set; }
    }

    public class PatternsWPFAdapter : IPatternsWPFAdapter
    {
        private PatternsUIManager PatternsUIManager { get; set; }
        public bool IsExternalUsing { get; set; }

        #region Events (replace IPatternEvent)

        public EventHandler<IntervalEventArgs> IntervalMouseOver;
        public void OnIntervalMouseOver(object sender, DateTime from, DateTime to)
        {
            if(IntervalMouseOver != null)
                IntervalMouseOver(sender, new IntervalEventArgs() { StartTime = from, EndTime = to });
        }

        public EventHandler<IntervalEventArgs> IntervalMouseLeave;
        public void OnIntervalMouseLeave(object sender, DateTime from, DateTime to)
        {
            if (IntervalMouseLeave != null)
                IntervalMouseLeave(sender, new IntervalEventArgs() { StartTime = from, EndTime = to });
        }

        public EventHandler<IntervalEventArgs> IntervalPressed;
        public void OnIntervalPressed(object sender, IntervalEventArgs data)
        {
            if (IntervalPressed != null)
                IntervalPressed(sender, data);
        }

        public EventHandler<IntervalEventArgs> ExportDialogClosed;
        public void OnExportDialogClosed(object sender, DateTime from, DateTime to)
        {
            if (ExportDialogClosed != null)
                ExportDialogClosed(sender, new IntervalEventArgs() { StartTime = from, EndTime = to });
        }

        public EventHandler<TimeRangeArgs> TimeRangeChanged;
        public void OnTimeRangeChanged(object sender, int timeRange)
        {
            if (TimeRangeChanged != null)
                TimeRangeChanged(sender, new TimeRangeArgs() { TimeRange = timeRange });
        }


        public EventHandler<TracingsViewStatusArgs> ToggleSwitch;
        public void OnToggleSwitchEvent(object sender, bool bSwitchRigth)
        {
            if (ToggleSwitch != null)
                ToggleSwitch(sender, new TracingsViewStatusArgs() { SwitchRight = bSwitchRigth, SwitchTo15Min = false });
        }

        public EventHandler<TracingsViewStatusArgs> ViewSwitch;
        public void OnViewSwitchEvent(object sender, bool bSwitchTo15MinView)
        {
            if (ViewSwitch != null)
                ViewSwitch(sender, new TracingsViewStatusArgs() { SwitchRight = true, SwitchTo15Min = bSwitchTo15MinView });
        }

        public EventHandler<TracingsViewStatusArgs> SwitchTo15MinView;
        public void OnViewSwitchTo15MinViewEvent(object sender, bool right)
        {
            if (SwitchTo15MinView != null)
                SwitchTo15MinView(sender, new TracingsViewStatusArgs() { SwitchRight = right, SwitchTo15Min = true });
        }

    

        #endregion

        public PatternsWPFAdapter()
        {
            PatternsUIManager = new PatternsUIManager();
            PatternsUIManager.WPFAdapter = this;
        }

        public bool InitNavigationPanel(IntPtr parentHandle, int x, int y, int width, int height)
        {
            //PatternsUIManager.IsMontevideoVisible = false;
            bool res = PatternsUIManager.InitNavigationPanel(parentHandle, x, y, width, height);

            return res;
        }

        public void ResizeNavigationPanel(int x, int y, int width, int height)
        {
            PatternsUIManager.ResizeNavigationPanel(x, y, width, height);
        }

        public bool InitExportButton(IntPtr parentHandle, int x, int y, int width, int height)
        {
            bool res = PatternsUIManager.InitExportButton(parentHandle, x, y, width, height);

            return res;
        }

        public void ResizeExportButton(int x, int y, int width, int height)
        {
            PatternsUIManager.ResizeExportButton(x, y, width, height);
        }

        public bool InitExportContainer(IntPtr parentHandle, int x, int y, int width, int height)
        {
            bool res = PatternsUIManager.InitExportContainer(parentHandle, x, y, width, height);

            return res;
        }
        public void ResizeExportContainer(int x, int y, int width, int height)
        {
            PatternsUIManager.ResizeExportContainer(x, y, width, height);
        }

        public void SetTimeRange(int timeRange)
        {
            PatternsUIManager.SetTimeRange(timeRange);
        }

        public void AddListener(IPatternEvent evt)
        {
            PatternsUIManager.AddListener(evt);
        }

        public void SetStartTime(DateTime startTime)
        {
            PatternsUIManager.StripStartTime = startTime;
        }

        public void SetInitParams(string userID, int episodeID, bool canModify, IntPtr CRIHandle)
        {
            PatternsUIManager.UserID = userID;
            PatternsUIManager.EpisodeID = episodeID;
            PatternsUIManager.CanModify = canModify;
            PatternsUIManager.CRIHandle = CRIHandle;
        }

        public void SetExportDialogParamsEx(double meanContractionInterval, int meanBaseline, int meanBaselineVariability, double montevideoUnits)
        {
            PatternsUIManager.SetExportDialogParams(meanContractionInterval,
                                                             meanBaseline,
                                                             meanBaselineVariability,
                                                             montevideoUnits);
        }

        public void SetScreenWidth(double screenWidth)
        {
            PatternsUIManager.ScreenWidthInMinutes = screenWidth;
        }

        public void BeginUpdateChunks()
        {
            PatternsUIManager.ExportableChunks.Clear();
            GC.Collect();
        }

        public void AddChunk(IPatternsExportableChunk section)
        {
            PatternsUIManager.ExportableChunks.Add(section);
        }

        public void AddChunkEx(int exportID, int intervalID, DateTime startTime, int timeRange, bool isExported, int x1, int x2)
        {
            IPatternsExportableChunk chunk = new PatternsExportableChunk();
            chunk.putExportID(exportID);
            chunk.putIntervalID(intervalID);
            chunk.putStartTime(startTime);
            chunk.putTimeRange(timeRange);
            chunk.putIsExported(isExported);
            chunk.putX1(x1);
            chunk.putX2(x2);

            PatternsUIManager.ExportableChunks.Add(chunk);
        }

        public void EndUpdateChunks()
        {
            PatternsUIManager.FireEventUpdateData();
        }

        public void HideControls()
        {
            PatternsUIManager.CloseOpenedWindows();
            PatternsUIManager.HideControls();
        }

        public void DisposeControls()
        {
            PatternsUIManager.CloseOpenedWindows();
            PatternsUIManager.DisposeControls();
        }

        public void SetPluginURL(string url)
        {
            PatternsUIManager.PluginURL = url;
            PatternsUIManager.GetExportConfig();
        }

        public void SetMontevideoVisible(bool isPressed)
        {
            PatternsUIManager.IsMontevideoVisible = isPressed;
        }

        public void SetPanelTooltip(string message, DateTime end36Weeks)
        {
            PatternsUIManager.PanelTooltip = message;
            PatternsUIManager.End36Weeks = end36Weeks;
        }

        public IntervalEventArgs GetIntervalData(DateTime startTime, DateTime endTime)
        {
            return PatternsUIManager.GetIntervalData(startTime, endTime);
        }

        public DateTime GetTestStartTime()
        {
            return DateTime.MinValue;
        }

        public void SetSelectedInterval(DateTime startTime)
        {
            IPatternsExportableChunk chunkData = PatternsUIManager.ExportableChunks.FirstOrDefault(t => t.getStartTime() == startTime);

            if (chunkData != null)
            {
                PatternsUIManager.SetSelectedChunk(chunkData.getStartTime(), true);
                PatternsUIManager.RaiseBtnPressedEvent(chunkData.getStartTime(), chunkData.getStartTime().AddMinutes(chunkData.getTimeRange()), chunkData.getIntervalID());
            }
        }

        public void SaveIntervalExportedState(DateTime startTime, DateTime endTime, string visitKey)
        {
            IPatternsExportableChunk chunkData = PatternsUIManager.ExportableChunks.FirstOrDefault(t => t.getStartTime() == startTime);

            if (chunkData != null)
            {               
                PatternsUIManager.SaveIntervalExportedState(chunkData.getStartTime(), chunkData.getStartTime().AddMinutes(chunkData.getTimeRange()), chunkData.getIntervalID(), visitKey);
            }

        }

        public void ResetIntervalSelection(DateTime startTime, bool isExported)
        {
            PatternsUIManager.ResetIntervalSelection(startTime, isExported);
        }
        //FOR TEST ONLY!!!
        private void AddChunks()
        {
            DateTime dt = new DateTime(2015, 10, 12, 12, 40, 0);

            AddChunkEx(0, 0, dt.AddMinutes(-105), 15, false, -20, 40);
            AddChunkEx(0, 0, dt.AddMinutes(-90), 15, true, 40, 100);
            AddChunkEx(0, 0, dt.AddMinutes(-75), 15, true, 100, 160);
            AddChunkEx(0, 0, dt.AddMinutes(-60), 15, false, 160, 220);
            AddChunkEx(0, 0, dt.AddMinutes(-45), 30, false, 220, 340);
            AddChunkEx(0, 0, dt.AddMinutes(-15), 30, true, 340, 460);
            AddChunkEx(0, 0, dt.AddMinutes(15), 30, false, 900, 1020);
            AddChunkEx(0, 0, dt.AddMinutes(45), 30, false, 1020, 1140);
            AddChunkEx(0, 0, dt.AddMinutes(75), 30, false, 1140, 1260);
        }

        public void SetViewStatusParams(bool is15MinView, bool isLeftPartShown)
        {
            PatternsUIManager.Is15MinView = is15MinView;
            PatternsUIManager.Is15MinLeftView = isLeftPartShown;
        }

        public void SetRoundExportValueParams(int roundBaselineFHRValue)
        {
            PatternsUIManager.RoundBaselineFHRValue = roundBaselineFHRValue;
        }
    }
}
