using ServerManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public partial class TestService : ServiceBase
    {
        public TestService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartManager();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            StopManager();
            base.OnStop();
        }

        public void StartManager()
        {
            Manager.Instance.Start(typeof(WCFTestService));
        }

        public void StopManager()
        {
            Manager.Instance.Stop();
        }
    }
}
