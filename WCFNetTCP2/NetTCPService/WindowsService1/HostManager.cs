using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NetTCPService
{
    public class HostManager
    {
        private ServiceHost Host { get; set; }
        public DateTime StartTime { get; private set; }
        #region Singleton operations

        private static Object s_lock = new Object();
        private static HostManager s_instance = null;

        public static HostManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new HostManager();
                    }
                }

                return s_instance;
            }
        }

        private HostManager()
        {
        }

        #endregion

        public void Start()
        {
            InitHost();
        }

        public void Stop()
        {
            Host.Close();
        }

        private void InitHost()
        {

            Host = new ServiceHost(typeof(WCFService));
            Host.Open();
            StartTime = DateTime.UtcNow;
        }
    }
}
