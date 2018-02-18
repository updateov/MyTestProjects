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
using System.Xml.Serialization;
using System.IO;
using RestSharp;
using System.Xml.Linq;
using System.Xml;
using Export.Entities.ExportControlConfig;
using Export.Entities;

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

    public class ExportContainerLocation
    {
        public IntPtr ExportContainerHandle { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ExportContainerLocation()
        {
            ExportContainerHandle = IntPtr.Zero;
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }
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

        private ExportContainerLocation m_exportContainerLocation;
        private HwndSource m_exportContainerSource = null;
        private HwndSourceParameters m_exportContainerSourceParams;
        private ExportControlsContainer m_exportContainer = null;

        private string m_contractionIntervalRange = String.Empty;
        private double? m_meanContractionInterval = null;       
        private int? m_meanBaseline = null;
        private double? m_meanBaselineVariability = null;
        private int? m_montevideoUnits = null;

        private List<IPatternsExportableChunk> m_sections = new List<IPatternsExportableChunk>();

        private List<IPatternEvent> m_listeners = new List<IPatternEvent>();

        private List<Window> m_activeWindows = new List<Window>();

        XmlSerializer m_intervalSerializer = new XmlSerializer(typeof(Interval));
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

        public ExportConfig ExportDataConfig { get; set; }

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

        public PatternsWPFAdapter WPFAdapter { get; set; }

        public string PluginURL { get; set; }

        public const string IntervalGet = "ExportPlugin/Episodes/{episodeId}/Intervals/{intervalId}";

        public const string IntervalSet = "ExportPlugin/Episodes/{episodeId}/Intervals";
        public const string IntervalSetStatus = "ExportPlugin/Visits/{visitkey}/Intervals/";

        public const string LoginSet = "ExportPlugin/Users/";
        public const string ExportConfigGet = "ExportPlugin/ExportConfig/";

        public const int RequestTimeout = 10000;

        public string UserID{ get; set; }

        public int EpisodeID { get; set; }

        public bool CanModify { get; set; }

        public Interval ConceptsList { get; set; }

        public bool IsMontevideoVisible { get; set; }

        public string PanelTooltip { get; set; }
       
        public DateTime End36Weeks { get; set; }

        public bool Is15MinView { get; set; }

        public bool Is15MinLeftView { get; set; }

        public int RoundBaselineFHRValue { get; set; }

        public List<IValidatableControl> StopExportHolder { get; set; }

        #region Singleton functionality

        //private static volatile PatternsUIManager s_instance;
        private static object s_lockObject = new Object();

        public PatternsUIManager()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(WindowLoaded));
            EventManager.RegisterClassHandler(typeof(Window), Window.UnloadedEvent, new RoutedEventHandler(WindowUnloaded));

            this.SelectedChunkStartTime = DateTime.MinValue;
            this.CRIHandle = IntPtr.Zero;
            this.ConceptsList = new Interval();
            this.IsMontevideoVisible = false;
            this.RoundBaselineFHRValue = 1;
            this.StopExportHolder = new List<IValidatableControl>();
            //this.DpiFactor = 1.0;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message, "Uncaught Thread Exception",MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //public static PatternsUIManager Instance
        //{
        //    get
        //    {
        //        if (s_instance == null)
        //        {
        //            lock (s_lockObject)
        //            {
        //                if (s_instance == null)
        //                    s_instance = new PatternsUIManager();
        //            }
        //        }
        //        return s_instance;
        //    }
        //}

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

                m_navigationPanel = new NavigationPanelUserControl(this);
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
                m_exportButton = new ExportButtonUserControl(this);
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

        public bool InitExportContainer(IntPtr parentHandle, int x, int y, int width, int height)
        {
            bool res = true;

            try
            {
                m_exportContainerLocation = new ExportContainerLocation()
                {
                    ExportContainerHandle = parentHandle,
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                };

                //m_exportContainer = new ExportControlsContainer(this);
                //m_exportContainer.Width = width;
                //m_exportContainer.Height = height;

                //m_exportContainerSourceParams = new HwndSourceParameters("ExportContainer");
                //m_exportContainerSourceParams.PositionX = x;
                //m_exportContainerSourceParams.PositionY = y;
                //m_exportContainerSourceParams.Height = height;
                //m_exportContainerSourceParams.Width = width;
                //m_exportContainerSourceParams.ParentWindow = m_exportContainerHandle;
                //m_exportContainerSourceParams.WindowStyle = 0x50000000;

                //m_exportContainerSource = new HwndSource(m_exportContainerSourceParams);
                //m_exportContainerSource.RootVisual = m_exportContainer;
            }
            catch (Exception)
            {
                res = false;
            }

            return res;
        }
        public void ResizeExportContainer(int x, int y, int width, int height)
        {
            try
            {
                m_exportContainerLocation.X = x;
                m_exportContainerLocation.Y = y;
                m_exportContainerLocation.Width = width;
                m_exportContainerLocation.Height = height;

                if (m_exportContainer != null)
                {
                    m_exportContainer.Width = width;
                    m_exportContainer.Height = height;

                    m_exportContainerSourceParams.PositionX = x;
                    m_exportContainerSourceParams.PositionY = y;
                    m_exportContainerSourceParams.Height = height;
                    m_exportContainerSourceParams.Width = width;

                    if (m_exportContainerSource.IsDisposed == false)
                    {
                        m_exportContainerSource.Dispose();
                    }
                    m_exportContainerSource = new HwndSource(m_exportContainerSourceParams);
                    m_exportContainerSource.RootVisual = m_exportContainer;
                }
            }
            catch (Exception)
            {
            }
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

        public void SaveExportIntervalData(Interval interval)
        {
            interval.LoginName = this.UserID;

            using (StringWriter stream = new StringWriter())
            {
                interval.RemoveSubItems();
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
                    msgBox.MessageDescription.Text = "Failed to export data.\nStatusCode = " + response.StatusCode.ToString();
                }
            }
        }
        public void SaveIntervalExportedState(DateTime startTime, DateTime endTime, int intervalID, string visitKey)
        {
            Interval interval = new Interval();
            interval.LoginName = this.UserID;
            interval.StartTime = startTime;
            interval.EndTime = endTime;
            interval.IntervalId = intervalID;
            IntConcept dummyConcept = new IntConcept();
            dummyConcept.Id = 1;
            dummyConcept.Value = 1;
            dummyConcept.OriginalValue = 1;
            interval.Concepts.Add(dummyConcept);

            using (StringWriter stream = new StringWriter())
            {
                m_intervalSerializer.Serialize(stream, interval);

                string data = stream.ToString();
                string encryptedString = EncDec.RijndaelEncrypt(data, "12345");

                var client = new RestClient(PluginURL);
                var request = new RestRequest(IntervalSetStatus, Method.POST);

                request.Timeout = RequestTimeout;
                request.AddParameter("visitkey", visitKey, ParameterType.UrlSegment);
                request.AddParameter("application/x-www-form-urlencoded", encryptedString, ParameterType.RequestBody);

                var response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    //MessageBoxWindow msgBox = new MessageBoxWindow();
                    //msgBox.MessageTitle.Text = "Error";
                    //msgBox.MessageDescription.Text = "Failed to export data.";
                }
            }

        }
        public Interval FillExportIntervalData(int intervalId)
        {
            Interval exportInterval = null;

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

                exportInterval = (Interval)this.IntervalSerializer.Deserialize(decryptedStringReader);
            }
            catch (Exception ex)
            {
                MessageBoxWindow msgBox = new MessageBoxWindow();
                msgBox.MessageTitle.Text = "Error";
                msgBox.MessageDescription.Text = ex.ToString();
            }
            return exportInterval;
        }

        public string GetExportConfig()
        {
            string exportConfig = String.Empty;

            try
            {
                var client = new RestClient(PluginURL);
                var request = new RestRequest(ExportConfigGet, Method.GET);
                request.Timeout = RequestTimeout;

                var response = client.Execute(request);
                var xElementResponse = XElement.Parse(response.Content);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var serializer = new XmlSerializer(typeof(ExportConfig));

                    using (StringReader reader = new StringReader(xElementResponse.Value))
                    {
                        ExportDataConfig = (ExportConfig)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    MessageBoxWindow msgBox = new MessageBoxWindow();
                    msgBox.MessageTitle.Text = "Error";
                    msgBox.MessageDescription.Text = "Failed to export configuration.";
                }
            }
            catch { }

            return exportConfig;
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
                try
                {
                    if (Application.Current != null && Application.Current.MainWindow.WindowState != WindowState.Maximized)
                    {
                        Application.Current.MainWindow.WindowState = WindowState.Maximized;
                    }
                    RunCRIExport(startTime, timeRange);
                }
                catch(Exception ex) { }
            }
        }

        private void RunCRIExport(DateTime startTime, int timeRange)
        {
            LoginWindow login = new LoginWindow(this, 4000600, 1071010);

            login.ShowDialog();

            if (login.DialogResult.HasValue && login.DialogResult.Value == true)
            {
                OpenExportDialog(startTime, timeRange);
            }
        }

        private void ChangeExportIntervalData(Interval interval)
        {
            foreach (BaseConcept entity in interval.Concepts)
            {
                switch (entity.Id)
                {
                    case 9901731: // MeanContractionInterval
                        if(entity is StringConcept)
                        {
                            ((StringConcept)entity).Value = m_meanContractionInterval.HasValue ? Math.Round(m_meanContractionInterval.Value, 1).ToString() : "-1";
                            ((StringConcept)entity).OriginalValue = m_meanContractionInterval.HasValue ? Math.Round(m_meanContractionInterval.Value, 1).ToString() : "-1";

                        }
                        else if (entity is DoubleConcept)
                        {
                            ((DoubleConcept)entity).Value = m_meanContractionInterval.HasValue ? Math.Round(m_meanContractionInterval.Value, 1) : -1;
                            ((DoubleConcept)entity).OriginalValue = m_meanContractionInterval.HasValue ? Math.Round(m_meanContractionInterval.Value, 1) : -1;

                        }

                        break;
                    case -101395: // MeanMontevideoUnits
                        ((IntConcept)entity).Value = m_montevideoUnits;
                        ((IntConcept)entity).OriginalValue = m_montevideoUnits;
                        break;

                    case -101385: // MeanBaseline
                        ((IntConcept)entity).Value = m_meanBaseline;
                        ((IntConcept)entity).OriginalValue = m_meanBaseline;
                        break;

                    case -101420: // MeanBaselineVariability
                        ((CalculatedComboConcept)entity).Value = m_meanBaselineVariability.HasValue == true ? m_meanBaselineVariability.Value.ToString() : null;
                        ((CalculatedComboConcept)entity).OriginalValue = m_meanBaselineVariability.HasValue == true ? m_meanBaselineVariability.Value.ToString() : null;
                       break;

                    //case -101417: // NumOfAccelerations
                    //   if (m_meanBaseline == null && m_meanBaselineVariability == null)
                    //   {
                    //       ((CalculatedComboConcept)entity).Value = null;
                    //       ((CalculatedComboConcept)entity).OriginalValue = null;
                    //   }
                    //   break;
                }
            }

            m_contractionIntervalRange = String.Empty;
            m_meanContractionInterval = null;
            m_montevideoUnits = null;
            m_meanBaseline = null;
            m_meanBaselineVariability = null;
        }

        private IntervalEventArgs ConvertExportIntervalToEventArgs(Interval interval)
        {
            IntervalEventArgs data = new IntervalEventArgs();

            data.IntervalID = interval.IntervalId;
            data.IntervalDuration = interval.IntervalDuration;
            data.StartTime = interval.StartTime.ToLocalTime();
            data.EndTime = interval.EndTime.ToLocalTime();

            data.ContractionDurationRange = String.Empty;
            data.OriginalContractionDurationRange = String.Empty;
            data.ContractionIntensityRange = String.Empty;
            data.OriginalContractionIntensityRange = String.Empty;

            if (this.IsMontevideoVisible == false)
            {
                data.MontevideoUnits = null;
            }

            foreach (BaseConcept entity in interval.Concepts)
            {
                switch (entity.Id)
                {
                    case -102100:
                        data.IntervalDuration = ((IntConcept)entity).Value != null ? Convert.ToInt32(((IntConcept)entity).Value) : 0;
                        break;

                    case -102101:// ContractionIntervalRange
                        {
                            if (entity is StringConcept && entity.Value != null)
                                data.ContractionIntervalRange = Convert.ToString(entity.Value);
                            else
                                data.ContractionIntervalRange = String.Empty;
                        }
                        break;
                    case 9901731: //MeanContractionInterval
                        {
                            double meanContractionInterval = -1;
                            if (entity is StringConcept && entity.Value != null)
                                meanContractionInterval = ((StringConcept)entity).Value.ToString() != String.Empty ? Convert.ToDouble(((StringConcept)entity).Value) : -1;
                            else if(entity is DoubleConcept && entity.Value != null)
                                meanContractionInterval = Convert.ToDouble(((DoubleConcept)entity).Value);
                            data.MeanContractionInterval = Math.Round(meanContractionInterval, 1);
                        }
                        break;
                    case -101392: // ContractionDuration Range
                        {
                            if (entity is StringConcept && entity.Value != null)
                            {
                                data.ContractionDurationRange = Convert.ToString(entity.Value);
                            }
                            else
                                data.ContractionDurationRange = String.Empty;
                        }
                        break;
                    case 9901732: //Calculated ContractionDuration Range
                        {
                            if (entity is StringConcept && entity.Value != null)
                                data.OriginalContractionDurationRange = Convert.ToString(entity.Value);
                            else
                                data.OriginalContractionDurationRange = String.Empty;
                        }
                        break;

                    case -101393: //InternalIntensity Range
                        {
                            if (entity is StringConcept && entity.Value != null)
                            {
                                data.ContractionIntensityRange = Convert.ToString(entity.Value);
                            }
                            else
                                data.ContractionIntensityRange = String.Empty;
                        }
                        break;
                    case 9901733: //Calculated Intensity Range
                        {
                            if (entity is StringConcept && entity.Value != null)
                                data.OriginalContractionIntensityRange = Convert.ToString(entity.Value);
                            else
                                data.OriginalContractionIntensityRange = String.Empty;
                        }
                        break;

                    case -102102:
                        data.ContractionCount10min = ((IntConcept)entity).Value != null ? Convert.ToInt32(((IntConcept)entity).Value) : 0;
                        break;

                    case -102103:
                        data.LongContractionCount = ((IntConcept)entity).Value != null ? Convert.ToInt32(((IntConcept)entity).Value) : 0;
                        break;

                    case -101395:
                        if (this.IsMontevideoVisible == false)
                        {
                            data.MontevideoUnits = null;
                        }
                        else
                        {
                            data.MontevideoUnits = ((IntConcept)entity).Value != null ? Convert.ToInt32(((IntConcept)entity).Value) : 0;
                        }
                        break;

                    case -101385: //"Baseline FHR"
                        {
                            data.OriginalBaselineFHR = ((IntConcept)entity).Value != null ? Convert.ToInt32(((IntConcept)entity).Value) : 0;
                            double roundBy = 1;
                            if (WPFAdapter.IsExternalUsing)
                                roundBy = 1;//RoundBaselineFHRValue;
                            else
                            {
                                ExportEntity configEntity = this.ExportDataConfig.GetEntityById(-101385);
                                if (configEntity.RoundBy != null && configEntity.RoundBy.HasValue)
                                {
                                    roundBy = configEntity.RoundBy.Value;
                                }
                            }

                            if (roundBy != 1)
                            {
                                if (((IntConcept)entity).Value != null)
                                {
                                    int val = Int32.Parse(((IntConcept)entity).Value.ToString());

                                    data.BaselineFHR = (int)(Math.Round(val / roundBy, MidpointRounding.AwayFromZero) * roundBy);
                                }
                                else
                                {
                                    data.BaselineFHR = 0;
                                }
                            }
                            else
                            {
                                data.BaselineFHR = ((IntConcept)entity).Value != null ? Convert.ToInt32(((IntConcept)entity).Value) : 0;
                            }
                        }
                        break;

                    case -101420: //Variability
                        data.BaselineVariability = ((CalculatedComboConcept)entity).Value != null ? ((CalculatedComboConcept)entity).Value.ToString() : String.Empty;
                        break;

                    case -102123: //FHR Rhythm
                        //data.FHRRhythm = ((ComboConcept)entity).Value.Value;
                        break;

                    case -101417: // "NumOfAccelerations" 
                        double accels = 0;
                        Double.TryParse(((CalculatedComboConcept)entity).Value.ToString(), out accels);

                        data.Accels = accels > 0 ? true : false;
                        break;


                    case -101418: //"NumOfDecelerations"
                        {
                            CalculatedCheckboxGroupConcept decels = (CalculatedCheckboxGroupConcept)entity;
                            int decelsCount = 0;
                            string originValue = String.Empty;

                            foreach (CalculatedCheckboxGroupConceptItem item in decels.Items)
                            {
                                double val = 0;
                                switch (item.Id)
                                {
                                    case 9900021:
                                        Double.TryParse(item.CalculatedValue, out val);
                                        data.Decels.None = val > 0 ? true : false;
                                        break;

                                    case 9901382:
                                        Double.TryParse(item.CalculatedValue, out val);
                                        decelsCount += val > 0 ? 1 : 0;
                                        data.Decels.Early = val > 0 ? true : false;
                                        break;

                                    case 9900798:
                                        Double.TryParse(item.CalculatedValue, out val);
                                        decelsCount += val > 0 ? 1 : 0;
                                        data.Decels.Variable = val > 0 ? true : false;
                                        break;

                                    case 9901383:
                                        Double.TryParse(item.CalculatedValue, out val);
                                        decelsCount += val > 0 ? 1 : 0;
                                        data.Decels.Late = val > 0 ? true : false;
                                        break;

                                    case 9901381:
                                        Double.TryParse(item.CalculatedValue, out val);
                                        decelsCount += val > 0 ? 1 : 0;
                                        data.Decels.Prolonged = val > 0 ? true : false;
                                        break;

                                    case -100451:
                                        Double.TryParse(item.CalculatedValue, out val);
                                        decelsCount += val > 0 ? 1 : 0;
                                        data.Decels.Other = val > 0 ? true : false;
                                        break;
                                }

                                //if (String.IsNullOrEmpty(item.CalculatedValue) == false)
                                //{
                                //    int calcVal = Int32.Parse(item.CalculatedValue);
                                //    decelsCount += calcVal;

                                //    if (calcVal > 0)
                                //    {
                                //        originValue += item.Value + ";";
                                //    }
                                //}
                            }
                            
                            if (decelsCount == 0)
                            {
                                data.Decels.None = true;
                            //    // None CheckBox should be checked
                            //    NoneDecelerations.CalculatedValue = "1";
                            //    Decels.OriginalValue = NoneDecelerations.Value;
                            }
                            else
                            {
                                data.Decels.None = false;
                                //    Decels.OriginalValue = originValue.TrimEnd(';');
                            }
                        }
                        break;

                        //case -102124:
                        //    Recurrence = (ComboConcept)entity;
                        //    break;

                        //case -102013: //"Category"
                        //    Categories = (ComboConcept)entity;
                        //    break;

                        //case -102125: //Checklist
                        //    Checklist = (ComboConcept)entity;
                        //    break;

                        //case -102114: //Comment
                        //    Comment = (ExportStringEntity)entity;
                        //    break;
                }
            }
            return data;
        }

        private void OpenExportDialog(DateTime startTime, int timeRange)
        {
            IPatternsExportableChunk chunkData = this.ExportableChunks.FirstOrDefault(t => t.getStartTime() == startTime);
            Interval interval = FillExportIntervalData(chunkData.getIntervalID());

            if (interval != null)
            {
                this.SelectedChunkStartTime = startTime;

                if (WPFAdapter.IsExternalUsing == false)
                {
                    this.RaiseBtnPressedEvent(startTime, startTime.AddMinutes(timeRange), chunkData.getIntervalID());
                }
                else
                {
                    this.SendIntervalData(interval);
                }

                if (timeRange > 15)
                {
                    ChangeExportIntervalData(interval);
                }

                // If a part of external application
                if (!WPFAdapter.IsExternalUsing == true)               
                {
                    if (m_exportContainerSource != null)
                    {
                        m_exportContainerSource.Dispose();
                        m_exportContainerSource = null;
                        m_exportContainer = null;
                    }

                    m_exportContainer = new ExportControlsContainer(this, interval);
                    m_exportContainer.Width = m_exportContainerLocation.Width;
                    m_exportContainer.Height = m_exportContainerLocation.Height;

                    m_exportContainerSourceParams = new HwndSourceParameters("ExportContainer");
                    m_exportContainerSourceParams.PositionX = m_exportContainerLocation.X;
                    m_exportContainerSourceParams.PositionY = m_exportContainerLocation.Y;
                    m_exportContainerSourceParams.Height = m_exportContainerLocation.Height;
                    m_exportContainerSourceParams.Width = m_exportContainerLocation.Width;
                    m_exportContainerSourceParams.ParentWindow = m_exportContainerLocation.ExportContainerHandle;
                    m_exportContainerSourceParams.WindowStyle = 0x50000000;

                    m_exportContainerSource = new HwndSource(m_exportContainerSourceParams);
                    m_exportContainerSource.RootVisual = m_exportContainer;

                    Point pnt = m_exportContainer.PointToScreen(new Point(m_exportContainerLocation.X, m_exportContainerLocation.Y));

                    ExportToEMRWindow wnd = new ExportToEMRWindow(this, startTime, startTime.AddMinutes(timeRange), interval);
                    wnd.Height = m_exportContainerLocation.Height;
                    wnd.Width = m_exportContainerLocation.Width;
                    wnd.Top = pnt.Y;
                    wnd.Left = pnt.X;

                    if (Application.Current != null && Application.Current.MainWindow != null)
                    {
                        wnd.Owner = Application.Current.MainWindow;
                    }
                    else
                    {
                        // GET CALM hWnd and
                        // Window window = (Window)HwndSource.FromHwnd(hWnd).RootVisual

                        //var hwnd = _dte.MainWindow.HWnd;
                        //var window = HwndSource.FromHwnd((IntPtr)hwnd);
                        //dynamic customWindow = window.RootVisual;

                        //System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
                        //System.Windows.Forms.Application.OpenForms[0].Handle

                        //    [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
                        //    public static extern int FindWindowEx(int hwndParent, int hwndEnfant, int lpClasse, string lpTitre);


                        //    int hwnd = FindWindowEx(0, 0, 0, title);//where title is the windowtitle

                        //            //verification of the window
                        //            if (hwnd == 0)
                        //            {
                        //                throw new Exception("Window not found");
                        //            }

                        wnd.Topmost = true;
                    }
                    wnd.ShowDialog();
                }
            }
            else
            {
                MessageBoxWindow msgBox = new MessageBoxWindow();
                msgBox.MessageTitle.Text = "Error";
                msgBox.MessageDescription.Text = "Failed to get export data.";
            }
        }

        public void AfterExportClosed(DateTime startTime, int timeRange)
        {
            this.RaiseExportDialogClosedEvent(startTime, startTime.AddMinutes(timeRange));

            m_navigationPanel.SetSelectedChunk(startTime, false);

            this.SelectedChunkStartTime = DateTime.MinValue;
        }

        public void ResetIntervalSelection(DateTime startTime, bool isExported)
        {
            this.SelectedChunkStartTime = DateTime.MinValue;
            if (isExported)
                this.m_navigationPanel.ShowChunkExported(startTime);           
            this.m_navigationPanel.SetSelectedChunk(startTime, false);           
            
        }

        public void SetSelectedChunk(DateTime startTime, bool selected)
        {
            this.SelectedChunkStartTime = startTime;
            this.m_navigationPanel.SetSelectedChunk(startTime, false);          
            
        }
        public IntervalEventArgs GetIntervalData(DateTime startTime, DateTime endTime)
        {
            IntervalEventArgs eventData = null;
            IPatternsExportableChunk chunkData = this.ExportableChunks.FirstOrDefault(t => t.getStartTime() == startTime);
            Interval interval = chunkData != null? FillExportIntervalData(chunkData.getIntervalID()) : null;

            if (interval != null)
            {
                eventData = ConvertExportIntervalToEventArgs(interval);
            }

            return eventData;
        }

        public void SetExportDialogParams(double meanContractionInterval, int meanBaseline, int meanBaselineVariability, double montevideoUnits)
        {
            if (meanContractionInterval >= 0)
            {
                m_meanContractionInterval = Math.Round(meanContractionInterval, 1);        
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
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseBtnPressedEvent(from, to, intervalID);
                }
            }
            else 
            {
                bool isExported = false;
                if (WPFAdapter.IsExternalUsing)
                {
                    ChunkControl chunk = m_navigationPanel.GetChunk(from);
                    if (chunk != null && chunk.IsExported)
                        isExported = true;
                }
                if (!isExported)
                {
                    Interval interval = FillExportIntervalData(intervalID);
                    IntervalEventArgs args = ConvertExportIntervalToEventArgs(interval);
                    WPFAdapter.OnIntervalPressed(this, args);
                }
            }
        }

        public void SendIntervalData(Interval interval)
        {
            IntervalEventArgs args = ConvertExportIntervalToEventArgs(interval);
            WPFAdapter.OnIntervalPressed(this, args);
        }

        public void RaiseExportDialogClosedEvent(DateTime from, DateTime to)
        {
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseExportDialogClosedEvent(from, to);
                }
            }
            else
            {
                WPFAdapter.OnExportDialogClosed(this, from, to);           
            }

            m_navigationPanel.UpdateChunks();
            m_exportButton.UpdateButton();
        }

        public void RaiseMouseOverEvent(DateTime from, DateTime to)
        {
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseMouseOverEvent(from, to);
                }
            }
            else
            {
                WPFAdapter.OnIntervalMouseOver(this, from, to);
            }
        }

        public void RaiseMouseLeaveEvent(DateTime from, DateTime to)
        {
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseMouseLeaveEvent(from, to);
                }
            }
            else
            {
                WPFAdapter.OnIntervalMouseLeave(this, from, to);
            }
        }

        public void RaiseTimeRangeChangedEvent(int timeRange)
        {
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseTimeRangeChangedEvent(timeRange);
                }
            }
            else
            {
                WPFAdapter.OnTimeRangeChanged(this, timeRange);
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

        public void RaiseToggleSwitchEvent(bool bRight)
        {
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseToggleSwitchEvent(bRight);
                }
            }
            else
            {
                WPFAdapter.OnToggleSwitchEvent(this, bRight);
            }

        }

        public void RaiseViewSwitchEvent(bool to15Min)
        {
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseViewSwitchEvent(to15Min);
                }
            }
            else
            {
                WPFAdapter.OnViewSwitchEvent(this, to15Min);
            }

        }

        public void RaiseViewSwitchToLeft15MinEvent()
        {
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseViewSwitchToLeft15MinEvent();
                }
            }
            else
            {
                WPFAdapter.OnViewSwitchTo15MinViewEvent(this, false);
            }

        }

        public void RaiseViewSwitchToRight15MinEvent()
        {
            if (m_listeners != null && m_listeners.Count > 0)
            {
                foreach (IPatternEvent evt in m_listeners)
                {
                    evt.RaiseViewSwitchToRight15MinEvent();
                }
            }
            else
            {
                WPFAdapter.OnViewSwitchTo15MinViewEvent(this, true); 
            }

        }


    }
}
