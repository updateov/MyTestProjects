using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TalTun.WCF.WebClient
{
    public partial class SalesDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }

        void LoadProducts()
        {
            try
            {
                SalesSrvRef.SalesDataSrvClient srv = new SalesSrvRef.SalesDataSrvClient();
                List<SalesSrvRef.entSalesTrx> prodList = srv.GetSalesTrx().ToList();
                //List<entProduct> productList = new List<entProduct>();
                //srv.GetProducts();

                RepInfoOne.DataSource = prodList;
                RepInfoOne.DataBind();
            }
            catch (Exception ex)
            {
            }
        }
    }
}