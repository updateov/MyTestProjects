using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using TalTun.WCF.WCFServiceHost.Services;

namespace TalTun.WCF.WCFServiceHost
{
    partial class SalesTrxService : ServiceBase
    {
        internal static ServiceHost myServiceHost = null; 
        public SalesTrxService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (myServiceHost != null)
            {
                myServiceHost.Close();
            }

            myServiceHost = new ServiceHost(typeof(SalesDataSrv));
            myServiceHost.Open();
        }

        protected override void OnStop()
        {
            if (myServiceHost != null)
            {
                myServiceHost.Close();
                myServiceHost = null;
            }
        }
    }
}
