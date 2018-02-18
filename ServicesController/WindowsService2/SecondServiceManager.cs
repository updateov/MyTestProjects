using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsService2
{
    public class SecondServiceManager
    {
        private static Timer m_timer;
        private static Object s_lock = new Object();

        private static SecondServiceManager s_instance = null;

        private SecondServiceManager()
        {
            m_timer = new Timer();
            m_timer.Interval = 105000;
            m_timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Trace.WriteLine("bla1");
        }

        public static SecondServiceManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new SecondServiceManager();
                        }
                    }
                }

                return s_instance;
            }
        }

        public void Start()
        {
            m_timer.Start();
        }

        public void Stop()
        {
            m_timer.Stop();
        }
    }
}
