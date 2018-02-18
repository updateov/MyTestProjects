//Reviewed: 12/01/16
using CommonLogger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PatternsCRIClient.Downloader
{
    public class DownloadsManager
    {
        private const string PATTERNS = "PATTERNS";
        private const string COMMON_DIR = "common/";
        private const string ACTIONS_XML = "Actions.xml"; //TODO: Change to PluginsActions.xml
		private const int SCAN_INTERVAL = 10 * 60 * 1000; //10 min.

		private Timer m_timer;		
        private XmlSerializer m_actionsSerializer = new XmlSerializer(typeof(Actions));

        #region Properties

        private Actions m_actions = null;
        public string CommonDirUrl { get; set; }
        public string AppLocation { get; set; }
        public string ServerUrl { get; set; }

        #endregion

        #region Singleton functionality

        private static volatile DownloadsManager s_instance;
        private static object s_lockObject = new Object();

        public static DownloadsManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new DownloadsManager();
                    }
                }
                return s_instance;
            }
        }

        private DownloadsManager()
        {
            AppLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
			
			m_timer = new Timer();
            m_timer.Interval = SCAN_INTERVAL;
            m_timer.Elapsed += TimerElapsed;
            m_timer.AutoReset = true;
            m_timer.Enabled = true;
        }

        #endregion

		#region Events & Delegates

        public event EventHandler UpdateVersionEvent;
        public void FireEventUpdateVersion()
        {
            var tempHandler = UpdateVersionEvent;
            if (tempHandler != null)
            {
                tempHandler(this, new EventArgs());
            }
        }

        #endregion
		
		void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            CreateActionsList(ServerUrl);

            if (m_actions != null)
            {
                foreach (DownloadAction action in m_actions.Items)
                {
                    if(!CheckVersion(action))
                    {
                        FireEventUpdateVersion();
                        return;
                    }
                }
            }
        }
		
		private bool CheckVersion(DownloadAction action)
        {
            string destPath = action.Destination + action.FileName;

            if (File.Exists(destPath))
            {               
                return CheckFileVersion(destPath, action.Version);
            }
            
            return false;
        }

        private bool CheckFileVersion(string destPath, string version)
        {
            FileVersionInfo destFileVer = FileVersionInfo.GetVersionInfo(destPath);
            Version sourceVer = new Version(version);

            if (destFileVer.FileMajorPart == sourceVer.Major &&
               destFileVer.FileMinorPart == sourceVer.Minor &&
               destFileVer.FileBuildPart == sourceVer.Build &&
               destFileVer.FilePrivatePart == sourceVer.Revision)
            {
                return true;
            }

            return false;
        }

        public void CheckDependencies(string url)
        {
            ServerUrl = url;

            CreateActionsList(ServerUrl);

            if (m_actions != null)
            {
                foreach (DownloadAction action in m_actions.Items)
                {
                    try
                    {
                        RunAction(action);
                    }
                    catch(Exception ex)
                    {
                        Logger.WriteLogEntry(TraceEventType.Critical, Properties.Resources.CRIClient_ModuleName, "Failed to run Action", ex);
                    }
                }
            }
        }      

        private void CreateActionsList(string url)
        {
            try
            {
                if (m_actions != null)
                {
                    m_actions.Items.Clear();
                    m_actions = null;
                }

                string actionXml = String.Empty;
                int lastSlash = url.LastIndexOf('/');

                CommonDirUrl = url.Substring(0, lastSlash + 1);
            
                using (WebClient webClient = new WebClient())
                {
                    actionXml = webClient.DownloadString(CommonDirUrl + ACTIONS_XML);
                }
               
                if (!String.IsNullOrEmpty(actionXml))
                {
                    using (StringReader stringReader = new StringReader(actionXml))
                    {
                        m_actions = (Actions)m_actionsSerializer.Deserialize(stringReader);
                    }
                }
            }
            catch (Exception ex)
            {
                if (m_actions != null)
                {
                    m_actions.Items.Clear();
                    m_actions = null;
                }

                Logger.WriteLogEntry(TraceEventType.Critical, Properties.Resources.CRIClient_ModuleName, "Failed to create Actions List", ex);
            }
        }

        private void RunAction(DownloadAction action)
        {
            string destPath = action.Destination + action.FileName;
            bool downloadFile = false;

            if(File.Exists(destPath))
            {
                if (CheckFileVersion(destPath, action.Version) == true)
                {
                    downloadFile = false;

                    if (action.RegisterAction != null)
                    {
                        //Unregister
                        RegisterDll(action.RegisterAction.Command, "/u \"" + action.Destination + action.RegisterAction.FileName + "\"");
                        //Register
                        RegisterDll(action.RegisterAction.Command, " \"" + action.Destination + action.RegisterAction.FileName + "\"");
                    }
                }
                else
                {
                    if (action.RegisterAction != null)
                    {
                        //Unregister
                        RegisterDll(action.RegisterAction.Command, "/u \"" + action.Destination + action.RegisterAction.FileName + "\"");
                    }

                    File.Delete(destPath);

                    downloadFile = true;
                }
            }
            else
            {
                downloadFile = true;
            }

            if(downloadFile)
            {
                DownloadFile(CommonDirUrl + action.FileName, destPath);

                if (action.RegisterAction != null)
                {
                    //Register
                    RegisterDll(action.RegisterAction.Command, " \"" + action.Destination + action.RegisterAction.FileName + "\"");
                }
            }
        }

        public void RegisterDll(string command, string args)
        {
            switch (command.ToUpper())
            {
                case "REGASM.EXE":
                    RunRegasm(args);
                    break;

                case "REGSVR32.EXE":
                    RunRegsvr32(args);
                    break;
            }
        }

        private void RunRegsvr32(string args)
        {
            try
            {
                string regsvr32 = "REGSVR32.EXE";

                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = regsvr32;
                processInfo.Arguments = "/s" + args;
                processInfo.UseShellExecute = false;
                processInfo.CreateNoWindow = true;
                processInfo.RedirectStandardOutput = true;
                processInfo.Verb = "runas";

                using (Process regProcess = Process.Start(processInfo))
                {
                    regProcess.WaitForExit();
                    regProcess.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, Properties.Resources.CRIClient_ModuleName, "Failed to run RegSvr.", ex);
            }
        }

        private void RunRegasm(string args)
        {
            try
            {
                string regasmPath = GetRegAsmPath();

                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = regasmPath;
                processInfo.Arguments = args;
                processInfo.UseShellExecute = false;
                processInfo.CreateNoWindow = true;
                processInfo.RedirectStandardOutput = true;
                processInfo.Verb = "runas";

                using (Process regProcess = Process.Start(processInfo))
                {
                    regProcess.WaitForExit();
                    regProcess.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, Properties.Resources.CRIClient_ModuleName, "Failed to run RegAsm.", ex);
            }
        }

        private void DownloadFile(string fromURL, string toPath)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(new Uri(fromURL), toPath);
            }
        }

        public string GetRegAsmPath()
        {           
            return Microsoft.Build.Utilities.ToolLocationHelper.GetPathToDotNetFrameworkFile("RegAsm.exe", Microsoft.Build.Utilities.TargetDotNetFrameworkVersion.Version40);

            //string framworkRegPath = @"Software\Microsoft\.NetFramework";
            //RegistryKey netFramework = Registry.LocalMachine.OpenSubKey(framworkRegPath, false);
            //string installRoot = netFramework.GetValue("InstallRoot").ToString();

            //return System.IO.Path.Combine(installRoot, @"v4.0.30319\") + "RegAsm.exe";
        }
    }
}
