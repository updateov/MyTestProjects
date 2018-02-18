using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using PeriGen.Patterns.Engine.Data;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using CommonLogger;
using PeriGen.Patterns.Engine;

namespace PatternsAddOnManager
{
    public sealed class PatternsSessionData : IDisposable
    {
		/// <summary>
		/// No data constant for HR and UP (pen-up)
		/// </summary>
		public const byte NoData = 255;

        /// <summary>
        /// Minimum tracing length to invoke the patterns calculation
        /// </summary>
        public const int MinimumTracingsLength = 60;

        /// <summary>
        /// Maximum length of bridgeable gap
        /// </summary>
        public const int MaximumBridgeableGap = 300;

        public PatternsSessionData(String guidStr = "")
        {
            GUID = guidStr;
            ProcessAccumulator = 0;
            Init();
        }

        private void Init()
        {
            lock (s_mainLockObject)
            {
                Tracings = new TracingBlock();
                LastRequest = DateTime.Now;
                LastResultRequest = DateTime.Now;
                Results = new List<DetectionArtifact>();
                for (int i = 0; i < 15 && Engine == null; i++)
                {
                    Engine = new PatternsProcessorWrapper(GUID, DateTime.Now);
                }
                // PAW20171123
                if (Engine == null)
                    Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Patterns Session Data, Init", "PAW debug: Failed to create an Engine");
                else
                    Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Patterns Session Data, Init", "PAW debug: Engine created");

                StartTime = DateTime.MinValue;
                EndTimeOfPreviousTracings = DateTime.MinValue;
                Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Patterns Session Data, Init", "Session created");
            }
        }

        public void Dispose()
        {
            lock (s_mainLockObject)
            {
                UnInitSessionData();
                Results.Clear();
            }       
        }

        private void ReInstance(TracingData inData, int nTracingLength)
        {
            lock (s_mainLockObject)
            {
                UnInitSessionData();
                LastRequest = DateTime.Now;

                // Clear buffers of live calculation
                m_fhrToProcess.Clear();
                m_upToProcess.Clear();

                String logHeader = "Patterns Add On Manager, Patterns Session Data, ReInstance";
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Engine should be null, is " + Engine == null ? "null" : "exists");
                Engine = new PatternsProcessorWrapper(GUID, DateTime.Now);
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Engine should exist, is " + Engine == null ? "null" : "exists");
                System.Diagnostics.Trace.WriteLine("ReInstance: inData.StartTime (before RI)= " + StartTime.ToString());
                StartTime = inData.StartTime;
                EndTimeOfPreviousTracings = inData.StartTime.AddSeconds(nTracingLength);
                System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns: EndTimeOfPreviousTracings (after RI)= " + EndTimeOfPreviousTracings.ToString());
                System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns: inData.StartTime (after RI)= " + StartTime.ToString());
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Engine recreated");
            }
        }

        private void UnInitSessionData()
        {
            Engine.Dispose();
        }

