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

    public class PatternsWPFAdapter : IPatternsWPFAdapter
    {
        //private DispatcherTimer m_chunksTimer;

        public bool InitNavigationPanel(IntPtr parentHandle, int x, int y, int width, int height)
        {
            PatternsUIManager.Instance.IsMontevideoVisible = false;
            return PatternsUIManager.Instance.InitNavigationPanel(parentHandle, x, y, width, height);
        }

        public void ResizeNavigationPanel(int x, int y, int width, int height)
        {
            PatternsUIManager.Instance.ResizeNavigationPanel(x, y, width, height);
        }

        public bool InitExportButton(IntPtr parentHandle, int x, int y, int width, int height)
        {
            return PatternsUIManager.Instance.InitExportButton(parentHandle, x, y, width, height);
        }

        public void ResizeExportButton(int x, int y, int width, int height)
        {
            PatternsUIManager.Instance.ResizeExportButton(x, y, width, height);
        }

        public void SetTimeRange(int timeRange)
        {
            PatternsUIManager.Instance.SetTimeRange(timeRange);
        }

        public void AddListener(IPatternEvent evt)
        {
            PatternsUIManager.Instance.AddListener(evt);
        }

        public void SetStartTime(DateTime startTime)
        {
            PatternsUIManager.Instance.StripStartTime = startTime;

            //FOR TEST ONLY!!!
            //m_chunksTimer = new DispatcherTimer();
            //m_chunksTimer.Tick += new EventHandler(timer_Tick);
            //m_chunksTimer.Interval = new TimeSpan(0, 0, 2);
            //m_chunksTimer.Start();

            //AddChunks();
        }

        public void SetInitParams(string userID, int episodeID, bool canModify, IntPtr CRIHandle)
        {
            PatternsUIManager.Instance.UserID = userID;
            PatternsUIManager.Instance.EpisodeID = episodeID;
            PatternsUIManager.Instance.CanModify = canModify;
            PatternsUIManager.Instance.CRIHandle = CRIHandle;
        }

        public void SetExportDialogParamsEx(double meanContractionInterval, double meanBaseline, double meanBaselineVariability, double montevideoUnits)
        {
            PatternsUIManager.Instance.SetExportDialogParams(meanContractionInterval.ToString(),
                                                             meanBaseline.ToString(),
                                                             meanBaselineVariability.ToString(),
                                                             montevideoUnits.ToString());
        }

        private void StamMessage(string msg)
        {
            System.Windows.MessageBox.Show(msg);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            BeginUpdateChunks();
            AddChunks();
            EndUpdateChunks();
        }

        public void SetScreenWidth(double screenWidth)
        {
            PatternsUIManager.Instance.ScreenWidthInMinutes = screenWidth;
        }

        public void BeginUpdateChunks()
        {
            //SetPanelTooltip("PanelTooltip!!!", DateTime.Now.AddMinutes(-10));
            PatternsUIManager.Instance.ExportableChunks.Clear();
        }

        public void AddChunk(IPatternsExportableChunk section)
        {
            PatternsUIManager.Instance.ExportableChunks.Add(section);
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

            PatternsUIManager.Instance.ExportableChunks.Add(chunk);
        }

        public void EndUpdateChunks()
        {
            PatternsUIManager.Instance.FireEventUpdateData();
        }

        public void HideControls()
        {
            PatternsUIManager.Instance.CloseOpenedWindows();
            PatternsUIManager.Instance.HideControls();
        }

        public void DisposeControls()
        {
            PatternsUIManager.Instance.CloseOpenedWindows();
            PatternsUIManager.Instance.DisposeControls();
        }

        public void SetPluginURL(string url)
        {
            PatternsUIManager.Instance.PluginURL = url;
        }

        public void SetMontevideoVisible(bool isPressed)
        {
            PatternsUIManager.Instance.IsMontevideoVisible = isPressed;
        }

        public void SetPanelTooltip(string message, DateTime end36Weeks)
        {
            PatternsUIManager.Instance.PanelTooltip = message;
            PatternsUIManager.Instance.End36Weeks = end36Weeks;
        }

        public DateTime GetTestStartTime()
        {
            return DateTime.MinValue;
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
    }
}
