using System;
using System.ServiceModel;

namespace ServerManager
{
    public class Manager
    {
        private static Object s_lock = new Object();

        private static Manager s_instance = null;

        public ServiceHost Host { get; set; }
        public DateTime StartTime { get; set; }

        private Manager()
        {
        }

        public static Manager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new Manager();
                        }
                    }
                }

                return s_instance;
            }
        }

        public void Start(Type serviceType)
        {
            Host = new ServiceHost(serviceType);
            Host.Open();
            StartTime = DateTime.UtcNow;
        }

        public void Stop()
        {
            Host.Close();
        }
    }
}
