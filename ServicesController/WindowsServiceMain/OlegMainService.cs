using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WindowsServiceMain
{
    public partial class OlegMainService : ServiceBase
    {
        public OlegMainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartManager();
        }

        protected override void OnStop()
        {
            StopManager();
        }

        public void StartManager()
        {
            MainServiceManager.Instance.Start();
        }

        public void StopManager()
        {
            MainServiceManager.Instance.Stop();
        }
    }
}
