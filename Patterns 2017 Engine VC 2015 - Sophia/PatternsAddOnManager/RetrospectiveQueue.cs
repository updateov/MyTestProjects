using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CommonLogger;

namespace PatternsAddOnManager
{
    public class RetrospectiveQueue
    {
        #region Construction

        private RetrospectiveQueue()
        {
            NumberOfRunningRetos = 0;
            NumberOfAllowedRetros = Environment.ProcessorCount / 2;
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, RetrospectiveQueue, ctor", "Number of Allowed Retros is: " + NumberOfAllowedRetros.ToString());
            QueueList = new List<RetrospectiveQueueStructure>();
            QueueTimer = new System.Timers.Timer();
            QueueTimer.Elapsed += new System.Timers.ElapsedEventHandler(QueueTimer_Elapsed);
            QueueTimer.Interval = 1000;
            QueueTimer.Start();
        }

        public static RetrospectiveQueue Queue
        {
            get
            {
                if (s_retroQueue == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_retroQueue == null)
                            s_retroQueue = new RetrospectiveQueue();
                    }
                }

                return s_retroQueue;
            }
        }

        #endregion

        public void AddToQueue(PatternsSessionData session, byte[] upPByte, byte[] hrByte)
        {
            lock (s_lockObject)
            {
                var item = new RetrospectiveQueueStructure()
                {
                    FHRByte = hrByte,
                    UPByte = upPByte,
                    Session = session
                };

                QueueList.Add(item);
                if (!QueueTimer.Enabled)
                {
                    QueueTimer.Start();
                    Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, RetrospectiveQueue, Add To Queue", "QueueTimer started");
                }

                Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, RetrospectiveQueue, Add To Queue", "AddToQueue finished");
            }
        }

        #region Event handlers

        void QueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, RetrospectiveQueue, Add To Queue", "QueueTimer tick");
            if (QueueList.Count <= 0)
                QueueTimer.Stop();

            if (NumberOfRunningRetos < NumberOfAllowedRetros)
            {
                Task.Factory.StartNew(() =>
                {
                    PrepareProcessPatterns();
                });
            }
        }

        #endregion

        private void PrepareProcessPatterns()
        {
            if (NumberOfRunningRetos < NumberOfAllowedRetros && QueueList.Count > 0)
            {
                RetrospectiveQueueStructure item = null;
                lock (s_lockObject)
                {
                    if (NumberOfRunningRetos < NumberOfAllowedRetros && QueueList.Count > 0)
                    {
                        NumberOfRunningRetos++;
                        item = GetQueueHead();
                    }
                }

                if (item == null || item.Session == null)
                    return;

                item.Session.ProcessPatterns(item.UPByte, item.FHRByte);
                lock (s_lockObject)
                {
                    if (NumberOfRunningRetos > 0)
                        NumberOfRunningRetos--;

                    if (QueueList.Count <= 0)
                    {
                        Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Retrospective Queue, Prepare Process Patterns", "Stopping QueueTimer");
                        QueueTimer.Stop();
                    }
                }

            }
        }

        private RetrospectiveQueueStructure GetQueueHead()
        {
            lock (s_lockObject)
            {
                if (QueueList.Count <= 0)
                    return null;

                var item = QueueList[0];
                QueueList.RemoveAt(0);
                Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Retrospective Queue, Get QueueHead", "Found next retrospective item");
                return item;
            }
        }

        #region Members and properties

        public System.Timers.Timer QueueTimer { get; set; }
        public List<RetrospectiveQueueStructure> QueueList { get; set; }
        private static Object s_lockObject = new Object();
        private static RetrospectiveQueue s_retroQueue = null;
        private int NumberOfRunningRetos { get; set; }
        private int NumberOfAllowedRetros { get; set; }

        public class RetrospectiveQueueStructure
        {
            public RetrospectiveQueueStructure()
            {
            }

            public byte[] UPByte { get; set; }
            public byte[] FHRByte { get; set; }
            public PatternsSessionData Session { get; set; }
        }

        #endregion
    }
}
