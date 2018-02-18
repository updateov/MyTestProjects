using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WCFNetTCPService
{
    public partial class WCFService : ServiceBase
    {
        public WCFService()
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
            HostManager.Instance.Start();
        }

        public void StopManager()
        {
            HostManager.Instance.Stop();
        }
    }
}
