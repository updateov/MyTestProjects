using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace BuildRunnerService
{
    public class BuildTask
    {
        private static object s_lock = new object();

        private WebServiceHost Host { get; set; }

        #region Singleton

        private static BuildTask s_instance;

        private BuildTask()
        {
        }

        public static BuildTask Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new BuildTask();
                    }
                }

                return s_instance;
            }
        }

        #endregion

        public void Start()
        {
            InitBuilderWebHost();
            Host.Open();
        }

        public void Stop()
        {
            if (Host != null)
                Host.Close();
        }

        private void InitBuilderWebHost()
        {
            String uriStr = "http://localhost:8190" + "/BuilderService";
            Host = new WebServiceHost(typeof(Builder), new Uri(uriStr));
            var bind = new WebHttpBinding();
            bind.MaxReceivedMessageSize = 2147483647;
            bind.ReaderQuotas.MaxArrayLength = 2147483647;
            bind.ReaderQuotas.MaxStringContentLength = 2147483647;
        }
    }
}
