using CommonLogger;
using PatternsPluginsManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PatternsPluginsService
{
    public partial class PluginsService : ServiceBase
    {
        public PluginsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Logger.WriteLogEntry(TraceEventType.Information, "PluginsService", "PatternsPluginsService is Starting");
                
                StartManager();
                base.OnStart(args);

                Logger.WriteLogEntry(TraceEventType.Information, "PluginsService", "PatternsPluginsService is Starting");
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "PluginsService", "PatternsPluginsService failed to Start.", ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                Logger.WriteLogEntry(TraceEventType.Information, "PluginsService", "PatternsPluginsService is Stopping");

                StopManager();
                base.OnStop();

                Logger.WriteLogEntry(TraceEventType.Information, "PluginsService", "PatternsPluginsService has Stopped");
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "PluginsService", "PatternsPluginsService failed to Stop.", ex);
            }
        }

        internal void StartManager()
        {
            PatternsManager.Instance.Start();
        }

        internal void StopManager()
        {
            PatternsManager.Instance.Stop();
        }
    }
}
