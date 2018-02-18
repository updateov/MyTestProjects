using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Perigen.Patterns.NnetControls.Controls;
using System.Windows.Input;
using Perigen.Patterns.NnetControls.Screens;
using System.Windows;
using System.Windows.Media;
using Perigen.Patterns.NnetControls.DataEntities;
using System.Xml.Serialization;
using System.IO;
using RestSharp;
using System.Xml.Linq;
using System.Xml;

namespace Perigen.Patterns.NnetControls
{  
    public class Win32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        // http://msdn.microsoft.com/en-us/library/ms633519(VS.85).aspx
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        // http://msdn.microsoft.com/en-us/library/a5ch4fda(VS.80).aspx
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }

    public class UpdateDataEventArgs : EventArgs
    {
        public UpdateDataEventArgs(List<IPatternsExportableChunk> data)
        {
            ExportableChunks = new List<IPatternsExportableChunk>(data);
        }

        public List<IPatternsExportableChunk> ExportableChunks { get; set; }
    }

    public class PatternsUIManager
    {
        private IntPtr m_navigationPanelHandle = IntPtr.Zero;
        private HwndSource m_navigationPanelSource = null;
        private HwndSourceParameters m_navigationPanelsourceParams;
        private NavigationPanelUserControl m_navigationPanel = null;

        private IntPtr m_exportButtonHandle = IntPtr.Zero;
        private HwndSource m_exportButtonSource = null;
        private HwndSourceParameters m_exportButtonSourceParams;
        private ExportButtonUserControl m_exportButton = null;

        private double? m_meanContractionInterval = null;
        private int? m_meanBaseline = null;
        private double? m_meanBaselineVariability = null;
        private int? m_montevideoUnits = null;

        private List<IPatternsExportableChunk> m_sections = new List<IPatternsExportableChunk>();

        private List<IPatternEvent> m_listeners = new List<IPatternEvent>();

        private List<Window> m_activeWindows = new List<Window>();

        XmlSerializer m_intervalSerializer = new XmlSerializer(typeof(ExportInterval));
        XmlSerializer m_loginSerializer = new XmlSerializer(typeof(UserDetails));


        public XmlSerializer IntervalSerializer { get { return m_intervalSerializer; } }

        private object m_validationLock = new object();
        public object ValidationLock { get { return m_validationLock; } }

        public double PixelsInMinute { get; set; }

        public double ScreenWidthInMinutes { get; set; }

        public DateTime StripStartTime { get; set; }

        public DateTime SelectedChunkStartTime { get; set; }

        public IntPtr CRIHandle { get; set; }

        //public double DpiFactor { get; set; }

        public List<IPatternsExportableChunk> ExportableChunks
        {
            get
            {
                return m_sections;
            }
            set
            {
                m_sections = value;
            }
        }

        public string PluginURL { get; set; }

        public const string IntervalGet = "ExportPlugin/Episodes/{episodeId}/Intervals/{intervalId}";

        public const string IntervalSet = "ExportPlugin/Episodes/{episodeId}/Intervals";

        public const string LoginSet = "ExportPlugin/Users/";

        public const int RequestTimeout = 10000;

        public string UserID{ get; set; }

        public int EpisodeID { get; set; }

        public bool CanModify { get; set; }

        public ExportInterval ConceptsList { get; set; }

        public bool IsMontevideoVisible { get; set; }

        public string PanelTooltip { get; set; }
       
        public DateTime End36Weeks { get; set; }

        #region Singleton functionality

        private static volatile PatternsUIManager s_instance;
        private static object s_lockObject = new Object();


        private PatternsUIManager()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(WindowLoaded));
            EventManager.RegisterClassHandler(typeof(Window), Window.UnloadedEvent, new RoutedEventHandler(WindowUnloaded));


            this.SelectedChunkStartTime = DateTime.MinValue;
            this.CRIHandle = IntPtr.Zero;
            this.ConceptsList = new ExportInterval();
            this.IsMontevideoVisible = false;
            //this.DpiFactor = 1.0;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message, "Uncaught Thread Exception",MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static PatternsUIManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new PatternsUIManager();
                    }
                }
                return s_instance;
            }
        }

        #endregion

        #region Events & Delegates

        public EventHandler<UpdateDataEventArgs> UpdateDataEvent;

        public void FireEventUpdateData()
        {
            var tempHandler = UpdateDataEvent;
            if (tempHandler != null)
            {
                tempHandler(this, new UpdateDataEventArgs(ExportableChunks));
            }
        }

        #endregion

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window)
            {
                m_activeWindows.Add((Window)sender);
            }
        }

        void WindowUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window)
            {
                m_activeWindows.Remove((Window)sender);
            }
        }

        public void CloseOpenedWindows()
        {
            foreach (Window window in m_activeWindows)
            {
                window.Close();
            }

            m_activeWindows.Clear();
        }

        public void DisposeControls()
        {  
            if(m_sections != null)
                m_sections.Clear();

            if (m_navigationPanel != null)
            {
                m_navigationPanel.ReleaseResources();
                m_navigationPanel = null;
            }

            if(m_listeners != null)
                m_listeners.Clear();

            if (m_navigationPanelSource != null)
            {
                m_navigationPanelSource.Dispose();
                m_navigationPanelSource = null;
            }

            if (m_exportButtonSource != null)
            {
                m_exportButtonSource.Dispose();
                m_exportButtonSource = null;
            }
        }

        public void AddListener(IPatternEvent evt)
        {
            m_listeners.Add(evt);
        }

        public void SetScreenWidth(double screenWidth)
        {
            ScreenWidthInMinutes = screenWidth;
        }

        public bool InitNavigationPanel(IntPtr parentHandle, int x, int y, int width, int height)
        {
            bool res = true;
            m_navigationPanelHandle = parentHandle;

            try
            {
                this.PixelsInMinute = width / ScreenWidthInMinutes;

                m_navigationPanel = new NavigationPanelUserControl();
                m_navigationPanel.Width = width;
                m_navigationPanel.Height = height;

                m_navigationPanelsourceParams = new HwndSourceParameters("PatternsNavigationPanel");
                m_navigationPanelsourceParams.PositionX = x;
                m_navigationPanelsourceParams.PositionY = y;
                m_navigationPanelsourceParams.Height = height;
                m_navigationPanelsourceParams.Width = width;
                m_navigationPanelsourceParams.ParentWindow = m_navigationPanelHandle;
                m_navigationPanelsourceParams.WindowStyle = 0x50000000;

                m_navigationPanelSource = new HwndSource(m_navigationPanelsourceParams);
                m_navigationPanelSource.RootVisual = m_navigationPanel;
            }
            catch (Exception)
            {
                res = false;
            }

            return res;
        }

        public void ResizeNavigationPanel(int x, int y, int width, int height)
        {
            try
            {
                this.PixelsInMinute = width / ScreenWidthInMinutes;

                m_navigationPanel.Width = width;
                m_navigationPanel.Height = height;

                m_navigationPanelsourceParams.PositionX = x;
                m_navigationPanelsourceParams.PositionY = y;
                m_navigationPanelsourceParams.Height = height;
                m_navigationPanelsourceParams.Width = width;

                if (m_navigationPanelSource.IsDisposed == false)
                {
                    m_navigationPanelSource.Dispose();
                }
                m_navigationPanelSource = new HwndSource(m_navigationPanelsourceParams);
                m_navigationPanelSource.RootVisual = m_navigationPanel;
            }
            catch (Exception)
            {
            }
        }

        public bool InitExportButton(IntPtr parentHandle, int x, int y, int width, int height)
        {
            bool res = true;
            m_exportButtonHandle = parentHandle;

            try
            {
                m_exportButton = new ExportButtonUserControl();
                m_exportButton.Width = width;
                m_exportButton.Height = height;

                m_exportButtonSourceParams = new HwndSourceParameters("PatternsExportButton");
                m_exportButtonSourceParams.PositionX = x;
                m_exportButtonSourceParams.PositionY = y;
                m_exportButtonSourceParams.Height = height;
                m_exportButtonSourceParams.Width = width;
                m_exportButtonSourceParams.ParentWindow = m_exportButtonHandle;
                m_exportButtonSourceParams.WindowStyle = 0x50000000;

                m_exportButtonSource = new HwndSource(m_exportButtonSourceParams);
                m_exportButtonSource.RootVisual = m_exportButton;
            }
            catch (Exception)
            {
                res = false;
            }

            return res;
        }

        public void ResizeExportButton(int x, int y, int width, int height)
        {
            try
            {
                m_exportButton.Width = width;
                m_exportButton.Height = height;

                m_exportButtonSourceParams.PositionX = x;
                m_exportButtonSourceParams.PositionY = y;
                m_exportButtonSourceParams.Height = height;
                m_exportButtonSourceParams.Width = width;

                if (m_exportButtonSource.IsDisposed == false)
                {
                    m_exportButtonSource.Dispose();
                }
                m_exportButtonSource = new HwndSource(m_exportButtonSourceParams);
                m_exportButtonSource.RootVisual = m_exportButton;
            }
            catch (Exception)
            {
            }
        }

        public void SetTimeRange(int timeRange)
        {
            if (m_exportButton != null)
            {
                m_exportButton.SetTimeRange(timeRange == 30 ? true : false);
            }
        }

        public LoginStatus Login(UserDetails userData)
        {
            LoginStatus loginStatus = LoginStatus.Error;

            try
            {
                using (StringWriter stream = new StringWriter())
                {
                    m_loginSerializer.Serialize(stream, userData);

                    string data = stream.ToString();
                    string encryptedString = EncDec.RijndaelEncrypt(data, "12345");

                    var client = new RestClient(PluginURL);
                    var request = new RestRequest(LoginSet, Method.POST);
                    request.Timeout = RequestTimeout;
                    request.AddParameter("application/x-www-form-urlencoded", encryptedString, ParameterType.RequestBody);

                    var response = client.Execute<Boolean>(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        loginStatus = LoginStatus.Ok;
                    }
                    else if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        loginStatus = LoginStatus.NoPermissions;
                    }
                    else if(response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        loginStatus = LoginStatus.PwdInvalid;
                    }
                }
            }
            catch (Exception)
            { 
            }

            return loginStatus;
        }

        public void SaveExportIntervalData(ExportInterval interval)
        {
            interval.LoginName = this.UserID;

            using (StringWriter stream = new StringWriter())
            {
                m_intervalSerializer.Serialize(stream, interval);

                string data = stream.ToString();
                string encryptedString = EncDec.RijndaelEncrypt(data, "12345");

                var client = new RestClient(PluginURL);
                var request = new RestRequest(IntervalSet, Method.POST);

                request.Timeout = RequestTimeout;
                request.AddParameter("episodeId", EpisodeID, ParameterType.UrlSegment);
                request.AddParameter("application/x-www-form-urlencoded", encryptedString, ParameterType.RequestBody);

                var response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    MessageBoxWindow msgBox = new MessageBoxWindow();
                    msgBox.MessageTitle.Text = "Error";
                    msgBox.MessageDescription.Text = "Failed to export data.";
                }
            }
        }

        public ExportInterval FillExportIntervalData(int intervalId)
        {
            ExportInterval exportInterval = null;

            try
            {
                var client = new RestClient(PluginURL);
                var request = new RestRequest(IntervalGet, Method.GET);

                request.AddParameter("episodeId", EpisodeID, ParameterType.UrlSegment);
                request.AddParameter("intervalId", intervalId, ParameterType.UrlSegment);

                request.Timeout = RequestTimeout;

                var response = client.Execute(request);
                var xElementResponse = XElement.Parse(response.Content);
                string encryptedString = xElementResponse.Value;

                string decryptedString = EncDec.RijndaelDecrypt(encryptedString, "12345");
                StringReader decryptedStringReader = new StringReader(decryptedString);

                exportInterval = (ExportInterval)this.IntervalSerializer.Deserialize(decryptedStringReader);
            }
            catch (Exception)
            {
            }
            return exportInterval;
        }

        public void StartExport(DateTime startTime, int timeRange)
        {
            if (CRIHandle == IntPtr.Zero) //Patterns
            {
                if (this.CanModify == true)
                {
                    OpenExportDialog(startTime, timeRange);
                }
                else
                {
                    MessageBoxWindow msgBox = new MessageBoxWindow();
                    msgBox.MessageTitle.Text = "Insufficient Privilege";
                    msgBox.MessageDescription.Text = "Cannot perform export because you do not have sufficient privileges.";

                    msgBox.ShowDialog();
                }
            }
            else //CheckList
            {
                RunCRIExport(startTime, timeRange);
            }
        }

        private void RunCRIExport(DateTime startTime, int timeRange)
        {
            LoginWindow login = new LoginWindow(4000600, 1071010);

            login.ShowDialog();

            if (login.DialogResult.HasValue && login.DialogResult.Value == true)
            {
                OpenExportDialog(startTime, timeRange);
            }
        }

        private void ChangeExportIntervalData(ExportInterval interval)
        {
            foreach (BaseExportEntity entity in interval.Concepts)
            {
                switch (entity.Id)
                {
                    case -102101: // MeanContractionInterval
                        ((ExportDoubleEntity)entity).Value = m_meanContractionInterval;
                        ((ExportDoubleEntity)entity).OriginValue = m_meanContractionInterval;
                        break;

                    case -101395: // MeanMontevideoUnits
                        ((ExportIntEntity)entity).Value = m_montevideoUnits;
                        ((ExportIntEntity)entity).OriginValue = m_montevideoUnits;
                        break;

                    case -101385: // MeanBaseline
                        ((ExportIntEntity)entity).Value = m_meanBaseline;
                        ((ExportIntEntity)entity).OriginValue = m_meanBaseline;
                        break;

                    case -101420: // MeanBaselineVariability
                        ((ExportCalculatedComboEntity)entity).Value = m_meanBaselineVariability.HasValue == true ? m_meanBaselineVariability.Value.ToString() : null;
                        ((ExportCalculatedComboEntity)entity).OriginalValue = m_meanBaselineVariability.HasValue == true ? m_meanBaselineVariability.Value.ToString() : null;
                       break;

                    //case -101417: // NumOfAccelerations
                    //   if (m_meanBaseline == null && m_meanBaselineVariability == null)
                    //   {
                    //       ((ExportCalculatedComboEntity)entity).Value = null;
                    //       ((ExportCalculatedComboEntity)entity).OriginalValue = null;
                    //   }
                    //   break;
                }
            }

            m_meanContractionInterval = null;
            m_montevideoUnits = null;
            m_meanBaseline = null;
            m_meanBaselineVariability = null;
        }

        private void OpenExportDialog(DateTime startTime, int timeRange)
        {
            IPatternsExportableChunk chunkData = this.ExportableChunks.FirstOrDefault(t => t.getStartTime() == startTime);
            ExportInterval interval = FillExportIntervalData(chunkData.getIntervalID());

            if (interval != null)
            {
                this.SelectedChunkStartTime = startTime;

                PatternsUIManager.Instance.RaiseBtnPressedEvent(startTime, startTime.AddMinutes(timeRange), chunkData.getIntervalID());

                //PresentationSource MainWindowPresentationSource = PresentationSource.FromVisual(m_navigationPanel);
                //Matrix m = MainWindowPresentationSource.CompositionTarget.TransformToDevice;
                //this.DpiFactor = m.M22;

                if (timeRange > 15)
                {
                    ChangeExportIntervalData(interval);
                }
                
                ChunkControl chunk = m_navigationPanel.GetChunk(startTime);
                Rect chunkLocation = GetControlRect(chunk);

                ExportToEMRWindow exportDlg = new ExportToEMRWindow(startTime, startTime.AddMinutes(timeRange), interval);
                exportDlg.WindowStartupLocation = WindowStartupLocation.Manual;

                if (chunkLocation.Left < 0)
                {
                    Point rightPnt = m_navigationPanel.PointToScreen(new Point(m_navigationPanelsourceParams.Width, 0));
                    exportDlg.Left = rightPnt.X - exportDlg.Width;
                    exportDlg.Bottom = rightPnt.Y;
                }
                else if ((chunkLocation.Left + chunkLocation.Width) > m_navigationPanelsourceParams.Width)
                {
                    Point leftPnt = m_navigationPanel.PointToScreen(new Point(0, 0));
                    exportDlg.Left = leftPnt.X;
                    exportDlg.Bottom = leftPnt.Y;
                }
                else
                {
                    if (chunkLocation.Left - exportDlg.Width < 0)
                    {
                        Point rightPnt = m_navigationPanel.PointToScreen(new Point(m_navigationPanelsourceParams.Width, 0));
                        exportDlg.Left = rightPnt.X - exportDlg.Width;
                        exportDlg.Bottom = rightPnt.Y;
                    }
                    else
                    {
                        Point floatPnt = chunk.PointToScreen(new Point(0, 0));
                        if (chunk.TimeRange == 15)
                        {
                            if (floatPnt.X - chunk.Width - exportDlg.Width > 0)
                            {
                                exportDlg.Left = floatPnt.X - chunk.Width - exportDlg.Width;
                            }
                            else
                            {
                                floatPnt = m_navigationPanel.PointToScreen(new Point(m_navigationPanelsourceParams.Width, 0));
                                exportDlg.Left = floatPnt.X - exportDlg.Width;
                            }
                        }
                        else
                        {
                            exportDlg.Left = floatPnt.X - exportDlg.Width;
                        }
                        exportDlg.Bottom = floatPnt.Y;
                    }
                }

                exportDlg.ShowDialog();

                if (exportDlg.DialogResult.HasValue == true && exportDlg.DialogResult.Value == true)
                {
                    SaveExportIntervalData(exportDlg.Interval);
                }

                PatternsUIManager.Instance.RaiseExportDialogClosedEvent(startTime, startTime.AddMinutes(timeRange));

                m_navigationPanel.SetSelectedChunk(startTime, false);

                this.SelectedChunkStartTime = DateTime.MinValue;
            }
            else
            {
                MessageBoxWindow msgBox = new MessageBoxWindow();
                msgBox.MessageTitle.Text = "Error";
                msgBox.MessageDescription.Text = "Failed to get export data.";
            }
        }

        public void SetExportDialogParams(double meanContractionInterval, int meanBaseline, int meanBaselineVariability, double montevideoUnits)
        {
            if (meanContractionInterval >= 0)
            {
                m_meanContractionInterval = meanContractionInterval;
            }
            else
            {
                m_meanContractionInterval = null;
            }

            if (meanBaseline >= 0)
            {
                m_meanBaseline = meanBaseline;
            }
            else
            {
                m_meanBaseline = null;
            }

            if (meanBaselineVariability >= 0)
            {
                m_meanBaselineVariability = meanBaselineVariability;
            }
            else
            {
                m_meanBaselineVariability = null;
            }

            if (montevideoUnits >= 0)
            {
                m_montevideoUnits = (int)Math.Round(montevideoUnits);
            }
            else
            {
                m_montevideoUnits = null;
            }         
        }

        private Rect GetControlRect(System.Windows.Controls.UserControl ctrl)
        {
            double x = System.Windows.Controls.Canvas.GetLeft(ctrl);
            double y = System.Windows.Controls.Canvas.GetTop(ctrl);

            return new Rect(x, y, ctrl.Width, ctrl.Height);
        }

        public void HideControls()
        {
            ExportableChunks.Clear();
            FireEventUpdateData();

            if (m_navigationPanel != null)
            {
                m_navigationPanel.Width = 0;
                m_navigationPanel.Height = 0;

                m_navigationPanelsourceParams.PositionX = 0;
                m_navigationPanelsourceParams.PositionY = 0;
                m_navigationPanelsourceParams.Height = 0;
                m_navigationPanelsourceParams.Width = 0;

                if (m_navigationPanelSource.IsDisposed == false)
                {
                    m_navigationPanelSource.Dispose();
                }
                m_navigationPanelSource = new HwndSource(m_navigationPanelsourceParams);
                m_navigationPanelSource.RootVisual = m_navigationPanel;
            }

            if (m_exportButton != null)
            {
                m_exportButton.Width = 0;
                m_exportButton.Height = 0;

                m_exportButtonSourceParams.PositionX = 0;
                m_exportButtonSourceParams.PositionY = 0;
                m_exportButtonSourceParams.Height = 0;
                m_exportButtonSourceParams.Width = 0;

                if (m_exportButtonSource.IsDisposed == false)
                {
                    m_exportButtonSource.Dispose();
                }
                m_exportButtonSource = new HwndSource(m_exportButtonSourceParams);
                m_exportButtonSource.RootVisual = m_exportButton;
            }
        }

        public bool IsValidKey(Key e)
        {
            switch (e)
            {
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                case Key.NumLock:
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.OemPeriod:
                case Key.Back:
                case Key.Tab:
                case Key.Delete:
                    return true;

                default:
                    break;
            }
            return false;
        }

        public void RaiseBtnPressedEvent(DateTime from, DateTime to, int intervalID)
        {
            foreach (IPatternEvent evt in m_listeners)
            {
                evt.RaiseBtnPressedEvent(from, to, intervalID);
            }
        }

        public void RaiseExportDialogClosedEvent(DateTime from, DateTime to)
        {
            foreach (IPatternEvent evt in m_listeners)
            {
                evt.RaiseExportDialogClosedEvent(from, to);
            }
        }

        public void RaiseMouseOverEvent(DateTime from, DateTime to)
        {
            foreach (IPatternEvent evt in m_listeners)
            {
                evt.RaiseMouseOverEvent(from, to);

                //string outStr = String.Empty;
                //evt.GetExportDialogParams(50);

                //MessageBox.Show(outStr);
            }
        }

        public void RaiseMouseLeaveEvent(DateTime from, DateTime to)
        {
            foreach (IPatternEvent evt in m_listeners)
            {
                evt.RaiseMouseLeaveEvent(from, to);
            }
        }

        public void RaiseTimeRangeChangedEvent(int timeRange)
        {
            foreach (IPatternEvent evt in m_listeners)
            {
                evt.RaiseTimeRangeChangedEvent(timeRange);
            }
        }

        public void SetExportableChunks(IPatternsExportableChunk[] sections)
        {
            try
            {
                m_sections.Clear();

                m_sections.AddRange(sections);
            }
            catch (Exception)
            {

            }
        }
    }
}
