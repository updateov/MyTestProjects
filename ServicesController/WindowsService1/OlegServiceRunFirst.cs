using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WindowsService1
{
    public partial class OlegServiceRunFirst : ServiceBase
    {
        public OlegServiceRunFirst()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            FirstServiceManager.Instance.Start();
        }

        protected override void OnStop()
        {
            FirstServiceManager.Instance.Stop();
        }
    }
}
