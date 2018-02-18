using System;
using System.Diagnostics;
using System.Timers;

namespace WindowsService1
{
    public class FirstServiceManager
    {
        private static Timer m_timer;
        private static Object s_lock = new Object();

        private static FirstServiceManager s_instance = null;

        private FirstServiceManager()
        {
            m_timer = new Timer();
            m_timer.Interval = 100000;
            m_timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Trace.WriteLine("bla1");
        }

        public static FirstServiceManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new FirstServiceManager();
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