        public void CleanSessionData(DateTime lastPoll)
        {
            lock (s_mainLockObject)
            {
                if (Results.Count <= 0)
                    return;

                // Oleg: No Chrono fix
                if (LastDetectedArtifact != lastPoll)
                {
                    LastDetectedArtifact = lastPoll;
                    RemoveReceivedData(LastDetectedArtifact);
                    Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Patterns Session Data, Clean Session Data", "Session data deleted");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastPoll">End time of last artifact</param>
        /// <returns>Start time of last artifact</returns>
        private DateTime GetDetectionStartTime(DateTime lastPollEndTime)
        {
            lock (s_mainLockObject)
            {
                String logHeader = "Patterns Add On Manager, Patterns Session Data, Get Detection Start Time";
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Last Poll End Time: " + lastPollEndTime.ToString());
                var elems = from c in Results
                            where c.EndTime.Equals(lastPollEndTime)
                            select c.StartTime;

                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Results size before: " + Results.Count.ToString());

                if (elems.Count() <= 0)
                {
                    var elem = (from c in Results
                                select c.StartTime).Min();

                    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Results size when there are no equal end times: " + Results.Count.ToString());
                    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Detection Start Time: " + elem.ToString());
                    return elem;
                }

                var elemRet = elems.Max();
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Detection Start Time: " + elemRet.ToString());
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Results size after: " + Results.Count.ToString());
                return elemRet;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastPoll">Start time of last artifact</param>
        private void RemoveReceivedData(DateTime lastArtifactEndTime)
        {
            lock (s_mainLockObject)
            {
                String logHeader = "Patterns Add On Manager, Patterns Session Data, Remove Received Data";
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, lastArtifactEndTime.ToString());
                int size = Results.Count;
                for (int i = 0; i < size; i++)
                {
                    if (Results[i].EndTime <= lastArtifactEndTime)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, Results[i].ToString());
                        Results.RemoveAt(i);
                        size = Results.Count;
                        --i;
                    }
                }
            }
        }

        public bool AppendRequest(TracingData inData)
        {
            String logHeader = "Patterns Add On Manager, Patterns Session Data, Append Request";
            String inDataTrace = "PreviousDetectededEndTime = " + inData.PreviousDetectededEndTime + "\nStartTime = " + inData.StartTime;
            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, inDataTrace);

            //PAW
            Logger.WriteLogEntry(TraceEventType.Verbose, "Debug PAW:FHR", inData.Fhr);
            Logger.WriteLogEntry(TraceEventType.Verbose, "Debug PAW:UP", inData.Up);


            System.Diagnostics.Trace.WriteLine("AppendRequest reached");
            if (inData.Fhr == null || inData.Up == null)
            {
                Logger.WriteLogEntry(TraceEventType.Error, logHeader, "Either FHR or UP is NULL");
                return false;
            }

            System.Diagnostics.Trace.WriteLine("AppendRequest: m_GUIDQueue size (before nq) = " + m_GUIDQueue.Count);

            m_GUIDQueue.Enqueue(inData);
            System.Diagnostics.Trace.WriteLine("AppendRequest: m_GUIDQueue size (after nq) = " + m_GUIDQueue.Count);
            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "GUIDQueue size: " + m_GUIDQueue.Count);
            if (m_GUIDQueue.Count == 1)
                PrepareProcessPatterns();

            return true;
        }

        /// <summary>
        /// High level method called by WCF module, while requesting patterns calculation.
        /// </summary>
        /// <param name="inData">Contains the current chunk of tracings data</param>
        /// <returns></returns>
        public void PrepareProcessPatterns()
        {
            String logHeader = "Patterns Add On Manager, Patterns Session Data, Process Patterns";
            System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns reached, GUIDQueue size: " + m_GUIDQueue.Count);
            if (m_GUIDQueue.Count <= 0)
            {
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "No requests in queue, waiting for new requests.");
                System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns: m_GUIDQueue size <= 0");
                return;
            }

            System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns: (before TryPeek) GUIDQueue size: " + m_GUIDQueue.Count);
            TracingData inData;
            bool bSucc = m_GUIDQueue.TryPeek(out inData);
            if (!bSucc || inData == null)
            {
                System.Diagnostics.Trace.WriteLine("Couldn't retrieve TracingData, probably broken GUID, try reinitializing.");
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Couldn't retrieve TracingData, probably broken GUID, try reinitializing.");
                return;
            }

