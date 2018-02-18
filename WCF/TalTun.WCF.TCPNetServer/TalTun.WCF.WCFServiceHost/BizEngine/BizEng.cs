using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalTun.WCF.WCFServiceHost.Platform;

namespace TalTun.WCF.WCFServiceHost.BizEngine
{
    public class BizEng
    {
        private static BizEng _instance = null;

        public static BizEng GetInstance()
        {
            if (_instance == null)
                _instance = new BizEng();

            return _instance;
        }

        private BizEng() { }

        public List<entProduct> GetProductList(int cata)
        {
            try
            {
                LabDbEntities db = new LabDbEntities();

                List<entProduct> list = (from prod in db.tt_WCF_Lab1_Products
                                         where prod.IsActive == true
                                         select new entProduct
                                         {
                                             ProdSysId = prod.ProdSysId,
                                             ProdName = prod.Name.Trim(),
                                             ProdImg = GenericCode.ImgPath + prod.Img.Trim(),
                                             ProdPrice = prod.Price,
                                         }).ToList();

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<entSalesTrx> GetSalesTrx()
        {
            try
            {
                LabDbEntities db = new LabDbEntities();

                List<entSalesTrx> list = (from sales in db.tt_WCF_Lab1_SalesTrx
                                          join prod in db.tt_WCF_Lab1_Products on sales.ProdSysId equals prod.ProdSysId
                                          where prod.IsActive == true
                                          select new entSalesTrx
                                          {
                                              TrsSysId = sales.TrsSysId,
                                              TrxDateTime = sales.TrxDateTime,
                                              CustId = sales.CustId,
                                              Qnt = sales.Qnt,
                                              ProdSysId = prod.ProdSysId,
                                              ProdName = prod.Name.Trim(),
                                              ProdImg = GenericCode.ImgPath + prod.Img.Trim(),
                                              ProdPrice = prod.Price,
                                          }).ToList();

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
