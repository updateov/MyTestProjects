using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalTun.WCF.WCFServiceHost.BizEngine;
using TalTun.WCF.WCFServiceHost.Platform;

namespace TalTun.WCF.WCFServiceHost.Services
{
    public class SalesDataSrv : ISalesDataSrv
    {
        public List<entSalesTrx> GetSalesTrx()
        {
            try
            {
                BizEng biz = BizEng.GetInstance();
                return biz.GetSalesTrx();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