            System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns: GUIDQueue size: " + m_GUIDQueue.Count);
            String inDataTrace = "PreviousDetectededEndTime = " + inData.PreviousDetectededEndTime + "\nStartTime = " + inData.StartTime;
            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, inDataTrace);
            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "GUIDQueue size: " + m_GUIDQueue.Count);

            //Remove calculated data
            CleanSessionData(inData.PreviousDetectededEndTime);

			// Do it only once in the beginning, otherwise causes time slides in results (see GEII-2)
            if (StartTime == DateTime.MinValue)
                StartTime = inData.StartTime;

            LastRequest = DateTime.Now;
            List<byte> FHRsList = Convert.FromBase64String(inData.Fhr).ToList();
            List<byte> UPsList = Convert.FromBase64String(inData.Up).ToList();

            if (UPsList.Count * 4 != FHRsList.Count)
                EqualizeData(UPsList, FHRsList);

            bool bBridgeableGap = true;
            if (EndTimeOfPreviousTracings == DateTime.MinValue)
                EndTimeOfPreviousTracings = inData.StartTime.AddSeconds(UPsList.Count - 1);
            else
                bBridgeableGap = UpdateEndTracingTime(inData, FHRsList, UPsList);

            System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns: EndTimeOfPreviousTracings = " + EndTimeOfPreviousTracings.ToString());
            System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns: inData.StartTime = " + inData.StartTime.ToString());

            if (!bBridgeableGap)
            {
                Logger.WriteLogEntry(TraceEventType.Error, logHeader, "Reinstance Engine");
                System.Diagnostics.Trace.WriteLine("PrepareProcessPatterns: Reinstance");
                ReInstance(inData, UPsList.Count);
            }

            int nExistingTracingsLength = m_upToProcess.Count;

            m_fhrToProcess.AddRange(FHRsList);
            m_upToProcess.AddRange(UPsList);

            if (m_upToProcess.Count >= MinimumTracingsLength)
            {
                //bool blive = (m_upToProcess.Count < 900 || (m_upToProcess.Count < 1800 && CalcTime30Sec < 1000));
                var FHRs = m_fhrToProcess.ToArray();
                var UPs = m_upToProcess.ToArray();

                DateTime curStartTime = inData.StartTime.AddSeconds(-nExistingTracingsLength);
                String msg = "Tracing Start Time: " + curStartTime.ToString() +
                             "\nAbsolute Start Time: " + StartTime.ToString() +
                             "\nTracing Length: " + UPs.Length.ToString();

                //if (UPs.Length < 900)
                //{
                Task.Factory.StartNew(() =>
                {
                    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Processing live");
                    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, msg);
                    ProcessPatterns(UPs, FHRs, true);
                });
                //}
                //else if (UPs.Length > 1800)
                //{
                //    Task.Factory.StartNew(() =>
                //    {
                //        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Processing retrospective");
                //        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, msg);
                //        RetrospectiveQueue.Queue.AddToQueue(this, UPs, FHRs);
                //    });
                //}
                //else
                //{
                //    if (CalcTime30Sec > 1000)
                //    {
                //        Task.Factory.StartNew(() =>
                //        {
                //            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Processing retrospective by load calculation");
                //            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, msg);
                //            RetrospectiveQueue.Queue.AddToQueue(this, UPs, FHRs);
                //        });
                //    }
                //    else
                //    {
                //        Task.Factory.StartNew(() =>
                //        {
                //            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Processing live by load calculation");
                //            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, msg);
                //            ProcessPatterns(UPs, FHRs, blive);
                //        });
                //    }
                //}
            }
            else
            {
                TracingData td;
                bool bSuccess = m_GUIDQueue.TryDequeue(out td);
                Task.Factory.StartNew(() => PrepareProcessPatterns());
            }
        }

        /// <summary>
        /// For testing purposes
        /// </summary>
        /// <param name="UPByte"></param>
        /// <param name="FHRByte"></param>
        public void ProcessPatterns(List<byte> UPSubList, List<byte> FHRSubList, DateTime startTime, DateTime lastDetectedEnd)
        {
            var newBlock = new TracingBlock() { HRs = FHRSubList, UPs = UPSubList, Start = startTime };
            Tracings.HRs = FHRSubList;
            Tracings.UPs = UPSubList;
            Tracings.Start = startTime;
            var FHRs = Tracings.HRs.ToArray();
            var UPs = Tracings.UPs.ToArray();
            ProcessPatterns(UPs, FHRs);
        }

        internal void ProcessPatterns(byte[] UPByte, byte[] FHRByte, bool bLive = false)
        {
            lock (s_mainLockObject)
            {
                int startIndex = 0;
                int length = UPByte.Length;
                String logHeader = "Patterns Add On Manager, Patterns Session Data, Process Patterns";
                System.Diagnostics.Trace.WriteLine("ProcessPatterns reached");
                try
                {
                    //int time_frame = 86400;
                    //int position = 0;
                    //int end = UPByte.Length;
                    ////ProcessAccumulator++;

                    //// In case of live calculation we send to Engine chunks of 60 seconds of tracings each iteration.
                    //int time_increment = bLive ? 60 : time_frame;

                    //// Process the data AS IS
                    //int buffer_size = 0;
                    //while (position < end)
                    //{
                    //    int block_size = Math.Min(time_increment, end - position);
                    //    if (bLive && UPByte.Length - position < 60)
                    //        break;

                    //    DateTime startCalc = DateTime.Now;

                    //    buffer_size = NativeMethods.EngineProcessUP(EngineWorkerHandle, UPByte, position, block_size);
                    //    buffer_size = NativeMethods.EngineProcessHR(EngineWorkerHandle, FHRByte, 4 * position, 4 * block_size);

                    //    position += time_increment;

                    //    if (bLive)
                    //    {
                    //        m_upToProcess.RemoveRange(0, time_increment);
                    //        m_fhrToProcess.RemoveRange(0, (time_increment * 4));
                    //    }
                    //    else
                    //    {
                    //        m_upToProcess.Clear();
                    //        m_fhrToProcess.Clear();
                    //    }

                    //    // Read the results
                    //    if (buffer_size > 0)
                    //    {
                    //        var data = new StringBuilder(buffer_size);
                    //        bool moreData = NativeMethods.EngineReadResults(EngineWorkerHandle, data, buffer_size);

                    //        //if ((DateTime.Now - LastMeasure).TotalSeconds > 30)
                    //        //{
                    //        //    var nTime = (((DateTime.Now - startCalc).TotalMilliseconds) / (block_size * ProcessAccumulator)) * 1800f;
                    //        //    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Required time for 30 minutes calculation is: " + nTime.ToString());

                    //        //    CalcTime30Sec = nTime;
                    //        //}

                    //        //ProcessAccumulator = 0;
                    //        string[] lines = data.ToString().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    //        foreach (string line in lines)
                    //        {
                    //            Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Unformatted Artifact: " + line);
                    //            var artifact = line.ToDetectionArtifact(StartTime);
                    //            if (artifact != null)
                    //            {
                    //                Results.Add(artifact);
                    //                //Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Artifact: " + artifact.ToString() + "Absolute StartTime: " + StartTime.ToString());

                    //                //if (artifact is PeriGen.Patterns.Engine.Data.Baseline)
                    //                //{
                    //                //    var bl = artifact as PeriGen.Patterns.Engine.Data.Baseline;
                    //                //    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Baseline: Y1: " + bl.Y1.ToString() + "Baseline Y2: " + bl.Y2.ToString());
                    //                //}
                    //            }
                    //        }

                    //        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "StartTime: " + StartTime.ToString());
                    //    }

                    bool engineInitialized = PatternsProcessorManager.Instance.InitializeVisit(GUID);
                    if (!engineInitialized)
                        throw new InvalidOperationException("Unable to initialize the calculation engine");

                    StringBuilder data1 = new StringBuilder();
                    bool moreData1 = PatternsProcessorManager.Instance.ProcessVisit(GUID, UPByte, FHRByte, startIndex, length, ref data1);

                    if (data1.Length > 0)
                    {
                        Debug.Assert(!moreData1);
                        string[] lines = data1.ToString().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        foreach (string line in lines)
                        {
                            var artifact = line.ToDetectionArtifact(this.StartTime);
                            if (artifact != null)
                            {
                                Results.Add(artifact);
                            }
                        }
                    }


                    TracingData td;
                    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Before dequeue GUIDQueue size: " + m_GUIDQueue.Count);
                    bool bSucc = m_GUIDQueue.TryDequeue(out td);
                    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "After dequeue GUIDQueue size: " + m_GUIDQueue.Count);
                    if (!bSucc || td == null)
                        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Couldn't retrieve TracingData, probably broken GUID, try reinitializing.");

                    DetectionArtifact first = Results.FirstOrDefault(c => c.StartTime.Equals(Results.Min(d => d.StartTime)));
                    DetectionArtifact last = Results.FirstOrDefault(c => c.StartTime.Equals(Results.Max(d => d.StartTime)));

                    if (first != null && last != null)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "ProcessPatterns: Results size = " + Results.Count);
                        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "First Element: Category: " + first.Category.ToString() + ", Start Time: " + first.StartTime.ToString() + ", End Time: " + first.EndTime.ToString());
                        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Last Element: Category: " + last.Category.ToString() + ", Start Time: " + last.StartTime.ToString() + ", End Time: " + last.EndTime.ToString());
                        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "ProcessPatterns: call PrepareProcessPatterns");

                        System.Diagnostics.Trace.WriteLine("ProcessPatterns: Results size = " + Results.Count);
                        System.Diagnostics.Trace.WriteLine("============================================\nFirst Element: Category: " + first.Category.ToString() + ", Start Time: " + first.StartTime.ToString() + ", End Time: " + first.EndTime.ToString());
                        System.Diagnostics.Trace.WriteLine("Last Element: Category: " + last.Category.ToString() + ", Start Time: " + last.StartTime.ToString() + ", End Time: " + last.EndTime.ToString() + "\n============================================");
                        System.Diagnostics.Trace.WriteLine("ProcessPatterns: call PrepareProcessPatterns");
                    }

                    PrepareProcessPatterns();
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, logHeader, ex.Message);
                }
            }
        }

        /// <summary>
        /// Must make sure the input tracings are continuous in time e.g. no overlaps or gaps
        /// </summary>
        /// <param name="inData"></param>
        /// <param name="FHRsList"></param>
        /// <param name="UPsList"></param>
        private bool UpdateEndTracingTime(TracingData inData, List<byte> FHRsList, List<byte> UPsList)
        {
            lock (s_mainLockObject)
            {
                String logHeader = "Patterns Add On Manager, Patterns Session Data, Update End Tracing Time";
                int nTimeDiff = (int)(inData.StartTime - EndTimeOfPreviousTracings).TotalSeconds;
                if (nTimeDiff < 1) // Overlap
                {
                    int toRem = Math.Abs(nTimeDiff) + 1;
                    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Overlap (sec): " + toRem.ToString());
                    UPsList.RemoveRange(0, toRem);
                    FHRsList.RemoveRange(0, toRem * 4);
                }
                else if (nTimeDiff > 1) // Gap
                {
                    if (nTimeDiff > MaximumBridgeableGap)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "The gap is too long, engine restarting.");
                        return false;
                    }

                    Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "Gap (sec): " + nTimeDiff.ToString());
                    for (int toAdd = nTimeDiff - 1; toAdd > 0; toAdd--)
                    {
                        UPsList.Insert(0, NoData);
                        FHRsList.InsertRange(0, new byte[] { NoData, NoData, NoData, NoData });
                    }
                }

                EndTimeOfPreviousTracings = EndTimeOfPreviousTracings.AddSeconds(UPsList.Count);
                Logger.WriteLogEntry(TraceEventType.Verbose, logHeader, "EndTimeOfPreviousTracings: " + EndTimeOfPreviousTracings.ToString());
                return true;
            }
        }

        private void EqualizeData(List<byte> UPsList, List<byte> FHRsList)
        {
            int diff = (UPsList.Count * 4) - FHRsList.Count;
            if (diff < 0)
                FHRsList.RemoveRange(FHRsList.Count - Math.Abs(diff), Math.Abs(diff));
            else
            {
                for (int i = 0; i < diff; i++)
                {
                    FHRsList.Add(NoData);
                }
            }
        }

        private static DateTime s_lastMeasure = DateTime.Now;
        private static DateTime LastMeasure
        {
            get 
            {
                lock (s_lockObject)
                {
                    return s_lastMeasure;
                }
            }
        }
         

        /// <summary>
        /// Don't ever call this method
        /// </summary>
        private static void SetLastMeasure()
        {
            lock (s_lockObject)
            {
                s_lastMeasure = DateTime.Now;
            }
        }

        private static double s_calcTime30Sec = 0;
        public static double CalcTime30Sec
        {
            get 
            {
                lock (s_lockObject)
                {
                    return s_calcTime30Sec;
                }
            }

            private set
            {
                if ((DateTime.Now - LastMeasure).TotalSeconds > 30)
                    lock (s_lockObject)
                    {
                        if ((DateTime.Now - LastMeasure).TotalSeconds > 30)
                        {
                            SetLastMeasure();
                            s_calcTime30Sec = value;
                        }
                    }
            }
        }

        public bool EngineCreated { get { return Engine != null; } }

        public DateTime LastRequest { get; private set; }
        public DateTime LastResultRequest { get; set; }
        public List<DetectionArtifact> Results { get; set; }
        public TracingBlock Tracings { get; set; }
        public DateTime LastDetectedArtifact { get; set; }
        private DateTime StartTime { get; set; }
        private DateTime EndTimeOfPreviousTracings { get; set; }
        private int ProcessAccumulator { get; set; }
        public int GestationalAge { get; set; }
        public String GUID { get; set; }

        private List<byte> m_fhrToProcess = new List<byte>();
        private List<byte> m_upToProcess = new List<byte>();

        internal PatternsProcessorWrapper Engine { get; set; }
        private static volatile Object s_mainLockObject = new Object();
        public Object LockObject { get { return s_mainLockObject; } }
        private static volatile Object s_lockObject = new Object();
        private ConcurrentQueue<TracingData> m_GUIDQueue = new ConcurrentQueue<TracingData>();
    }

}
