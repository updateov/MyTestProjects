using BuildRunnerManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace BuildRunnerService
{
    public partial class BuildService : ServiceBase
    {
        public BuildService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartBuilder();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            StopBuilder();
            base.OnStop();
        }

        internal void StartBuilder()
        {
            BuildTask.Instance.Start();
        }

        internal void StopBuilder()
        {
            BuildTask.Instance.Stop();
        }
    }
}
