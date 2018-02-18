using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TalTun.WCF.WCFServiceHost.Platform
{
    public class entProduct
    {
        public long ProdSysId;
        public string ProdName;
        public string ProdImg;
        public double ProdPrice;
        public double ProdCurrency;
    }

    [DataContract]
    public class entSalesTrx
    {
        [DataMember]
        public long TrsSysId { get; set; }
        [DataMember]
        public DateTime TrxDateTime { get; set; }
        [DataMember]
        public long CustId { get; set; }
        [DataMember]
        public long ProdSysId { get; set; }
        [DataMember]
        public int Qnt { get; set; }
        [DataMember]
        public string ProdName { get; set; }
        [DataMember]
        public string ProdImg { get; set; }
        [DataMember]
        public double ProdPrice { get; set; }
        [DataMember]
        public double ProdCurrency { get; set; }
    }
}
