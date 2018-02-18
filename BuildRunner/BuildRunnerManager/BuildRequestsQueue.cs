using PeriGenLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BuildRunnerManager
{
    public class BuildRequestsQueue
    {
        #region Members & Properties

        private static BuildRequestsQueue s_queue;
        private static object s_lock = new object();

        public String CurrentBuildingLog { get; private set; }

        public bool Building { get; set; }
        public List<BuildElement> QueueList { get; set; }
        public System.Timers.Timer QueueTimer { get; set; }

        public class BuildElement
        {
            public BuildElement()
            {
            }

            public String Name { get; set; }
            public DateTime RequestTime { get; set; }
        }
        #endregion Members & Properties

        #region Construction

        private BuildRequestsQueue()
        {
            Building = false;
            QueueList = new List<BuildElement>();
            QueueTimer = new System.Timers.Timer();
            QueueTimer.Elapsed += new System.Timers.ElapsedEventHandler(QueueTimer_Elapsed);
            QueueTimer.Interval = 3000;
            QueueTimer.Start();
        }

        public static BuildRequestsQueue Queue
        {
            get
            {
                if (s_queue == null)
                {
                    lock (s_lock)
                    {
                        if (s_queue == null)
                        {
                            s_queue = new BuildRequestsQueue();
                            var now = DateTime.Now;
                        }
                    }
                }

                return s_queue;
            }
        }

        #endregion Construction

        public void AddToQueue(String buildName)
        {
            lock (s_lock)
            {
                var item = new BuildElement() { Name = buildName, RequestTime = DateTime.Now };
                QueueList.Add(item);
                if (!QueueTimer.Enabled)
                {
                    QueueTimer.Start();
                }
            }
        }


        #region Event handlers

        void QueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (QueueList.Count <= 0)
                QueueTimer.Stop();

            Console.Out.WriteLine("Buildig = " + Building);
            if (!Building)
            {
                Task.Factory.StartNew(() =>
                {
                    PrepareBuild();
                });
            }
        }

        #endregion Event handlers

        private void PrepareBuild()
        {
            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "PrepareBuild starting");
            Console.Out.WriteLine("Buildig = " + Building);
            if (!Building && QueueList.Count > 0)
            {
                BuildElement nextBuild = null;
                lock (s_lock)
                {
                    if (!Building && QueueList.Count > 0)
                    {
                        Building = true;
                        nextBuild = GetQueueHead();
                    }
                    else
                        return;
                }

                if (nextBuild == null || nextBuild.Name.Equals(String.Empty))
                {
                    Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "nextBuild == null || Empty");
                    return;
                }

                //RunBatch
                var buildName = nextBuild.Name;
                var de = (from c in BuildManager.Instance.BuildName2Path
                          where c.Key.Equals(buildName)
                          select c).FirstOrDefault();

                var path = de.Value;
                var folder = GetFolder(path);
                using (Process proc = new Process())
                {
                    ProcessStartInfo psi = new ProcessStartInfo(path);
                    Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "ProcessStartInfo created");
                    psi.UseShellExecute = false;
                    psi.WorkingDirectory = folder;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardInput = true;
                    psi.RedirectStandardError = true;
                    psi.ErrorDialog = false;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    //Console.Out.WriteLine("Executing: " + path);
                    Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Executing " + path);
                    proc.StartInfo = psi;
                    proc.Start();
                    System.IO.StreamReader myOutput = proc.StandardOutput;
                    //build.WaitForExit(2000);
                    CurrentBuildingLog = String.Empty;
                    using (StreamWriter wr = new StreamWriter("BuildLog.log"))
                    {
                        while (!proc.HasExited)
                        {
                            string output = myOutput.ReadToEnd();
                            CurrentBuildingLog += output;
                            //Console.Out.WriteLine(output);
                            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", output);
                            wr.WriteLine(output);
                        }
                    }

                    if (proc.HasExited)
                    {
                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Process exited");
                        var logStrList = CurrentBuildingLog.Split('\n').ToList();
                        var buildNum = (from c in logStrList
                                        where c.Contains("Build=")
                                        select c).FirstOrDefault();

                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "got buildNum = " + buildNum);
                        buildNum = buildNum.Remove(0, buildNum.IndexOf("=") + 1).Trim();
                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "extracted buildNum = " + buildNum);
                        if (!Directory.Exists(folder + "\\Log"))
                        {
                            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Create dir");
                            Directory.CreateDirectory(folder + "\\Log");
                        }

                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "dir exist");
                        String logfile = folder + "\\Log\\BuildLog" + buildNum + ".log";
                        using (StreamWriter sw = new StreamWriter(logfile))
                        {
                            sw.Write(CurrentBuildingLog);
                            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Writing log");
                        }

                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Writing log DONE");
                        bool bSucc = false;
                        if (proc.ExitCode == 0)
                            bSucc = true;

                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Get mail list");
                        var mailList = File.ReadAllLines(folder + "\\MailList.txt").ToList(); ;
                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Get mail list DONE");
                        String mailTo = String.Empty;
                        foreach (var item in mailList)
                        {
                            if (item.Equals(String.Empty))
                                continue;

                            if (!mailTo.Equals(String.Empty))
                                mailTo += ", ";

                            mailTo += item;
                        }

                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Send mail start");
                        if (mailTo.Equals(String.Empty))
                            mailTo = "oleg.romanowski@perigen.com";

                        using (MailMessage mail = new MailMessage("lmsbuild3@perigen.com", mailTo))
                        {
                            mail.Subject = buildName + " build " + buildNum + (bSucc ? " Successful" : " Failed");
                            String body = bSucc ? "New build is available" : "Build Failed";
                            mail.Body = body;

                            mail.Attachments.Add(new Attachment(logfile));
                            SmtpClient client = new SmtpClient("notestlv2");
                            client.UseDefaultCredentials = true;

                            try
                            {
                                client.Send(mail);
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Critical, "BuildRunner BuildRequestsQueue, PrepareBuild", "Send mail failed");
                            }
                        }

                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Send mail end");
                    }


                    try
                    {
                        if (!proc.HasExited)
                            proc.Kill();
                        
                        if (File.Exists("BuildLog.log"))
                            File.Delete("BuildLog.log");

                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Delete temp log");

                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Critical, "BuildRunner BuildRequestsQueue, PrepareBuild", ex.ToString());
                    }

                }

                

                lock (s_lock)
                {
                    Building = false;
                    if (QueueList.Count <= 0)
                    {
                        QueueTimer.Stop();
                        Console.Out.WriteLine("Queue list empty, timer stopped");
                    }
                }

                Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Verbose, "BuildRunner BuildRequestsQueue, PrepareBuild", "Exit");

            }
        }

        private String GetFolder(String path)
        {
            String toRet = path.Remove(path.LastIndexOf("\\"));
            return toRet;
        }

        private BuildElement GetQueueHead()
        {
            lock (s_lock)
            {
                if (QueueList.Count <= 0)
                    return null;

                var item = QueueList[0];
                QueueList.RemoveAt(0);
                return item;
            }
        }
    }
}
