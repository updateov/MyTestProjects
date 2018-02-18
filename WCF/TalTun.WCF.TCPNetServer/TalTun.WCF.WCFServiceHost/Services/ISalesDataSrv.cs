using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TalTun.WCF.WCFServiceHost.Platform;

namespace TalTun.WCF.WCFServiceHost.Services
{
    [ServiceContract]
    public interface ISalesDataSrv
    {
        [OperationContract]
        List<entSalesTrx> GetSalesTrx();
    }
}
