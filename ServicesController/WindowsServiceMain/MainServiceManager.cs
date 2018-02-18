using PeriGenLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsServiceMain
{
    public class MainServiceManager
    {
        ServiceController m_scFirst;
        ServiceController m_scSecond;
        private static Timer m_timer;

        private static Object s_lock = new Object();

        private static MainServiceManager s_instance = null;

        private MainServiceManager()
        {
            m_scFirst = new ServiceController("OlegServiceRunFirst");
            m_scSecond = new ServiceController("OlegServiceRunSecond");
            m_timer = new Timer();
            m_timer.Interval = 30000;
            m_timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_scFirst.Refresh();
            m_scSecond.Refresh();
            Logger.WriteLogEntry(TraceEventType.Information, "MainService", "m_scFirst status = " + m_scFirst.Status);
            Logger.WriteLogEntry(TraceEventType.Information, "MainService", "m_scSecond status = " + m_scSecond.Status);
            if (m_scSecond.Status != ServiceControllerStatus.Running || m_scFirst.Status != ServiceControllerStatus.Running)
            {
                ServiceController sc = new ServiceController("OlegMainService");
                Task.Factory.StartNew(() => sc.Stop());
            }
        }

        public static MainServiceManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new MainServiceManager();
                        }
                    }
                }

                return s_instance;
            }
        }

        public void Start()
        {
            //m_scFirst.ServiceName = "OlegServiceRunFirst";
            if ((m_scFirst.Status.Equals(ServiceControllerStatus.Stopped)) ||
                (m_scFirst.Status.Equals(ServiceControllerStatus.StopPending)))
                m_scFirst.Start();

            m_scFirst.Refresh();
            Console.Out.WriteLine(m_scFirst.Status.ToString());

            //m_scSecond.ServiceName = "OlegServiceRunSecond";
            m_scSecond.Start();
            m_timer.Start();
        }

        public void Stop()
        {
            //m_scFirst.ServiceName = "OlegServiceRunFirst";
            m_scFirst.Stop();
            //m_scSecond.ServiceName = "OlegServiceRunSecond";
            m_scSecond.Stop();
            m_timer.Stop();
        }
    }
}
